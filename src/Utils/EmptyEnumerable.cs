/*
 * SPDX-License-Identifier: CC0-1.0
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace UsbtempServer.Utils;

public class EmptyEnumerable<T> : IEnumerable<T>
{
	private class EmptyEnumerator : IEnumerator<T>
	{
		object IEnumerator.Current
		{
			[DoesNotReturn, Pure]
			get => throw new InvalidOperationException("Current element is invalid in empty enumerator");
		}

		T IEnumerator<T>.Current
		{
			[DoesNotReturn, Pure]
			get => throw new InvalidOperationException("Current element is invalid in empty enumerator");
		}

		[Pure]
		bool IEnumerator.MoveNext()
		{
			return false;
		}

		[Pure]
		void IEnumerator.Reset()
		{
			// nothing
		}

		[Pure]
		void IDisposable.Dispose()
		{
			// nothing
		}
	}

	[Pure]
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return (IEnumerator<T>)(new EmptyEnumerable<T>());
	}

	[Pure]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return (IEnumerator)(new EmptyEnumerable<T>());
	}
}
