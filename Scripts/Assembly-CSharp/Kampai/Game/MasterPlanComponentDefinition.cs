using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MasterPlanComponentDefinition : TaxonomyDefinition, IBuilder<Instance>
	{
		public List<MasterPlanComponentTaskDefinition> Tasks = new List<MasterPlanComponentTaskDefinition>();

		public override int TypeCode
		{
			get
			{
				return 1107;
			}
		}

		public string BenefitDescription { get; set; }

		public string OnClickSound { get; set; }

		public MasterPlanComponentRewardDefinition Reward { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, BenefitDescription);
			BinarySerializationUtil.WriteString(writer, OnClickSound);
			BinarySerializationUtil.WriteMasterPlanComponentRewardDefinition(writer, Reward);
			BinarySerializationUtil.WriteList(writer, BinarySerializationUtil.WriteMasterPlanComponentTaskDefinition, Tasks);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			BenefitDescription = BinarySerializationUtil.ReadString(reader);
			OnClickSound = BinarySerializationUtil.ReadString(reader);
			Reward = BinarySerializationUtil.ReadMasterPlanComponentRewardDefinition(reader);
			Tasks = BinarySerializationUtil.ReadList(reader, BinarySerializationUtil.ReadMasterPlanComponentTaskDefinition, Tasks);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "BENEFITDESCRIPTION":
				reader.Read();
				BenefitDescription = ReaderUtil.ReadString(reader, converters);
				break;
			case "ONCLICKSOUND":
				reader.Read();
				OnClickSound = ReaderUtil.ReadString(reader, converters);
				break;
			case "REWARD":
				reader.Read();
				Reward = ReaderUtil.ReadMasterPlanComponentRewardDefinition(reader, converters);
				break;
			case "TASKS":
				reader.Read();
				Tasks = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadMasterPlanComponentTaskDefinition, Tasks);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public virtual Instance Build()
		{
			return new MasterPlanComponent(this);
		}

		public bool AreTasksComplete(IPlayerService playerService)
		{
			MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(ID);
			if (firstInstanceByDefinitionId == null)
			{
				return false;
			}
			for (int i = 0; i < firstInstanceByDefinitionId.tasks.Count; i++)
			{
				if (!firstInstanceByDefinitionId.tasks[i].isComplete)
				{
					return false;
				}
			}
			return true;
		}
	}
}
