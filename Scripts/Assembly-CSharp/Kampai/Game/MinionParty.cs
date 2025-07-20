using System;
using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MinionParty : Instance<MinionPartyDefinition>
	{
		public int BuffStartTime { get; set; }

		public int NewBuffStartTime { get; set; }

		public int PartyStartTier { get; set; }

		public MinionPartyType PartyType { get; set; }

		public bool IsPartyHappening { get; set; }

		public bool IsBuffHappening { get; set; }

		public int CurrentPartyIndex { get; set; }

		public int TotalLevelPartiesCount { get; set; }

		public uint CurrentPartyPoints { get; set; }

		public uint CurrentPartyPointsRequired { get; set; }

		public List<int> lastGuestsOfHonorPrestigeIDs { get; set; }

		[JsonIgnore]
		public bool CharacterUnlocking { get; set; }

		[JsonIgnore]
		public bool PartyPreSkip { get; set; }

		public bool IsPartyReady
		{
			get
			{
				return CurrentPartyPointsRequired != 0 && CurrentPartyPoints >= CurrentPartyPointsRequired;
			}
		}

		public MinionParty(MinionPartyDefinition definition)
		{
			lastGuestsOfHonorPrestigeIDs = new List<int>();
			base.Definition = definition;
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BUFFSTARTTIME":
				reader.Read();
				BuffStartTime = Convert.ToInt32(reader.Value);
				break;
			case "NEWBUFFSTARTTIME":
				reader.Read();
				NewBuffStartTime = Convert.ToInt32(reader.Value);
				break;
			case "PARTYSTARTTIER":
				reader.Read();
				PartyStartTier = Convert.ToInt32(reader.Value);
				break;
			case "PARTYTYPE":
				reader.Read();
				PartyType = ReaderUtil.ReadEnum<MinionPartyType>(reader);
				break;
			case "ISPARTYHAPPENING":
				reader.Read();
				IsPartyHappening = Convert.ToBoolean(reader.Value);
				break;
			case "ISBUFFHAPPENING":
				reader.Read();
				IsBuffHappening = Convert.ToBoolean(reader.Value);
				break;
			case "CURRENTPARTYINDEX":
				reader.Read();
				CurrentPartyIndex = Convert.ToInt32(reader.Value);
				break;
			case "TOTALLEVELPARTIESCOUNT":
				reader.Read();
				TotalLevelPartiesCount = Convert.ToInt32(reader.Value);
				break;
			case "CURRENTPARTYPOINTS":
				reader.Read();
				CurrentPartyPoints = Convert.ToUInt32(reader.Value);
				break;
			case "CURRENTPARTYPOINTSREQUIRED":
				reader.Read();
				CurrentPartyPointsRequired = Convert.ToUInt32(reader.Value);
				break;
			case "LASTGUESTSOFHONORPRESTIGEIDS":
				reader.Read();
				lastGuestsOfHonorPrestigeIDs = ReaderUtil.PopulateListInt32(reader, lastGuestsOfHonorPrestigeIDs);
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
			writer.WritePropertyName("BuffStartTime");
			writer.WriteValue(BuffStartTime);
			writer.WritePropertyName("NewBuffStartTime");
			writer.WriteValue(NewBuffStartTime);
			writer.WritePropertyName("PartyStartTier");
			writer.WriteValue(PartyStartTier);
			writer.WritePropertyName("PartyType");
			writer.WriteValue((int)PartyType);
			writer.WritePropertyName("IsPartyHappening");
			writer.WriteValue(IsPartyHappening);
			writer.WritePropertyName("IsBuffHappening");
			writer.WriteValue(IsBuffHappening);
			writer.WritePropertyName("CurrentPartyIndex");
			writer.WriteValue(CurrentPartyIndex);
			writer.WritePropertyName("TotalLevelPartiesCount");
			writer.WriteValue(TotalLevelPartiesCount);
			writer.WritePropertyName("CurrentPartyPoints");
			writer.WriteValue(CurrentPartyPoints);
			writer.WritePropertyName("CurrentPartyPointsRequired");
			writer.WriteValue(CurrentPartyPointsRequired);
			if (lastGuestsOfHonorPrestigeIDs == null)
			{
				return;
			}
			writer.WritePropertyName("lastGuestsOfHonorPrestigeIDs");
			writer.WriteStartArray();
			List<int>.Enumerator enumerator = lastGuestsOfHonorPrestigeIDs.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					writer.WriteValue(current);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
			writer.WriteEndArray();
		}

		public void ResolveBuffStartTime()
		{
			BuffStartTime = ((NewBuffStartTime <= BuffStartTime) ? BuffStartTime : NewBuffStartTime);
		}

		public int DeterminePartyTier(uint currentLevel)
		{
			int result = 0;
			for (int i = 0; i < base.Definition.partyMeterDefinition.Tiers.Count && currentLevel >= base.Definition.partyMeterDefinition.Tiers[i].Level; i++)
			{
				result = i;
			}
			return result;
		}
	}
}
