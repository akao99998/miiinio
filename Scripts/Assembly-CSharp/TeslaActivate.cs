using System;
using System.Security.Cryptography;
using System.Text;

public static class TeslaActivate
{
	public class PKCSKeyGenerator
	{
		private byte[] key = new byte[8];

		private byte[] iv = new byte[8];

		public PKCSKeyGenerator()
		{
		}

		public PKCSKeyGenerator(string keystring, byte[] salt, int md5iterations, int segments, DESCryptoServiceProvider provider)
		{
			using (Generate(keystring, salt, md5iterations, segments, provider))
			{
			}
		}

		public byte[] GetKey()
		{
			return (byte[])key.Clone();
		}

		public byte[] GetIV()
		{
			return (byte[])iv.Clone();
		}

		public ICryptoTransform Encryptor(DESCryptoServiceProvider provider)
		{
			return provider.CreateEncryptor(key, iv);
		}

		public ICryptoTransform Generate(string keystring, byte[] salt, int md5iterations, int segments, DESCryptoServiceProvider provider)
		{
			int num = 16;
			byte[] array = new byte[num * segments];
			byte[] bytes = Encoding.UTF8.GetBytes(keystring);
			byte[] array2 = new byte[bytes.Length + salt.Length];
			Array.Copy(bytes, array2, bytes.Length);
			Array.Copy(salt, 0, array2, bytes.Length, salt.Length);
			MD5 mD = new MD5CryptoServiceProvider();
			byte[] array3 = null;
			byte[] array4 = new byte[num + array2.Length];
			for (int i = 0; i < segments; i++)
			{
				if (i == 0)
				{
					array3 = array2;
				}
				else
				{
					Array.Copy(array3, array4, array3.Length);
					Array.Copy(array2, 0, array4, array3.Length, array2.Length);
					array3 = array4;
				}
				for (int j = 0; j < md5iterations; j++)
				{
					array3 = mD.ComputeHash(array3);
				}
				Array.Copy(array3, 0, array, i * num, array3.Length);
			}
			Array.Copy(array, 0, key, 0, 8);
			Array.Copy(array, 8, iv, 0, 8);
			return Encryptor(provider);
		}
	}

	public static string Encrypt(string json, string tokenSecret)
	{
		using (DESCryptoServiceProvider provider = new DESCryptoServiceProvider())
		{
			PKCSKeyGenerator pKCSKeyGenerator = new PKCSKeyGenerator(tokenSecret, new byte[8] { 162, 21, 55, 8, 202, 98, 193, 210 }, 20, 1, provider);
			using (ICryptoTransform cryptoTransform = pKCSKeyGenerator.Encryptor(provider))
			{
				byte[] inArray = cryptoTransform.TransformFinalBlock(Encoding.UTF8.GetBytes(json), 0, json.Length);
				return Convert.ToBase64String(inArray);
			}
		}
	}
}
