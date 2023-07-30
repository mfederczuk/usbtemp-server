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
	private static readonly UpdateCheckComponent updateCheckComponent = new UpdateCheckComponent();

	public static async Task<int> Main(string[] args)
	{
		Cli cli = new();

		await checkForUpdate(cli);

		using (IThermometer? thermometer = promptForThermometer(cli))
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

	private static IThermometer? promptForThermometer(Cli cli)
	{
		StateStorage.PreviouslyUsedThermometerInfo? previouslyUsedDevice = StateStorage.GetPreviouslyUsedDeviceInfo();

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

			SerialPortName portName = SerialPortName.OfString(responseValue);
			IThermometer thermometer = ThermometerFactory.OpenNew(portName);

			previouslyUsedDevice =
				new StateStorage.PreviouslyUsedThermometerInfo(
					PortName: portName,
					SerialNumber: thermometer.GetSerialNumber()
				);
			StateStorage.SetPreviouslyUsedThermometerInfo(previouslyUsedDevice);

			return thermometer;
		}
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
