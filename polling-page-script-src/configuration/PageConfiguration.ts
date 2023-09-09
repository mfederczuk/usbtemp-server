/*
 * SPDX-License-Identifier: CC0-1.0
 */

import { Duration } from "../utils/Duration";

export class PageConfiguration {

	public static readonly MIN_POLLING_INTERVAL: Duration = Duration.fromMilliseconds(500);
	public static readonly MAX_POLLING_INTERVAL: Duration = Duration.fromMilliseconds(999_999_999);

	public readonly pollingInterval: Duration;

	public constructor(pollingInterval: Duration) {
		if (pollingInterval.isLessThan(PageConfiguration.MIN_POLLING_INTERVAL)) {
			const msg: string = `Polling interval (${pollingInterval.toString()})` +
				` must not be less than ${PageConfiguration.MIN_POLLING_INTERVAL.toString()}`;
			throw new Error(msg);
		}
		if (pollingInterval.isGreaterThan(PageConfiguration.MAX_POLLING_INTERVAL)) {
			const msg: string = `Polling interval (${pollingInterval.toString()})` +
				` must not be greater than ${PageConfiguration.MAX_POLLING_INTERVAL.toString()}`;
			throw new Error(msg);
		}

		this.pollingInterval = pollingInterval;

		Object.freeze(this);
	}
}

Object.freeze(PageConfiguration);
