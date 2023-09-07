/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import { PageConfiguration } from "./configuration";
import type { TemperatureFormatter } from "./thermology/TemperatureFormatter";
import type { Duration } from "./utils/Duration";
import type { Logger } from "./utils/logging/Logger";

export class PollingHandler {

	static readonly #MIN_POLLING_INTERVAL: Duration = PageConfiguration.MIN_POLLING_INTERVAL;
	static readonly #MAX_POLLING_INTERVAL: Duration = PageConfiguration.MAX_POLLING_INTERVAL;

	static readonly #TEMPERATURE_TEXT_ELEMENT_ID: string = "temperature-text";

	readonly #targetWindow: Window;
	readonly #logger: Logger;
	readonly #pollingInterval: Duration;
	readonly #temperatureFormatter: TemperatureFormatter;
	#intervalId: number | null = null;

	public constructor(
		targetWindow: Window,
		logger: Logger,
		pollingInterval: Duration,
		temperatureFormatter: TemperatureFormatter,
	) {
		if (pollingInterval.isLessThan(PollingHandler.#MIN_POLLING_INTERVAL)) {
			const msg: string =
				`Polling interval must not be less than ${PollingHandler.#MIN_POLLING_INTERVAL.toString()}`;
			throw new Error(msg);
		}
		// (2023-09-06)
		// one some browsers, (tested with Firefox v117.0 and Chromium v116.0.5845.96) if the delay for setInterval()
		// is too high, it seems to overflow and set to delay to a very low value. (possible even the lowest value)
		// we have to manually limit it so that it doesn't happen
		if (pollingInterval.isGreaterThan(PollingHandler.#MAX_POLLING_INTERVAL)) {
			const msg: string =
				`Polling interval must not be greater than ${PollingHandler.#MAX_POLLING_INTERVAL.toString()}`;
			throw new Error(msg);
		}

		this.#targetWindow = targetWindow;
		this.#logger = logger;
		this.#pollingInterval = pollingInterval;
		this.#temperatureFormatter = temperatureFormatter;

		Object.seal(this);
	}

	public start(): void {
		if (typeof this.#intervalId === "number") {
			this.#logError("startPolling(): Interval already running");
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
		this.#intervalId = this.#targetWindow.setInterval(
			() => {
				this.#doPoll(temperatureTextElement);
			},
			this.#pollingInterval.toMilliseconds(),
		);
	}

	public stop(): void {
		if (typeof this.#intervalId !== "number") {
			this.#logError("stopPolling(): Interval not running");
			return;
		}

		this.#targetWindow.clearInterval(this.#intervalId);
		this.#intervalId = null;
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
			});
	}

	#logError(message: string): void {
		this.#logger.logError(message);
	}
}

Object.freeze(PollingHandler);
