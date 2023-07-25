/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;

namespace UsbtempServer;

public readonly struct Temperature
{
	private readonly float kelvinFloat;

	private Temperature(float kelvinFloat)
	{
		if (kelvinFloat < 0)
		{
			throw new ArgumentException(
				message: "Temperature must not be below absolute zero (0K)",
				paramName: "kelvinFloat"
			);
		}

		this.kelvinFloat = kelvinFloat;
	}

	public static Temperature OfDegreeCelsiusFloat(float degreeCelsius)
	{
		if (degreeCelsius < -273.15f)
		{
			throw new ArgumentException(
				message: "Temperature must not be below absolute zero (-273.15°C)",
				paramName: "degreeCelsius"
			);
		}

		return new Temperature(degreeCelsius + 273.15f);
	}

	public float ToDegreeCelsiusFloat()
	{
		return (this.kelvinFloat - 273.15f);
	}

	public override string ToString()
	{
		return $"{this.kelvinFloat}K";
	}

	public override bool Equals(object? obj)
	{
		return ((obj is not null) &&
		        (this.GetType() == obj.GetType()) &&
		        (this.kelvinFloat == ((Temperature)obj).kelvinFloat));
	}

	public override int GetHashCode()
	{
		return this.kelvinFloat.GetHashCode();
	}
}
