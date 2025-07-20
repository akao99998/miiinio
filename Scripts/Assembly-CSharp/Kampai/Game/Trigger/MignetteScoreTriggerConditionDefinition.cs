using System;
using System.IO;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class MignetteScoreTriggerConditionDefinition : TriggerConditionDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1160;
			}
		}

		public int score { get; set; }

		public int mignetteBuildingId { get; set; }

		public bool useTotalMignetteScore { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.MignetteScore;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(score);
			writer.Write(mignetteBuildingId);
			writer.Write(useTotalMignetteScore);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			score = reader.ReadInt32();
			mignetteBuildingId = reader.ReadInt32();
			useTotalMignetteScore = reader.ReadBoolean();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SCORE":
				reader.Read();
				score = Convert.ToInt32(reader.Value);
				break;
			case "MIGNETTEBUILDINGID":
				reader.Read();
				mignetteBuildingId = Convert.ToInt32(reader.Value);
				break;
			case "USETOTALMIGNETTESCORE":
				reader.Read();
				useTotalMignetteScore = Convert.ToBoolean(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, score: {3}, mignetteBuildingId: {4}", GetType(), base.conditionOp, type, score, mignetteBuildingId);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			int actualValue = 0;
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			if (useTotalMignetteScore)
			{
				IPlayerService instance = injectionBinder.GetInstance<IPlayerService>();
				MignetteBuilding byInstanceId = instance.GetByInstanceId<MignetteBuilding>(mignetteBuildingId);
				if (byInstanceId != null)
				{
					actualValue = byInstanceId.TotalScore;
				}
				else
				{
					IKampaiLogger kampaiLogger = LogManager.GetClassLogger("MignetteScoreTriggerConditionDefinition") as IKampaiLogger;
					if (kampaiLogger != null)
					{
						kampaiLogger.Error("Mignette {0} is not found, 0 score will be used for the triigger", mignetteBuildingId);
					}
				}
			}
			else
			{
				MignetteCollectionService instance2 = injectionBinder.GetInstance<MignetteCollectionService>();
				RewardCollection activeCollectionForMignette = instance2.GetActiveCollectionForMignette(mignetteBuildingId);
				actualValue = activeCollectionForMignette.CollectionScoreProgress;
			}
			return TestOperator(score, actualValue);
		}
	}
}
