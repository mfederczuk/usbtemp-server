/*
 * SPDX-License-Identifier: CC0-1.0
 */

namespace UsbtempServer.Utils;

public static class FileSystem
{
	public static bool Exists(Pathname pathname)
	{
		return (System.IO.File.Exists(pathname.ToString()) || System.IO.Directory.Exists(pathname.ToString()));
	}

	public static string ReadTextContents(Pathname filePathname)
	{
		return System.IO.File.ReadAllText(filePathname.ToString());
	}

	public static void WriteTextContents(Pathname filePathname, string contents)
	{
		System.IO.File.WriteAllText(filePathname.ToString(), contents);
	}

	public static void CreateDirectories(Pathname directoryPathname)
	{
		System.IO.Directory.CreateDirectory(directoryPathname.ToString());
	}
}
