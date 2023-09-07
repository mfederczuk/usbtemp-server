/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import { Duration } from "./utils/Duration";
import type { Logger } from "./utils/logging/Logger";

export class PageConfiguration {

	static readonly #SEARCH_PARAM_NAME_POLLING_INTERVAL: string = "updateInterval";

	static readonly #DEFAULT_POLLING_INTERVAL: Duration = Duration.fromSeconds(5);
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

	public static fromUrlSearchParams(searchParams: URLSearchParams, logger: Logger): PageConfiguration {
		const pollingInterval: Duration =
			PageConfiguration.#extractPollingIntervalFromUrlSearchParams(searchParams, logger);

		return new PageConfiguration(
			pollingInterval,
		);
	}

	static #extractPollingIntervalFromUrlSearchParams(searchParams: URLSearchParams, logger: Logger): Duration {
		const pollingIntervalStr: string =
			(searchParams.get(PageConfiguration.#SEARCH_PARAM_NAME_POLLING_INTERVAL) ?? "");
		const pollingIntervalMs: number = Number.parseFloat(pollingIntervalStr);

		if (Number.isNaN(pollingIntervalMs)) {
			const msg: string = `Configured polling interval ("${pollingIntervalStr}") is an invalid number.` +
				" Ignoring it and using" +
				` the default value (${PageConfiguration.#DEFAULT_POLLING_INTERVAL.toString()}) instead`;
			logger.logWarning(msg);

			return PageConfiguration.#DEFAULT_POLLING_INTERVAL;
		}

		const pollingInterval: Duration = Duration.fromMilliseconds(pollingIntervalMs);

		if (pollingInterval.isLessThan(PageConfiguration.MIN_POLLING_INTERVAL)) {
			const msg: string = `Configured polling interval (${pollingInterval.toString()})` +
				` is less than the minimum of ${PageConfiguration.MIN_POLLING_INTERVAL.toString()}.` +
				" Ignoring it and using the minimum instead";
			logger.logWarning(msg);

			return PageConfiguration.MIN_POLLING_INTERVAL;
		}

		if (pollingInterval.isGreaterThan(PageConfiguration.MAX_POLLING_INTERVAL)) {
			const msg: string = `Configured polling interval (${pollingInterval.toString()})` +
				` is greater than the maximum of ${PageConfiguration.MAX_POLLING_INTERVAL.toString()}.` +
				" Ignoring it and using the maximum instead";
			logger.logWarning(msg);

			return PageConfiguration.MAX_POLLING_INTERVAL;
		}

		return pollingInterval;
	}
}

Object.freeze(PageConfiguration);
