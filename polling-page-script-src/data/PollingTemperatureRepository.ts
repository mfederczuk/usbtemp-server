/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import type { ApiService } from "../api/ApiService";
import type { Duration } from "../utils/Duration";
import { Poller } from "../utils/Poller";
import type { ActionScheduler } from "../utils/actionScheduling/ActionScheduler";
import { Logger } from "../utils/logging/Logger";
import type { Temperature } from "../utils/thermology/Temperature";
import type { TemperatureRepository } from "./TemperatureRepository";

export class PollingTemperatureRepository implements TemperatureRepository {

	readonly #actionScheduler: ActionScheduler;
	readonly #pollingInterval: Duration;
	readonly #apiService: ApiService;
	readonly #logger: Logger;

	#poller: Poller | null = null;

	public constructor(
		actionScheduler: ActionScheduler,
		pollingInterval: Duration,
		apiService: ApiService,
		logger: Logger,
	) {
		this.#actionScheduler = actionScheduler;
		this.#pollingInterval = pollingInterval;
		this.#apiService = apiService;
		this.#logger = logger;

		Object.seal(this);
	}

	public setTemperatureListener(listener: (temperature: Temperature) => void): void {
		if (this.#poller !== null) {
			this.clearTemperatureListener();
		}

		this.#poller =
			new Poller(this.#actionScheduler, this.#pollingInterval, async (): Promise<void> => {
				let temperature: Temperature;
				try {
					temperature = await this.#apiService.getTemperature();
				} catch (error) {
					if (!(error instanceof Error)) {
						throw error;
					}

					this.#logger.logError(`Error fetching temperature: ${error.message}`);
					return;
				}

				listener(temperature);
			});

		this.#poller.start();
	}

	public clearTemperatureListener(): void {
		if (this.#poller === null) {
			return;
		}

		this.#poller.stop();
		this.#poller = null;
	}
}
