/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using UsbtempServer.Utils.SemVer2;
using UsbtempServer.Utils.Sys;

public static class GitHubHttpApiClientFactory
{
	private const string PRODUCT_NAME = "usbtemp-server";

	public static HttpClient Create(SemanticVersion usbtempServerVersion)
	{
		HttpClient httpClient = new HttpClient
		{
			BaseAddress = new Uri("https://api.github.com"),
			DefaultRequestVersion = HttpVersion.Version20,
		};

		httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(buildUserAgent(usbtempServerVersion));
		httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
		httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

		return httpClient;
	}

	private static string buildUserAgent(SemanticVersion usbtempServerVersion)
	{
		StringBuilder sb = new();

		sb.Append(PRODUCT_NAME);
		sb.Append('/');
		sb.Append(usbtempServerVersion.ToString());

		sb.Append(" (System.Net.HttpClient; .NET; ");
		sb.Append(PlatformInfo.GetCurrent().ToString());
		sb.Append(')');

		return sb.ToString();
	}
}
