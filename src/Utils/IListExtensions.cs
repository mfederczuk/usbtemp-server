/*
 * SPDX-License-Identifier: CC0-1.0
 */

using System.Collections.Generic;

namespace UsbtempServer.Utils;

public static class IListExtensions
{
	public static void AddAll<T>(this IList<T> list, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			list.Add(item);
		}
	}
}
