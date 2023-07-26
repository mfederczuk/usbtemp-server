/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UsbtempServer.UpdateCheck.GitHub;
using UsbtempServer.Utils.SemVer2;

namespace UsbtempServer.UpdateCheck;

public class UpdateChecker : IUpdateChecker
{
	private const string GITHUB_REPO_OWNER = "mfederczuk";
	private const string GITHUB_REPO_NAME = "usbtemp-server";

	private readonly IGitHubApi githubApi;
	private readonly SemanticVersion usbtempServerVersion;

	[Pure]
	public UpdateChecker(
		IGitHubApi githubApi,
		SemanticVersion usbtempServerVersion
	)
	{
		this.githubApi = githubApi;
		this.usbtempServerVersion = usbtempServerVersion;
	}

	async Task<UpdateCheckResult> IUpdateChecker.CheckForUpdate()
	{
		IEnumerable<GitHubRelease>? releases = null;
		try
		{
			releases = await this.githubApi
				.FetchReleasesOfRepo(GITHUB_REPO_OWNER, GITHUB_REPO_NAME);
		}
		catch (HttpRequestException) { }
		catch (JsonSerializationException) { }

		if (releases is null)
		{
			return new UpdateCheckResult(
				ResultType: UpdateCheckResultType.Failed,
				LatestReleasePageUrl: null
			);
		}

		GitHubRelease? latestAvailableRelease = this.findLatestAvailableRelease(releases);

		if (latestAvailableRelease is null)
		{
			return new UpdateCheckResult(
				ResultType: UpdateCheckResultType.UpToDate,
				LatestReleasePageUrl: null
			);
		}

		return new UpdateCheckResult(
			ResultType: UpdateCheckResultType.UpdateAvailable,
			LatestReleasePageUrl: latestAvailableRelease.HtmlPageUrl
		);
	}

	private GitHubRelease? findLatestAvailableRelease(IEnumerable<GitHubRelease> releases)
	{
		GitHubRelease? latestRelease = null;

		foreach (GitHubRelease release in releases)
		{
			if ((release.Version <= this.usbtempServerVersion) ||
			    ((latestRelease is not null) && (release.Version <= latestRelease.Version)))
			{
				continue;
			}

			latestRelease = release;
		}

		return latestRelease;
	}
}
