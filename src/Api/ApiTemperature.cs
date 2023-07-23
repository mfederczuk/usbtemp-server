/*
 * SPDX-License-Identifier: CC0-1.0
 */

using System.Text.Json.Serialization;

namespace UsbtempServer.Api;

public record ApiTemperature(
	[field: JsonPropertyName("degreeCelsius")] float DegreeCelsius
);
