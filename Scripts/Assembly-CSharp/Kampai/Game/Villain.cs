using System;
using Kampai.Game.View;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class Villain : NamedCharacter<VillainDefinition>
	{
		private CabanaBuilding _deprecatedCabanaBuilding;

		public CabanaBuilding Cabana
		{
			get
			{
				if (_deprecatedCabanaBuilding != null)
				{
					Debug.LogError("Getter should only be used by PlayerVersion!");
					return _deprecatedCabanaBuilding;
				}
				return null;
			}
			set
			{
				_deprecatedCabanaBuilding = value;
			}
		}

		public int CabanaBuildingId { get; set; }

		public Villain(VillainDefinition def)
			: base(def)
		{
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
					CabanaBuildingId = Convert.ToInt32(reader.Value);
					break;
				}
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			case "CABANA":
				reader.Read();
				Cabana = (CabanaBuilding)converters.instanceConverter.ReadJson(reader, converters);
				break;
			}
			return true;
		}

		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected override void SerializeProperties(JsonWriter writer)
		{
			base.SerializeProperties(writer);
			if (Cabana != null)
			{
				writer.WritePropertyName("Cabana");
				Cabana.Serialize(writer);
			}
			writer.WritePropertyName("CabanaBuildingId");
			writer.WriteValue(CabanaBuildingId);
		}

		public override NamedCharacterObject Setup(GameObject go)
		{
			return go.AddComponent<VillainView>();
		}

		public override string ToString()
		{
			return string.Format("Villain(ID:{0}, LocalizedKey:{1})", ID, base.Definition.LocalizedKey);
		}
	}
}
