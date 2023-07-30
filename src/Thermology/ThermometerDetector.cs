/*
 * Copyright (c) 2023 Michael Federczuk
 *
 * SPDX-License-Identifier: MPL-2.0 AND Apache-2.0
 */

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using UsbtempServer.Utils;

namespace UsbtempServer.Thermology;

public static class ThermometerDetector
{
	public static IList<IThermometer> DetectThermometers()
	{
		IList<IThermometer> detectedThermometers = new List<IThermometer>();
		detectedThermometers.AddAll(ThermometerDetector.detectPhysicalThermometers());
		detectedThermometers.AddAll(VirtualThermometersManager.GetAllRegisteredVirtualThermometers());
		return detectedThermometers;
	}

	private static IEnumerable<PhysicalThermometer> detectPhysicalThermometers()
	{
		IList<PhysicalThermometer> detectedPhysicalThermometers = new List<PhysicalThermometer>();

		foreach (SerialPortName potentialPortName in ThermometerDetector.getPhysicalPortNames())
		{
			PhysicalThermometer? thermometer = ThermometerDetector.tryOpenPhysicalThermometer(potentialPortName);

			if (thermometer is null)
			{
				continue;
			}

			detectedPhysicalThermometers.Add(thermometer);
		}

		return detectedPhysicalThermometers;
	}

	private static IEnumerable<SerialPortName> getPhysicalPortNames()
	{
		return SerialPort.GetPortNames()
			.Select(SerialPortName.OfString);
	}

	private static PhysicalThermometer? tryOpenPhysicalThermometer(SerialPortName portName)
	{
		PhysicalThermometer thermometer;
		try
		{
			thermometer = PhysicalThermometer.OpenNew(portName);
			thermometer.GetSerialNumber();
		}
		catch (usbtemp.ThermometerException)
		{
			return null;
		}
		catch (TimeoutException)
		{
			return null;
		}
		return thermometer;
	}
}
