/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { Logger } from "./Logger";

export class ConsoleLogger implements Logger {

	readonly #console: Console;

	public constructor(console: Console) {
		this.#console = console;

		Object.seal(this);
	}

	public logWarning(msg: string): void {
		this.#console.warn(msg);
	}

	public logError(msg: string): void {
		this.#console.error(msg);
	}
}

Object.freeze(ConsoleLogger);
