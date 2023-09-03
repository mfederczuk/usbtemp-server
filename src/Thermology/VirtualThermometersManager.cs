/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Newtonsoft.Json;
using UsbtempServer.Utils;

namespace UsbtempServer.Thermology;

public static class VirtualThermometersManager
{
	private record VirtualThermometerDefinition(
		[property: JsonProperty("portName", Required = Required.Always)] string PortName,
		[property: JsonProperty("serialNumber", Required = Required.Always)] string SerialNumber,
		[property: JsonProperty("enabled", Required = Required.Default)] bool? Enabled
	);

	public static IEnumerable<VirtualThermometer> GetAllRegisteredVirtualThermometers()
	{
		return VirtualThermometersManager.readRegisteredVirtualThermometers().Values;
	}

	public static VirtualThermometer GetRegisteredVirtualThermometerByPortName(SerialPortName portName)
	{
		if (!(portName.IsVirtual()))
		{
			throw new ArgumentException(
				message: "Port name must be virtual",
				paramName: nameof(portName)
			);
		}

		IDictionary<SerialPortName, VirtualThermometer> registeredVirtualThermometers = VirtualThermometersManager.
			readRegisteredVirtualThermometers();

		if (!(registeredVirtualThermometers.ContainsKey(portName)))
		{
			string msg = $"No virtual thermometer registered with port name \"{portName}\"";
			throw new InvalidOperationException(msg);
		}

		return registeredVirtualThermometers[portName];
	}

	private static IDictionary<SerialPortName, VirtualThermometer> readRegisteredVirtualThermometers()
	{
		IEnumerable<VirtualThermometerDefinition>? virtualThermometerDefinitions = VirtualThermometersManager
			.readVirtualThermometerDefinitions();

		IDictionary<SerialPortName, VirtualThermometer> registeredVirtualThermometers =
			new Dictionary<SerialPortName, VirtualThermometer>();

		foreach (VirtualThermometerDefinition virtualThermometerDefinition in virtualThermometerDefinitions)
		{
			if (!(virtualThermometerDefinition.Enabled ?? true))
			{
				continue;
			}

			SerialPortName portName = SerialPortName
				.OfString(SerialPortName.VIRTUAL_PREFIX + virtualThermometerDefinition.PortName);

			ulong serialNumberUInt64 = ulong.Parse(
					virtualThermometerDefinition.SerialNumber,
					style: System.Globalization.NumberStyles.HexNumber
				);
			VirtualThermometer virtualThermometer =
				new(
					portName,
					serialNumber: IThermometer.SerialNumber.OfUInt64(serialNumberUInt64)
				);

			registeredVirtualThermometers.Add(portName, virtualThermometer);
		}

		return registeredVirtualThermometers;
	}

	private static IEnumerable<VirtualThermometerDefinition> readVirtualThermometerDefinitions()
	{
		Pathname? virtualThermometerDefinitionsJsonFilePathname = VirtualThermometersManager.
			getVirtualThermometerDefinitionsJsonFilePathname();

		IEnumerable<VirtualThermometerDefinition>? virtualThermometerDefinitions = null;

		if ((virtualThermometerDefinitionsJsonFilePathname is not null) &&
			VirtualThermometersManager.exists(virtualThermometerDefinitionsJsonFilePathname.Value))
		{
			virtualThermometerDefinitions = VirtualThermometersManager
				.DeserializeJsonFile<IEnumerable<VirtualThermometerDefinition>>(
					virtualThermometerDefinitionsJsonFilePathname.Value
				);
		}

		return virtualThermometerDefinitions.OrEmpty();
	}

	[Pure]
	private static Pathname? getVirtualThermometerDefinitionsJsonFilePathname()
	{
		return ProgramDirs.GetConfigDirectoryPathname()
			?.JoinWith(Pathname.Component.OfString("virtual_devices.json"));
	}

	private static bool exists(Pathname pathname)
	{
		return File.Exists(pathname.ToString());
	}

	private static T? DeserializeJsonFile<T>(Pathname filePathname)
	{
		string fileContents = File.ReadAllText(filePathname.ToString());
		return JsonConvert.DeserializeObject<T>(fileContents);
	}
}
