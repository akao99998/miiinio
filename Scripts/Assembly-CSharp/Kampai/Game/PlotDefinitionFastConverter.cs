using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class PlotDefinitionFastConverter : FastJsonCreationConverter<PlotDefinition>
	{
		private PlotType plotType;

		private readonly IKampaiLogger logger;

		public PlotDefinitionFastConverter(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public override PlotDefinition ReadJson(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			JObject jObject = JObject.Load(reader);
			JProperty jProperty = jObject.Property("type");
			if (jProperty != null)
			{
				string value = jProperty.Value.ToString();
				plotType = (PlotType)(int)Enum.Parse(typeof(PlotType), value);
			}
			else
			{
				plotType = PlotType.UNKNOWN;
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, converters);
		}

		public override PlotDefinition Create()
		{
			PlotType plotType = this.plotType;
			if (plotType == PlotType.RED_CARPET)
			{
				return new NoOpPlotDefinition();
			}
			logger.Fatal(FatalCode.EX_INVALID_ENUM);
			return null;
		}
	}
}
