using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class MasterPlanDefinition : Definition, IBuilder<Instance>
	{
		public override int TypeCode
		{
			get
			{
				return 1108;
			}
		}

		public string DescriptionKey { get; set; }

		public string Image { get; set; }

		public string Mask { get; set; }

		public string IntroDialog { get; set; }

		public int RewardTransactionID { get; set; }

		public int CooldownRewardTransactionID { get; set; }

		public int SubsequentCooldownRewardTransactionID { get; set; }

		public List<int> ComponentDefinitionIDs { get; set; }

		public List<int> CompBuildingDefinitionIDs { get; set; }

		public int BuildingDefID { get; set; }

		public int LeavebehindBuildingDefID { get; set; }

		public int BuildingCustomCameraPosID { get; set; }

		public int VillainCharacterDefID { get; set; }

		public int CooldownDuration { get; set; }

		public string CooldownRewardDialogKey { get; set; }

		public string RewardStateMachine { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, DescriptionKey);
			BinarySerializationUtil.WriteString(writer, Image);
			BinarySerializationUtil.WriteString(writer, Mask);
			BinarySerializationUtil.WriteString(writer, IntroDialog);
			writer.Write(RewardTransactionID);
			writer.Write(CooldownRewardTransactionID);
			writer.Write(SubsequentCooldownRewardTransactionID);
			BinarySerializationUtil.WriteListInt32(writer, ComponentDefinitionIDs);
			BinarySerializationUtil.WriteListInt32(writer, CompBuildingDefinitionIDs);
			writer.Write(BuildingDefID);
			writer.Write(LeavebehindBuildingDefID);
			writer.Write(BuildingCustomCameraPosID);
			writer.Write(VillainCharacterDefID);
			writer.Write(CooldownDuration);
			BinarySerializationUtil.WriteString(writer, CooldownRewardDialogKey);
			BinarySerializationUtil.WriteString(writer, RewardStateMachine);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			DescriptionKey = BinarySerializationUtil.ReadString(reader);
			Image = BinarySerializationUtil.ReadString(reader);
			Mask = BinarySerializationUtil.ReadString(reader);
			IntroDialog = BinarySerializationUtil.ReadString(reader);
			RewardTransactionID = reader.ReadInt32();
			CooldownRewardTransactionID = reader.ReadInt32();
			SubsequentCooldownRewardTransactionID = reader.ReadInt32();
			ComponentDefinitionIDs = BinarySerializationUtil.ReadListInt32(reader, ComponentDefinitionIDs);
			CompBuildingDefinitionIDs = BinarySerializationUtil.ReadListInt32(reader, CompBuildingDefinitionIDs);
			BuildingDefID = reader.ReadInt32();
			LeavebehindBuildingDefID = reader.ReadInt32();
			BuildingCustomCameraPosID = reader.ReadInt32();
			VillainCharacterDefID = reader.ReadInt32();
			CooldownDuration = reader.ReadInt32();
			CooldownRewardDialogKey = BinarySerializationUtil.ReadString(reader);
			RewardStateMachine = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "DESCRIPTIONKEY":
				reader.Read();
				DescriptionKey = ReaderUtil.ReadString(reader, converters);
				break;
			case "IMAGE":
				reader.Read();
				Image = ReaderUtil.ReadString(reader, converters);
				break;
			case "MASK":
				reader.Read();
				Mask = ReaderUtil.ReadString(reader, converters);
				break;
			case "INTRODIALOG":
				reader.Read();
				IntroDialog = ReaderUtil.ReadString(reader, converters);
				break;
			case "REWARDTRANSACTIONID":
				reader.Read();
				RewardTransactionID = Convert.ToInt32(reader.Value);
				break;
			case "COOLDOWNREWARDTRANSACTIONID":
				reader.Read();
				CooldownRewardTransactionID = Convert.ToInt32(reader.Value);
				break;
			case "SUBSEQUENTCOOLDOWNREWARDTRANSACTIONID":
				reader.Read();
				SubsequentCooldownRewardTransactionID = Convert.ToInt32(reader.Value);
				break;
			case "COMPONENTDEFINITIONIDS":
				reader.Read();
				ComponentDefinitionIDs = ReaderUtil.PopulateListInt32(reader, ComponentDefinitionIDs);
				break;
			case "COMPBUILDINGDEFINITIONIDS":
				reader.Read();
				CompBuildingDefinitionIDs = ReaderUtil.PopulateListInt32(reader, CompBuildingDefinitionIDs);
				break;
			case "BUILDINGDEFID":
				reader.Read();
				BuildingDefID = Convert.ToInt32(reader.Value);
				break;
			case "LEAVEBEHINDBUILDINGDEFID":
				reader.Read();
				LeavebehindBuildingDefID = Convert.ToInt32(reader.Value);
				break;
			case "BUILDINGCUSTOMCAMERAPOSID":
				reader.Read();
				BuildingCustomCameraPosID = Convert.ToInt32(reader.Value);
				break;
			case "VILLAINCHARACTERDEFID":
				reader.Read();
				VillainCharacterDefID = Convert.ToInt32(reader.Value);
				break;
			case "COOLDOWNDURATION":
				reader.Read();
				CooldownDuration = Convert.ToInt32(reader.Value);
				break;
			case "COOLDOWNREWARDDIALOGKEY":
				reader.Read();
				CooldownRewardDialogKey = ReaderUtil.ReadString(reader, converters);
				break;
			case "REWARDSTATEMACHINE":
				reader.Read();
				RewardStateMachine = ReaderUtil.ReadString(reader, converters);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}

		public Instance Build()
		{
			return new MasterPlan(this);
		}
	}
}
