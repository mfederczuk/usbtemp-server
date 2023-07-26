/*
 * SPDX-License-Identifier: CC0-1.0
 */

using System;

namespace UsbtempServer.Thermology;

public interface IThermometer : IDisposable
{
	public string ReadSerialNumber();
	public Temperature ReadTemperature();
}
