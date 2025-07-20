using System;
using System.IO;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;

public class LoadDefinitionsCommand : Command
{
	public class LoadDefinitionsData
	{
		public string Path { get; set; }

		public string Json { get; set; }
	}

	public IKampaiLogger logger = LogManager.GetClassLogger("LoadDefinitionsCommand") as IKampaiLogger;

	[Inject]
	public bool hotSwap { get; set; }

	[Inject]
	public LoadDefinitionsData defData { get; set; }

	[Inject]
	public IDefinitionService definitionService { get; set; }

	[Inject]
	public DefinitionsChangedSignal definitionsChangedSignal { get; set; }

	[Inject]
	public ITelemetryService telemetryService { get; set; }

	[Inject]
	public IDLCService dlcService { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public IInvokerService invokerService { get; set; }

	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public SplashProgressUpdateSignal splashProgressUpdateSignal { get; set; }

	public override void Execute()
	{
		logger.EventStart("LoadDefinitionsCommand.Execute");
		string jsonString = defData.Json;
		if (jsonString != null)
		{
			logger.Debug("LoadDefinitions: Starting json deserialization from string");
			routineRunner.StartAsyncConditionTask(() => DeserializeDefinitionsFromJsonString(jsonString), OnDeserializationSuccess);
		}
		else
		{
			string jsonPath = defData.Path;
			if (string.IsNullOrEmpty(jsonPath))
			{
				throw new ArgumentException("LoadDefinitionsCommand: neither json content nor path to file is specified");
			}
			bool flag = true;
			string binaryDefinitionsPath = DefinitionService.GetBinaryDefinitionsPath();
			if (File.Exists(binaryDefinitionsPath))
			{
				logger.Debug("LoadDefinitions: Starting binary deserialization");
				if (DeserializeDefinitionsFromBinaryFile(binaryDefinitionsPath))
				{
					flag = false;
					OnDeserializationSuccess();
				}
				else
				{
					DefinitionService.DeleteBinarySerialization();
				}
			}
			if (flag)
			{
				logger.Debug("LoadDefinitions: Starting json deserialization");
				routineRunner.StartAsyncConditionTask(delegate
				{
					bool flag2 = DeserializeDefinitionsFromJsonFile(jsonPath);
					if (!flag2)
					{
						RemoveCachedDefinitions(jsonPath);
					}
					return flag2;
				}, OnDeserializationSuccess);
			}
		}
		logger.EventStop("LoadDefinitionsCommand.Execute");
	}

	private void RemoveCachedDefinitions(string path)
	{
		try
		{
			File.Delete(path);
		}
		catch (Exception)
		{
		}
	}

	private void OnDeserializationSuccess()
	{
		this.telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("80 - Loaded Definitions", playerService.SWRVEGroup, dlcService.GetDownloadQualityLevel());
		logger.Debug("LoadDefinitions: Deserialized successfully");
		TelemetryService telemetryService = this.telemetryService as TelemetryService;
		if (telemetryService != null)
		{
			telemetryService.SetDefinitionServiceReference(definitionService);
		}
		definitionsChangedSignal.Dispatch(hotSwap);
		splashProgressUpdateSignal.Dispatch(35, 10f);
	}

	private bool DeserializeDefinitionsFromBinaryFile(string path)
	{
		try
		{
			using (BinaryReader binaryReader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
			{
				definitionService.DeserializeBinary(binaryReader);
			}
			return true;
		}
		catch (Exception ex)
		{
			logger.Error("DeserializeDefinitionsFromBinaryFile: can't deserialize from binary file. Reason: {0}", ex);
			return false;
		}
	}

	private bool DeserializeDefinitionsFromJsonString(string jsonString)
	{
		using (StringReader textReader = new StringReader(jsonString))
		{
			return DeserializeDefinitionsFromJson(textReader);
		}
	}

	private bool DeserializeDefinitionsFromJsonFile(string jsonPath)
	{
		try
		{
			using (StreamReader textReader = new StreamReader(jsonPath))
			{
				return DeserializeDefinitionsFromJson(textReader);
			}
		}
		catch (Exception e)
		{
			HandleDefinitionFileOpenError(e);
			return false;
		}
	}

	private bool HandleDefinitionFileOpenError(Exception e)
	{
		logger.Error("Definition file open error: {0}", e);
		int reasonCode = 0;
		if (e is FileNotFoundException)
		{
			reasonCode = 1;
		}
		else if (e is IOException)
		{
			reasonCode = 2;
		}
		invokerService.Add(delegate
		{
			logger.FatalNoThrow(FatalCode.DS_UNABLE_TO_LOAD, reasonCode, "Reason: {0}", e);
		});
		return false;
	}

	private bool DeserializeDefinitionsFromJson(TextReader textReader)
	{
		try
		{
			definitionService.DeserializeJson(textReader);
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
}
