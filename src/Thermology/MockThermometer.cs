/*
 * SPDX-License-Identifier: CC0-1.0
 */

using System;
using System.Diagnostics.Contracts;

namespace UsbtempServer.Thermology;

public class MockThermometer : IThermometer
{
	private readonly Random rng = new Random();
	private readonly IThermometer.SerialNumber serialNumber;

	public MockThermometer()
	{
		this.serialNumber = IThermometer.SerialNumber.CreateRandom(this.rng);
	}

	[Pure]
	public IThermometer.SerialNumber GetSerialNumber()
	{
		return this.serialNumber;
	}

	public Temperature ReadTemperature()
	{
		float randomDegreeCelsiusFloat = ((this.rng.NextSingle() * 13) + 19); // range of [19, 32)
		return Temperature.OfDegreeCelsiusFloat(randomDegreeCelsiusFloat);
	}

	[Pure]
	public void Dispose()
	{
		// nothing
	}
}
