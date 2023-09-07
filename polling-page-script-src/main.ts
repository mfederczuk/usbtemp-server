/*
 * SPDX-License-Identifier: CC0-1.0
 */

import { PageConfiguration } from "./configuration";
import { PollingHandler } from "./polling";
import { IntlTemperatureFormatter } from "./thermology/IntlTemperatureFormatter";
import type { TemperatureFormatter } from "./thermology/TemperatureFormatter";

window.addEventListener("load", () => {
	const pageConfiguration: PageConfiguration =
		PageConfiguration.fromUrlSearchParams(
			new URLSearchParams(window.location.search),
			console,
		);

	const temperatureFormatter: TemperatureFormatter = new IntlTemperatureFormatter(new Intl.Locale("en-US"));

	const pollingHandler =
		new PollingHandler(
			window,
			console,
			pageConfiguration.pollingInterval,
			temperatureFormatter,
		);

	pollingHandler.start();

	document.addEventListener("visibilitychange", () => {
		if (document.visibilityState === "visible") {
			pollingHandler.start();
		} else {
			pollingHandler.stop();
		}
	});
});
