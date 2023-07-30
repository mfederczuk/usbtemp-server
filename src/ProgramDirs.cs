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
	[Pure]
	public static Pathname? GetConfigDirectoryPathname()
	{
		OperatingSystemKind currentOperatingSystemKind = OperatingSystemKind.GetCurrent();

		if (currentOperatingSystemKind == OperatingSystemKind.LinuxBased)
		{
			return XdgBaseDirs.GetConfigHome()
				.JoinWith(Pathname.Component.OfString("usbtemp-server"));
		}

		if (currentOperatingSystemKind == OperatingSystemKind.Windows)
		{
			return Pathname.OfString(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
				.JoinWith(
					Pathname.Component.OfString("Usbtemp Server"),
					Pathname.Component.OfString("Config")
				);
		}

		// don't know, don't care
		return null;
	}
}
