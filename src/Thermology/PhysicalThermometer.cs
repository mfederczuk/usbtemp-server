/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Diagnostics.Contracts;
using System.IO.Ports;

namespace UsbtempServer.Thermology;

public class PhysicalThermometer : IThermometer, IDisposable
{
	private const int BAUD_RATE = 115_200;
	private const int DATA_BITS = 6;
	private const int READ_WRITE_TIMEOUT = 500;

	private usbtemp.Thermometer? internalThermometer;
	private SerialPort? serialPort;
	private readonly Lazy<IThermometer.SerialNumber> lazySerialNumber;

	[Pure]
	private PhysicalThermometer(SerialPort openedSerialPort)
	{
		this.serialPort = openedSerialPort;
		this.lazySerialNumber = new Lazy<IThermometer.SerialNumber>(valueFactory: this.readSerialNumber);
	}

	public static PhysicalThermometer OpenNew(SerialPortName portName)
	{
		if (portName.IsVirtual())
		{
			throw new ArgumentException(
				message: "The port name for a physical thermometer must not be virtual",
				paramName: nameof(portName)
			);
		}

		SerialPort serialPort = new(
			portName: portName.ToString(),
			baudRate: BAUD_RATE,
			parity: Parity.None,
			dataBits: DATA_BITS,
			stopBits: StopBits.One
		)
		{
			Handshake = Handshake.None,
			RtsEnable = false,
			ReadTimeout = READ_WRITE_TIMEOUT,
			WriteTimeout = READ_WRITE_TIMEOUT,
		};

		serialPort.Open();

		return new PhysicalThermometer(openedSerialPort: serialPort);
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
		if (this.serialPort is null)
		{
			return;
		}

		this.serialPort.Close();
	}

	private IThermometer.SerialNumber readSerialNumber()
	{
		SerialPort serialPort = this.ensureNotDisposedNew();

		if (OneWire.WriteReset(serialPort) != OneWire.ResultStatus.Success)
		{
		}

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

	private SerialPort ensureNotDisposedNew()
	{
		if (this.serialPort is null)
		{
			throw new InvalidOperationException(message: "Thermometer resources is already closed");
		}

		return this.serialPort;
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
