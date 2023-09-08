/*
 * SPDX-License-Identifier: CC0-1.0
 */

export class Lazy<T> {

	readonly #valueSupplier: () => T;
	#valueWrapper: ({ readonly initializedValue: T; } | null) = null;

	public constructor(valueSupplier: () => T) {
		this.#valueSupplier = valueSupplier;

		Object.seal(this);
	}

	public get(): T {
		if (this.#valueWrapper === null) {
			const value: T = this.#valueSupplier();
			this.#valueWrapper = { initializedValue: value };
		}

		return this.#valueWrapper.initializedValue;
	}
}

Object.freeze(Lazy);
