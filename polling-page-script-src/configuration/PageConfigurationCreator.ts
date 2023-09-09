/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import { Duration } from "../utils/Duration";
import type { Logger } from "../utils/logging/Logger";
import { PageConfiguration } from "./PageConfiguration";

export class PageConfigurationCreator {

	static readonly #POLLING_INTERVAL_QUERY_PARAM_NAME: string = "updateInterval";
	static readonly #POLLING_INTERVAL_DEFAULT_VALUE: Duration = Duration.fromSeconds(5);

	readonly #logger: Logger;

	public constructor(logger: Logger) {
		this.#logger = logger;

		Object.seal(this);
	}

	public createFromUrlQueryParams(queryParams: URLSearchParams): PageConfiguration {
		const pollingInterval: Duration = this.#extractPollingIntervalFromUrlQueryParams(queryParams);

		return new PageConfiguration(
			pollingInterval,
		);
	}

	#extractPollingIntervalFromUrlQueryParams(queryParams: URLSearchParams): Duration {
		const pollingIntervalStr: string | null =
			queryParams.get(PageConfigurationCreator.#POLLING_INTERVAL_QUERY_PARAM_NAME);

		if (pollingIntervalStr === null) {
			return PageConfigurationCreator.#POLLING_INTERVAL_DEFAULT_VALUE;
		}

		const pollingIntervalMs: number = Number.parseFloat(pollingIntervalStr);

		if (Number.isNaN(pollingIntervalMs)) {
			const msg: string = `Configured polling interval ("${pollingIntervalStr}") is an invalid number.` +
				" Ignoring it and using" +
				` the default value (${PageConfigurationCreator.#POLLING_INTERVAL_DEFAULT_VALUE.toString()}) instead`;
			this.#logger.logWarning(msg);

			return PageConfigurationCreator.#POLLING_INTERVAL_DEFAULT_VALUE;
		}

		const pollingInterval: Duration = Duration.fromMilliseconds(pollingIntervalMs);

		if (pollingInterval.isLessThan(PageConfiguration.MIN_POLLING_INTERVAL)) {
			const msg: string = `Configured polling interval (${pollingInterval.toString()})` +
				` is less than the minimum of ${PageConfiguration.MIN_POLLING_INTERVAL.toString()}.` +
				" Ignoring it and using the minimum instead";
			this.#logger.logWarning(msg);

			return PageConfiguration.MIN_POLLING_INTERVAL;
		}

		if (pollingInterval.isGreaterThan(PageConfiguration.MAX_POLLING_INTERVAL)) {
			const msg: string = `Configured polling interval (${pollingInterval.toString()})` +
				` is greater than the maximum of ${PageConfiguration.MAX_POLLING_INTERVAL.toString()}.` +
				" Ignoring it and using the maximum instead";
			this.#logger.logWarning(msg);

			return PageConfiguration.MAX_POLLING_INTERVAL;
		}

		return pollingInterval;
	}
}

Object.freeze(PageConfigurationCreator);
