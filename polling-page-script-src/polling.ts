/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import { PageConfiguration } from "./configuration";
import type { TemperatureFormatter } from "./thermology/TemperatureFormatter";

export class PollingHandler {

	static readonly #MIN_POLLING_INTERVAL_MS: number = PageConfiguration.MIN_POLLING_INTERVAL_MS;
	static readonly #MAX_POLLING_INTERVAL_MS: number = PageConfiguration.MAX_POLLING_INTERVAL_MS;

	static readonly #TEMPERATURE_TEXT_ELEMENT_ID: string = "temperature-text";

	readonly #targetWindow: Window;
	readonly #loggingConsole: Console;
	readonly #pollingIntervalMs: number;
	readonly #temperatureFormatter: TemperatureFormatter;
	#intervalId: number | null = null;

	public constructor(
		targetWindow: Window,
		loggingConsole: Console,
		pollingIntervalMs: number,
		temperatureFormatter: TemperatureFormatter,
	) {
		if (Number.isNaN(pollingIntervalMs)) {
			throw new Error("Polling interval must not be NaN");
		}
		if (pollingIntervalMs < PollingHandler.#MIN_POLLING_INTERVAL_MS) {
			throw new Error(`Polling interval must not be less than ${PollingHandler.#MIN_POLLING_INTERVAL_MS}ms`);
		}
		// (2023-09-06)
		// one some browsers, (tested with Firefox v117.0 and Chromium v116.0.5845.96) if the delay for setInterval()
		// is too high, it seems to overflow and set to delay to a very low value. (possible even the lowest value)
		// we have to manually limit it so that it doesn't happen
		if (pollingIntervalMs > PollingHandler.#MAX_POLLING_INTERVAL_MS) {
			throw new Error(`Polling interval must not be greater than ${PollingHandler.#MAX_POLLING_INTERVAL_MS}ms`);
		}

		this.#targetWindow = targetWindow;
		this.#loggingConsole = loggingConsole;
		this.#pollingIntervalMs = pollingIntervalMs;
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
			this.#pollingIntervalMs,
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
		this.#loggingConsole.error(message);
	}
}

Object.freeze(PollingHandler);
