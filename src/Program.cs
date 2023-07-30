/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Collections.Generic;
using System.Text;
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
		using (Cli.Paragraph paragraph = cli.BeginNewParagraph())
		{
			IList<IThermometer> detectedThermometers = ThermometerDetector.DetectThermometers();
			switch (detectedThermometers.Count)
			{
				case 0:
					paragraph.PrintLine("No USB thermometers detected.");
					break;
				case 1:
					{
						IThermometer detectedThermometer = detectedThermometers[0];

						StringBuilder sb = new("A USB thermometer on port ");
						sb.Append(detectedThermometer.GetPortName().ToString());
						sb.Append(", with the serial number ");
						sb.Append(detectedThermometer.GetSerialNumber().ToString());
						sb.Append(" was detected.\nDo you want to use this device?");

						Cli.BoolResponse boolResponse = paragraph.PromptForBool(sb.ToString(), defaultValue: true);

						if (boolResponse.IsEof())
						{
							paragraph.PrintBlankLine();
							paragraph.PrintLine("Aborted.");
							return null;
						}

						if (boolResponse.IsYes())
						{
							return detectedThermometer;
						}

						paragraph.PrintBlankLine();
						break;
					}
				default:
					{
						StringBuilder sb = new("The following USB thermometers were detected:");
						foreach (IThermometer detectedThermometer in detectedThermometers)
						{
							sb.Append("\n  * Port: ");
							sb.Append(detectedThermometer.GetPortName().ToString());
							sb.Append("  |  Serial number: ");
							sb.Append(detectedThermometer.GetSerialNumber().ToString());
						}

						paragraph.PrintLine(sb.ToString());
						break;
					}
			}

			Cli.StringResponse portNameResponse = paragraph
				.PromptForString(msg: "Enter the port name of the device to use");

			if (portNameResponse.IsEof())
			{
				paragraph.PrintBlankLine();
				paragraph.PrintLine("Aborted.");
				return null;
			}

			string responseValue = portNameResponse.GetValue();

			if (responseValue == string.Empty)
			{
				paragraph.PrintLine("Aborted.");
				return null;
			}

			return ThermometerFactory.OpenNew(portName: SerialPortName.OfString(responseValue));
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
