/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { Temperature } from "../thermology/Temperature";

export interface Page {

	setTemperature(temperature: Temperature): void;
}
