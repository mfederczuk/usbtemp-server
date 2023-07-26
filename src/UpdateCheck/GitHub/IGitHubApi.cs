/*
 * SPDX-License-Identifier: CC0-1.0
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UsbtempServer.Utils.SemVer2;

namespace UsbtempServer.UpdateCheck.GitHub;

public record GitHubRelease(
	SemanticVersion Version,
	Uri HtmlPageUrl
);

public interface IGitHubApi
{
	public Task<IEnumerable<GitHubRelease>> FetchReleasesOfRepo(string repoOwner, string repoName);
}
