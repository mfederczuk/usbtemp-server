/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

export class Duration {

	readonly #milliseconds: number;

	private constructor(milliseconds: number) {
		if (Number.isNaN(milliseconds)) {
			throw new Error("Duration must not be NaN");
		}
		if (milliseconds < 0) {
			throw new Error("Duration must not be negative");
		}

		this.#milliseconds = milliseconds;

		Object.freeze(this);
	}

	// #region factory functions

	public static fromMilliseconds(n: number): Duration {
		return new Duration(n);
	}

	public static fromSeconds(n: number): Duration {
		return Duration.fromMilliseconds(n * 1000);
	}

	// #endregion

	// #region comparison functions

	public compareTo(other: Duration): number {
		return (this.#milliseconds - other.#milliseconds);
	}

	public isLessThan(other: Duration): boolean {
		return (this.compareTo(other) < 0);
	}

	public isGreaterThan(other: Duration): boolean {
		return (this.compareTo(other) > 0);
	}

	// #endregion

	public toMilliseconds(): number {
		return this.#milliseconds;
	}

	public toString(): string {
		return `${this.#milliseconds}ms`;
	}
}

Object.freeze(Duration);
