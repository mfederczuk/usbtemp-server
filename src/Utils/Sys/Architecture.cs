/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace UsbtempServer.Utils.Sys;

public readonly struct Architecture
{
	private readonly string id;
	private readonly string displayName;

	[Pure]
	private Architecture(string id, string displayName)
	{
		this.id = id;
		this.displayName = displayName;
	}

	public static Architecture x86_64 = new Architecture("x86_64", "x86_64");
	public static Architecture i386 = new Architecture("i386", "i386");
	public static Architecture Arm = new Architecture("arm", "ARM");
	public static Architecture AArch64 = new Architecture("aarch64", "AArch64");
	public static Architecture PowerPc = new Architecture("powerpc", "PowerPC");
	public static Architecture Unknown = new Architecture("unknown", "Unknown");

	public static Architecture GetCurrent()
	{
		switch (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture)
		{
			case System.Runtime.InteropServices.Architecture.X64: return x86_64;
			case System.Runtime.InteropServices.Architecture.X86: return i386;
			case System.Runtime.InteropServices.Architecture.Arm:
			case System.Runtime.InteropServices.Architecture.Armv6: return Arm;
			case System.Runtime.InteropServices.Architecture.Arm64: return AArch64;
			case System.Runtime.InteropServices.Architecture.Ppc64le: return PowerPc;
			default: return Unknown;
		}
	}

	[Pure]
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return ((obj is not null) &&
		        (this.GetType() == obj.GetType()) &&
		        (this.id == ((Architecture)obj).id));
	}

	[Pure] public static bool operator ==(Architecture lhs, Architecture rhs) => lhs.Equals(rhs);
	[Pure] public static bool operator !=(Architecture lhs, Architecture rhs) => !(lhs.Equals(rhs));

	[Pure]
	public override int GetHashCode()
	{
		return this.id.GetHashCode();
	}

	[Pure]
	public override string ToString()
	{
		return this.displayName;
	}
}
