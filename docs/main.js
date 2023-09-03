/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

const determineOperatingSystemFromUserAgent = () => {
	const userAgent = window.navigator.userAgent;

	if (/linux/gi.test(userAgent)) {
		return "linux";
	}

	if (/windows/gi.test(userAgent)) {
		return "windows";
	}

	return "unknown";
};

/**
 * @param {string} className
 */
const openElementsWithClassName = (className) => {
	for (const element of document.getElementsByClassName(className)) {
		element.open = true;
	}
};

/**
 * @param {Element} element
 */
const moveElementToFirstChild = (element) => {
	const parent = element.parentElement;

	if (parent === null) {
		return;
	}

	const firstParentsChild = parent.firstElementChild;

	if ((firstParentsChild === null) || (firstParentsChild === element)) {
		return;
	}

	parent.insertBefore(element, firstParentsChild);
};

/**
 * @param {HTMLHeadingElement} headingElement
 */
const makeHeadingElementClickable = (headingElement) => {
	if (headingElement.id === "") {
		return;
	}

	headingElement.addEventListener("click", () => {
		window.location.hash = headingElement.id;
	});

	headingElement.classList.add("clickable-heading");
};

window.addEventListener("load", () => {
	const osName = determineOperatingSystemFromUserAgent();
	const osClassName = `os-details-type-${osName}`;

	openElementsWithClassName(osClassName);

	for (const element of document.getElementsByClassName(osClassName)) {
		moveElementToFirstChild(element);
	}

	for (const headingElement of document.getElementsByTagName("h2")) {
		makeHeadingElementClickable(headingElement);
	}
	for (const headingElement of document.getElementsByTagName("h3")) {
		makeHeadingElementClickable(headingElement);
	}
	for (const headingElement of document.getElementsByTagName("h4")) {
		makeHeadingElementClickable(headingElement);
	}
});
