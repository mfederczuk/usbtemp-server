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

public readonly struct PreReleaseIdentifier : IComparable<PreReleaseIdentifier>
{
	private static readonly Regex VALIDITY_PATTERN = new Regex("^[0-9A-Za-z-]+$");
	private static readonly Regex LEADING_ZEROS_PATTERN = new Regex("^0+[0-9]+$");
	private static readonly Regex NUMERIC_PATTERN = new Regex("^(0|[1-9][0-9]*)$");

	private readonly string str;

	[Pure]
	private PreReleaseIdentifier(string str)
	{
		if (str == string.Empty)
		{
			throw new ArgumentException(
				message: "Pre-release identifier must not be empty",
				paramName: nameof(str)
			);
		}

		if (!(VALIDITY_PATTERN.IsMatch(str)))
		{
			string message = "Pre-release identifier must comprise only ASCII alphanumerics and hyphens [0-9A-Za-z-]";
			throw new ArgumentException(message, paramName: nameof(str));
		}

		if (LEADING_ZEROS_PATTERN.IsMatch(str))
		{
			throw new ArgumentException(
				message: "Numeric pre-release identifier must not include leading zeroes",
				paramName: nameof(str)
			);
		}

		this.str = str;
	}

	[Pure]
	private bool isNumerical()
	{
		return NUMERIC_PATTERN.IsMatch(this.str);
	}

	[Pure]
	public static PreReleaseIdentifier? OfStringOrNull(string str)
	{
		if ((str == string.Empty) || !(VALIDITY_PATTERN.IsMatch(str)) || LEADING_ZEROS_PATTERN.IsMatch(str))
		{
			return null;
		}

		return new PreReleaseIdentifier(str);
	}

	[Pure]
	public int CompareTo(PreReleaseIdentifier other)
	{
		// <https://semver.org/#spec-item-11>

		if (this.isNumerical() && other.isNumerical())
		{
			// 11.4.1. Identifiers consisting of only digits are compared numerically.
			return ((long)(uint.Parse(this.str)) - (long)(uint.Parse(other.str))).ClampToInt();
		}

		if (!(this.isNumerical()) && !(other.isNumerical()))
		{
			// 11.4.2. Identifiers with letters or hyphens are compared lexically in ASCII sort order.
			return this.str.CompareTo(other.str);
		}

		// 11.4.3. Numeric identifiers always have lower precedence than non-numeric identifiers.
		if (this.isNumerical()) return -1;
		return 1;
	}

	[Pure] public static bool operator <(PreReleaseIdentifier lhs, PreReleaseIdentifier rhs) => (lhs.CompareTo(rhs) < 0);
	[Pure] public static bool operator <=(PreReleaseIdentifier lhs, PreReleaseIdentifier rhs) => (lhs.CompareTo(rhs) <= 0);
	[Pure] public static bool operator >(PreReleaseIdentifier lhs, PreReleaseIdentifier rhs) => (lhs.CompareTo(rhs) > 0);
	[Pure] public static bool operator >=(PreReleaseIdentifier lhs, PreReleaseIdentifier rhs) => (lhs.CompareTo(rhs) >= 0);

	[Pure]
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return ((obj is not null) &&
		        (this.GetType() == obj.GetType()) &&
		        (this.str == ((PreReleaseIdentifier)obj).str));
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
