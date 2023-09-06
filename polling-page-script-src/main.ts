/*
 * SPDX-License-Identifier: CC0-1.0
 */

import { PollingHandler } from "./polling";

const pollingHandler = new PollingHandler(window, console, 5000);

window.addEventListener("load", () => {
	pollingHandler.start();
});

document.addEventListener("visibilitychange", () => {
	if (document.visibilityState === "visible") {
		pollingHandler.start();
	} else {
		pollingHandler.stop();
	}
});
