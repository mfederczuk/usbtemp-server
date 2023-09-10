/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { ClosedRange } from "../ClosedRange";
import type { Temperature } from "./Temperature";
import type { TemperatureFormatter } from "./TemperatureFormatter";

export class IntlTemperatureFormatter implements TemperatureFormatter {

	readonly #numberFormat: Intl.NumberFormat;

	public constructor(locale: Intl.Locale, decimalDigitsRange: ClosedRange) {
		this.#numberFormat =
			new Intl.NumberFormat(
				locale.toString(),
				{
					style: "unit",
					unit: "celsius",
					minimumFractionDigits: decimalDigitsRange.min,
					maximumFractionDigits: decimalDigitsRange.max,
				},
			);

		Object.seal(this);
	}

	public format(temperature: Temperature): string {
		return this.#numberFormat.format(temperature.toDegreeCelsius());
	}
}

Object.freeze(IntlTemperatureFormatter);
