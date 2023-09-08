/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import { Temperature } from "../thermology/Temperature";
import type { ApiService } from "./ApiService";

export class WindowFetchApiService implements ApiService {

	readonly #window: Window;

	public constructor(window: Window) {
		this.#window = window;

		Object.seal(this);
	}

	public async getTemperature(): Promise<Temperature> {
		const response: Response = await this.#window.fetch("/temperature");

		const responseBodyJson: unknown = await response.json();

		if ((typeof responseBodyJson !== "object") || (responseBodyJson === null)) {
			throw new Error("Response body is not JSON object");
		}

		const degreeCelsius: unknown = (responseBodyJson as Record<string, unknown>)["degreeCelsius"];

		if (typeof degreeCelsius !== "number") {
			throw new Error("JSON property \"degreeCelsius\" of the response body object is not a number");
		}

		if (degreeCelsius < -273.15) {
			const msg: string = `JSON property "degreeCelsius" (value ${degreeCelsius})` +
				" of the response body is below absolute zero (-273.15°C)";
			throw new Error(msg);
		}

		return Temperature.ofDegreeCelsius(degreeCelsius);
	}
}

Object.freeze(WindowFetchApiService);
