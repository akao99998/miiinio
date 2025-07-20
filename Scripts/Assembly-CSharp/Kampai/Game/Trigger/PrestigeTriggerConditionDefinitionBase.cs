using System;
using System.IO;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public abstract class PrestigeTriggerConditionDefinitionBase : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1164;
			}
		}

		public int prestigeDefinitionID { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(prestigeDefinitionID);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			prestigeDefinitionID = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PRESTIGEDEFINITIONID":
				reader.Read();
				prestigeDefinitionID = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IPrestigeService instance = gameContext.injectionBinder.GetInstance<IPrestigeService>();
			if (instance == null)
			{
				return false;
			}
			Prestige prestige = instance.GetPrestige(prestigeDefinitionID, false);
			return IsTriggered(instance, prestige);
		}

		protected abstract bool IsTriggered(IPrestigeService prestigeService, Prestige prestigeCharacter);
	}
}
