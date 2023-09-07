/*
 * SPDX-License-Identifier: CC0-1.0
 */

import { PageConfiguration } from "./configuration";
import { PollingHandler } from "./polling";
import { IntlTemperatureFormatter } from "./thermology/IntlTemperatureFormatter";
import type { TemperatureFormatter } from "./thermology/TemperatureFormatter";
import { ConsoleLogger } from "./utils/logging/ConsoleLogger";
import type { Logger } from "./utils/logging/Logger";

window.addEventListener("load", () => {
	const logger: Logger = new ConsoleLogger(console);

	const pageConfiguration: PageConfiguration =
		PageConfiguration.fromUrlSearchParams(
			new URLSearchParams(window.location.search),
			logger,
		);

	const temperatureFormatter: TemperatureFormatter = new IntlTemperatureFormatter(new Intl.Locale("en-US"));

	const pollingHandler =
		new PollingHandler(
			window,
			logger,
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
