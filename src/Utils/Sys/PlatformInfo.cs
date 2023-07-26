/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;

namespace UsbtempServer.Utils.Sys;

public readonly struct PlatformInfo
{
	private readonly OperatingSystemKind operatingSystemKind;
	private readonly Architecture architecture;

	[Pure]
	private PlatformInfo(
		OperatingSystemKind operatingSystemKind,
		Architecture architecture
	)
	{
		this.operatingSystemKind = operatingSystemKind;
		this.architecture = architecture;
	}

	public static PlatformInfo GetCurrent()
	{
		return new PlatformInfo(
			operatingSystemKind: OperatingSystemKind.GetCurrent(),
			architecture: Architecture.GetCurrent()
		);
	}

	[Pure]
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return ((obj is not null) &&
		        (this.GetType() == obj.GetType()) &&
		        (this.operatingSystemKind == ((PlatformInfo)obj).operatingSystemKind) &&
		        (this.architecture == ((PlatformInfo)obj).architecture));
	}

	[Pure] public static bool operator ==(PlatformInfo lhs, PlatformInfo rhs) => lhs.Equals(rhs);
	[Pure] public static bool operator !=(PlatformInfo lhs, PlatformInfo rhs) => !(lhs.Equals(rhs));

	[Pure]
	public override int GetHashCode()
	{
		return System.HashCode.Combine(this.operatingSystemKind, this.architecture);
	}

	[Pure]
	public override string ToString()
	{
		StringBuilder sb = new(this.operatingSystemKind.ToString());

		if ((this.operatingSystemKind == OperatingSystemKind.Windows) && (this.architecture == Architecture.x86_64))
		{
			// in the windows world, x86_64 is more commonly referred to as just "x64"
			sb.Append(" x64");
		}
		else
		{
			sb.Append(' ');
			sb.Append(this.architecture.ToString());
		}

		return sb.ToString();
	}
}
