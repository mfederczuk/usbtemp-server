/*
 * SPDX-License-Identifier: CC0-1.0
 */

using System;
using System.Threading.Tasks;

namespace UsbtempServer.UpdateCheck;

public enum UpdateCheckResultType
{
	UpToDate,
	UpdateAvailable,
	Failed,
}

public readonly record struct UpdateCheckResult(
	UpdateCheckResultType ResultType,
	Uri? LatestReleasePageUrl
);

public interface IUpdateChecker
{
	public Task<UpdateCheckResult> CheckForUpdate();
}
