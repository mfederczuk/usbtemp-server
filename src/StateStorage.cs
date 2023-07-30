/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System.Diagnostics.Contracts;
using Newtonsoft.Json;
using UsbtempServer.Thermology;
using UsbtempServer.Utils;

namespace UsbtempServer;

public static class StateStorage
{
	private record PreviouslyUsedThermometerInfoJson(
		[property: JsonProperty("portName", Required = Required.Always)] string PortName,
		[property: JsonProperty("serialNumber", Required = Required.Always)] string SerialNumber
	);

	public record PreviouslyUsedThermometerInfo(
		SerialPortName PortName,
		IThermometer.SerialNumber SerialNumber
	);

	public static PreviouslyUsedThermometerInfo? GetPreviouslyUsedDeviceInfo()
	{
		Pathname? filePathname = StateStorage.getPreviouslyUsedThermometerJsonFilePathname();

		if ((filePathname is null) || !(FileSystem.Exists(filePathname.Value)))
		{
			return null;
		}

		string fileContents = FileSystem.ReadTextContents(filePathname.Value);
		PreviouslyUsedThermometerInfoJson? previouslyUsedThermometerInfoJson = JsonConvert
			.DeserializeObject<PreviouslyUsedThermometerInfoJson>(fileContents);

		if (previouslyUsedThermometerInfoJson is null)
		{
			return null;
		}

		ulong serialNumberUInt64 = ulong
			.Parse(
				previouslyUsedThermometerInfoJson.SerialNumber,
				style: System.Globalization.NumberStyles.HexNumber
			);

		return new PreviouslyUsedThermometerInfo(
			PortName: SerialPortName.OfString(previouslyUsedThermometerInfoJson.PortName),
			SerialNumber: IThermometer.SerialNumber.OfUInt64(serialNumberUInt64)
		);
	}

	public static void SetPreviouslyUsedThermometerInfo(PreviouslyUsedThermometerInfo previouslyUsedThermometerInfo)
	{
		Pathname? filePathname = StateStorage.getPreviouslyUsedThermometerJsonFilePathname();

		if (filePathname is null)
		{
			return;
		}

		PreviouslyUsedThermometerInfoJson previouslyUsedThermometerInfoJson =
			new(
				PortName: previouslyUsedThermometerInfo.PortName.ToString(),
				SerialNumber: previouslyUsedThermometerInfo.SerialNumber.ToString()
			);
		string previouslyUsedThermometerInfoJsonString = JsonConvert.SerializeObject(previouslyUsedThermometerInfoJson);

		FileSystem.CreateDirectories(filePathname.Value.Dirname());
		FileSystem.WriteTextContents(filePathname.Value, previouslyUsedThermometerInfoJsonString);
	}

	[Pure]
	private static Pathname? getPreviouslyUsedThermometerJsonFilePathname()
	{
		return ProgramDirs.GetStateDirectoryPathname()
			?.JoinWith(Pathname.Component.OfString("previously_used_device.json"));
	}
}
