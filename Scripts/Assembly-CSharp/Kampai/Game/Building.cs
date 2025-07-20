using System;
using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public abstract class Building<T> : Instance<T>, Building, Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable where T : BuildingDefinition
	{
		BuildingDefinition Building.Definition
		{
			get
			{
				return base.Definition;
			}
		}

		public BuildingState State { get; set; }

		public Location Location { get; set; }

		public int BuildingNumber { get; set; }

		public bool IsFootprintable
		{
			get
			{
				return State != BuildingState.Disabled;
			}
		}

		public int StateStartTime { get; set; }

		protected Building(T definition)
			: base(definition)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "STATE":
				reader.Read();
				State = ReaderUtil.ReadEnum<BuildingState>(reader);
				break;
			case "LOCATION":
				reader.Read();
				Location = ReaderUtil.ReadLocation(reader, converters);
				break;
			case "BUILDINGNUMBER":
				reader.Read();
				BuildingNumber = Convert.ToInt32(reader.Value);
				break;
			case "STATESTARTTIME":
				reader.Read();
				StateStartTime = Convert.ToInt32(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
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
			writer.WritePropertyName("State");
			writer.WriteValue((int)State);
			if (Location != null)
			{
				writer.WritePropertyName("Location");
				writer.WriteStartObject();
				writer.WritePropertyName("x");
				writer.WriteValue(Location.x);
				writer.WritePropertyName("y");
				writer.WriteValue(Location.y);
				writer.WriteEndObject();
			}
			writer.WritePropertyName("BuildingNumber");
			writer.WriteValue(BuildingNumber);
			writer.WritePropertyName("StateStartTime");
			writer.WriteValue(StateStartTime);
		}

		public virtual bool HasDetailMenuToShow()
		{
			T definition = base.Definition;
			return !string.IsNullOrEmpty(definition.MenuPrefab);
		}

		public abstract BuildingObject AddBuildingObject(GameObject gameObject);

		public void SetState(BuildingState buildingState)
		{
			State = buildingState;
		}

		public virtual string GetPrefab(int index = 0)
		{
			T definition = base.Definition;
			return definition.GetPrefab(index);
		}

		public virtual string GetPaintover()
		{
			T definition = base.Definition;
			return definition.Paintover;
		}

		public virtual bool IsBuildingRepaired()
		{
			return true;
		}

		public virtual bool IsTikiSignRepaired()
		{
			return false;
		}
	}
	public interface Building : Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		BuildingState State { get; }

		new BuildingDefinition Definition { get; }

		int BuildingNumber { get; set; }

		bool IsFootprintable { get; }

		int StateStartTime { get; set; }

		bool HasDetailMenuToShow();

		BuildingObject AddBuildingObject(GameObject gameObject);

		void SetState(BuildingState buildingState);

		string GetPrefab(int index);

		string GetPaintover();

		bool IsBuildingRepaired();
	}
}
