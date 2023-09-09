/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { Temperature } from "../utils/thermology/Temperature";

export interface ApiService {

	getTemperature(): PromiseLike<Temperature>;
}
