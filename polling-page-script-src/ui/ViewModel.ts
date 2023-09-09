/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { TemperatureRepository } from "../data/TemperatureRepository";
import type { Temperature } from "../thermology/Temperature";

export class ViewModel {

	readonly #temperatureRepository: TemperatureRepository;

	public constructor(temperatureRepository: TemperatureRepository) {
		this.#temperatureRepository = temperatureRepository;

		Object.seal(this);
	}

	public setTemperatureListener(listener: (temperature: Temperature) => void): void {
		this.#temperatureRepository.setTemperatureListener(listener);
	}

	public clearTemperatureListener(): void {
		this.#temperatureRepository.clearTemperatureListener();
	}
}

Object.freeze(ViewModel);
