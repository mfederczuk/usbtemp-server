/*
 * SPDX-License-Identifier: CC0-1.0
 */

namespace UsbtempServer.Thermology;

public static class ThermometerFactory
{
	public static IThermometer OpenNew(SerialPortName portName)
	{
		return PhysicalThermometer.OpenNew(portName);
	}
}
