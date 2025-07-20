using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class PetsXPromoDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1189;
			}
		}

		public bool PetsXPromoEnabled { get; set; }

		public int PetsXPromoSurfaceLevel { get; set; }

		public string IOSPetsSmartURL { get; set; }

		public string AndroidPetsSmartURL { get; set; }

		public string IOSInstallURL { get; set; }

		public string AndroidInstallURL { get; set; }

		public string PetsImageEN_US { get; set; }

		public string PetsImageFR_FR { get; set; }

		public string PetsImageDE_DE { get; set; }

		public string PetsImageES_ES { get; set; }

		public string PetsImageES_PR { get; set; }

		public string PetsImageID { get; set; }

		public string PetsImagePT_BR { get; set; }

		public string PetsImageNL_NL { get; set; }

		public string PetsImageRU_RU { get; set; }

		public string PetsImageIT_IT { get; set; }

		public string PetsImageJA { get; set; }

		public string PetsImageKO_KR { get; set; }

		public string PetsImageTR { get; set; }

		public string PetsImageZH_CN { get; set; }

		public string PetsImageZH_TW { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(PetsXPromoEnabled);
			writer.Write(PetsXPromoSurfaceLevel);
			BinarySerializationUtil.WriteString(writer, IOSPetsSmartURL);
			BinarySerializationUtil.WriteString(writer, AndroidPetsSmartURL);
			BinarySerializationUtil.WriteString(writer, IOSInstallURL);
			BinarySerializationUtil.WriteString(writer, AndroidInstallURL);
			BinarySerializationUtil.WriteString(writer, PetsImageEN_US);
			BinarySerializationUtil.WriteString(writer, PetsImageFR_FR);
			BinarySerializationUtil.WriteString(writer, PetsImageDE_DE);
			BinarySerializationUtil.WriteString(writer, PetsImageES_ES);
			BinarySerializationUtil.WriteString(writer, PetsImageES_PR);
			BinarySerializationUtil.WriteString(writer, PetsImageID);
			BinarySerializationUtil.WriteString(writer, PetsImagePT_BR);
			BinarySerializationUtil.WriteString(writer, PetsImageNL_NL);
			BinarySerializationUtil.WriteString(writer, PetsImageRU_RU);
			BinarySerializationUtil.WriteString(writer, PetsImageIT_IT);
			BinarySerializationUtil.WriteString(writer, PetsImageJA);
			BinarySerializationUtil.WriteString(writer, PetsImageKO_KR);
			BinarySerializationUtil.WriteString(writer, PetsImageTR);
			BinarySerializationUtil.WriteString(writer, PetsImageZH_CN);
			BinarySerializationUtil.WriteString(writer, PetsImageZH_TW);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			PetsXPromoEnabled = reader.ReadBoolean();
			PetsXPromoSurfaceLevel = reader.ReadInt32();
			IOSPetsSmartURL = BinarySerializationUtil.ReadString(reader);
			AndroidPetsSmartURL = BinarySerializationUtil.ReadString(reader);
			IOSInstallURL = BinarySerializationUtil.ReadString(reader);
			AndroidInstallURL = BinarySerializationUtil.ReadString(reader);
			PetsImageEN_US = BinarySerializationUtil.ReadString(reader);
			PetsImageFR_FR = BinarySerializationUtil.ReadString(reader);
			PetsImageDE_DE = BinarySerializationUtil.ReadString(reader);
			PetsImageES_ES = BinarySerializationUtil.ReadString(reader);
			PetsImageES_PR = BinarySerializationUtil.ReadString(reader);
			PetsImageID = BinarySerializationUtil.ReadString(reader);
			PetsImagePT_BR = BinarySerializationUtil.ReadString(reader);
			PetsImageNL_NL = BinarySerializationUtil.ReadString(reader);
			PetsImageRU_RU = BinarySerializationUtil.ReadString(reader);
			PetsImageIT_IT = BinarySerializationUtil.ReadString(reader);
			PetsImageJA = BinarySerializationUtil.ReadString(reader);
			PetsImageKO_KR = BinarySerializationUtil.ReadString(reader);
			PetsImageTR = BinarySerializationUtil.ReadString(reader);
			PetsImageZH_CN = BinarySerializationUtil.ReadString(reader);
			PetsImageZH_TW = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PETSXPROMOENABLED":
				reader.Read();
				PetsXPromoEnabled = Convert.ToBoolean(reader.Value);
				break;
			case "PETSXPROMOSURFACELEVEL":
				reader.Read();
				PetsXPromoSurfaceLevel = Convert.ToInt32(reader.Value);
				break;
			case "IOSPETSSMARTURL":
				reader.Read();
				IOSPetsSmartURL = ReaderUtil.ReadString(reader, converters);
				break;
			case "ANDROIDPETSSMARTURL":
				reader.Read();
				AndroidPetsSmartURL = ReaderUtil.ReadString(reader, converters);
				break;
			case "IOSINSTALLURL":
				reader.Read();
				IOSInstallURL = ReaderUtil.ReadString(reader, converters);
				break;
			case "ANDROIDINSTALLURL":
				reader.Read();
				AndroidInstallURL = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEEN_US":
				reader.Read();
				PetsImageEN_US = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEFR_FR":
				reader.Read();
				PetsImageFR_FR = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEDE_DE":
				reader.Read();
				PetsImageDE_DE = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEES_ES":
				reader.Read();
				PetsImageES_ES = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEES_PR":
				reader.Read();
				PetsImageES_PR = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEID":
				reader.Read();
				PetsImageID = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEPT_BR":
				reader.Read();
				PetsImagePT_BR = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGENL_NL":
				reader.Read();
				PetsImageNL_NL = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGERU_RU":
				reader.Read();
				PetsImageRU_RU = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEIT_IT":
				reader.Read();
				PetsImageIT_IT = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEJA":
				reader.Read();
				PetsImageJA = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEKO_KR":
				reader.Read();
				PetsImageKO_KR = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGETR":
				reader.Read();
				PetsImageTR = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEZH_CN":
				reader.Read();
				PetsImageZH_CN = ReaderUtil.ReadString(reader, converters);
				break;
			case "PETSIMAGEZH_TW":
				reader.Read();
				PetsImageZH_TW = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
