/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace UsbtempServer.Utils.Sys;

public readonly struct OperatingSystemKind
{
	private readonly string id;
	private readonly string displayName;

	[Pure]
	private OperatingSystemKind(string id, string displayName)
	{
		this.id = id;
		this.displayName = displayName;
	}

	public static OperatingSystemKind LinuxBased = new OperatingSystemKind("linux_based", "Linux");
	public static OperatingSystemKind Windows = new OperatingSystemKind("windows", "Windows");
	public static OperatingSystemKind MacOs = new OperatingSystemKind("macos", "OSX");
	public static OperatingSystemKind FreeBsd = new OperatingSystemKind("freebsd", "FreeBSD");
	public static OperatingSystemKind Unknown = new OperatingSystemKind("unknown", "Unknown");

	public static OperatingSystemKind GetCurrent()
	{
		if (System.OperatingSystem.IsLinux()) return LinuxBased;
		if (System.OperatingSystem.IsWindows()) return Windows;
		if (System.OperatingSystem.IsFreeBSD()) return FreeBsd;
		if (System.OperatingSystem.IsMacOS()) return MacOs;
		return Unknown;
	}

	[Pure]
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return ((obj is not null) &&
		        (this.GetType() == obj.GetType()) &&
		        (this.id == ((OperatingSystemKind)obj).id));
	}

	[Pure] public static bool operator ==(OperatingSystemKind lhs, OperatingSystemKind rhs) => lhs.Equals(rhs);
	[Pure] public static bool operator !=(OperatingSystemKind lhs, OperatingSystemKind rhs) => !(lhs.Equals(rhs));

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
