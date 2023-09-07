/*
 * SPDX-License-Identifier: CC0-1.0
 */

import { PageConfiguration } from "./configuration";
import { PollingHandler } from "./polling";
import { IntlTemperatureFormatter } from "./thermology/IntlTemperatureFormatter";
import type { TemperatureFormatter } from "./thermology/TemperatureFormatter";

let pollingHandler: PollingHandler | null = null;

const documentVisibilityChangedEventListener = (event: Event): void => {
	if (pollingHandler === null) {
		console.error(`Document event "${event.type}": Polling handler is null`);
		return;
	}

	if (document.visibilityState === "visible") {
		pollingHandler.start();
	} else {
		pollingHandler.stop();
	}
};

window.addEventListener("load", () => {
	const pageConfiguration: PageConfiguration =
		PageConfiguration.fromUrlSearchParams(
			new URLSearchParams(window.location.search),
			console,
		);

	const temperatureFormatter: TemperatureFormatter = new IntlTemperatureFormatter(new Intl.Locale("en-US"));

	pollingHandler =
		new PollingHandler(
			window,
			console,
			pageConfiguration.pollingIntervalMs,
			temperatureFormatter,
		);

	pollingHandler.start();

	document.addEventListener("visibilitychange", documentVisibilityChangedEventListener);
});
