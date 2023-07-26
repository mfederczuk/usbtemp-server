/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using UsbtempServer.Thermology;
using UsbtempServer.UpdateCheck;

namespace UsbtempServer;

public static class Program
{
	private static readonly UpdateCheckComponent updateCheckComponent = new UpdateCheckComponent();

	public static async Task<int> Main(string[] args)
	{
		await checkForUpdate();

		Console.Error.Write("\nEnter the port name of the USB-thermometer: ");
		string? portName = Console.ReadLine();

		if ((portName is null) || (portName == string.Empty))
		{
			string prefix = ((portName is null) ? "\n" : "");
			Console.Error.WriteLine(prefix + "Aborted.");

			return 1;
		}

		using (IThermometer thermometer = Thermometer.OpenNew(portName))
		{
			string serialNumber = thermometer.ReadSerialNumber();
			Temperature initialTemperature = thermometer.ReadTemperature();

			Console.Error.WriteLine($"\nSerial number: {serialNumber}");
			Console.Error.WriteLine($"Initial temperature: {initialTemperature.ToDegreeCelsiusFloat()}°C\n");

			runServer(args, thermometer);
		}

		return 0;
	}

	private static async Task checkForUpdate()
	{
		Console.Error.Write("Checking for updates...");

		UpdateCheckResult result = await updateCheckComponent
			.GetUpdateChecker()
			.CheckForUpdate();

		switch (result.ResultType)
		{
			case UpdateCheckResultType.UpToDate:
				Console.Error.WriteLine(" You're up-to-date.");
				return;
			case UpdateCheckResultType.UpdateAvailable:
				break;
			case UpdateCheckResultType.Failed:
				Console.Error.WriteLine(" Failed to check for updates. Ignoring and moving on.");
				return;
			default:
				throw new InvalidOperationException(message: $"Unhandled case for enum value {result.ResultType}");
		}

		if (result.LatestReleasePageUrl is null)
		{
			throw new InvalidOperationException("Latest release page URL is null");
		}

		Console.Error.WriteLine($" An update is available!\nDownload it here: {result.LatestReleasePageUrl}");
	}

	private static void runServer(string[] args, IThermometer thermometer)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		builder.Services
			.AddSingleton(thermometer)
			.AddControllers();

		WebApplication app = builder.Build();
		app.UseStaticFiles();
		app.MapControllers();

		app.Run();
	}
}
