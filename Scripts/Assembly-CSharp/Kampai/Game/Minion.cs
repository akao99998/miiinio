using System;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class Minion : Character<MinionDefinition>, Prestigable, Taskable
	{
		public int BuildingID { get; set; }

		public MinionState State { get; set; }

		public int TaskDuration { get; set; }

		public int UTCTaskStartTime { get; set; }

		public int PartyTimeReduction { get; set; }

		public bool AlreadyRushed { get; set; }

		public int PrestigeId { get; set; }

		public bool IsInMinionParty { get; set; }

		public int Level { get; set; }

		[JsonIgnore]
		public bool IsDoingPartyFavorAnimation { get; set; }

		[JsonIgnore]
		public bool Partying { get; set; }

		[JsonIgnore]
		public bool IsInIncidental { get; set; }

		[JsonIgnore]
		public bool HasPrestige
		{
			get
			{
				return PrestigeId > 0;
			}
		}

		public Minion(MinionDefinition def)
			: base(def)
		{
			base.Definition = def;
			Partying = true;
			PrestigeId = -1;
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BUILDINGID":
				reader.Read();
				BuildingID = Convert.ToInt32(reader.Value);
				break;
			case "STATE":
				reader.Read();
				State = ReaderUtil.ReadEnum<MinionState>(reader);
				break;
			case "TASKDURATION":
				reader.Read();
				TaskDuration = Convert.ToInt32(reader.Value);
				break;
			case "UTCTASKSTARTTIME":
				reader.Read();
				UTCTaskStartTime = Convert.ToInt32(reader.Value);
				break;
			case "PARTYTIMEREDUCTION":
				reader.Read();
				PartyTimeReduction = Convert.ToInt32(reader.Value);
				break;
			case "ALREADYRUSHED":
				reader.Read();
				AlreadyRushed = Convert.ToBoolean(reader.Value);
				break;
			case "PRESTIGEID":
				reader.Read();
				PrestigeId = Convert.ToInt32(reader.Value);
				break;
			case "ISINMINIONPARTY":
				reader.Read();
				IsInMinionParty = Convert.ToBoolean(reader.Value);
				break;
			case "LEVEL":
				reader.Read();
				Level = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
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
			writer.WritePropertyName("BuildingID");
			writer.WriteValue(BuildingID);
			writer.WritePropertyName("State");
			writer.WriteValue((int)State);
			writer.WritePropertyName("TaskDuration");
			writer.WriteValue(TaskDuration);
			writer.WritePropertyName("UTCTaskStartTime");
			writer.WriteValue(UTCTaskStartTime);
			writer.WritePropertyName("PartyTimeReduction");
			writer.WriteValue(PartyTimeReduction);
			writer.WritePropertyName("AlreadyRushed");
			writer.WriteValue(AlreadyRushed);
			writer.WritePropertyName("PrestigeId");
			writer.WriteValue(PrestigeId);
			writer.WritePropertyName("IsInMinionParty");
			writer.WriteValue(IsInMinionParty);
			writer.WritePropertyName("Level");
			writer.WriteValue(Level);
		}

		public int GetCostumeId(IPlayerService playerService, IDefinitionService definitionService)
		{
			int result = 99;
			if (HasPrestige)
			{
				Prestige byInstanceId = playerService.GetByInstanceId<Prestige>(PrestigeId);
				if (byInstanceId != null && byInstanceId.Definition != null && byInstanceId.Definition.CostumeDefinitionID > 0)
				{
					result = byInstanceId.Definition.CostumeDefinitionID;
				}
			}
			else
			{
				MinionBenefitLevelBandDefintion minionBenefitLevelBandDefintion = definitionService.Get<MinionBenefitLevelBandDefintion>(StaticItem.MINION_BENEFITS_DEF_ID);
				MinionBenefitLevel minionBenefit = minionBenefitLevelBandDefintion.GetMinionBenefit(Level);
				if (minionBenefit != null)
				{
					result = minionBenefit.costumeId;
				}
			}
			return result;
		}
	}
}
