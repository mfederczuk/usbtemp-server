/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

import { ClosedRange } from "../utils/ClosedRange";
import { Duration } from "../utils/Duration";
import type { Logger } from "../utils/logging/Logger";
import { PageConfiguration } from "./PageConfiguration";

export class PageConfigurationCreator {

	static readonly #POLLING_INTERVAL_QUERY_PARAM_NAME: string = "updateInterval";
	static readonly #POLLING_INTERVAL_DEFAULT_VALUE: Duration = Duration.fromSeconds(5);

	static readonly #DECIMAL_DIGITS_MIN_QUERY_PARAM_NAME: string = "minDecimalDigits";
	static readonly #DECIMAL_DIGITS_MAX_QUERY_PARAM_NAME: string = "maxDecimalDigits";
	static readonly #DECIMAL_DIGITS_RANGE_DEFAULT_VALUE: ClosedRange = new ClosedRange(1, 1);

	readonly #logger: Logger;

	public constructor(logger: Logger) {
		this.#logger = logger;

		Object.seal(this);
	}

	public createFromUrlQueryParams(queryParams: URLSearchParams): PageConfiguration {
		const pollingInterval: Duration = this.#extractPollingIntervalFromUrlQueryParams(queryParams);
		const decimalDigitsRange: ClosedRange = this.#extractDecimalDigitsRangeFromUrlQueryParams(queryParams);

		return new PageConfiguration(
			pollingInterval,
			decimalDigitsRange,
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

	#extractDecimalDigitsRangeFromUrlQueryParams(queryParams: URLSearchParams): ClosedRange {
		const minDecimalDigits: number = this.#extractMinDecimalDigitsFromUrlQueryParams(queryParams);
		const maxDecimalDigits: number = this.#extractMaxDecimalDigitsFromUrlQueryParams(queryParams);

		if (minDecimalDigits > maxDecimalDigits) {
			const msg: string = `Configured minimum decimal digits (${minDecimalDigits})` +
				` is greater than the configured maximum value (${maxDecimalDigits}).` +
				" Ignoring it and using the default values" +
				` (minimum ${PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.min} and` +
				` maximum ${PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.max}) instead`;
			this.#logger.logWarning(msg);

			return PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE;
		}

		return new ClosedRange(minDecimalDigits, maxDecimalDigits);
	}

	#extractMinDecimalDigitsFromUrlQueryParams(queryParams: URLSearchParams): number {
		const minDecimalDigitsStr: string | null =
			queryParams.get(PageConfigurationCreator.#DECIMAL_DIGITS_MIN_QUERY_PARAM_NAME);

		if (minDecimalDigitsStr === null) {
			return PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.min;
		}

		const minDecimalDigits: number = Number.parseFloat(minDecimalDigitsStr);

		if (Number.isNaN(minDecimalDigits)) {
			const msg: string = `Configured minimum decimal digits ("${minDecimalDigitsStr}") is an invalid number.` +
				" Ignoring it and using" +
				` the default value (${PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.min}) instead`;
			this.#logger.logWarning(msg);

			return PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.min;
		}

		if (minDecimalDigits < 0) {
			const msg: string = `Configured minimum decimal digits (${minDecimalDigits}) is negative.` +
				" Ignoring it and using" +
				` the default of ${PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.min} instead`;
			this.#logger.logWarning(msg);

			return PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.min;
		}

		if (minDecimalDigits > PageConfiguration.MAX_DECIMAL_DIGITS) {
			const msg: string = `Configured minimum decimal digits (${minDecimalDigits})` +
				` is greater than the maximum of ${PageConfiguration.MAX_DECIMAL_DIGITS}.` +
				" Ignoring it and using the maximum instead";
			this.#logger.logWarning(msg);

			return PageConfiguration.MAX_DECIMAL_DIGITS;
		}

		return minDecimalDigits;
	}

	#extractMaxDecimalDigitsFromUrlQueryParams(queryParams: URLSearchParams): number {
		const maxDecimalDigitsStr: string | null =
			queryParams.get(PageConfigurationCreator.#DECIMAL_DIGITS_MAX_QUERY_PARAM_NAME);

		if (maxDecimalDigitsStr === null) {
			return PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.max;
		}

		const maxDecimalDigits: number = Number.parseFloat(maxDecimalDigitsStr);

		if (Number.isNaN(maxDecimalDigits)) {
			const msg: string = `Configured maximum decimal digits ("${maxDecimalDigitsStr}") is an invalid number.` +
				" Ignoring it and using" +
				` the default value (${PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.max}) instead`;
			this.#logger.logWarning(msg);

			return PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.max;
		}

		if (maxDecimalDigits < 0) {
			const msg: string = `Configured maximum decimal digits (${maxDecimalDigits}) is negative.` +
				" Ignoring it and using" +
				` the default of ${PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.max} instead`;
			this.#logger.logWarning(msg);

			return PageConfigurationCreator.#DECIMAL_DIGITS_RANGE_DEFAULT_VALUE.max;
		}

		if (maxDecimalDigits > PageConfiguration.MAX_DECIMAL_DIGITS) {
			const msg: string = `Configured maximum decimal digits (${maxDecimalDigits})` +
				` is greater than the maximum of ${PageConfiguration.MAX_DECIMAL_DIGITS}.` +
				" Ignoring it and using the maximum instead";
			this.#logger.logWarning(msg);

			return PageConfiguration.MAX_DECIMAL_DIGITS;
		}

		return maxDecimalDigits;
	}
}

Object.freeze(PageConfigurationCreator);
