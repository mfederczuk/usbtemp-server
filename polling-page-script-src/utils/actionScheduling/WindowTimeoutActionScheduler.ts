/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import type { Duration } from "../Duration";
import type { Action } from "./Action";
import type { ActionScheduler, ScheduledActionHandle } from "./ActionScheduler";

export class WindowTimeoutActionScheduler implements ActionScheduler {

	readonly #window: Window;

	public constructor(window: Window) {
		this.#window = window;

		Object.seal(this);
	}

	public scheduleAction(delay: Duration, action: Action): ScheduledActionHandle {
		const timeoutId: number = this.#window
			.setTimeout(
				() => {
					action();
				},
				delay.toMilliseconds(),
			);

		return new WindowTimeoutScheduledActionHandle(() => {
			this.#window.clearTimeout(timeoutId);
		});
	}
}

class WindowTimeoutScheduledActionHandle implements ScheduledActionHandle {

	#clearFunction: ((() => void) | null);

	public constructor(clearFunction: () => void) {
		this.#clearFunction = clearFunction;

		Object.seal(this);
	}

	public cancel(): void {
		if (this.#clearFunction === null) {
			return;
		}

		this.#clearFunction();
		this.#clearFunction = null;
	}
}

Object.freeze(WindowTimeoutActionScheduler);
Object.freeze(WindowTimeoutScheduledActionHandle);
