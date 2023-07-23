/*
 * MIT License
 *
 * Copyright (c) 2018 usbtemp.com
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#nullable disable

// === DISCLAIMER ===
//
// This file was originally downloaded and copied from
//         <https://github.com/usbtemp/usbtemp-csharp/blob/master/usbtemp-demo/usbtemp.cs>
//         date 2023-07-22
//         commit c7c2a01026f8d30fe62c955068f11b226e96389f
//
// The following was modified:
//  * The license & copyright header comment was added
//  * The `#nullable disable` directive was added
//  * This disclaimer comment was added
//  * 4-space indentation was changed to tabs
//
// Everything else is a 1:1 copy.

using System;
using System.IO.Ports;

namespace usbtemp
{
	public class ThermometerException : Exception
	{
		public ThermometerException(string message) : base(message)
		{
		}
	}

	public class Thermometer
	{
		const int ok = 0;
		const int fail = -1;

		const byte generator = 0x8c;
		const int rom_size = 8;
		const int sp_size = 9;

		private SerialPort mySerialPort;

		protected byte lsb_crc8(byte[] buf, int buflen, byte generator)
		{
			int i, bit_counter;
			byte crc = 0;

			for (i = 0; i < buflen; i++)
			{
				crc ^= buf[i];
				bit_counter = 8;
				do
				{
					crc = (crc & 0x01) == 0x01 ? (byte)(((crc >> 1) & 0x7f) ^ generator) : (byte)((crc >> 1) & 0x7f);
					bit_counter--;
				} while (bit_counter > 0);
			}
			return crc;
		}

		protected int owReset()
		{
			int rv;
			byte wbuff;
			byte rbuff;
			byte[] buf;

			mySerialPort.BaudRate = 9600;
			mySerialPort.DataBits = 8;

			buf = new byte[1];

			wbuff = 0xf0;
			buf[0] = wbuff;
			mySerialPort.Write(buf, 0, 1);

			mySerialPort.Read(buf, 0, 1);
			rbuff = buf[0];
			rv = (rbuff == 0x00 || rbuff == 0xf0) ? fail : ok;

			mySerialPort.BaudRate = 115200;
			mySerialPort.DataBits = 6;

			return rv;
		}

		protected byte owWriteByte(byte wbuff)
		{
			int rbytes, remaining, i;
			byte[] buf;
			byte rbuff;

			buf = new byte[8];
			for (i = 0; i < 8; i++)
			{
				buf[i] = Convert.ToBoolean(wbuff & (1 << (i & 0x7))) ? (byte)0xff : (byte)0x00;
			}
			mySerialPort.Write(buf, 0, 8);

			rbuff = 0;
			remaining = 8;
			while (remaining > 0)
			{
				rbytes = mySerialPort.Read(buf, 0, remaining);
				for (i = 0; i < rbytes; i++)
				{
					rbuff >>= 1;
					rbuff |= (buf[i] & 0x01) == 0x01 ? (byte)0x80 : (byte)0x00;
					remaining--;
				}
			}
			return rbuff;
		}

		protected byte owRead()
		{
			return owWriteByte(0xff);
		}

		protected int owWrite(byte wbuff)
		{
			if (owWriteByte(wbuff) != wbuff)
			{
				return fail;
			}
			return ok;
		}

		public void Open(string name)
		{
			mySerialPort = new SerialPort(name);

			mySerialPort.BaudRate = 115200;
			mySerialPort.Parity = Parity.None;
			mySerialPort.StopBits = StopBits.One;
			mySerialPort.DataBits = 6;
			mySerialPort.Handshake = Handshake.None;
			mySerialPort.RtsEnable = false;

			mySerialPort.ReadTimeout = 500;
			mySerialPort.WriteTimeout = 500;

			mySerialPort.Open();
		}

		public void Close()
		{
			mySerialPort.Close();
		}

		public byte[] Rom()
		{
			byte crc;
			byte[] rom;
			int i;

			if (owReset() == fail || owWrite(0x33) == fail)
			{
				throw new ThermometerException("Cannot read ROM!");
			}

			rom = new byte[rom_size];
			for (i = 0; i < rom_size; i++)
			{
				rom[i] = owRead();
			}

			crc = lsb_crc8(rom, rom_size - 1, generator);
			if (rom[rom_size - 1] != crc)
			{
				throw new ThermometerException("CRC Error!");
			}

			return rom;
		}

		public float Temperature()
		{
			byte crc;
			byte[] sp_sensor;
			short T;
			int i;

			if (owReset() == fail || owWrite(0xcc) == fail || owWrite(0x44) == fail)
			{
				throw new ThermometerException("Cannot start the conversion process!");
			}

			System.Threading.Thread.Sleep(800);

			if (owReset() == fail || owWrite(0xcc) == fail || owWrite(0xbe) == fail)
			{
				throw new ThermometerException("Cannot read the temperature!");
			}

			sp_sensor = new byte[sp_size];
			for (i = 0; i < sp_size; i++)
			{
				sp_sensor[i] = owRead();
			}

			if ((sp_sensor[4] & 0x9f) != 0x1f)
			{
				throw new ThermometerException("Invalid device!");
			}

			crc = lsb_crc8(sp_sensor, sp_size - 1, generator);
			if (sp_sensor[sp_size - 1] != crc)
			{
				throw new ThermometerException("CRC Error!");
			}

			T = BitConverter.ToInt16(sp_sensor, 0);
			return (float)T / 16;
		}
	}
}
