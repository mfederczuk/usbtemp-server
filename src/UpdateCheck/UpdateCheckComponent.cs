/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Net.Http;
using UsbtempServer.UpdateCheck.GitHub;
using UsbtempServer.Utils.SemVer2;

namespace UsbtempServer.UpdateCheck;

public class UpdateCheckComponent
{
	private static readonly SemanticVersion USBTEMP_SERVER_VERSION = SemanticVersion.OfString("0.1.0-indev03");

	private readonly Lazy<IUpdateChecker> lazyUpdateChecker = new Lazy<IUpdateChecker>(createUpdateChecker);

	public IUpdateChecker GetUpdateChecker()
	{
		return this.lazyUpdateChecker.Value;
	}

	private static IUpdateChecker createUpdateChecker()
	{
		HttpClient githubHttpApiClient = GitHubHttpApiClientFactory.Create(USBTEMP_SERVER_VERSION);
		GitHubApi githubApi = new(githubHttpApiClient);

		return new UpdateChecker(githubApi, USBTEMP_SERVER_VERSION);
	}
}
