/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import type { ApiService } from "./api/ApiService";
import { PageConfiguration } from "./configuration";
import type { Temperature } from "./thermology/Temperature";
import type { Page } from "./ui/Page";
import type { Duration } from "./utils/Duration";
import type { ActionScheduler, ScheduledActionHandle } from "./utils/actionScheduling/ActionScheduler";
import type { Logger } from "./utils/logging/Logger";

export class PollingHandler {

	static readonly #MIN_POLLING_INTERVAL: Duration = PageConfiguration.MIN_POLLING_INTERVAL;
	static readonly #MAX_POLLING_INTERVAL: Duration = PageConfiguration.MAX_POLLING_INTERVAL;

	readonly #actionScheduler: ActionScheduler;
	readonly #apiService: ApiService;
	readonly #logger: Logger;
	readonly #pollingInterval: Duration;
	readonly #page: Page;
	#scheduledPollActionHandle: ScheduledActionHandle | null = null;

	public constructor(
		actionScheduler: ActionScheduler,
		apiService: ApiService,
		logger: Logger,
		pollingInterval: Duration,
		page: Page,
	) {
		if (pollingInterval.isLessThan(PollingHandler.#MIN_POLLING_INTERVAL)) {
			const msg: string = `Polling interval (${pollingInterval.toString()})` +
				` must not be less than ${PollingHandler.#MIN_POLLING_INTERVAL.toString()}`;
			throw new Error(msg);
		}
		// (2023-09-06)
		// one some browsers, (tested with Firefox v117.0 and Chromium v116.0.5845.96) if the delay for setInterval()
		// is too high, it seems to overflow and set to delay to a very low value. (possible even the lowest value)
		// we have to manually limit it so that it doesn't happen
		if (pollingInterval.isGreaterThan(PollingHandler.#MAX_POLLING_INTERVAL)) {
			const msg: string = `Polling interval (${pollingInterval.toString()})` +
				` must not be greater than ${PollingHandler.#MAX_POLLING_INTERVAL.toString()}`;
			throw new Error(msg);
		}

		this.#actionScheduler = actionScheduler;
		this.#apiService = apiService;
		this.#logger = logger;
		this.#pollingInterval = pollingInterval;
		this.#page = page;

		Object.seal(this);
	}

	public start(): void {
		if (this.#scheduledPollActionHandle !== null) {
			this.#logError(`${PollingHandler.name}.${this.start.name}(): Polling process already running`);
			return;
		}

		this.#doPoll();
	}

	public stop(): void {
		if (this.#scheduledPollActionHandle === null) {
			this.#logError(`${PollingHandler.name}.${this.stop.name}(): Polling process not running`);
			return;
		}

		this.#scheduledPollActionHandle.cancel();
		this.#scheduledPollActionHandle = null;
	}

	#doPoll(): void {
		void this.#apiService.getTemperature()
			.then((temperature: Temperature) => {
				this.#page.setTemperature(temperature);

				this.#scheduledPollActionHandle =
					this.#actionScheduler.scheduleAction(
						this.#pollingInterval,
						() => {
							this.#doPoll();
						},
					);
			});
	}

	#logError(message: string): void {
		this.#logger.logError(message);
	}
}

Object.freeze(PollingHandler);
