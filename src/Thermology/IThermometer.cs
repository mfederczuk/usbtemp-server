/*
 * SPDX-License-Identifier: CC0-1.0
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace UsbtempServer.Thermology;

public interface IThermometer : IDisposable
{
	public readonly struct SerialNumber
	{
		private readonly ulong value;

		[Pure]
		private SerialNumber(ulong value)
		{
			this.value = value;
		}

		[Pure]
		public static SerialNumber OfUInt64(ulong value)
		{
			return new SerialNumber(value);
		}

		public static SerialNumber CreateRandom(Random rng)
		{
			return SerialNumber.OfUInt64((ulong)(rng.NextInt64()));
		}

		[Pure]
		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			return ((obj is not null) &&
			        (this.GetType() == obj.GetType()) &&
			        (this.value == ((SerialNumber)obj).value));
		}

		[Pure]
		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}

		[Pure]
		public override string ToString()
		{
			return this.value.ToString("X");
		}
	}

	public SerialNumber ReadSerialNumber();
	public Temperature ReadTemperature();
}
