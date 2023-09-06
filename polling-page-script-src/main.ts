/*
 * SPDX-License-Identifier: CC0-1.0
 */

import { PollingHandler } from "./polling";

const pollingHandler = new PollingHandler(window, console);

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
