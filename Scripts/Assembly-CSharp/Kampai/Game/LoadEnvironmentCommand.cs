using System;
using System.IO;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class LoadEnvironmentCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadEnvironmentCommand") as IKampaiLogger;

		private EnvironmentResourceDefinition definition;

		[Inject]
		public IInvokerService invokerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public override void Execute()
		{
			TextAsset textAsset = KampaiResources.Load("environment") as TextAsset;
			if (textAsset == null)
			{
				logger.Fatal(FatalCode.DS_NO_ENVIRONMENT_DEF, "Unable to read environment.json");
			}
			DeserializeEnvironmentDefinition(textAsset.text);
			DeserializeEnvironmentResources(textAsset.text);
			LoadEnvironmentResources();
		}

		private void DeserializeEnvironmentResources(string json)
		{
			using (StringReader textReader = new StringReader(json))
			{
				DeserializeEnvironmentResources(textReader);
			}
		}

		private void DeserializeEnvironmentResources(TextReader textReader)
		{
			using (JsonTextReader reader = new JsonTextReader(textReader))
			{
				definition = FastJSONDeserializer.Deserialize<EnvironmentResourceDefinition>(reader);
			}
		}

		private bool DeserializeEnvironmentDefinition(string json)
		{
			using (StringReader textReader = new StringReader(json))
			{
				return DeserializeEnvironmentDefinition(textReader);
			}
		}

		private bool DeserializeEnvironmentDefinition(TextReader textReader)
		{
			try
			{
				definitionService.DeserializeEnvironmentDefinition(textReader);
				return true;
			}
			catch (FatalException ex)
			{
				FatalException ex2 = ex;
				FatalException e2 = ex2;
				logger.Error("Can't deserialize: {0}", e2);
				invokerService.Add(delegate
				{
					logger.FatalNoThrow(e2.FatalCode, e2.ReferencedId, "Message: {0}, Reason: {1}", e2.Message, e2.InnerException ?? e2);
				});
			}
			catch (Exception ex3)
			{
				Exception ex4 = ex3;
				Exception e = ex4;
				logger.Error("Can't deserialize: {0}", e);
				invokerService.Add(delegate
				{
					logger.FatalNoThrow(FatalCode.DS_PARSE_ERROR, 0, "Reason: {0}", e);
				});
			}
			return false;
		}

		private void LoadEnvironmentResources()
		{
			foreach (string environmentResource in definition.environmentResources)
			{
				GameObject gameObject = KampaiResources.Load<GameObject>(environmentResource);
				if (gameObject == null)
				{
					logger.Fatal(FatalCode.DLC_ENVIRONMENT_MISSING, "Unable to load {0}", environmentResource);
				}
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
				if (gameObject2 == null)
				{
					logger.Fatal(FatalCode.DLC_ENVIRONMENT_MISSING, "Unable to instantiate {0}", environmentResource);
				}
			}
		}
	}
}
