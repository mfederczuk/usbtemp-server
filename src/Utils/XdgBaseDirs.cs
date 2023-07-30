/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;

namespace UsbtempServer.Utils;

public static class XdgBaseDirs
{
	public static Pathname GetConfigHome()
	{
		return getDirPathname(
			environmentVariableName: "XDG_CONFIG_HOME",
			defaultRelativePathnameSupplier: () =>
			{
				return Pathname.CreateRelativeOfSingleComponent(Pathname.Component.OfString(".config"));
			}
		);
	}

	public static Pathname GetStateHome()
	{
		return getDirPathname(
			environmentVariableName: "XDG_STATE_HOME",
			defaultRelativePathnameSupplier: () =>
			{
				return Pathname.CreateRelativeOfSingleComponent(Pathname.Component.OfString(".local"))
					.JoinWith(Pathname.Component.OfString("state"));
			}
		);
	}

	private static Pathname getDirPathname(
		string environmentVariableName,
		Func<Pathname> defaultRelativePathnameSupplier
	)
	{
		Pathname? pathname = Pathname
			.OfStringOrNull(Environment.GetEnvironmentVariable(environmentVariableName) ?? string.Empty);

		if ((pathname is null) || pathname.Value.IsRelative())
		{
			// as per XDG spec, relative paths are invalid and should be ignored

			Pathname defaultRelativePathname = defaultRelativePathnameSupplier();

			pathname = getHomePathname()
				.JoinWith(defaultRelativePathname);
		}

		return pathname.Value;
	}

	private static Pathname getHomePathname()
	{
		string homePathnameStr = (Environment.GetEnvironmentVariable("HOME") ?? string.Empty);

		if (homePathnameStr == string.Empty)
		{
			throw new InvalidOperationException(message: "The environment variable 'HOME' must not be unset or empty");
		}

		Pathname homePathname = Pathname.OfString(homePathnameStr);

		if (homePathname.IsRelative())
		{
			throw new InvalidOperationException(
				message: $"The environment variable 'HOME' (\"{homePathname}\") must be absolute"
			);
		}

		return homePathname;
	}
}
