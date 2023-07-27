using System;
using System.IO;
using System.IO.Ports;
using UsbtempServer.Utils;

namespace UsbtempServer.Thermology;

public static class OneWire
{
	public enum ResultStatus
	{
		Success,
		Failure,
	}

	private const int RESET_BAUD_RATE = 9600;
	private const int RESET_DATA_BITS = 8;

	public static ResultStatus WriteReset(SerialPort serialPort)
	{
		int originalBaudRate = serialPort.BaudRate;
		int originalDataBits = serialPort.DataBits;

		serialPort.BaudRate = RESET_BAUD_RATE;
		serialPort.DataBits = RESET_DATA_BITS;

		try
		{
			serialPort.WriteByte(0xF0); // some magic number ¯\_(ツ)_/¯

			int ret = serialPort.ReadByte();

			if ((ret == 0x00) || (ret == 0xF0)) // some magic numbers ¯\_(ツ)_/¯
			{
				return ResultStatus.Failure;
			}

			return ResultStatus.Success;
		}
		finally
		{
			serialPort.DataBits = originalDataBits;
			serialPort.BaudRate = originalBaudRate;
		}
	}

	public static byte WriteByte(Stream serialPort, byte wbuff)
	{
		byte[] buf = new byte[8];
		for (int i = 0; i < 8; i++)
		{
			buf[i] = Convert.ToBoolean(wbuff & (1 << (i & 0x7))) ? (byte)0xFF : (byte)0x00;
		}
		serialPort.Write(buf, 0, 8);

		byte rbuff = 0;
		int remaining = 8;
		while (remaining > 0)
		{
			int rbytes = serialPort.Read(buf, 0, remaining);
			for (int i = 0; i < rbytes; i++)
			{
				rbuff >>= 1;
				rbuff |= (buf[i] & 0x01) == 0x01 ? (byte)0x80 : (byte)0x00;
				remaining--;
			}
		}
		return rbuff;
	}

	public static byte WriteByte(SerialPort serialPort, byte wbuff)
	{
		return WriteByte(serialPort.BaseStream, wbuff);
	}
}
