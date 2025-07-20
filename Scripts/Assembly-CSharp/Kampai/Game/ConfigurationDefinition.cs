using System;
using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class ConfigurationDefinition : IFastJSONDeserializable
	{
		public enum RateAppAfterEvent
		{
			UnknownEvent = 0,
			LevelUp = 1,
			VillainCutscene = 2,
			XPPayout = 3
		}

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool serverPushNotifications { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public float minimumVersion { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		[Deserializer("ReaderUtil.ReadRateAppTriggerConfig")]
		public Dictionary<RateAppAfterEvent, bool> rateAppAfter { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int maxRPS { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<KillSwitch> killSwitches { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int msHeartbeat { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int fpsHeartbeat { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int logLevel { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int healthMetricPercentage { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int nudgeUpgradePercentage { get; set; }

		[Deserializer("ReaderUtil.ReadDictionaryString")]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<string, string> dlcManifests { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<string, FeatureAccess> featureAccess { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public TargetPerformance targetPerformance { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string definitionId { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string definitions { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool isAllowed { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool isNudgeAllowed { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string videoUri { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int httpRequestTimeout { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int httpRequestReadWriteTimeout { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int autoSaveIntervalUnlinkedAccount { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int autoSaveIntervalLinkedAccount { get; set; }

		[Deserializer("ReaderUtil.ReadNestedDictionary")]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<string, object> loggingConfig { get; set; }

		public virtual object Deserialize(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			ReaderUtil.EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string propertyName = ((string)reader.Value).ToUpper();
					if (!DeserializeProperty(propertyName, reader, converters))
					{
						reader.Skip();
					}
					break;
				}
				case JsonToken.EndObject:
					return this;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, ReaderUtil.GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		protected virtual bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SERVERPUSHNOTIFICATIONS":
				reader.Read();
				serverPushNotifications = Convert.ToBoolean(reader.Value);
				break;
			case "MINIMUMVERSION":
				reader.Read();
				minimumVersion = Convert.ToSingle(reader.Value);
				break;
			case "RATEAPPAFTER":
				reader.Read();
				rateAppAfter = ReaderUtil.ReadRateAppTriggerConfig(reader, converters);
				break;
			case "MAXRPS":
				reader.Read();
				maxRPS = Convert.ToInt32(reader.Value);
				break;
			case "KILLSWITCHES":
				reader.Read();
				killSwitches = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadKillSwitch, killSwitches);
				break;
			case "MSHEARTBEAT":
				reader.Read();
				msHeartbeat = Convert.ToInt32(reader.Value);
				break;
			case "FPSHEARTBEAT":
				reader.Read();
				fpsHeartbeat = Convert.ToInt32(reader.Value);
				break;
			case "LOGLEVEL":
				reader.Read();
				logLevel = Convert.ToInt32(reader.Value);
				break;
			case "HEALTHMETRICPERCENTAGE":
				reader.Read();
				healthMetricPercentage = Convert.ToInt32(reader.Value);
				break;
			case "NUDGEUPGRADEPERCENTAGE":
				reader.Read();
				nudgeUpgradePercentage = Convert.ToInt32(reader.Value);
				break;
			case "DLCMANIFESTS":
				reader.Read();
				dlcManifests = ReaderUtil.ReadDictionaryString(reader, converters);
				break;
			case "FEATUREACCESS":
				reader.Read();
				featureAccess = ReaderUtil.ReadDictionary<FeatureAccess>(reader, converters);
				break;
			case "TARGETPERFORMANCE":
				reader.Read();
				targetPerformance = ReaderUtil.ReadEnum<TargetPerformance>(reader);
				break;
			case "DEFINITIONID":
				reader.Read();
				definitionId = ReaderUtil.ReadString(reader, converters);
				break;
			case "DEFINITIONS":
				reader.Read();
				definitions = ReaderUtil.ReadString(reader, converters);
				break;
			case "ISALLOWED":
				reader.Read();
				isAllowed = Convert.ToBoolean(reader.Value);
				break;
			case "ISNUDGEALLOWED":
				reader.Read();
				isNudgeAllowed = Convert.ToBoolean(reader.Value);
				break;
			case "VIDEOURI":
				reader.Read();
				videoUri = ReaderUtil.ReadString(reader, converters);
				break;
			case "HTTPREQUESTTIMEOUT":
				reader.Read();
				httpRequestTimeout = Convert.ToInt32(reader.Value);
				break;
			case "HTTPREQUESTREADWRITETIMEOUT":
				reader.Read();
				httpRequestReadWriteTimeout = Convert.ToInt32(reader.Value);
				break;
			case "AUTOSAVEINTERVALUNLINKEDACCOUNT":
				reader.Read();
				autoSaveIntervalUnlinkedAccount = Convert.ToInt32(reader.Value);
				break;
			case "AUTOSAVEINTERVALLINKEDACCOUNT":
				reader.Read();
				autoSaveIntervalLinkedAccount = Convert.ToInt32(reader.Value);
				break;
			case "LOGGINGCONFIG":
				reader.Read();
				loggingConfig = ReaderUtil.ReadNestedDictionary(reader, converters);
				break;
			default:
				return false;
			}
			return true;
		}

		public virtual object DeserializeOverride(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			ReaderUtil.EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string propertyName = ((string)reader.Value).ToUpper();
					if (!DeserializePropertyOverride(propertyName, reader, converters))
					{
						reader.Skip();
					}
					break;
				}
				case JsonToken.EndObject:
					return this;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, ReaderUtil.GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		protected virtual bool DeserializePropertyOverride(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "SERVERPUSHNOTIFICATIONS":
				reader.Read();
				serverPushNotifications = Convert.ToBoolean(reader.Value);
				break;
			case "MINIMUMVERSION":
				reader.Read();
				minimumVersion = Convert.ToSingle(reader.Value);
				break;
			case "RATEAPPAFTER":
				reader.Read();
				rateAppAfter = ReaderUtil.ReadRateAppTriggerConfig(reader, converters);
				break;
			case "MAXRPS":
				reader.Read();
				maxRPS = Convert.ToInt32(reader.Value);
				break;
			case "KILLSWITCHES":
				reader.Read();
				killSwitches = ReaderUtil.PopulateList(reader, converters, ReaderUtil.ReadKillSwitch);
				break;
			case "MSHEARTBEAT":
				reader.Read();
				msHeartbeat = Convert.ToInt32(reader.Value);
				break;
			case "FPSHEARTBEAT":
				reader.Read();
				fpsHeartbeat = Convert.ToInt32(reader.Value);
				break;
			case "LOGLEVEL":
				reader.Read();
				logLevel = Convert.ToInt32(reader.Value);
				break;
			case "HEALTHMETRICPERCENTAGE":
				reader.Read();
				healthMetricPercentage = Convert.ToInt32(reader.Value);
				break;
			case "NUDGEUPGRADEPERCENTAGE":
				reader.Read();
				nudgeUpgradePercentage = Convert.ToInt32(reader.Value);
				break;
			case "DLCMANIFESTS":
				reader.Read();
				dlcManifests = ReaderUtil.ReadDictionaryString(reader, converters);
				break;
			case "FEATUREACCESS":
				reader.Read();
				featureAccess = ReaderUtil.ReadDictionary<FeatureAccess>(reader, converters);
				break;
			case "TARGETPERFORMANCE":
				reader.Read();
				targetPerformance = ReaderUtil.ReadEnum<TargetPerformance>(reader);
				break;
			case "DEFINITIONID":
				reader.Read();
				definitionId = ReaderUtil.ReadString(reader, converters);
				break;
			case "DEFINITIONS":
				reader.Read();
				definitions = ReaderUtil.ReadString(reader, converters);
				break;
			case "ISALLOWED":
				reader.Read();
				isAllowed = Convert.ToBoolean(reader.Value);
				break;
			case "ISNUDGEALLOWED":
				reader.Read();
				isNudgeAllowed = Convert.ToBoolean(reader.Value);
				break;
			case "VIDEOURI":
				reader.Read();
				videoUri = ReaderUtil.ReadString(reader, converters);
				break;
			case "HTTPREQUESTTIMEOUT":
				reader.Read();
				httpRequestTimeout = Convert.ToInt32(reader.Value);
				break;
			case "HTTPREQUESTREADWRITETIMEOUT":
				reader.Read();
				httpRequestReadWriteTimeout = Convert.ToInt32(reader.Value);
				break;
			case "AUTOSAVEINTERVALUNLINKEDACCOUNT":
				reader.Read();
				autoSaveIntervalUnlinkedAccount = Convert.ToInt32(reader.Value);
				break;
			case "AUTOSAVEINTERVALLINKEDACCOUNT":
				reader.Read();
				autoSaveIntervalLinkedAccount = Convert.ToInt32(reader.Value);
				break;
			case "LOGGINGCONFIG":
				reader.Read();
				loggingConfig = ReaderUtil.ReadNestedDictionary(reader, converters);
				break;
			default:
				return false;
			}
			return true;
		}
	}
}
