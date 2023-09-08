/*
 * SPDX-License-Identifier: CC0-1.0
 */

import { DiComponent } from "./DiComponent";
import type { PollingHandler } from "./polling";

window.addEventListener("load", () => {
	const diComponent =
		new DiComponent(
			window,
			console,
			new Intl.Locale("en-US"),
		);

	const pollingHandler: PollingHandler = diComponent.getPollingHandler();

	pollingHandler.start();

	document.addEventListener("visibilitychange", () => {
		if (document.visibilityState === "visible") {
			pollingHandler.start();
		} else {
			pollingHandler.stop();
		}
	});
});
