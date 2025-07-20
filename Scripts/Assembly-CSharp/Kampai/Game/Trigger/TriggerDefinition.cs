using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	[RequiresJsonConverter]
	public abstract class TriggerDefinition : Definition, IBuilder<TriggerInstance>, IDisplayableDefinition, IIsTriggerable, IComparable<TriggerDefinition>, IEquatable<TriggerDefinition>, IComparer<TriggerDefinition>
	{
		public override int TypeCode
		{
			get
			{
				return 1150;
			}
		}

		public string Title { get; set; }

		public string Description { get; set; }

		public string Image { get; set; }

		public string Mask { get; set; }

		public string WayFinderIcon { get; set; }

		public abstract TriggerDefinitionType.Identifier type { get; }

		public int priority { get; set; }

		public int cooldownSeconds { get; set; }

		public bool ForceOverride { get; set; }

		public bool TreasureIntro { get; set; }

		public IList<TriggerConditionDefinition> conditions { get; set; }

		public IList<int> reward { get; set; }

		[JsonIgnore]
		public IList<TriggerRewardDefinition> rewards { get; private set; }

		protected TriggerDefinition()
		{
			rewards = new List<TriggerRewardDefinition>();
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Title);
			BinarySerializationUtil.WriteString(writer, Description);
			BinarySerializationUtil.WriteString(writer, Image);
			BinarySerializationUtil.WriteString(writer, Mask);
			BinarySerializationUtil.WriteString(writer, WayFinderIcon);
			writer.Write(priority);
			writer.Write(cooldownSeconds);
			writer.Write(ForceOverride);
			writer.Write(TreasureIntro);
			BinarySerializationUtil.WriteList(writer, conditions);
			BinarySerializationUtil.WriteListInt32(writer, reward);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Title = BinarySerializationUtil.ReadString(reader);
			Description = BinarySerializationUtil.ReadString(reader);
			Image = BinarySerializationUtil.ReadString(reader);
			Mask = BinarySerializationUtil.ReadString(reader);
			WayFinderIcon = BinarySerializationUtil.ReadString(reader);
			priority = reader.ReadInt32();
			cooldownSeconds = reader.ReadInt32();
			ForceOverride = reader.ReadBoolean();
			TreasureIntro = reader.ReadBoolean();
			conditions = BinarySerializationUtil.ReadList(reader, conditions);
			reward = BinarySerializationUtil.ReadListInt32(reader, reward);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TITLE":
				reader.Read();
				Title = ReaderUtil.ReadString(reader, converters);
				break;
			case "DESCRIPTION":
				reader.Read();
				Description = ReaderUtil.ReadString(reader, converters);
				break;
			case "IMAGE":
				reader.Read();
				Image = ReaderUtil.ReadString(reader, converters);
				break;
			case "MASK":
				reader.Read();
				Mask = ReaderUtil.ReadString(reader, converters);
				break;
			case "WAYFINDERICON":
				reader.Read();
				WayFinderIcon = ReaderUtil.ReadString(reader, converters);
				break;
			case "PRIORITY":
				reader.Read();
				priority = Convert.ToInt32(reader.Value);
				break;
			case "COOLDOWNSECONDS":
				reader.Read();
				cooldownSeconds = Convert.ToInt32(reader.Value);
				break;
			case "FORCEOVERRIDE":
				reader.Read();
				ForceOverride = Convert.ToBoolean(reader.Value);
				break;
			case "TREASUREINTRO":
				reader.Read();
				TreasureIntro = Convert.ToBoolean(reader.Value);
				break;
			case "CONDITIONS":
				reader.Read();
				conditions = ReaderUtil.PopulateList(reader, converters, converters.triggerConditionDefinitionConverter, conditions);
				break;
			case "REWARD":
				reader.Read();
				reward = ReaderUtil.PopulateListInt32(reader, reward);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public abstract TriggerInstance Build();

		public virtual bool IsTriggered(ICrossContextCapable gameContext)
		{
			IPlayerService instance = gameContext.injectionBinder.GetInstance<IPlayerService>();
			if (instance == null || conditions == null || conditions.Count == 0)
			{
				return false;
			}
			TriggerInstance triggerByDefinitionId = instance.GetTriggerByDefinitionId(ID);
			if (triggerByDefinitionId != null && triggerByDefinitionId.StartGameTime != -1)
			{
				return false;
			}
			bool result = true;
			for (int i = 0; i < conditions.Count; i++)
			{
				TriggerConditionDefinition triggerConditionDefinition = conditions[i];
				if (triggerConditionDefinition == null || !triggerConditionDefinition.IsTriggered(gameContext))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public virtual void PrintTriggerConditions(ICrossContextCapable gameContext, StringBuilder outBuilder)
		{
			IPlayerService instance = gameContext.injectionBinder.GetInstance<IPlayerService>();
			if (instance == null || conditions == null || conditions.Count == 0)
			{
				return;
			}
			TriggerInstance triggerByDefinitionId = instance.GetTriggerByDefinitionId(ID);
			if (triggerByDefinitionId != null && triggerByDefinitionId.StartGameTime != -1)
			{
				return;
			}
			for (int i = 0; i < conditions.Count; i++)
			{
				TriggerConditionDefinition triggerConditionDefinition = conditions[i];
				if (triggerConditionDefinition != null)
				{
					outBuilder.AppendLine(triggerConditionDefinition.ToString() + " is triggered: " + triggerConditionDefinition.IsTriggered(gameContext));
				}
			}
		}

		public virtual int CompareTo(TriggerDefinition rhs)
		{
			if (rhs == null)
			{
				return 1;
			}
			int num = rhs.priority.CompareTo(priority);
			if (num != 0)
			{
				return num;
			}
			int num2 = type.CompareTo(rhs.type);
			if (num2 != 0)
			{
				return num2;
			}
			return ID.CompareTo(rhs.ID);
		}

		public int Compare(TriggerDefinition x, TriggerDefinition y)
		{
			if (x == null)
			{
				return -1;
			}
			return x.CompareTo(y);
		}

		public bool Equals(TriggerDefinition obj)
		{
			return obj != null && Equals((object)obj);
		}

		public override string ToString()
		{
			return string.Format("{0}, PRIORITY: {1}, COOLDOWN: {2}, Reward: {3}", base.ToString(), priority, cooldownSeconds, rewards);
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			TriggerDefinition triggerDefinition = obj as TriggerDefinition;
			return !object.ReferenceEquals(null, triggerDefinition) && CompareTo(triggerDefinition) == 0;
		}

		public override int GetHashCode()
		{
			return new { type, priority, ID }.GetHashCode();
		}
	}
}
