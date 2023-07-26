/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;

namespace UsbtempServer.Thermology;

public class Thermometer : IDisposable
{
	private usbtemp.Thermometer? internalThermometer;

	private Thermometer(usbtemp.Thermometer internalThermometer)
	{
		this.internalThermometer = internalThermometer;
	}

	public static Thermometer OpenNew(string portName)
	{
		usbtemp.Thermometer internalThermometer = new();
		internalThermometer.Open(portName);

		return new Thermometer(internalThermometer);
	}

	public string ReadSerialNumber()
	{
		usbtemp.Thermometer internalThermometer = this.ensureNotDisposed();

		byte[] rawSerialNumber = internalThermometer.Rom();

		return BitConverter
			.ToString(rawSerialNumber)
			.Replace("-", string.Empty);
	}

	public Temperature ReadTemperature()
	{
		usbtemp.Thermometer internalThermometer = this.ensureNotDisposed();

		return Temperature.OfDegreeCelsiusFloat(internalThermometer.Temperature());
	}

	void IDisposable.Dispose()
	{
		if (this.internalThermometer is null)
		{
			return;
		}

		this.internalThermometer.Close();
	}

	private usbtemp.Thermometer ensureNotDisposed()
	{
		if (this.internalThermometer is null)
		{
			throw new InvalidOperationException(message: "Thermometer resources is already closed");
		}

		return this.internalThermometer;
	}
}
