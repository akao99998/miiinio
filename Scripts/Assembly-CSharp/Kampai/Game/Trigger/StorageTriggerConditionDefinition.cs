using System;
using System.IO;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class StorageTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1176;
			}
		}

		public int percent { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.Storage;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(percent);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			percent = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PERCENT":
				reader.Read();
				percent = Convert.ToInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IPlayerService instance = gameContext.injectionBinder.GetInstance<IPlayerService>();
			if (instance == null)
			{
				return false;
			}
			int currentStorageSpace = GetCurrentStorageSpace(instance);
			if (currentStorageSpace == -1)
			{
				return false;
			}
			return TestOperator(percent, currentStorageSpace);
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, percent: {3}", GetType(), base.conditionOp, type, percent);
		}

		private int GetCurrentStorageSpace(IPlayerService playerService)
		{
			StorageBuilding byInstanceId = playerService.GetByInstanceId<StorageBuilding>(314);
			if (byInstanceId == null)
			{
				return -1;
			}
			uint totalQuantity = 0u;
			playerService.GetStorableItems(out totalQuantity);
			uint storageCapacity = byInstanceId.Definition.StorageUpgrades[byInstanceId.CurrentStorageBuildingLevel].StorageCapacity;
			return (int)((float)totalQuantity / (float)storageCapacity * 100f);
		}
	}
}
