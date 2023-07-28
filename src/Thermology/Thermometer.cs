/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;

namespace UsbtempServer.Thermology;

public class Thermometer : IThermometer, IDisposable
{
	private usbtemp.Thermometer? internalThermometer;
	private readonly Lazy<IThermometer.SerialNumber> lazySerialNumber;

	private Thermometer(usbtemp.Thermometer internalThermometer)
	{
		this.internalThermometer = internalThermometer;
		this.lazySerialNumber = new Lazy<IThermometer.SerialNumber>(valueFactory: this.readSerialNumber);
	}

	public static Thermometer OpenNew(SerialPortName portName)
	{
		usbtemp.Thermometer internalThermometer = new();
		internalThermometer.Open(portName.ToString());

		return new Thermometer(internalThermometer);
	}

	public IThermometer.SerialNumber GetSerialNumber()
	{
		return this.lazySerialNumber.Value;
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

	private IThermometer.SerialNumber readSerialNumber()
	{
		usbtemp.Thermometer internalThermometer = this.ensureNotDisposed();

		byte[] rawSerialNumber = internalThermometer.Rom();
		ulong serialNumberUInt64 = uInt64FromBytes(rawSerialNumber);
		return IThermometer.SerialNumber.OfUInt64(serialNumberUInt64);
	}

	private usbtemp.Thermometer ensureNotDisposed()
	{
		if (this.internalThermometer is null)
		{
			throw new InvalidOperationException(message: "Thermometer resources is already closed");
		}

		return this.internalThermometer;
	}

	private static ulong uInt64FromBytes(byte[] bytes)
	{
		if (bytes.Length != 8)
		{
			throw new ArgumentException(
				message: "Byte array must have a size of exactly 8",
				paramName: nameof(bytes)
			);
		}

		return (ulong)(bytes[0]) << (8 * 7) |
		       (ulong)(bytes[1]) << (8 * 6) |
		       (ulong)(bytes[2]) << (8 * 5) |
		       (ulong)(bytes[3]) << (8 * 4) |
		       (ulong)(bytes[4]) << (8 * 3) |
		       (ulong)(bytes[5]) << (8 * 2) |
		       (ulong)(bytes[6]) << (8 * 1) |
		       (ulong)(bytes[7]) << (8 * 0);
	}
}
