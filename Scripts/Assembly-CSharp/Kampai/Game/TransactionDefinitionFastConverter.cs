using System;
using Kampai.Game.Transaction;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class TransactionDefinitionFastConverter : FastJsonCreationConverter<TransactionDefinition>
	{
		private IDefinitionService definitionService;

		public TransactionDefinitionFastConverter(IDefinitionService definitionService)
		{
			this.definitionService = definitionService;
		}

		public override TransactionDefinition ReadJson(JsonReader reader, JsonConverters converters)
		{
			return definitionService.Get<TransactionDefinition>(Convert.ToInt32(reader.Value));
		}

		public override TransactionDefinition Create()
		{
			return null;
		}
	}
}
