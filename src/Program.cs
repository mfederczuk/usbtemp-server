/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace UsbtempServer;

public static class Program
{
	public static int Main(string[] args)
	{
		Console.Error.Write("Enter the port name of the USB-thermometer: ");
		string? portName = Console.ReadLine();

		if ((portName is null) || (portName == string.Empty))
		{
			string prefix = ((portName is null) ? "\n" : "");
			Console.Error.WriteLine(prefix + "Aborted.");

			return 1;
		}

		using (Thermometer thermometer = Thermometer.OpenNew(portName))
		{
			string serialNumber = thermometer.ReadSerialNumber();
			Temperature initialTemperature = thermometer.ReadTemperature();

			Console.Error.WriteLine($"\nSerial number: {serialNumber}");
			Console.Error.WriteLine($"Initial temperature: {initialTemperature.ToDegreeCelsiusFloat()}°C\n");

			runServer(args, thermometer);
		}

		return 0;
	}

	private static void runServer(string[] args, Thermometer thermometer)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		builder.Services
			.AddSingleton(thermometer)
			.AddControllers();

		WebApplication app = builder.Build();
		app.MapControllers();

		app.Run();
	}
}
