/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import type { Duration } from "./Duration";
import type { ActionScheduler, ScheduledActionHandle } from "./actionScheduling/ActionScheduler";

export class Poller {

	readonly #actionScheduler: ActionScheduler;
	readonly #interval: Duration;
	readonly #action: () => (void | PromiseLike<void>);

	#scheduledActionHandle: ScheduledActionHandle | null = null;

	public constructor(actionScheduler: ActionScheduler, interval: Duration, action: () => (void | PromiseLike<void>)) {
		this.#actionScheduler = actionScheduler;
		this.#interval = interval;
		this.#action = action;

		Object.seal(this);
	}

	public start(): void {
		if (this.#scheduledActionHandle !== null) {
			throw new Error("Polling process is already active");
		}

		this.#doExecutionCycle();
	}

	public stop(): void {
		if (this.#scheduledActionHandle === null) {
			throw new Error("Polling process is not active");
		}

		this.#scheduledActionHandle.cancel();
		this.#scheduledActionHandle = null;
	}

	#doExecutionCycle(): void {
		// errors are ignored
		void this.#executeAction()
			.then(() => {
				this.#scheduledActionHandle =
					this.#actionScheduler.scheduleAction(this.#interval, () => {
						this.#doExecutionCycle();
					});
			});
	}

	async #executeAction(): Promise<void> {
		return this.#action();
	}
}
