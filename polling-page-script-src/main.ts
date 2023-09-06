/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

const TEMPERATURE_VALUE_ELEMENT_ID: string = "temperature-value";
const POLLING_INTERVAL_DURATION_MS: number = 5000;

let intervalId: number | null = null;

const stopPolling = () => {
	if (typeof intervalId !== "number") {
		console.error("stopPolling(): Interval not running");
		return;
	}

	window.clearInterval(intervalId);
	intervalId = null;
};

const poll = (temperatureValueElement: HTMLElement) => {
	fetch("/temperature")
		.then((response: Response) => response.json())
		.then((json: Record<string, unknown>) => {
			const degreeCelsius = json["degreeCelsius"];
			if (typeof degreeCelsius !== "number") {
				console.error("Invalid response; property \"degreeCelsius\" is not a number");
				return;
			}

			temperatureValueElement.innerHTML = degreeCelsius.toFixed();
		});
};

const startPolling = () => {
	if (typeof intervalId === "number") {
		console.error("startPolling(): Interval already running");
		return;
	}

	const temperatureValueElement: HTMLElement | null = document.getElementById(TEMPERATURE_VALUE_ELEMENT_ID);

	if (!(temperatureValueElement instanceof HTMLElement)) {
		console.error(`Could not find HTML element with ID "${TEMPERATURE_VALUE_ELEMENT_ID}"`);
		return;
	}

	poll(temperatureValueElement);
	intervalId = window.setInterval(
		() => {
			poll(temperatureValueElement);
		},
		POLLING_INTERVAL_DURATION_MS
	);
};

window.addEventListener("load", () => {
	startPolling();
});

document.addEventListener("visibilitychange", () => {
	if (document.visibilityState === "visible") {
		startPolling();
	} else {
		stopPolling();
	}
});
