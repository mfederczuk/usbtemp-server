/*
 * SPDX-License-Identifier: CC0-1.0
 */

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace UsbtempServer.Utils;

public static class IEnumerableExtensions
{
	[Pure]
	public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? enumerable)
	{
		return (enumerable ?? new EmptyEnumerable<T>());
	}
}
