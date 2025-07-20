using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Elevation.Logging;
using Kampai.Util;

public class EncryptionService : IEncryptionService
{
	public IKampaiLogger logger = LogManager.GetClassLogger("EncryptionService") as IKampaiLogger;

	private int Iterations = 2;

	private int KeySize = 256;

	private string Salt = "s499bgcalptrefxe";

	private string Vector = "087gbfgx3278kmnu";

	public string Encrypt(string plainText, string password)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(Vector);
		byte[] bytes2 = Encoding.ASCII.GetBytes(Salt);
		byte[] bytes3 = Encoding.UTF8.GetBytes(plainText);
		byte[] inArray;
		using (SymmetricAlgorithm symmetricAlgorithm = new AesManaged())
		{
			Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, bytes2, Iterations);
			byte[] bytes4 = rfc2898DeriveBytes.GetBytes(KeySize / 8);
			symmetricAlgorithm.Mode = CipherMode.CBC;
			using (ICryptoTransform transform = symmetricAlgorithm.CreateEncryptor(bytes4, bytes))
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
					{
						cryptoStream.Write(bytes3, 0, bytes3.Length);
						cryptoStream.FlushFinalBlock();
						inArray = memoryStream.ToArray();
					}
				}
			}
			symmetricAlgorithm.Clear();
		}
		return Convert.ToBase64String(inArray);
	}

	public bool TryDecrypt(string cipherText, string password, out string plainText)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(Vector);
		byte[] bytes2 = Encoding.ASCII.GetBytes(Salt);
		byte[] array = Convert.FromBase64String(cipherText);
		int count = 0;
		byte[] array2;
		using (SymmetricAlgorithm symmetricAlgorithm = new AesManaged())
		{
			Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, bytes2, Iterations);
			byte[] bytes3 = rfc2898DeriveBytes.GetBytes(KeySize / 8);
			symmetricAlgorithm.Mode = CipherMode.CBC;
			try
			{
				using (ICryptoTransform transform = symmetricAlgorithm.CreateDecryptor(bytes3, bytes))
				{
					using (MemoryStream stream = new MemoryStream(array))
					{
						using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
						{
							array2 = new byte[array.Length];
							count = cryptoStream.Read(array2, 0, array2.Length);
						}
					}
				}
			}
			catch (CryptographicException ex)
			{
				logger.Error("failed to decrypt data " + ex.Message);
				plainText = string.Empty;
				return false;
			}
			symmetricAlgorithm.Clear();
		}
		plainText = Encoding.UTF8.GetString(array2, 0, count);
		return true;
	}
}
