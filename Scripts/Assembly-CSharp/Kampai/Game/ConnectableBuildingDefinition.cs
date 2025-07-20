using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class ConnectableBuildingDefinition : DecorationBuildingDefinition
	{
		private const int NumPrefabs = 7;

		public override int TypeCode
		{
			get
			{
				return 1043;
			}
		}

		public int connectableType { get; set; }

		public ConnectablePiecePrefabDefinition piecePrefabs { get; set; }

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(connectableType);
			BinarySerializationUtil.WriteConnectablePiecePrefabDefinition(writer, piecePrefabs);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			connectableType = reader.ReadInt32();
			piecePrefabs = BinarySerializationUtil.ReadConnectablePiecePrefabDefinition(reader);
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
					piecePrefabs = ReaderUtil.ReadConnectablePiecePrefabDefinition(reader, converters);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "CONNECTABLETYPE":
				reader.Read();
				connectableType = Convert.ToInt32(reader.Value);
				break;
			}
			return true;
		}

		public override Building BuildBuilding()
		{
			return new ConnectableBuilding(this);
		}

		public int GetNumPrefabs()
		{
			return 7;
		}

		public override string GetPrefab(int index = 0)
		{
			switch (index)
			{
			case 0:
				return piecePrefabs.straight;
			case 1:
				return piecePrefabs.cross;
			case 2:
				return piecePrefabs.post;
			case 3:
				return piecePrefabs.tshape;
			case 4:
				return piecePrefabs.endcap;
			case 5:
				return piecePrefabs.corner;
			default:
				return piecePrefabs.straight;
			}
		}

		public int GetDefaultPrefabIndex()
		{
			return 2;
		}
	}
}
