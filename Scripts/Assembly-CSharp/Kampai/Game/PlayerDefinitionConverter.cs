using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kampai.Game
{
	public class PlayerDefinitionConverter : JsonConverter
	{
		private DefinitionService defService;

		public PlayerDefinitionConverter(DefinitionService defService)
		{
			this.defService = defService;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);
			defService.SetInitialPlayer(jObject.ToString());
			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			if (objectType.BaseType == null)
			{
				return false;
			}
			if (typeof(PlayerVersion).IsAssignableFrom(objectType))
			{
				return true;
			}
			return false;
		}
	}
}
