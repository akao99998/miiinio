using System;
using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class PlotDefinitionConverter : CustomCreationConverter<PlotDefinition>
	{
		private PlotType plotType;

		private readonly IKampaiLogger logger;

		public PlotDefinitionConverter(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			if (jObject.Property("type") != null)
			{
				string value = jObject.Property("type").Value.ToString();
				plotType = (PlotType)(int)Enum.Parse(typeof(PlotType), value);
			}
			else
			{
				plotType = PlotType.UNKNOWN;
			}
			reader = jObject.CreateReader();
			return base.ReadJson(reader, objectType, existingValue, serializer);
		}

		public override PlotDefinition Create(Type objectType)
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
