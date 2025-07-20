using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public abstract class Plot<T> : Instance<T>, Instance, Locatable, Plot, IFastJSONDeserializable, IFastJSONSerializable, Identifiable where T : PlotDefinition
	{
		PlotDefinition Plot.Definition
		{
			get
			{
				return base.Definition;
			}
		}

		public Location Location { get; set; }

		public Plot(T definition)
			: base(definition)
		{
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "LOCATION":
				reader.Read();
				Location = ReaderUtil.ReadLocation(reader, converters);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
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
		}
	}
	public interface Plot : Instance, Locatable, IFastJSONDeserializable, IFastJSONSerializable, Identifiable
	{
		new PlotDefinition Definition { get; }
	}
}
