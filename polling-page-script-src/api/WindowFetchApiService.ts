/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import type { ApiService } from "./ApiService";

export class WindowFetchApiService implements ApiService {

	readonly #window: Window;

	public constructor(window: Window) {
		this.#window = window;

		Object.seal(this);
	}

	public async getTemperatureInDegreeCelsius(): Promise<number> {
		const response: Response = await this.#window.fetch("/temperature");

		const responseBodyJson: unknown = await response.json();

		if ((typeof responseBodyJson !== "object") || (responseBodyJson === null)) {
			throw new Error("Response body is not JSON object");
		}

		const degreeCelsius: unknown = (responseBodyJson as Record<string, unknown>)["degreeCelsius"];

		if (typeof degreeCelsius !== "number") {
			throw new Error("JSON property \"degreeCelsius\" of the response body object is not a number");
		}

		return degreeCelsius;
	}
}

Object.freeze(WindowFetchApiService);
