using System;
using Kampai.Game.Transaction;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kampai.Game
{
	public class TransactionDefinitionConverter : CustomCreationConverter<TransactionDefinition>
	{
		private IDefinitionService definitionService;

		public TransactionDefinitionConverter(IDefinitionService definitionService)
		{
			this.definitionService = definitionService;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return definitionService.Get<TransactionDefinition>(Convert.ToInt32(reader.Value));
		}

		public override TransactionDefinition Create(Type objectType)
		{
			return null;
		}
	}
}
