/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

export class PollingHandler {

	static readonly #TEMPERATURE_VALUE_ELEMENT_ID: string = "temperature-value";
	static readonly #POLLING_INTERVAL_DURATION_MS: number = 5000;

	readonly #targetWindow: Window;
	readonly #loggingConsole: Console;
	#intervalId: number | null = null;

	public constructor(targetWindow: Window, loggingConsole: Console) {
		this.#targetWindow = targetWindow;
		this.#loggingConsole = loggingConsole;

		Object.seal(this);
	}

	public start(): void {
		if (typeof this.#intervalId === "number") {
			this.#logError("startPolling(): Interval already running");
			return;
		}

		const temperatureValueElement: HTMLElement | null =
			this.#targetWindow.document.getElementById(PollingHandler.#TEMPERATURE_VALUE_ELEMENT_ID);

		if (!(temperatureValueElement instanceof HTMLElement)) {
			const msg: string = `Could not find HTML element with ID "${PollingHandler.#TEMPERATURE_VALUE_ELEMENT_ID}"`;
			this.#logError(msg);

			return;
		}

		this.#doPoll(temperatureValueElement);
		this.#intervalId = this.#targetWindow.setInterval(
			() => {
				this.#doPoll(temperatureValueElement);
			},
			PollingHandler.#POLLING_INTERVAL_DURATION_MS
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

	#doPoll(temperatureValueElement: HTMLElement): void {
		void this.#targetWindow.fetch("/temperature")
			.then((response: Response) => response.json())
			.then((json: Record<string, unknown>) => {
				const degreeCelsius = json["degreeCelsius"];
				if (typeof degreeCelsius !== "number") {
					this.#logError("Invalid response; property \"degreeCelsius\" is not a number");
					return;
				}

				temperatureValueElement.innerHTML = degreeCelsius.toFixed();
			});
	}

	#logError(message: string): void {
		this.#loggingConsole.error(message);
	}
}

Object.freeze(PollingHandler);
