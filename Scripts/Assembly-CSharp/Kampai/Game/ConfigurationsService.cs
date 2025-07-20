using System;
using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class ConfigurationsService : IConfigurationsService
	{
		private enum ConfigLoadingState
		{
			WAIT_DEFAULT_CONFIG = 0,
			WAIT_DEFAULT_CONFIG_OVERRIDE_KNOWN = 1,
			WAIT_OVERRIDEN_CONFIG = 2,
			OVERRIDEN_CONFIG_LOADED = 3
		}

		private const string DEFAULT_DEVICE_CONFIG_NAME = "anyDeviceType";

		private const string KILLSWITCH_PP_OVERRIDE_LEAD = "KS-";

		private ConfigurationDefinition config;

		private bool init = true;

		private int tries;

		private int tryCap = 5;

		public IKampaiLogger logger = LogManager.GetClassLogger("ConfigurationsService") as IKampaiLogger;

		[Inject("game.server.environment")]
		public string ServerEnv { get; set; }

		[Inject]
		public ConfigurationsLoadedSignal configurationsLoadedSignal { get; set; }

		[Inject]
		public IClientVersion clientVersion { get; set; }

		[Inject]
		public FPSUtil fpsUtil { get; set; }

		[Inject]
		public KillSwitchChangedSignal killSwitchChangedSignal { get; set; }

		[Inject]
		public LoadConfigurationSignal loadConfigurationSignal { get; set; }

		private List<KeyValuePair<KillSwitch, bool>> killswitchOverrides { get; set; }

		public ConfigurationDefinition GetConfigurations()
		{
			return config;
		}

		public int GetTries()
		{
			return tries;
		}

		public int GetTryCap()
		{
			return tryCap;
		}

		public void setInitonCallback(bool init)
		{
			this.init = init;
		}

		public void GetConfigurationCallback(IResponse response)
		{
			TimeProfiler.EndSection("retrieve config");
			if (response.Success)
			{
				logger.Info("ConfigurationsService.GetConfigurationCallback: Attempting to deserialize configuration definition...");
				string body = response.Body;
				ConfigurationDefinition configurationDefinition = null;
				try
				{
					TimeProfiler.StartSection("read config");
					configurationDefinition = LoadConfig(body);
				}
				catch (JsonSerializationException e)
				{
					configurationDefinition = TryAgainForConfigAfterException(e);
				}
				catch (JsonReaderException e2)
				{
					configurationDefinition = TryAgainForConfigAfterException(e2);
				}
				finally
				{
					TimeProfiler.EndSection("read config");
				}
				if (configurationDefinition != null)
				{
					tries = 0;
					if (GameConstants.StaticConfig.DEBUG_ENABLED)
					{
						TryLoadKillswitchOverrides();
					}
					logger.Info("ConfigurationDefinition is not null, carry on...");
					if (config == null)
					{
						config = new ConfigurationDefinition();
					}
					if (config.killSwitches != configurationDefinition.killSwitches)
					{
						config = configurationDefinition;
						killSwitchChangedSignal.Dispatch();
					}
					config = configurationDefinition;
					logger.SetAllowedLevel(configurationDefinition.logLevel);
					fpsUtil.SetFpsHeartbeat(configurationDefinition.fpsHeartbeat);
					HttpRequestConfig.SetConfig(configurationDefinition);
					configurationsLoadedSignal.Dispatch(init);
				}
			}
			else if (!response.Request.IsAborted())
			{
				if (!IgnoreError())
				{
					logger.Fatal(FatalCode.CONFIG_NETWORK_FAIL);
					return;
				}
				logger.Warning("Network error (code {0}) on non initial configuration request.", response.Code);
			}
		}

		private ConfigurationDefinition LoadConfig(string json)
		{
			KampaiStringReader kampaiStringReader = new KampaiStringReader(json);
			JsonTextReader jsonTextReader = new JsonTextReader(kampaiStringReader);
			MoveReadingPositionToFirstConfig(jsonTextReader);
			string deviceType = GetDeviceType();
			ConfigurationDefinition configurationDefinition = null;
			ConfigLoadingState configLoadingState = ConfigLoadingState.WAIT_DEFAULT_CONFIG;
			int position = -1;
			while (jsonTextReader.Read())
			{
				switch (jsonTextReader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string text = (string)jsonTextReader.Value;
					switch (text)
					{
					case "anyDeviceType":
						jsonTextReader.Read();
						configurationDefinition = new ConfigurationDefinition();
						configurationDefinition.Deserialize(jsonTextReader);
						switch (configLoadingState)
						{
						case ConfigLoadingState.WAIT_DEFAULT_CONFIG:
							configLoadingState = ConfigLoadingState.WAIT_OVERRIDEN_CONFIG;
							break;
						case ConfigLoadingState.WAIT_DEFAULT_CONFIG_OVERRIDE_KNOWN:
						{
							kampaiStringReader.SetPosition(position);
							JsonTextReader reader = new JsonTextReader(kampaiStringReader);
							configurationDefinition.DeserializeOverride(reader);
							configLoadingState = ConfigLoadingState.OVERRIDEN_CONFIG_LOADED;
							logger.Debug("LoadConfig: config loaded. Config is special for device {0}", deviceType);
							return configurationDefinition;
						}
						default:
							throw new JsonSerializationException(string.Format("Unexpected config loading state {0} for device: {1}. Json reader state: {2}. {3}", configLoadingState, deviceType, jsonTextReader.TokenType, ReaderUtil.GetPositionInSource(jsonTextReader)));
						}
						break;
					default:
						if (text == deviceType)
						{
							switch (configLoadingState)
							{
							case ConfigLoadingState.WAIT_OVERRIDEN_CONFIG:
								jsonTextReader.Read();
								configurationDefinition.Deserialize(jsonTextReader);
								configLoadingState = ConfigLoadingState.OVERRIDEN_CONFIG_LOADED;
								logger.Debug("LoadConfig: config loaded. Config is special for device {0}", deviceType);
								return configurationDefinition;
							case ConfigLoadingState.WAIT_DEFAULT_CONFIG:
								position = kampaiStringReader.GetPosition();
								jsonTextReader.Skip();
								configLoadingState = ConfigLoadingState.WAIT_DEFAULT_CONFIG_OVERRIDE_KNOWN;
								break;
							default:
								throw new JsonSerializationException(string.Format("Unexpected config loading state {0} for device: {1}. Json reader state: {2}. {3}", configLoadingState, deviceType, jsonTextReader.TokenType, ReaderUtil.GetPositionInSource(jsonTextReader)));
							}
						}
						else
						{
							jsonTextReader.Skip();
						}
						break;
					}
					break;
				}
				case JsonToken.EndObject:
					logger.Debug("LoadConfig: default config loaded.");
					return configurationDefinition ?? new ConfigurationDefinition();
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", jsonTextReader.TokenType, ReaderUtil.GetPositionInSource(jsonTextReader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		private void MoveReadingPositionToFirstConfig(JsonReader reader)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			ReaderUtil.EnsureToken(JsonToken.StartObject, reader);
			reader.Read();
			if (reader.TokenType != JsonToken.PropertyName || !"allConfigs".Equals(reader.Value))
			{
				throw new JsonSerializationException("unexpected config format. First field allConfig is missing.");
			}
			reader.Read();
			ReaderUtil.EnsureToken(JsonToken.StartObject, reader);
		}

		private bool ValidConfigurationExists()
		{
			return config != null;
		}

		private bool IgnoreError()
		{
			return !init && ValidConfigurationExists();
		}

		private ConfigurationDefinition TryAgainForConfigAfterException(Exception e)
		{
			if (!init && ValidConfigurationExists())
			{
				logger.Warning("Json error in ConfigurationService on non initial request: using the old configuration.");
				return config;
			}
			if (tries < tryCap)
			{
				logger.Info("Json serialization error in ConfigurationService: # of tries for a new service: ", tries);
				tries++;
				loadConfigurationSignal.Dispatch(init);
			}
			else
			{
				if (config != null)
				{
					logger.Error("Json serialization error in ConfigurationService: using the old configuration.");
					return config;
				}
				logger.Error("Error: {0}", e.Message);
				logger.Fatal(FatalCode.CONFIG_JSON_FAIL);
			}
			return null;
		}

		public string GetConfigURL()
		{
			string configVariant = GetConfigVariant();
			string clientPlatform = clientVersion.GetClientPlatform();
			string text = clientVersion.GetClientVersion();
			return string.Format(GameConstants.Server.CDN_URL + "/configs/{0}/{1}/{2}/{3}/config", ServerEnv.ToLower(), text, clientPlatform, configVariant);
		}

		public string GetDeviceType()
		{
			return Native.GetDeviceHardwareModel();
		}

		public string GetConfigVariant()
		{
			return (!ABTestModel.abtestEnabled) ? "anyVariant" : ABTestModel.configurationVariant;
		}

		public string GetDefinitionVariants()
		{
			if (ABTestModel.abtestEnabled && ABTestModel.definitionURL != null)
			{
				return ABTestModel.definitionVariants;
			}
			return string.Empty;
		}

		public void TryLoadKillswitchOverrides()
		{
			string empty = string.Empty;
			foreach (int value in Enum.GetValues(typeof(KillSwitch)))
			{
				empty = PlayerPrefs.GetString("KS-" + (KillSwitch)value, null);
				if (!string.IsNullOrEmpty(empty))
				{
					OverrideKillswitch((KillSwitch)value, Convert.ToBoolean(empty));
				}
			}
		}

		public bool isKillSwitchOn(KillSwitch killswitchType)
		{
			if (killswitchOverrides != null)
			{
				foreach (KeyValuePair<KillSwitch, bool> killswitchOverride in killswitchOverrides)
				{
					if (killswitchOverride.Key == killswitchType)
					{
						return killswitchOverride.Value;
					}
				}
			}
			return (config.killSwitches != null && config.killSwitches.Contains(killswitchType)) ? true : false;
		}

		public void OverrideKillswitch(KillSwitch killswitchType, bool killswitchValue)
		{
			if (killswitchOverrides == null)
			{
				killswitchOverrides = new List<KeyValuePair<KillSwitch, bool>>();
			}
			foreach (KeyValuePair<KillSwitch, bool> killswitchOverride in killswitchOverrides)
			{
				if (killswitchOverride.Key == killswitchType)
				{
					killswitchOverrides.Remove(killswitchOverride);
					break;
				}
			}
			killswitchOverrides.Add(new KeyValuePair<KillSwitch, bool>(killswitchType, killswitchValue));
			PlayerPrefs.SetString("KS-" + killswitchType, killswitchValue.ToString());
		}

		public void ClearKillswitchOverride(KillSwitch killswitchType)
		{
			if (killswitchOverrides == null)
			{
				return;
			}
			foreach (KeyValuePair<KillSwitch, bool> killswitchOverride in killswitchOverrides)
			{
				if (killswitchOverride.Key == killswitchType)
				{
					killswitchOverrides.Remove(killswitchOverride);
					PlayerPrefs.DeleteKey("KS-" + killswitchType);
					break;
				}
			}
		}

		public void ClearAllKillswitchOverrides()
		{
			if (killswitchOverrides != null)
			{
				for (int num = killswitchOverrides.Count - 1; num >= 0; num--)
				{
					PlayerPrefs.DeleteKey("KS-" + killswitchOverrides[num].Key);
					killswitchOverrides.RemoveAt(num);
				}
			}
		}
	}
}
