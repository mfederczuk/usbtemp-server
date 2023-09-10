/*
 * SPDX-License-Identifier: CC0-1.0
 */

import type { Duration } from "../Duration";
import type { Action } from "./Action";

export interface ScheduledActionHandle {

	cancel(): void;
}

export interface ActionScheduler {

	scheduleAction(delay: Duration, action: Action): ScheduledActionHandle;
}
