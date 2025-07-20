using System;
using Kampai.Common;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game.Transaction
{
	public class WeightedInstance : Instance<WeightedDefinition>
	{
		public int DeckIndex { get; set; }

		public int Seed { get; set; }

		public WeightedInstance(WeightedDefinition def)
			: base(def)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					Seed = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "DECKINDEX":
				reader.Read();
				DeckIndex = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}

		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected override void SerializeProperties(JsonWriter writer)
		{
			base.SerializeProperties(writer);
			writer.WritePropertyName("DeckIndex");
			writer.WriteValue(DeckIndex);
			writer.WritePropertyName("Seed");
			writer.WriteValue(Seed);
		}

		public virtual QuantityItem NextPick(IRandomService gameRandomService)
		{
			if (gameRandomService != null)
			{
				int num = Count();
				int num2 = DeckIndex;
				if (num2 < 1 || num2 > num)
				{
					Seed = gameRandomService.NextInt(int.MaxValue);
					int num4 = (DeckIndex = 1);
					num2 = num4;
				}
				IRandomService randomService = new RandomService(Seed);
				WeightedQuantityItem[] array = Shuffle(randomService, num);
				DeckIndex = num2 + 1;
				return array[num2 - 1];
			}
			return null;
		}

		private WeightedQuantityItem[] Shuffle(IRandomService randomService, int count)
		{
			DeckIndex = 1;
			WeightedQuantityItem[] array = new WeightedQuantityItem[count];
			int num = 0;
			foreach (WeightedQuantityItem entity in base.Definition.Entities)
			{
				uint weight = entity.Weight;
				for (uint num2 = 0u; num2 < weight; num2++)
				{
					array[num++] = entity;
				}
			}
			for (int i = 0; i < array.Length; i++)
			{
				int num3 = randomService.NextInt(i, array.Length);
				WeightedQuantityItem weightedQuantityItem = array[i];
				array[i] = array[num3];
				array[num3] = weightedQuantityItem;
			}
			return array;
		}

		protected int Count()
		{
			int num = 0;
			foreach (WeightedQuantityItem entity in base.Definition.Entities)
			{
				num += (int)entity.Weight;
			}
			return num;
		}
	}
}
