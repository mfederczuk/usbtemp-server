/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { TemperatureFormatter } from "./TemperatureFormatter";

export class IntlTemperatureFormatter implements TemperatureFormatter {

	readonly #numberFormat: Intl.NumberFormat;

	public constructor(locale: Intl.Locale) {
		this.#numberFormat =
			new Intl.NumberFormat(
				locale.toString(),
				{
					style: "unit",
					unit: "celsius",
					minimumFractionDigits: 0,
					maximumFractionDigits: 0,
				},
			);

		Object.seal(this);
	}

	public format(degreeCelsius: number): string {
		return this.#numberFormat.format(degreeCelsius);
	}
}

Object.freeze(IntlTemperatureFormatter);
