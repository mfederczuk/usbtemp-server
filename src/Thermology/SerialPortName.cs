/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace UsbtempServer.Thermology;

public readonly struct SerialPortName
{
	private readonly string value;

	[Pure]
	private SerialPortName(string value)
	{
		if (value == string.Empty)
		{
			throw new ArgumentException(
				message: "Serial port name must not be empty",
				paramName: nameof(value)
			);
		}

		this.value = value;
	}

	[Pure]
	public static SerialPortName OfString(string value)
	{
		return new SerialPortName(value);
	}

	[Pure]
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return ((obj is not null) &&
		        (this.GetType() == obj.GetType()) &&
		        (this.value == ((SerialPortName)obj).value));
	}

	[Pure]
	public override int GetHashCode()
	{
		return this.value.GetHashCode();
	}

	[Pure]
	public override string ToString()
	{
		return this.value;
	}
}
