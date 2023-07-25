/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;

namespace UsbtempServer.Utils.SemVer2;

public readonly struct SemanticVersion : IComparable<SemanticVersion>
{
	private static readonly Regex LEADING_NUMERIC_PATTERN = new Regex("^(0|[1-9][0-9]*)");

	private readonly uint majorNum;
	private readonly uint minorNum;
	private readonly uint patchNum;
	private readonly IReadOnlyList<PreReleaseIdentifier> preReleaseIdentifiers;
	private readonly IReadOnlyList<BuildMetadataIdentifier> buildMetadataIdentifiers;

	[Pure]
	private SemanticVersion(
		uint majorNum,
		uint minorNum,
		uint patchNum,
		IReadOnlyList<PreReleaseIdentifier> preReleaseIdentifiers,
		IReadOnlyList<BuildMetadataIdentifier> buildMetadataIdentifiers
	)
	{
		this.majorNum = majorNum;
		this.minorNum = minorNum;
		this.patchNum = patchNum;
		this.preReleaseIdentifiers = preReleaseIdentifiers;
		this.buildMetadataIdentifiers = buildMetadataIdentifiers;
	}

	[Pure]
	public static SemanticVersion? OfStringOfNull(string str)
	{
		return consumeStringBuilder(new StringBuilder(str));
	}

	[Pure]
	public static SemanticVersion OfString(string str)
	{
		SemanticVersion? semanticVersion = OfStringOfNull(str);

		if (semanticVersion is null)
		{
			throw new ArgumentException(
				message: "String is not a valid semantic version",
				paramName: nameof(str)
			);
		}

		return semanticVersion.Value;
	}

	private static SemanticVersion? consumeStringBuilder(StringBuilder sb)
	{
		// <MAJOR>
		uint? majorNum = nextInteger(sb);
		if (majorNum is null) return null;

		// <MAJOR>.
		if (!(sb.ToString().StartsWith('.'))) return null;
		sb.Remove(0, 1);

		// <MAJOR>.<MINOR>
		uint? minorNum = nextInteger(sb);
		if (minorNum is null) return null;

		// <MAJOR>.<MINOR>.
		if (!(sb.ToString().StartsWith('.'))) return null;
		sb.Remove(0, 1);

		// <MAJOR>.<MINOR>.<PATCH>
		uint? patchNum = nextInteger(sb);
		if (patchNum is null) return null;

		// <MAJOR>.<MINOR>.<PATCH>-<PRE_RELEASES>
		IReadOnlyList<PreReleaseIdentifier>? preReleaseIdentifiers = nextPreReleaseIdentifiers(sb);
		if (preReleaseIdentifiers is null) return null;

		// <MAJOR>.<MINOR>.<PATCH>-<PRE_RELEASE>+<BUILD_METADATA>
		IReadOnlyList<BuildMetadataIdentifier>? buildMetadataIdentifiers = nextBuildMetadataIdentifiers(sb);
		if (buildMetadataIdentifiers is null) return null;

		if (sb.Length > 0)
		{
			return null;
		}

		return new SemanticVersion(
			majorNum.Value,
			minorNum.Value,
			patchNum.Value,
			preReleaseIdentifiers,
			buildMetadataIdentifiers
		);
	}

	private static uint? nextInteger(StringBuilder sb)
	{
		Match? match = LEADING_NUMERIC_PATTERN.Match(sb.ToString());

		if (match is null)
		{
			return null;
		}

		uint integer = uint.Parse(match.Value);

		sb.Remove(0, match.Length);

		return integer;
	}

	private static IReadOnlyList<PreReleaseIdentifier>? nextPreReleaseIdentifiers(StringBuilder sb)
	{
		IList<PreReleaseIdentifier> identifiers = new List<PreReleaseIdentifier>();

		if (!(sb.ToString().StartsWith('-')))
		{
			return identifiers.AsReadOnly();
		}

		sb.Remove(0, 1);

		int nextIdentifierEndIndex;
		do
		{
			nextIdentifierEndIndex = sb.ToString().IndexOf('.');

			if (nextIdentifierEndIndex < 0)
			{
				nextIdentifierEndIndex = sb.ToString().IndexOf('+');
			}

			if (nextIdentifierEndIndex < 0)
			{
				nextIdentifierEndIndex = sb.Length;
			}

			if (nextIdentifierEndIndex > 0)
			{
				PreReleaseIdentifier? nextIdentifier =
					PreReleaseIdentifier.OfStringOrNull(sb.ToString().Substring(0, nextIdentifierEndIndex));

				if (nextIdentifier is null)
				{
					return null;
				}

				identifiers.Add(nextIdentifier.Value);

				sb.Remove(0, nextIdentifierEndIndex);
			}
		} while (nextIdentifierEndIndex > 0);

		return identifiers.AsReadOnly();
	}

	private static IReadOnlyList<BuildMetadataIdentifier>? nextBuildMetadataIdentifiers(StringBuilder sb)
	{
		IList<BuildMetadataIdentifier> identifiers = new List<BuildMetadataIdentifier>();

		if (!(sb.ToString().StartsWith('+')))
		{
			return identifiers.AsReadOnly();
		}

		sb.Remove(0, 1);

		int nextIdentifierEndIndex;
		do
		{
			nextIdentifierEndIndex = sb.ToString().IndexOf('.');

			if (nextIdentifierEndIndex < 0)
			{
				nextIdentifierEndIndex = sb.Length;
			}

			if (nextIdentifierEndIndex > 0)
			{
				BuildMetadataIdentifier? nextIdentifier =
					BuildMetadataIdentifier.OfStringOrNull(sb.ToString().Substring(0, nextIdentifierEndIndex));

				if (nextIdentifier is null)
				{
					return null;
				}

				identifiers.Add(nextIdentifier.Value);

				sb.Remove(0, nextIdentifierEndIndex);
			}
		} while (nextIdentifierEndIndex > 0);

		return identifiers.AsReadOnly();
	}

	[Pure]
	public bool IsPreRelease()
	{
		return (this.preReleaseIdentifiers.Count > 0);
	}

	[Pure]
	public int CompareTo(SemanticVersion other)
	{
		// <https://semver.org/#spec-item-11>

		// 11.1. Precedence MUST be calculated by separating the version into major, minor, patch and
		//       pre-release identifiers in that order (Build metadata does not figure into precedence).
		// 11.2. Precedence is determined by the first difference when comparing each of these identifiers from
		//       left to right as follows: Major, minor, and patch versions are always compared numerically.
		if (this.majorNum != other.majorNum) return ((long)(this.majorNum) - (long)(other.majorNum)).ClampToInt();
		if (this.minorNum != other.minorNum) return ((long)(this.minorNum) - (long)(other.minorNum)).ClampToInt();
		if (this.patchNum != other.patchNum) return ((long)(this.patchNum) - (long)(other.patchNum)).ClampToInt();

		if (this.IsPreRelease() != other.IsPreRelease())
		{
			// 11.3. When major, minor, and patch are equal, a pre-release version has lower precedence than
			//       a normal version.
			if (this.IsPreRelease()) return -1;
			return 1;
		}

		// 11.4. Precedence for two pre-release versions with the same major, minor, and patch version MUST be
		//       determined by comparing each dot separated identifier from left to right until a difference is found.
		for (int i = 0; i < int.Min(this.preReleaseIdentifiers.Count, other.preReleaseIdentifiers.Count); ++i)
		{
			int ordering = this.preReleaseIdentifiers[i].CompareTo(other.preReleaseIdentifiers[i]);

			if (ordering != 0)
			{
				return ordering;
			}
		}

		// 11.4.4 A larger set of pre-release fields has a higher precedence than a smaller set,
		//        if all of the preceding identifiers are equal.
		return (this.preReleaseIdentifiers.Count - other.preReleaseIdentifiers.Count);
	}

	[Pure] public static bool operator <(SemanticVersion lhs, SemanticVersion rhs) => (lhs.CompareTo(rhs) < 0);
	[Pure] public static bool operator <=(SemanticVersion lhs, SemanticVersion rhs) => (lhs.CompareTo(rhs) <= 0);
	[Pure] public static bool operator >(SemanticVersion lhs, SemanticVersion rhs) => (lhs.CompareTo(rhs) > 0);
	[Pure] public static bool operator >=(SemanticVersion lhs, SemanticVersion rhs) => (lhs.CompareTo(rhs) >= 0);

	[Pure]
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		if ((obj is null) || (this.GetType() != obj.GetType()))
		{
			return false;
		}

		if (this.CompareTo((SemanticVersion)obj) != 0)
		{
			return false;
		}

		return this.buildMetadataIdentifiers == ((SemanticVersion)obj).buildMetadataIdentifiers;
	}

	[Pure]
	public override int GetHashCode()
	{
		return HashCode
			.Combine(
				this.majorNum,
				this.minorNum,
				this.patchNum,
				this.preReleaseIdentifiers,
				this.buildMetadataIdentifiers
			);
	}

	[Pure]
	public override string ToString()
	{
		StringBuilder sb = new();

		sb.Append(this.majorNum);
		sb.Append('.');
		sb.Append(this.minorNum);
		sb.Append('.');
		sb.Append(this.patchNum);

		if (this.preReleaseIdentifiers.Count > 0)
		{
			sb.Append('-');
			sb.Append(this.preReleaseIdentifiers[0]);

			for (int i = 1; i < this.preReleaseIdentifiers.Count; ++i)
			{
				sb.Append('.');
				sb.Append(this.preReleaseIdentifiers[i].ToString());
			}
		}

		if (this.buildMetadataIdentifiers.Count > 0)
		{
			sb.Append('+');
			sb.Append(this.buildMetadataIdentifiers[0].ToString());

			for (int i = 1; i < this.buildMetadataIdentifiers.Count; ++i)
			{
				sb.Append('+');
				sb.Append(this.buildMetadataIdentifiers[i].ToString());
			}
		}

		return sb.ToString();
	}
}
