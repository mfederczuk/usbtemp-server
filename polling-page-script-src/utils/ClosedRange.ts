/*
 * SPDX-License-Identifier: CC0-1.0
 */

export class ClosedRange {

	public readonly min: number;
	public readonly max: number;

	public constructor(min: number, max: number) {
		if (Number.isNaN(min)) {
			throw new Error("The minimum value of a range must not be NaN");
		}
		if (Number.isNaN(max)) {
			throw new Error("The maximum value of a range must not be NaN");
		}
		if (min > max) {
			throw new Error(`The minimum value (${min}) of a range must not be greater than the maximum value ${max}`);
		}

		this.min = min;
		this.max = max;

		Object.freeze(this);
	}
}

Object.freeze(ClosedRange);
