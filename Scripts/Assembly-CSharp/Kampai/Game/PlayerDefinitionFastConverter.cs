using Kampai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class PlayerDefinitionFastConverter : FastJsonConverter<PlayerVersion>
	{
		private DefinitionService defService;

		public PlayerDefinitionFastConverter(DefinitionService defService)
		{
			this.defService = defService;
		}

		public PlayerVersion ReadJson(JsonReader reader, JsonConverters converters)
		{
			JObject jObject = JObject.Load(reader);
			defService.SetInitialPlayer(jObject.ToString());
			return null;
		}

		public PlayerVersion Create()
		{
			return null;
		}
	}
}
