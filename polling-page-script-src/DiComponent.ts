/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import type { ApiService } from "./api/ApiService";
import { WindowFetchApiService } from "./api/WindowFetchApiService";
import { PageConfiguration } from "./configuration";
import { PollingHandler } from "./polling";
import { IntlTemperatureFormatter } from "./thermology/IntlTemperatureFormatter";
import type { TemperatureFormatter } from "./thermology/TemperatureFormatter";
import { Lazy } from "./utils/Lazy";
import type { ActionScheduler } from "./utils/actionScheduling/ActionScheduler";
import { WindowTimeoutActionScheduler } from "./utils/actionScheduling/WindowTimeoutActionScheduler";
import { ConsoleLogger } from "./utils/logging/ConsoleLogger";
import type { Logger } from "./utils/logging/Logger";

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

			return PageConfiguration
				.fromUrlSearchParams(
					new URLSearchParams(this.#window.location.search),
					logger,
				);
		});

	readonly #lazyApiService: Lazy<ApiService> =
		new Lazy<ApiService>((): ApiService => {
			return new WindowFetchApiService(this.#window);
		});

	readonly #lazyTemperatureFormatter: Lazy<TemperatureFormatter> =
		new Lazy<TemperatureFormatter>((): TemperatureFormatter => {
			return new IntlTemperatureFormatter(this.#locale);
		});

	readonly #lazyPollingHandler: Lazy<PollingHandler> =
		new Lazy<PollingHandler>((): PollingHandler => {
			const actionScheduler: ActionScheduler = new WindowTimeoutActionScheduler(this.#window);
			const apiService: ApiService = this.#lazyApiService.get();
			const logger: Logger = this.#lazyLogger.get();
			const pageConfiguration: PageConfiguration = this.#lazyPageConfiguration.get();
			const temperatureFormatter: TemperatureFormatter = this.#lazyTemperatureFormatter.get();

			return new PollingHandler(
				actionScheduler,
				apiService,
				this.#window,
				logger,
				pageConfiguration.pollingInterval,
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

	public getPollingHandler(): PollingHandler {
		return this.#lazyPollingHandler.get();
	}
}

Object.freeze(DiComponent);
