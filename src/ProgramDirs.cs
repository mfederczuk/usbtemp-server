/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Diagnostics.Contracts;
using UsbtempServer.Utils;
using UsbtempServer.Utils.Sys;

namespace UsbtempServer;

public static class ProgramDirs
{
	private static readonly Pathname.Component PROGRAM_NAME_COMPONENT_XDG =
		Pathname.Component.OfString("usbtemp-server");

	private static readonly Pathname.Component PROGRAM_NAME_COMPONENT_COMPONENT =
		Pathname.Component.OfString("Usbtemp Server");

	[Pure]
	public static Pathname? GetConfigDirectoryPathname()
	{
		OperatingSystemKind currentOperatingSystemKind = OperatingSystemKind.GetCurrent();

		if (currentOperatingSystemKind == OperatingSystemKind.LinuxBased)
		{
			return XdgBaseDirs.GetConfigHome()
				.JoinWith(PROGRAM_NAME_COMPONENT_XDG);
		}

		if (currentOperatingSystemKind == OperatingSystemKind.Windows)
		{
			return Pathname.OfString(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
				.JoinWith(
					PROGRAM_NAME_COMPONENT_COMPONENT,
					Pathname.Component.OfString("Config")
				);
		}

		// don't know, don't care
		return null;
	}

	[Pure]
	public static Pathname? GetStateDirectoryPathname()
	{
		OperatingSystemKind currentOperatingSystemKind = OperatingSystemKind.GetCurrent();

		if (currentOperatingSystemKind == OperatingSystemKind.LinuxBased)
		{
			return XdgBaseDirs.GetStateHome()
				.JoinWith(PROGRAM_NAME_COMPONENT_XDG);
		}

		if (currentOperatingSystemKind == OperatingSystemKind.Windows)
		{
			return Pathname.OfString(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
				.JoinWith(
					PROGRAM_NAME_COMPONENT_COMPONENT,
					Pathname.Component.OfString("State")
				);
		}

		// don't know, don't care
		return null;
	}
}
