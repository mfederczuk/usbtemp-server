/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace UsbtempServer.Utils.SemVer2;

public readonly struct BuildMetadataIdentifier
{
	private static readonly Regex VALIDITY_PATTERN = new Regex("^[0-9A-Za-z-]+$");

	private readonly string str;

	[Pure]
	private BuildMetadataIdentifier(string str)
	{
		if (str == string.Empty)
		{
			throw new ArgumentException(
				message: "Build metadata identifier must not be empty",
				paramName: nameof(str)
			);
		}

		if (!(VALIDITY_PATTERN.IsMatch(str)))
		{
			string message = "Build metadata identifier must comprise" +
			                 " only ASCII alphanumerics and hyphens [0-9A-Za-z-]";
			throw new ArgumentException(message, paramName: nameof(str));
		}

		this.str = str;
	}

	[Pure]
	public static BuildMetadataIdentifier? OfStringOrNull(string str)
	{
		if ((str == string.Empty) || !(VALIDITY_PATTERN.IsMatch(str)))
		{
			return null;
		}

		return new BuildMetadataIdentifier(str);
	}

	[Pure]
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return ((obj is not null) &&
		        (this.GetType() == obj.GetType()) &&
		        (this.str == ((BuildMetadataIdentifier)obj).str));
	}

	[Pure]
	public override int GetHashCode()
	{
		return this.str.GetHashCode();
	}

	[Pure]
	public override string ToString()
	{
		return this.str;
	}
}
