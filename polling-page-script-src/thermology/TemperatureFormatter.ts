/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { Temperature } from "./Temperature";

export interface TemperatureFormatter {

	format(temperature: Temperature): string;
}
