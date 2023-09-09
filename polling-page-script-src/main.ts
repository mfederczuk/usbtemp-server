/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import { DiComponent } from "./DiComponent";
import type { Page } from "./ui/Page";

const setUpPage = (): void => {
	const diComponent =
		new DiComponent(
			window,
			console,
			new Intl.Locale("en-US"),
		);

	const page: Page = diComponent.getPage();

	page.onLoad(window.document);

	if (window.document.visibilityState === "visible") {
		page.onVisible();
	}

	window.document.addEventListener("visibilitychange", () => {
		if (document.visibilityState === "visible") {
			page.onVisible();
		} else {
			page.onHidden();
		}
	});
};

if ((window.document.readyState === "interactive") || (window.document.readyState === "complete")) {
	setUpPage();
} else {
	let isPageSetUp: boolean = false;

	const ensurePageIsSetUp = (): void => {
		if (isPageSetUp) {
			return;
		}

		setUpPage();
		isPageSetUp = true;
	};

	const handler = (): void => {
		if ((window.document.readyState !== "interactive") && (window.document.readyState !== "complete")) {
			return;
		}

		ensurePageIsSetUp();

		window.document.removeEventListener("readystatechange", handler);
	};

	window.document.addEventListener("readystatechange", handler);
}
