/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import type { ClosedRange } from "../utils/ClosedRange";
import { Duration } from "../utils/Duration";

export class PageConfiguration {

	public static readonly MIN_POLLING_INTERVAL: Duration = Duration.fromMilliseconds(500);
	public static readonly MAX_POLLING_INTERVAL: Duration = Duration.fromMilliseconds(999_999_999);

	public static readonly MAX_DECIMAL_DIGITS: number = 20;

	public readonly pollingInterval: Duration;
	public readonly decimalDigitsRange: ClosedRange;

	public constructor(
		pollingInterval: Duration,
		decimalDigitsRange: ClosedRange,
	) {
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

		if (decimalDigitsRange.min < 0) {
			throw new Error(`Minimum decimal digits amount (${decimalDigitsRange.min}) must not be negative`);
		}
		if (decimalDigitsRange.max < 0) {
			throw new Error(`Maximum decimal digits amount (${decimalDigitsRange.max}) must not be negative`);
		}
		if (decimalDigitsRange.max > PageConfiguration.MAX_DECIMAL_DIGITS) {
			const msg: string = `Maximum decimal digits amount (${decimalDigitsRange.max})` +
				` must not be greater than ${PageConfiguration.MAX_DECIMAL_DIGITS}`;
			throw new Error(msg);
		}

		this.pollingInterval = pollingInterval;
		this.decimalDigitsRange = decimalDigitsRange;

		Object.freeze(this);
	}
}

Object.freeze(PageConfiguration);
