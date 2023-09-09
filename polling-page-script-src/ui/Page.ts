/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import type { Temperature } from "../utils/thermology/Temperature";
import type { TemperatureFormatter } from "../utils/thermology/TemperatureFormatter";
import type { ViewModel } from "./ViewModel";

export class Page {

	static readonly #TEMPERATURE_TEXT_ELEMENT_ID: string = "temperature-text";

	readonly #viewModel: ViewModel;
	readonly #temperatureFormatter: TemperatureFormatter;

	#temperatureTextElement: HTMLElement | null = null;

	public constructor(
		viewModel: ViewModel,
		temperatureFormatter: TemperatureFormatter,
	) {
		this.#viewModel = viewModel;
		this.#temperatureFormatter = temperatureFormatter;

		Object.seal(this);
	}

	public onLoad(document: Document): void {
		const temperatureTextElement: HTMLElement | null = document.getElementById(Page.#TEMPERATURE_TEXT_ELEMENT_ID);

		if (temperatureTextElement === null) {
			throw new Error(`Could not find the HTML element with ID "${Page.#TEMPERATURE_TEXT_ELEMENT_ID}"`);
		}

		this.#temperatureTextElement = temperatureTextElement;
	}

	public onVisible(): void {
		this.#viewModel.setTemperatureListener((temperature: Temperature) => {
			this.#setTemperature(temperature);
		});
	}

	public onHidden(): void {
		this.#viewModel.clearTemperatureListener();
	}

	#setTemperature(temperature: Temperature): void {
		if (this.#temperatureTextElement === null) {
			throw new Error("Temperature text element is null");
		}

		this.#temperatureTextElement.innerHTML = this.#temperatureFormatter.format(temperature);
	}
}

Object.freeze(Page);
