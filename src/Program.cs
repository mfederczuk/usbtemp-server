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
using UsbtempServer.Utils;

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
		Cli cli = new();

		await checkForUpdate(cli);

		Func<Cli, IThermometer?>? thermometerSupplier = getThermometerSupplier(cli);
		if (thermometerSupplier is null) return 1;

		using (IThermometer? thermometer = thermometerSupplier(cli))
		{
			if (thermometer is null) return 1;

			IThermometer.SerialNumber serialNumber = thermometer.GetSerialNumber();
			Temperature initialTemperature = thermometer.ReadTemperature();

			using (Cli.Paragraph paragraph = cli.BeginNewParagraph())
			{
				paragraph.PrintLine($"Serial number: {serialNumber}");
				paragraph.PrintLine($"Initial temperature: {initialTemperature.ToDegreeCelsiusFloat()}°C");
			}

			cli.PrintBlankLine();

			runServer(args, thermometer);
		}

		return 0;
	}

	private static async Task checkForUpdate(Cli cli)
	{
		using (Cli.Paragraph paragraph = cli.BeginNewParagraph())
		{
			Cli.Paragraph.Action updateCheckAction = paragraph.StartNewAction(name: "Checking for updates");

			UpdateCheckResult result = await updateCheckComponent
				.GetUpdateChecker()
				.CheckForUpdate();

			switch (result.ResultType)
			{
				case UpdateCheckResultType.UpToDate:
					updateCheckAction.Finish(resultMsg: "You're up-to-date");
					return;
				case UpdateCheckResultType.UpdateAvailable:
					break;
				case UpdateCheckResultType.Failed:
					updateCheckAction.Finish(resultMsg: "Failed to check for updates. Ignoring and moving on.");
					return;
				default:
					throw new InvalidOperationException(message: $"Unhandled case for enum value {result.ResultType}");
			}

			if (result.LatestReleasePageUrl is null)
			{
				throw new InvalidOperationException("Latest release page URL is null");
			}

			updateCheckAction.Finish(resultMsg: $"An update is available!\nDownload it here: {result.LatestReleasePageUrl}");
		}
	}

	private static Func<Cli, IThermometer?>? getThermometerSupplier(Cli cli)
	{
		using (Cli.Paragraph paragraph = cli.BeginNewParagraph())
		{
			ThermometerKindSelection selection = promptForThermometerKind(paragraph);
			switch (selection)
			{
				case ThermometerKindSelection.Physical:
					return promptForPhysicalThermometer;
				case ThermometerKindSelection.Virtual:
					return (Cli _) => { return new MockThermometer(); };
				case ThermometerKindSelection.Invalid:
					paragraph.PrintLine("Invalid selection. Quitting.");
					return null;
				case ThermometerKindSelection.Cancelled:
					paragraph.PrintLine("Aborted.");
					return null;
				case ThermometerKindSelection.Eof:
					paragraph.PrintBlankLine();
					paragraph.PrintLine("Aborted.");
					return null;
				default:
					throw new InvalidOperationException(message: $"Unhandled case for enum value {selection}");
			}
		}
	}

	private static IThermometer? promptForPhysicalThermometer(Cli cli)
	{
		using (Cli.Paragraph paragraph = cli.BeginNewParagraph())
		{
			Cli.StringResponse response = paragraph.PromptForString(msg: "Enter the port name of the USB thermometer");

			if (response.IsEof())
			{
				paragraph.PrintBlankLine();
				paragraph.PrintLine("Aborted.");
				return null;
			}

			string responseValue = response.GetValue();

			if (responseValue == string.Empty)
			{
				paragraph.PrintLine("Aborted.");
				return null;
			}

			return ThermometerFactory.OpenNew(portName: SerialPortName.OfString(responseValue));
		}
	}

	private static ThermometerKindSelection promptForThermometerKind(Cli.Paragraph paragraph)
	{
#if DEBUG
		string message = "[DEBUG] Select the kind of thermometer to use.\n" +
		                 "[DEBUG] (1) real/physical USB thermometer\n" +
		                 "[DEBUG] (2) virtual mock thermometer\n" +
		                 "[DEBUG] (q) cancel\n" +
		                 "[DEBUG] Enter";
		Cli.StringResponse response = paragraph.PromptForString(message, defaultValue: "2");

		if (response.IsEof()) return ThermometerKindSelection.Eof;

		string responseValue = response.GetValue();

		if (responseValue == "1") return ThermometerKindSelection.Physical;
		if (responseValue == "2") return ThermometerKindSelection.Virtual;
		if (responseValue.ToLower() == "q") return ThermometerKindSelection.Cancelled;

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
