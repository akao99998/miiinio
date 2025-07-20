using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class TimedSocialEventDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1147;
			}
		}

		public int StartTime { get; set; }

		public int FinishTime { get; set; }

		public int MaxTeamSize { get; set; }

		public IList<SocialEventOrderDefinition> Orders { get; set; }

		public int RewardTransaction { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(StartTime);
			writer.Write(FinishTime);
			writer.Write(MaxTeamSize);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteSocialEventOrderDefinition, Orders);
			writer.Write(RewardTransaction);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			StartTime = reader.ReadInt32();
			FinishTime = reader.ReadInt32();
			MaxTeamSize = reader.ReadInt32();
			Orders = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadSocialEventOrderDefinition, Orders);
			RewardTransaction = reader.ReadInt32();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STARTTIME":
				reader.Read();
				StartTime = Convert.ToInt32(reader.Value);
				break;
			case "FINISHTIME":
				reader.Read();
				FinishTime = Convert.ToInt32(reader.Value);
				break;
			case "MAXTEAMSIZE":
				reader.Read();
				MaxTeamSize = Convert.ToInt32(reader.Value);
				break;
			case "ORDERS":
				reader.Read();
				Orders = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadSocialEventOrderDefinition, Orders);
				break;
			case "REWARDTRANSACTION":
				reader.Read();
				RewardTransaction = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public virtual TransactionDefinition GetReward(IDefinitionService definitionService)
		{
			return definitionService.Get<TransactionDefinition>(RewardTransaction);
		}
	}
}
