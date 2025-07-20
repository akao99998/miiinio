using System.IO;

namespace Swrve
{
	public static class CrossPlatformFile
	{
		public static void Delete(string path)
		{
			File.Delete(path);
		}

		public static bool Exists(string path)
		{
			return File.Exists(path);
		}

		public static byte[] ReadAllBytes(string path)
		{
			byte[] array = null;
			using (FileStream fileStream = new FileStream(path, FileMode.Open))
			{
				array = new byte[fileStream.Length];
				using (BinaryReader binaryReader = new BinaryReader(fileStream))
				{
					binaryReader.Read(array, 0, (int)fileStream.Length);
					return array;
				}
			}
		}

		public static string LoadText(string path)
		{
			string text = null;
			using (FileStream stream = new FileStream(path, FileMode.Open))
			{
				using (StreamReader streamReader = new StreamReader(stream))
				{
					return streamReader.ReadToEnd();
				}
			}
		}

		public static void SaveBytes(string path, byte[] bytes)
		{
			using (FileStream output = File.Open(path, FileMode.Create))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(output))
				{
					binaryWriter.Write(bytes);
				}
			}
		}

		public static void SaveText(string path, string data)
		{
			using (FileStream stream = new FileStream(path, FileMode.Create))
			{
				using (StreamWriter streamWriter = new StreamWriter(stream))
				{
					streamWriter.Write(data);
				}
			}
		}
	}
}
