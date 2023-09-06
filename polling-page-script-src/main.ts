/*
 * SPDX-License-Identifier: CC0-1.0
 */

import { PageConfiguration } from "./configuration";
import { PollingHandler } from "./polling";

let pollingHandler: PollingHandler | null = null;

window.addEventListener("load", (event: Event) => {
	console.debug(`Window event "${event.type}"`);
	const pageConfiguration: PageConfiguration =
		PageConfiguration.fromUrlSearchParams(
			new URLSearchParams(window.location.search),
			console,
		);

	pollingHandler = new PollingHandler(window, console, pageConfiguration.pollingIntervalMs);
	pollingHandler.start();
});

document.addEventListener("visibilitychange", (event: Event) => {
	console.debug(`Document event "${event.type}"`);
	if (pollingHandler === null) {
		console.error(`Document event "${event.type}": Polling handler is null`);
		return;
	}

	if (document.visibilityState === "visible") {
		pollingHandler.start();
	} else {
		pollingHandler.stop();
	}
});
