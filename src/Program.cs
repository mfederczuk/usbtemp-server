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
	private enum ThermometerKindSelection
	{
		Physical,
		Virtual,
		Cancelled,
		Invalid,
		Eof,
	}

	private static readonly UpdateCheckComponent updateCheckComponent = new UpdateCheckComponent();

	public static async Task<int> Main(string[] args)
	{
		await checkForUpdate();

		Console.Error.WriteLine();

		Func<IThermometer?>? thermometerSupplier = getThermometerSupplier();
		if (thermometerSupplier is null) return 1;

		using (IThermometer? thermometer = thermometerSupplier())
		{
			if (thermometer is null) return 1;

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

	private static Func<IThermometer?>? getThermometerSupplier()
	{
		ThermometerKindSelection selection = promptForThermometerKind();
		switch (selection)
		{
			case ThermometerKindSelection.Physical:
				return promptForPhysicalThermometer;
			case ThermometerKindSelection.Virtual:
				return () => { return new MockThermometer(); };
			case ThermometerKindSelection.Invalid:
				Console.Error.WriteLine("Invalid selection. Quitting.");
				return null;
			case ThermometerKindSelection.Cancelled:
				Console.Error.WriteLine("Aborted.");
				return null;
			case ThermometerKindSelection.Eof:
				Console.Error.WriteLine("\nAborted.");
				return null;
			default:
				throw new InvalidOperationException(message: $"Unhandled case for enum value {selection}");
		}
	}

	private static IThermometer? promptForPhysicalThermometer()
	{
		Console.Error.Write("Enter the port name of the USB thermometer: ");
		string? portName = Console.ReadLine();

		if (portName is null)
		{
			Console.Error.WriteLine("\nAborted.");
			return null;
		}

		if (portName == string.Empty)
		{
			Console.Error.WriteLine("Aborted.");
			return null;
		}

		return Thermometer.OpenNew(portName);
	}

	private static ThermometerKindSelection promptForThermometerKind()
	{
#if DEBUG
		string message = "[DEBUG] Select the kind of thermometer to use.\n" +
		                 "[DEBUG] (1) real/physical USB thermometer\n" +
		                 "[DEBUG] (2) virtual mock thermometer\n" +
		                 "[DEBUG] (q) cancel\n" +
		                 "[DEBUG] Enter: [2] ";
		Console.Error.Write(message);

		string? input = Console.ReadLine();

		if (input is null) return ThermometerKindSelection.Eof;

		input = input.Trim();

		if (input == "1") return ThermometerKindSelection.Physical;
		if ((input == "2") || (input == string.Empty)) return ThermometerKindSelection.Virtual;
		if (input.ToLower() == "q") return ThermometerKindSelection.Cancelled;

		return ThermometerKindSelection.Invalid;
#else
		return ThermometerKindSelection.Physical;
#endif
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
