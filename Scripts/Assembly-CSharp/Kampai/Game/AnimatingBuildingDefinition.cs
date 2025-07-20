using System;
using System.Collections.Generic;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public abstract class AnimatingBuildingDefinition : RepairableBuildingDefinition
	{
		public int GagFrequency;

		public override int TypeCode
		{
			get
			{
				return 1030;
			}
		}

		public IList<BuildingAnimationDefinition> AnimationDefinitions { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteList(writer, AnimationDefinitions);
			writer.Write(GagFrequency);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			AnimationDefinitions = BinarySerializationUtil.ReadList(reader, AnimationDefinitions);
			GagFrequency = reader.ReadInt32();
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
					GagFrequency = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "ANIMATIONDEFINITIONS":
				reader.Read();
				AnimationDefinitions = ReaderUtil.PopulateList(reader, converters, AnimationDefinitions);
				break;
			}
			return true;
		}

		public virtual IList<string> AnimationControllerKeys()
		{
			IList<string> list = new List<string>();
			if (AnimationDefinitions != null && AnimationDefinitions.Count > 0)
			{
				foreach (BuildingAnimationDefinition animationDefinition in AnimationDefinitions)
				{
					if (!string.IsNullOrEmpty(animationDefinition.BuildingController))
					{
						list.Add(animationDefinition.BuildingController);
					}
					if (!string.IsNullOrEmpty(animationDefinition.MinionController))
					{
						list.Add(animationDefinition.MinionController);
					}
				}
			}
			return list;
		}
	}
}
