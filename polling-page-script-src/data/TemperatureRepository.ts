/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { Temperature } from "../utils/thermology/Temperature";

export interface TemperatureRepository {

	setTemperatureListener(listener: (temperature: Temperature) => void): void;
	clearTemperatureListener(): void;
}
