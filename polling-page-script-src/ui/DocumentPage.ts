/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import type { Temperature } from "../thermology/Temperature";
import type { TemperatureFormatter } from "../thermology/TemperatureFormatter";
import type { Page } from "./Page";

export class DocumentPage implements Page {

	static readonly #TEMPERATURE_TEXT_ELEMENT_ID: string = "temperature-text";

	readonly #temperatureTextElement: HTMLElement;
	readonly #temperatureFormatter: TemperatureFormatter;

	public constructor(
		document: Document,
		temperatureFormatter: TemperatureFormatter,
	) {
		const temperatureTextElement: HTMLElement | null =
			document.getElementById(DocumentPage.#TEMPERATURE_TEXT_ELEMENT_ID);

		if (temperatureTextElement === null) {
			throw new Error(`Could not find the HTML element with ID "${DocumentPage.#TEMPERATURE_TEXT_ELEMENT_ID}"`);
		}

		this.#temperatureTextElement = temperatureTextElement;
		this.#temperatureFormatter = temperatureFormatter;

		Object.seal(this);
	}

	public setTemperature(temperature: Temperature): void {
		this.#temperatureTextElement.innerHTML = this.#temperatureFormatter.format(temperature);
	}
}

Object.freeze(DocumentPage);
