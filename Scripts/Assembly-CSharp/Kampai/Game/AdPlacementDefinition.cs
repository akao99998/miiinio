using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Game.Trigger;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game
{
	[RequiresJsonConverter]
	public class AdPlacementDefinition : Definition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1019;
			}
		}

		public AdPlacementName Name { get; set; }

		public int CooldownSeconds { get; set; }

		public int CooldownWatchDeclineSeconds { get; set; }

		public int MaxRewardsPerDay { get; set; }

		public List<TriggerConditionDefinition> Conditions { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteEnum(writer, Name);
			writer.Write(CooldownSeconds);
			writer.Write(CooldownWatchDeclineSeconds);
			writer.Write(MaxRewardsPerDay);
			BinarySerializationUtil.WriteList(writer, Conditions);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Name = BinarySerializationUtil.ReadEnum<AdPlacementName>(reader);
			CooldownSeconds = reader.ReadInt32();
			CooldownWatchDeclineSeconds = reader.ReadInt32();
			MaxRewardsPerDay = reader.ReadInt32();
			Conditions = BinarySerializationUtil.ReadList(reader, Conditions);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "NAME":
				reader.Read();
				Name = ReaderUtil.ReadEnum<AdPlacementName>(reader);
				break;
			case "COOLDOWNSECONDS":
				reader.Read();
				CooldownSeconds = Convert.ToInt32(reader.Value);
				break;
			case "COOLDOWNWATCHDECLINESECONDS":
				reader.Read();
				CooldownWatchDeclineSeconds = Convert.ToInt32(reader.Value);
				break;
			case "MAXREWARDSPERDAY":
				reader.Read();
				MaxRewardsPerDay = Convert.ToInt32(reader.Value);
				break;
			case "CONDITIONS":
				reader.Read();
				Conditions = ReaderUtil.PopulateList(reader, converters, converters.triggerConditionDefinitionConverter, Conditions);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public virtual bool IsAvailable(ICrossContextCapable gameContext)
		{
			if (Conditions == null)
			{
				return true;
			}
			for (int i = 0; i < Conditions.Count; i++)
			{
				TriggerConditionDefinition triggerConditionDefinition = Conditions[i];
				if (triggerConditionDefinition == null || !triggerConditionDefinition.IsTriggered(gameContext))
				{
					return false;
				}
			}
			return true;
		}

		public virtual Instance Build()
		{
			return new AdPlacementInstance(this);
		}
	}
}
