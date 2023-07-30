/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace UsbtempServer.Utils;

public readonly struct Pathname
{
	public readonly struct Component
	{
		public readonly char Separator = System.IO.Path.DirectorySeparatorChar;

		private readonly string value;

		[Pure]
		private Component(string value)
		{
			if (value == string.Empty)
			{
				throw new ArgumentException(
					message: "Pathname component must not be empty",
					paramName: nameof(value)
				);
			}

			if (value.Contains(Separator))
			{
				throw new ArgumentException(
					message: "Pathname component must not contain any component separator characters",
					paramName: nameof(value)
				);
			}

			if (System.IO.Path.GetInvalidFileNameChars().Any(value.Contains))
			{
				throw new ArgumentException(
					message: "Pathname component must not contain any invalid characters",
					paramName: nameof(value)
				);
			}

			this.value = value;
		}

		[Pure]
		public static Component OfString(string value)
		{
			return new Component(value);
		}

		[Pure]
		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			return ((obj is not null) &&
			        (this.GetType() == obj.GetType()) &&
			        (this.value == ((Pathname)obj).value));
		}

		[Pure]
		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}

		[Pure]
		public override string ToString()
		{
			return this.value;
		}
	}

	private readonly string value;

	[Pure]
	private Pathname(string value)
	{
		if (value == string.Empty)
		{
			throw new ArgumentException(
				message: "Pathname must not be empty",
				paramName: nameof(value)
			);
		}

		if (System.IO.Path.GetInvalidPathChars().Any(value.Contains))
		{
			throw new ArgumentException(
				message: "Pathname must not contain any invalid characters",
				paramName: nameof(value)
			);
		}

		this.value = value;
	}

	[Pure]
	public static Pathname OfString(string value)
	{
		return new Pathname(value);
	}

	[Pure]
	public static Pathname? OfStringOrNull(string value)
	{
		if ((value == string.Empty) || System.IO.Path.GetInvalidPathChars().Any(value.Contains))
		{
			return null;
		}

		return OfString(value);
	}

	[Pure]
	public static Pathname CreateRelativeOfSingleComponent(Component component)
	{
		return OfString(component.ToString());
	}

	[Pure]
	public bool IsRelative()
	{
		return !(System.IO.Path.IsPathRooted(this.value));
	}

	[Pure]
	public Pathname JoinWith(Pathname otherPathname)
	{
		return OfString(System.IO.Path.Join(this.value, otherPathname.value));
	}

	[Pure]
	public Pathname JoinWith(params Component[] components)
	{
		return components
			.Aggregate(
				this,
				(Pathname accumulatedPathname, Component component) =>
				{
					return accumulatedPathname.JoinWith(CreateRelativeOfSingleComponent(component));
				}
			);
	}

	[Pure]
	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return ((obj is not null) &&
		        (this.GetType() == obj.GetType()) &&
		        (this.value == ((Pathname)obj).value));
	}

	[Pure]
	public override int GetHashCode()
	{
		return this.value.GetHashCode();
	}

	[Pure]
	public override string ToString()
	{
		return this.value;
	}
}
