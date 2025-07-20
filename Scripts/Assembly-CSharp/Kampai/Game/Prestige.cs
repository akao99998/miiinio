using System;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class Prestige : Instance<PrestigeDefinition>, IGameTimeTracker, ItemAccumulator
	{
		public int trackedInstanceId { get; set; }

		public PrestigeState state { get; set; }

		public int CurrentPrestigeLevel { get; set; }

		public int CurrentPrestigePoints { get; set; }

		public int CurrentOrdersCompleted { get; set; }

		public int UTCTimeUnlocked { get; set; }

		public bool onCooldown { get; set; }

		public int numPartiesInvited { get; set; }

		public int numPartiesThrown { get; set; }

		public int StartGameTime { get; set; }

		[JsonIgnore]
		public int NeededPrestigePoints
		{
			get
			{
				return (int)GetCurrentPrestigeLevelDefinition().PointsNeeded;
			}
		}

		[JsonIgnore]
		public string CurrentWelcomeMessage
		{
			get
			{
				return GetCurrentPrestigeLevelDefinition().WelcomePanelMessageLocalizedKey;
			}
		}

		[JsonIgnore]
		public string CurrentFarewellMessage
		{
			get
			{
				return GetCurrentPrestigeLevelDefinition().FarewellPanelMessageLocalizedKey;
			}
		}

		public Prestige(PrestigeDefinition def)
			: base(def)
		{
			CurrentPrestigeLevel = -2;
			CurrentPrestigePoints = 0;
			trackedInstanceId = 0;
			state = PrestigeState.Locked;
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "TRACKEDINSTANCEID":
				reader.Read();
				trackedInstanceId = Convert.ToInt32(reader.Value);
				break;
			case "STATE":
				reader.Read();
				state = ReaderUtil.ReadEnum<PrestigeState>(reader);
				break;
			case "CURRENTPRESTIGELEVEL":
				reader.Read();
				CurrentPrestigeLevel = Convert.ToInt32(reader.Value);
				break;
			case "CURRENTPRESTIGEPOINTS":
				reader.Read();
				CurrentPrestigePoints = Convert.ToInt32(reader.Value);
				break;
			case "CURRENTORDERSCOMPLETED":
				reader.Read();
				CurrentOrdersCompleted = Convert.ToInt32(reader.Value);
				break;
			case "UTCTIMEUNLOCKED":
				reader.Read();
				UTCTimeUnlocked = Convert.ToInt32(reader.Value);
				break;
			case "ONCOOLDOWN":
				reader.Read();
				onCooldown = Convert.ToBoolean(reader.Value);
				break;
			case "NUMPARTIESINVITED":
				reader.Read();
				numPartiesInvited = Convert.ToInt32(reader.Value);
				break;
			case "NUMPARTIESTHROWN":
				reader.Read();
				numPartiesThrown = Convert.ToInt32(reader.Value);
				break;
			case "STARTGAMETIME":
				reader.Read();
				StartGameTime = Convert.ToInt32(reader.Value);
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
			writer.WritePropertyName("trackedInstanceId");
			writer.WriteValue(trackedInstanceId);
			writer.WritePropertyName("state");
			writer.WriteValue((int)state);
			writer.WritePropertyName("CurrentPrestigeLevel");
			writer.WriteValue(CurrentPrestigeLevel);
			writer.WritePropertyName("CurrentPrestigePoints");
			writer.WriteValue(CurrentPrestigePoints);
			writer.WritePropertyName("CurrentOrdersCompleted");
			writer.WriteValue(CurrentOrdersCompleted);
			writer.WritePropertyName("UTCTimeUnlocked");
			writer.WriteValue(UTCTimeUnlocked);
			writer.WritePropertyName("onCooldown");
			writer.WriteValue(onCooldown);
			writer.WritePropertyName("numPartiesInvited");
			writer.WriteValue(numPartiesInvited);
			writer.WritePropertyName("numPartiesThrown");
			writer.WriteValue(numPartiesThrown);
			writer.WritePropertyName("StartGameTime");
			writer.WriteValue(StartGameTime);
		}

		public void AwardOutput(QuantityItem item)
		{
			if (item.ID == 2)
			{
				CurrentPrestigePoints += (int)item.Quantity;
			}
		}

		public override string ToString()
		{
			return string.Format("{0}(ID:{1}, State:{2}, Definition:{3})", typeof(Prestige).FullName, ID, state, base.Definition);
		}

		private CharacterPrestigeLevelDefinition GetCurrentPrestigeLevelDefinition()
		{
			if (base.Definition.PrestigeLevelSettings == null)
			{
				return new CharacterPrestigeLevelDefinition();
			}
			CurrentPrestigeLevel = ((CurrentPrestigeLevel < base.Definition.PrestigeLevelSettings.Count) ? CurrentPrestigeLevel : (base.Definition.PrestigeLevelSettings.Count - 1));
			return base.Definition.PrestigeLevelSettings[(CurrentPrestigeLevel >= 1) ? CurrentPrestigeLevel : 0];
		}
	}
}
