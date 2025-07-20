using System;
using System.IO;
using Elevation.Logging;
using Kampai.Game.Mignette;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class MignetteScoreTriggerRewardDefinition : TriggerRewardDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1179;
			}
		}

		public int MignetteBuildingId { get; set; }

		public int Points { get; set; }

		public override TriggerRewardType.Identifier type
		{
			get
			{
				return TriggerRewardType.Identifier.MignetteScore;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(MignetteBuildingId);
			writer.Write(Points);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			MignetteBuildingId = reader.ReadInt32();
			Points = reader.ReadInt32();
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
					Points = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "MIGNETTEBUILDINGID":
				reader.Read();
				MignetteBuildingId = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}

		public override void RewardPlayer(ICrossContextCapable context)
		{
			ICrossContextInjectionBinder injectionBinder = context.injectionBinder;
			MignetteBuilding firstInstanceByDefinitionId = injectionBinder.GetInstance<IPlayerService>().GetFirstInstanceByDefinitionId<MignetteBuilding>(MignetteBuildingId);
			if (firstInstanceByDefinitionId != null)
			{
				MignetteGameModel instance = injectionBinder.GetInstance<MignetteGameModel>();
				instance.BuildingId = firstInstanceByDefinitionId.ID;
				instance.CurrentGameScore = Points;
				injectionBinder.GetInstance<ShowAndIncreaseMignetteScoreSignal>().Dispatch();
			}
			else
			{
				IKampaiLogger kampaiLogger = LogManager.GetClassLogger("MignetteScoreTriggerRewardDefinition") as IKampaiLogger;
				kampaiLogger.Error("Cannot find mignette building {0}", MignetteBuildingId);
			}
		}
	}
}
