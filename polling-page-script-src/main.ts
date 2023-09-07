/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { ApiService } from "./api/ApiService";
import { WindowFetchApiService } from "./api/WindowFetchApiService";
import { PageConfiguration } from "./configuration";
import { PollingHandler } from "./polling";
import { IntlTemperatureFormatter } from "./thermology/IntlTemperatureFormatter";
import type { TemperatureFormatter } from "./thermology/TemperatureFormatter";
import type { ActionScheduler } from "./utils/actionScheduling/ActionScheduler";
import { WindowTimeoutActionScheduler } from "./utils/actionScheduling/WindowTimeoutActionScheduler";
import { ConsoleLogger } from "./utils/logging/ConsoleLogger";
import type { Logger } from "./utils/logging/Logger";

window.addEventListener("load", () => {
	const logger: Logger = new ConsoleLogger(console);

	const actionScheduler: ActionScheduler = new WindowTimeoutActionScheduler(window);

	const apiService: ApiService = new WindowFetchApiService(window);

	const pageConfiguration: PageConfiguration =
		PageConfiguration.fromUrlSearchParams(
			new URLSearchParams(window.location.search),
			logger,
		);

	const temperatureFormatter: TemperatureFormatter = new IntlTemperatureFormatter(new Intl.Locale("en-US"));

	const pollingHandler =
		new PollingHandler(
			actionScheduler,
			apiService,
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
