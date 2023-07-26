/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UsbtempServer.Utils.SemVer2;

namespace UsbtempServer.UpdateCheck.GitHub;

public class GitHubApi : IGitHubApi
{
	private record ApiGitHubRelease(
		[property: JsonProperty("html_url", Required = Required.Always)] string HtmlUrl,
		[property: JsonProperty("tag_name", Required = Required.Always)] string TagName
	);

	private readonly HttpClient githubHttpApiClient;

	[Pure]
	public GitHubApi(HttpClient githubHttpApiClient)
	{
		this.githubHttpApiClient = githubHttpApiClient;
	}

	async Task<IEnumerable<GitHubRelease>> IGitHubApi.FetchReleasesOfRepo(string repoOwner, string repoName)
	{
		HttpResponseMessage response = await this.githubHttpApiClient
			.GetAsync($"repos/{repoOwner}/{repoName}/releases");

		response.EnsureSuccessStatusCode();

		string responseBody = await response.Content.ReadAsStringAsync();

		IEnumerable<ApiGitHubRelease>? apiReleases = JsonConvert
			.DeserializeObject<IEnumerable<ApiGitHubRelease>>(responseBody);

		if (apiReleases is null)
		{
			throw new JsonSerializationException(message: "Expected an object, but got null");
		}

		IList<GitHubRelease> releases = new List<GitHubRelease>();
		foreach (ApiGitHubRelease apiRelease in apiReleases)
		{
			GitHubRelease? release = tryMapApiGitHubReleaseToGitHubRelease(apiRelease);

			if (release is null)
			{
				continue;
			}

			releases.Add(release);
		}

		return releases;
	}

	[Pure]
	private static GitHubRelease? tryMapApiGitHubReleaseToGitHubRelease(ApiGitHubRelease apiRelease)
	{
		if (!(apiRelease.TagName.StartsWith('v')))
		{
			return null;
		}

		SemanticVersion? semanticVersion = SemanticVersion.OfStringOfNull(apiRelease.TagName.Substring(1));

		if (semanticVersion is null)
		{
			return null;
		}

		return new GitHubRelease(
			Version: semanticVersion.Value,
			HtmlPageUrl: new Uri(apiRelease.HtmlUrl)
		);
	}
}
