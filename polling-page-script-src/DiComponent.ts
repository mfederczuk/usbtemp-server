/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import type { ApiService } from "./api/ApiService";
import { WindowFetchApiService } from "./api/WindowFetchApiService";
import type { PageConfiguration } from "./configuration/PageConfiguration";
import { PageConfigurationCreator } from "./configuration/PageConfigurationCreator";
import { PollingTemperatureRepository } from "./data/PollingTemperatureRepository";
import type { TemperatureRepository } from "./data/TemperatureRepository";
import { Page } from "./ui/Page";
import { ViewModel } from "./ui/ViewModel";
import { Lazy } from "./utils/Lazy";
import { ActionScheduler } from "./utils/actionScheduling/ActionScheduler";
import { WindowTimeoutActionScheduler } from "./utils/actionScheduling/WindowTimeoutActionScheduler";
import { ConsoleLogger } from "./utils/logging/ConsoleLogger";
import type { Logger } from "./utils/logging/Logger";
import { IntlTemperatureFormatter } from "./utils/thermology/IntlTemperatureFormatter";
import type { TemperatureFormatter } from "./utils/thermology/TemperatureFormatter";

export class DiComponent {

	readonly #window: Window;
	readonly #console: Console;
	readonly #locale: Intl.Locale;

	readonly #lazyLogger: Lazy<Logger> =
		new Lazy<Logger>((): Logger => {
			return new ConsoleLogger(this.#console);
		});

	readonly #lazyPageConfiguration: Lazy<PageConfiguration> =
		new Lazy<PageConfiguration>((): PageConfiguration => {
			const logger: Logger = this.#lazyLogger.get();
			const creator = new PageConfigurationCreator(logger);

			return creator.createFromUrlQueryParams(new URLSearchParams(this.#window.location.search));
		});

	readonly #lazyApiService: Lazy<ApiService> =
		new Lazy<ApiService>((): ApiService => {
			return new WindowFetchApiService(this.#window);
		});

	readonly #lazyTemperatureRepository: Lazy<TemperatureRepository> =
		new Lazy<TemperatureRepository>((): TemperatureRepository => {
			const actionScheduler: ActionScheduler = new WindowTimeoutActionScheduler(this.#window);
			const pageConfiguration: PageConfiguration = this.#lazyPageConfiguration.get();
			const apiService: ApiService = this.#lazyApiService.get();
			const logger: Logger = this.#lazyLogger.get();

			return new PollingTemperatureRepository(
				actionScheduler,
				pageConfiguration.pollingInterval,
				apiService,
				logger,
			);
		});

	readonly #lazyPage: Lazy<Page> =
		new Lazy<Page>((): Page => {
			const temperatureRepository: TemperatureRepository = this.#lazyTemperatureRepository.get();
			const pageConfiguration: PageConfiguration = this.#lazyPageConfiguration.get();

			const viewModel: ViewModel = new ViewModel(temperatureRepository);
			const temperatureFormatter: TemperatureFormatter =
				new IntlTemperatureFormatter(this.#locale, pageConfiguration.decimalDigitsRange);

			return new Page(
				viewModel,
				temperatureFormatter,
			);
		});

	public constructor(
		window: Window,
		console: Console,
		locale: Intl.Locale,
	) {
		this.#window = window;
		this.#console = console;
		this.#locale = locale;

		Object.seal(this);
	}

	public getPage(): Page {
		return this.#lazyPage.get();
	}
}

Object.freeze(DiComponent);
