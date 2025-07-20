using System.Collections.Generic;
using System.IO;
using Kampai.Main;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class CountryTriggerConditionDefinition : TriggerConditionDefinition
	{
		public List<string> Countries;

		public override int TypeCode
		{
			get
			{
				return 1155;
			}
		}

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.Country;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteString, Countries);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Countries = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadString, Countries);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "COUNTRIES":
				reader.Read();
				Countries = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadString, Countries);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			string country = gameContext.injectionBinder.GetInstance<ILocalizationService>().GetCountry();
			return ListUtil.StringIsInList(country, Countries);
		}
	}
}
