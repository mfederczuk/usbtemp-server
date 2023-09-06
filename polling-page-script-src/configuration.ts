/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

export class PageConfiguration {

	static readonly #SEARCH_PARAM_NAME_POLLING_INTERVAL = "updateInterval";

	static readonly #DEFAULT_POLLING_INTERVAL_MS: number = 5000;
	public static readonly MIN_POLLING_INTERVAL_MS: number = 500;
	public static readonly MAX_POLLING_INTERVAL_MS: number = 999_999_999;

	public readonly pollingIntervalMs: number;

	public constructor(pollingIntervalMs: number) {
		if (Number.isNaN(pollingIntervalMs)) {
			throw new Error("Polling interval must not be NaN");
		}
		if (pollingIntervalMs < PageConfiguration.MIN_POLLING_INTERVAL_MS) {
			const msg: string = `Polling interval (${pollingIntervalMs})` +
				` must not be less than ${PageConfiguration.MIN_POLLING_INTERVAL_MS}`;
			throw new Error(msg);
		}
		if (pollingIntervalMs > PageConfiguration.MAX_POLLING_INTERVAL_MS) {
			const msg: string = `Polling interval (${pollingIntervalMs})` +
				` must not be greater than ${PageConfiguration.MAX_POLLING_INTERVAL_MS}`;
			throw new Error(msg);
		}

		this.pollingIntervalMs = pollingIntervalMs;

		Object.freeze(this);
	}

	public static fromUrlSearchParams(searchParams: URLSearchParams, loggingConsole: Console): PageConfiguration {
		const pollingIntervalMs: number =
			PageConfiguration.#extractPollingIntervalMsFromUrlSearchParams(searchParams, loggingConsole);

		return new PageConfiguration(
			pollingIntervalMs,
		);
	}

	static #extractPollingIntervalMsFromUrlSearchParams(
		searchParams: URLSearchParams,
		loggingConsole: Console,
	): number {
		const pollingIntervalStr: string =
			(searchParams.get(PageConfiguration.#SEARCH_PARAM_NAME_POLLING_INTERVAL) ?? "");
		const pollingIntervalMs: number = Number.parseFloat(pollingIntervalStr);

		if (Number.isNaN(pollingIntervalMs)) {
			const msg: string = `Configured polling interval ("${pollingIntervalStr}") is an invalid number.` +
				` Ignoring it and using the default value (${PageConfiguration.#DEFAULT_POLLING_INTERVAL_MS}ms) instead`;
			loggingConsole.warn(msg);

			return PageConfiguration.#DEFAULT_POLLING_INTERVAL_MS;
		}

		if (pollingIntervalMs < PageConfiguration.MIN_POLLING_INTERVAL_MS) {
			const msg: string = `Configured polling interval (${pollingIntervalMs}ms)` +
				` is less than the minimum of ${PageConfiguration.MIN_POLLING_INTERVAL_MS}ms.` +
				" Ignoring it and using the minimum instead";
			loggingConsole.warn(msg);

			return PageConfiguration.MIN_POLLING_INTERVAL_MS;
		}

		if (pollingIntervalMs > PageConfiguration.MAX_POLLING_INTERVAL_MS) {
			const msg: string = `Configured polling interval (${pollingIntervalMs}ms)` +
				` is greater than the maximum of ${PageConfiguration.MAX_POLLING_INTERVAL_MS}ms.` +
				" Ignoring it and using the maximum instead";
			loggingConsole.warn(msg);

			return PageConfiguration.MAX_POLLING_INTERVAL_MS;
		}

		return pollingIntervalMs;
	}
}

Object.freeze(PageConfiguration);
