/*
 * SPDX-License-Identifier: CC0-1.0
 */

using Microsoft.AspNetCore.Mvc;
using UsbtempServer.Thermology;

namespace UsbtempServer.Api;

[ApiController]
[Route("temperature")]
public class TemperatureController : ControllerBase
{
	private readonly IThermometer thermometer;

	public TemperatureController(IThermometer thermometer)
	{
		this.thermometer = thermometer;
	}

	[HttpGet]
	public ApiTemperature Get()
	{
		Temperature temperature = this.thermometer.ReadTemperature();

		return new ApiTemperature(
			DegreeCelsius: temperature.ToDegreeCelsiusFloat()
		);
	}
}
