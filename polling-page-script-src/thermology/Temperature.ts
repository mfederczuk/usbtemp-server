/*
 * SPDX-License-Identifier: CC0-1.0
 */

export class Temperature {

	readonly #kelvin: number;

	private constructor(kelvin: number) {
		if (Number.isNaN(kelvin)) {
			throw new Error("Temperature value must not be NaN");
		}
		if (kelvin < 0) {
			throw new Error(`Temperature (${kelvin}K) must not be below absolute zero (0K)`);
		}

		this.#kelvin = kelvin;

		Object.freeze(this);
	}

	public static ofDegreeCelsius(degreeCelsius: number): Temperature {
		if (Number.isNaN(degreeCelsius)) {
			throw new Error("Temperature value must not be NaN");
		}
		if (degreeCelsius < -273.15) {
			throw new Error(`Temperature (${degreeCelsius}°C) must not be below absolute zero (-273.15°C)`);
		}

		return new Temperature(degreeCelsius + 273.15);
	}

	public toDegreeCelsius(): number {
		return (this.#kelvin - 273.15);
	}
}
