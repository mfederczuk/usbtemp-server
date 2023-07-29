/*
 * SPDX-License-Identifier: CC0-1.0
 */

using System;
using System.Diagnostics.Contracts;

namespace UsbtempServer.Thermology;

public class VirtualThermometer : IThermometer
{
	private readonly Random rng = new Random();
	private readonly SerialPortName portName;
	private readonly IThermometer.SerialNumber serialNumber;

	[Pure]
	public VirtualThermometer(SerialPortName portName, IThermometer.SerialNumber serialNumber)
	{
		if (!(portName.IsVirtual()))
		{
			throw new ArgumentException(
				message: "The port name for a virtual thermometer must be virtual",
				paramName: nameof(portName)
			);
		}

		this.portName = portName;
		this.serialNumber = serialNumber;
	}

	[Pure]
	public SerialPortName GetPortName()
	{
		return this.portName;
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
