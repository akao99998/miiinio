using System;
using System.IO;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;

namespace Kampai.Util
{
	public static class DownloadUtil
	{
		public const int BUF_SIZE = 4096;

		public static string CreateBundleURL(string baseUrl, string name)
		{
			return string.Format("{0}/{1}.unity3d", baseUrl, name);
		}

		public static string CreateBundlePath(string baseDLCPath, string name)
		{
			return Path.Combine(baseDLCPath, string.Format("{0}.unity3d", name));
		}

		public static string GetBundleNameFromUrl(string url)
		{
			if (!url.EndsWith(".unity3d"))
			{
				return string.Empty;
			}
			int num = url.LastIndexOf('/') + 1;
			return url.Substring(num, url.Length - num - ".unity3d".Length);
		}

		public static bool IsGZipped(string filePath)
		{
			bool result = false;
			using (Stream stream = File.OpenRead(filePath))
			{
				byte[] array = new byte[2];
				if (stream.Length >= array.Length && stream.Read(array, 0, array.Length) != 0)
				{
					if (BitConverter.IsLittleEndian)
					{
						Array.Reverse(array);
					}
					result = BitConverter.ToInt16(array, 0) == 8075;
				}
			}
			return result;
		}

		public static string UnpackFile(string srcPath, string dstPath, string md5Sum = null, bool avoidBackup = false)
		{
			string text = null;
			try
			{
				bool flag = IsGZipped(srcPath);
				bool flag2 = string.IsNullOrEmpty(md5Sum);
				if (!flag2)
				{
					string text2 = null;
					using (MD5 mD = MD5.Create())
					{
						using (Stream stream = File.OpenRead(srcPath))
						{
							using (Stream stream2 = ((!flag) ? null : new GZipInputStream(stream)))
							{
								text2 = BitConverter.ToString(mD.ComputeHash(stream2 ?? stream)).Replace("-", string.Empty);
							}
						}
					}
					flag2 = md5Sum.Equals(text2, StringComparison.InvariantCultureIgnoreCase);
					if (!flag2)
					{
						text = string.Format("Invalid MD5SUM {0} != {1}", md5Sum, text2.ToLower());
					}
				}
				if (flag2)
				{
					string directoryName = Path.GetDirectoryName(dstPath);
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					else if (File.Exists(dstPath))
					{
						File.Delete(dstPath);
					}
					if (flag)
					{
						using (Stream baseInputStream = File.OpenRead(srcPath))
						{
							using (Stream source = new GZipInputStream(baseInputStream))
							{
								using (Stream destination = File.Create(dstPath))
								{
									StreamUtils.Copy(source, destination, new byte[4096]);
								}
							}
						}
					}
					else
					{
						File.Move(srcPath, dstPath);
					}
				}
				else if (string.IsNullOrEmpty(text))
				{
					text = "An unknown error occurred.";
				}
			}
			catch (SharpZipBaseException ex)
			{
				Native.LogError(string.Format("SharpZipBaseException Unpacking File {0}: {1}", srcPath, ex.Message));
				text = ex.Message;
			}
			return text;
		}
	}
}
