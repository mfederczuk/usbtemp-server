/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import { PageConfiguration } from "./configuration";
import type { TemperatureFormatter } from "./thermology/TemperatureFormatter";
import type { Duration } from "./utils/Duration";
import type { ActionScheduler, ScheduledActionHandle } from "./utils/actionScheduling/ActionScheduler";
import type { Logger } from "./utils/logging/Logger";

export class PollingHandler {

	static readonly #MIN_POLLING_INTERVAL: Duration = PageConfiguration.MIN_POLLING_INTERVAL;
	static readonly #MAX_POLLING_INTERVAL: Duration = PageConfiguration.MAX_POLLING_INTERVAL;

	static readonly #TEMPERATURE_TEXT_ELEMENT_ID: string = "temperature-text";

	readonly #actionScheduler: ActionScheduler;
	readonly #targetWindow: Window;
	readonly #logger: Logger;
	readonly #pollingInterval: Duration;
	readonly #temperatureFormatter: TemperatureFormatter;
	#scheduledPollActionHandle: ScheduledActionHandle | null = null;

	public constructor(
		actionScheduler: ActionScheduler,
		targetWindow: Window,
		logger: Logger,
		pollingInterval: Duration,
		temperatureFormatter: TemperatureFormatter,
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
		this.#targetWindow = targetWindow;
		this.#logger = logger;
		this.#pollingInterval = pollingInterval;
		this.#temperatureFormatter = temperatureFormatter;

		Object.seal(this);
	}

	public start(): void {
		if (this.#scheduledPollActionHandle !== null) {
			this.#logError(`${PollingHandler.name}.${this.start.name}(): Polling process already running`);
			return;
		}

		const temperatureTextElement: HTMLElement | null =
			this.#targetWindow.document.getElementById(PollingHandler.#TEMPERATURE_TEXT_ELEMENT_ID);

		if (!(temperatureTextElement instanceof HTMLElement)) {
			const msg: string = `Could not find HTML element with ID "${PollingHandler.#TEMPERATURE_TEXT_ELEMENT_ID}"`;
			this.#logError(msg);

			return;
		}

		this.#doPoll(temperatureTextElement);
	}

	public stop(): void {
		if (this.#scheduledPollActionHandle === null) {
			this.#logError(`${PollingHandler.name}.${this.stop.name}(): Polling process not running`);
			return;
		}

		this.#scheduledPollActionHandle.cancel();
		this.#scheduledPollActionHandle = null;
	}

	#doPoll(temperatureTextElement: HTMLElement): void {
		void this.#targetWindow.fetch("/temperature")
			.then((response: Response) => response.json())
			.then((json: Record<string, unknown>) => {
				const degreeCelsius = json["degreeCelsius"];
				if (typeof degreeCelsius !== "number") {
					this.#logError("Invalid response; property \"degreeCelsius\" is not a number");
					return;
				}

				temperatureTextElement.innerHTML = this.#temperatureFormatter.format(degreeCelsius);

				this.#scheduledPollActionHandle =
					this.#actionScheduler.scheduleAction(
						this.#pollingInterval,
						() => {
							this.#doPoll(temperatureTextElement);
						},
					);
			});
	}

	#logError(message: string): void {
		this.#logger.logError(message);
	}
}

Object.freeze(PollingHandler);
