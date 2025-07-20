using System.Collections.Generic;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public abstract class FrolicCharacterDefinition : NamedCharacterDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1077;
			}
		}

		public string WanderStateMachine { get; set; }

		public WeightedDefinition WanderWeightedDeck { get; set; }

		public IList<LocationIncidentalAnimationDefinition> WanderAnimations { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, WanderStateMachine);
			BinarySerializationUtil.WriteObject(writer, WanderWeightedDeck);
			BinarySerializationUtil.WriteList(writer, WanderAnimations);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			WanderStateMachine = BinarySerializationUtil.ReadString(reader);
			WanderWeightedDeck = BinarySerializationUtil.ReadObject<WeightedDefinition>(reader);
			WanderAnimations = BinarySerializationUtil.ReadList(reader, WanderAnimations);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "WANDERSTATEMACHINE":
				reader.Read();
				WanderStateMachine = ReaderUtil.ReadString(reader, converters);
				break;
			case "WANDERWEIGHTEDDECK":
				reader.Read();
				WanderWeightedDeck = FastJSONDeserializer.Deserialize<WeightedDefinition>(reader, converters);
				break;
			case "WANDERANIMATIONS":
				reader.Read();
				WanderAnimations = ReaderUtil.PopulateList(reader, converters, WanderAnimations);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
