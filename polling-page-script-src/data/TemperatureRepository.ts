/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { Temperature } from "../thermology/Temperature";

export interface TemperatureRepository {

	setTemperatureListener(listener: (temperature: Temperature) => void): void;
	clearTemperatureListener(): void;
}
