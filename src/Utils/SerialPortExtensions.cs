using System.IO.Ports;

namespace UsbtempServer.Utils;

public static class SerialPortExtensions
{
	public static void WriteByte(this SerialPort serialPort, byte b)
	{
		serialPort
			.Write(
				buffer: new byte[1] { b },
				offset: 0,
				count: 1
			);
	}
}
