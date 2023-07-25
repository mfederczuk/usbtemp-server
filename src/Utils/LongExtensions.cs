/*
 * SPDX-License-Identifier: CC0-1.0
 */

namespace UsbtempServer.Utils;

public static class LongExtensions
{
	public static int ClampToInt(this long value)
	{
		return (int)(long.Clamp(value, min: (long)(int.MinValue), max: (long)(int.MaxValue)));
	}
}
