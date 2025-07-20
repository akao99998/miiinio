using System;
using System.IO;
using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game.Trigger
{
	public class PartyPointsTriggerRewardDefinition : TriggerRewardDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1180;
			}
		}

		public uint Points { get; set; }

		public override TriggerRewardType.Identifier type
		{
			get
			{
				return TriggerRewardType.Identifier.PartyPoints;
			}
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(Points);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Points = reader.ReadUInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "POINTS":
				reader.Read();
				Points = Convert.ToUInt32(reader.Value);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override void RewardPlayer(ICrossContextCapable context)
		{
			ICrossContextInjectionBinder injectionBinder = context.injectionBinder;
			if (Points != 0)
			{
				AddPoints(injectionBinder, Points);
				return;
			}
			MinionParty minionPartyInstance = injectionBinder.GetInstance<IPlayerService>().GetMinionPartyInstance();
			if (minionPartyInstance != null && minionPartyInstance.CurrentPartyPointsRequired != 0)
			{
				AddPoints(injectionBinder, minionPartyInstance.CurrentPartyPointsRequired);
				return;
			}
			IKampaiLogger kampaiLogger = LogManager.GetClassLogger("PartyPointsTriggerRewardDefinition") as IKampaiLogger;
			kampaiLogger.Error("No party points found");
		}

		private void AddPoints(ICrossContextInjectionBinder binder, uint points)
		{
			binder.GetInstance<IPlayerService>().CreateAndRunCustomTransaction(2, (int)points, TransactionTarget.NO_VISUAL);
			binder.GetInstance<SetXPSignal>().Dispatch();
		}
	}
}
