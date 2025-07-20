using System;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Main;
using Kampai.Splash;
using Kampai.UI;
using Kampai.UI.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kampai.Util
{
	public static class ReaderUtil
	{
		public static LegalDocumentURL ReadLegalDocumentURL(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			LegalDocumentURL result = default(LegalDocumentURL);
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "LANGUAGE":
						reader.Read();
						result.language = ReadString(reader, converters);
						break;
					case "URL":
						reader.Read();
						result.url = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return result;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static NotificationReminder ReadNotificationReminder(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			NotificationReminder result = default(NotificationReminder);
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "LEVEL":
						reader.Read();
						result.level = Convert.ToInt32(reader.Value);
						break;
					case "MESSAGELOCALIZEDKEY":
						reader.Read();
						result.messageLocalizedKey = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return result;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static CharacterPrestigeLevelDefinition ReadCharacterPrestigeLevelDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			CharacterPrestigeLevelDefinition characterPrestigeLevelDefinition = new CharacterPrestigeLevelDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "UNLOCKLEVEL":
						reader.Read();
						characterPrestigeLevelDefinition.UnlockLevel = Convert.ToUInt32(reader.Value);
						break;
					case "UNLOCKQUESTID":
						reader.Read();
						characterPrestigeLevelDefinition.UnlockQuestID = Convert.ToInt32(reader.Value);
						break;
					case "POINTSNEEDED":
						reader.Read();
						characterPrestigeLevelDefinition.PointsNeeded = Convert.ToUInt32(reader.Value);
						break;
					case "ATTACHEDQUESTID":
						reader.Read();
						characterPrestigeLevelDefinition.AttachedQuestID = Convert.ToInt32(reader.Value);
						break;
					case "WELCOMEPANELMESSAGELOCALIZEDKEY":
						reader.Read();
						characterPrestigeLevelDefinition.WelcomePanelMessageLocalizedKey = ReadString(reader, converters);
						break;
					case "FAREWELLPANELMESSAGELOCALIZEDKEY":
						reader.Read();
						characterPrestigeLevelDefinition.FarewellPanelMessageLocalizedKey = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return characterPrestigeLevelDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static AchievementID ReadAchievementID(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			AchievementID achievementID = new AchievementID();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "GAMECENTERID":
						reader.Read();
						achievementID.GameCenterID = ReadString(reader, converters);
						break;
					case "GOOGLEPLAYID":
						reader.Read();
						achievementID.GooglePlayID = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return achievementID;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static ScreenPosition ReadScreenPosition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			ScreenPosition screenPosition = new ScreenPosition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "X":
						reader.Read();
						screenPosition.x = Convert.ToSingle(reader.Value);
						break;
					case "Z":
						reader.Read();
						screenPosition.z = Convert.ToSingle(reader.Value);
						break;
					case "ZOOM":
						reader.Read();
						screenPosition.zoom = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return screenPosition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static Vector3 ReadVector3(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			Vector3 result = default(Vector3);
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "X":
						reader.Read();
						result.x = Convert.ToSingle(reader.Value);
						break;
					case "Y":
						reader.Read();
						result.y = Convert.ToSingle(reader.Value);
						break;
					case "Z":
						reader.Read();
						result.z = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return result;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static ConnectablePiecePrefabDefinition ReadConnectablePiecePrefabDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			ConnectablePiecePrefabDefinition connectablePiecePrefabDefinition = new ConnectablePiecePrefabDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "STRAIGHT":
						reader.Read();
						connectablePiecePrefabDefinition.straight = ReadString(reader, converters);
						break;
					case "CROSS":
						reader.Read();
						connectablePiecePrefabDefinition.cross = ReadString(reader, converters);
						break;
					case "POST":
						reader.Read();
						connectablePiecePrefabDefinition.post = ReadString(reader, converters);
						break;
					case "TSHAPE":
						reader.Read();
						connectablePiecePrefabDefinition.tshape = ReadString(reader, converters);
						break;
					case "ENDCAP":
						reader.Read();
						connectablePiecePrefabDefinition.endcap = ReadString(reader, converters);
						break;
					case "CORNER":
						reader.Read();
						connectablePiecePrefabDefinition.corner = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return connectablePiecePrefabDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static SlotUnlock ReadSlotUnlock(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			SlotUnlock slotUnlock = new SlotUnlock();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "SLOTUNLOCKLEVELS":
						reader.Read();
						slotUnlock.SlotUnlockLevels = PopulateListInt32(reader, slotUnlock.SlotUnlockLevels);
						break;
					case "SLOTUNLOCKCOSTS":
						reader.Read();
						slotUnlock.SlotUnlockCosts = PopulateListInt32(reader, slotUnlock.SlotUnlockCosts);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return slotUnlock;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static UserSegment ReadUserSegment(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			UserSegment userSegment = new UserSegment();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "LEVELGREATERTHANOREQUALTO":
						reader.Read();
						userSegment.LevelGreaterThanOrEqualTo = Convert.ToInt32(reader.Value);
						break;
					case "FIRSTXRETURNREWARDSWEIGHTEDDEFINITIONID":
						reader.Read();
						userSegment.FirstXReturnRewardsWeightedDefinitionId = Convert.ToInt32(reader.Value);
						break;
					case "SECONDXRETURNREWARDSWEIGHTEDDEFINITIONID":
						reader.Read();
						userSegment.SecondXReturnRewardsWeightedDefinitionId = Convert.ToInt32(reader.Value);
						break;
					case "AFTERXRETURNREWARDS":
						reader.Read();
						userSegment.AfterXReturnRewards = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return userSegment;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static Location ReadLocation(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			Location location = new Location();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "X":
						reader.Read();
						location.x = Convert.ToInt32(reader.Value);
						break;
					case "Y":
						reader.Read();
						location.y = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return location;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MignetteRuleDefinition ReadMignetteRuleDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MignetteRuleDefinition mignetteRuleDefinition = new MignetteRuleDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "CAUSEIMAGE":
						reader.Read();
						mignetteRuleDefinition.CauseImage = ReadString(reader, converters);
						break;
					case "CAUSEIMAGEMASK":
						reader.Read();
						mignetteRuleDefinition.CauseImageMask = ReadString(reader, converters);
						break;
					case "EFFECTIMAGE":
						reader.Read();
						mignetteRuleDefinition.EffectImage = ReadString(reader, converters);
						break;
					case "EFFECTIMAGEMASK":
						reader.Read();
						mignetteRuleDefinition.EffectImageMask = ReadString(reader, converters);
						break;
					case "EFFECTAMOUNT":
						reader.Read();
						mignetteRuleDefinition.EffectAmount = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return mignetteRuleDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MignetteChildObjectDefinition ReadMignetteChildObjectDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MignetteChildObjectDefinition mignetteChildObjectDefinition = new MignetteChildObjectDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "PREFAB":
						reader.Read();
						mignetteChildObjectDefinition.Prefab = ReadString(reader, converters);
						break;
					case "POSITION":
						reader.Read();
						mignetteChildObjectDefinition.Position = ReadVector3(reader, converters);
						break;
					case "ISLOCAL":
						reader.Read();
						mignetteChildObjectDefinition.IsLocal = Convert.ToBoolean(reader.Value);
						break;
					case "ROTATION":
						reader.Read();
						mignetteChildObjectDefinition.Rotation = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return mignetteChildObjectDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MinionPartyPrefabDefinition ReadMinionPartyPrefabDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MinionPartyPrefabDefinition minionPartyPrefabDefinition = new MinionPartyPrefabDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "EVENTTYPE":
						reader.Read();
						minionPartyPrefabDefinition.EventType = ReadString(reader, converters);
						break;
					case "PREFAB":
						reader.Read();
						minionPartyPrefabDefinition.Prefab = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return minionPartyPrefabDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static Area ReadArea(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			Area area = new Area();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "A":
						reader.Read();
						area.a = ReadLocation(reader, converters);
						break;
					case "B":
						reader.Read();
						area.b = ReadLocation(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return area;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static StorageUpgradeDefinition ReadStorageUpgradeDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			StorageUpgradeDefinition storageUpgradeDefinition = new StorageUpgradeDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "LEVEL":
						reader.Read();
						storageUpgradeDefinition.Level = Convert.ToInt32(reader.Value);
						break;
					case "STORAGECAPACITY":
						reader.Read();
						storageUpgradeDefinition.StorageCapacity = Convert.ToUInt32(reader.Value);
						break;
					case "TRANSACTIONID":
						reader.Read();
						storageUpgradeDefinition.TransactionId = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return storageUpgradeDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static PlatformDefinition ReadPlatformDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			PlatformDefinition platformDefinition = new PlatformDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "BUILDINGREMOVALANIMCONTROLLER":
						reader.Read();
						platformDefinition.buildingRemovalAnimController = ReadString(reader, converters);
						break;
					case "CUSTOMCAMERAPOSID":
						reader.Read();
						platformDefinition.customCameraPosID = Convert.ToInt32(reader.Value);
						break;
					case "DESCRIPTION":
						reader.Read();
						platformDefinition.description = ReadString(reader, converters);
						break;
					case "OFFSET":
						reader.Read();
						platformDefinition.offset = ReadVector3(reader, converters);
						break;
					case "PLACEMENTLOCATION":
						reader.Read();
						platformDefinition.placementLocation = ReadLocation(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return platformDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static ResourcePlotDefinition ReadResourcePlotDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			ResourcePlotDefinition resourcePlotDefinition = new ResourcePlotDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "DESCRIPTIONKEY":
						reader.Read();
						resourcePlotDefinition.descriptionKey = ReadString(reader, converters);
						break;
					case "ISAUTOMATICALLYUNLOCKED":
						reader.Read();
						resourcePlotDefinition.isAutomaticallyUnlocked = Convert.ToBoolean(reader.Value);
						break;
					case "LOCATION":
						reader.Read();
						resourcePlotDefinition.location = ReadLocation(reader, converters);
						break;
					case "UNLOCKTRANSACTIONID":
						reader.Read();
						resourcePlotDefinition.unlockTransactionID = Convert.ToInt32(reader.Value);
						break;
					case "ROTATION":
						reader.Read();
						resourcePlotDefinition.rotation = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return resourcePlotDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static CharacterUIAnimationDefinition ReadCharacterUIAnimationDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			CharacterUIAnimationDefinition characterUIAnimationDefinition = new CharacterUIAnimationDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "STATEMACHINE":
						reader.Read();
						characterUIAnimationDefinition.StateMachine = ReadString(reader, converters);
						break;
					case "IDLEWEIGHTEDANIMATIONID":
						reader.Read();
						characterUIAnimationDefinition.IdleWeightedAnimationID = Convert.ToInt32(reader.Value);
						break;
					case "IDLECOUNT":
						reader.Read();
						characterUIAnimationDefinition.IdleCount = Convert.ToInt32(reader.Value);
						break;
					case "HAPPYWEIGHTEDANIMATIONID":
						reader.Read();
						characterUIAnimationDefinition.HappyWeightedAnimationID = Convert.ToInt32(reader.Value);
						break;
					case "HAPPYCOUNT":
						reader.Read();
						characterUIAnimationDefinition.HappyCount = Convert.ToInt32(reader.Value);
						break;
					case "SELECTEDWEIGHTEDANIMATIONID":
						reader.Read();
						characterUIAnimationDefinition.SelectedWeightedAnimationID = Convert.ToInt32(reader.Value);
						break;
					case "SELECTEDCOUNT":
						reader.Read();
						characterUIAnimationDefinition.SelectedCount = Convert.ToInt32(reader.Value);
						break;
					case "USELEGACY":
						reader.Read();
						characterUIAnimationDefinition.UseLegacy = Convert.ToBoolean(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return characterUIAnimationDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static FloatLocation ReadFloatLocation(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			FloatLocation floatLocation = new FloatLocation();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "X":
						reader.Read();
						floatLocation.x = Convert.ToSingle(reader.Value);
						break;
					case "Y":
						reader.Read();
						floatLocation.y = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return floatLocation;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static Angle ReadAngle(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			Angle angle = new Angle();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "DEGREES":
						reader.Read();
						angle.Degrees = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return angle;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static CollectionReward ReadCollectionReward(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			CollectionReward collectionReward = new CollectionReward();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "REQUIREDPOINTS":
						reader.Read();
						collectionReward.RequiredPoints = Convert.ToInt32(reader.Value);
						break;
					case "TRANSACTIONID":
						reader.Read();
						collectionReward.TransactionID = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return collectionReward;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static FlyOverNode ReadFlyOverNode(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			FlyOverNode flyOverNode = new FlyOverNode();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "X":
						reader.Read();
						flyOverNode.x = Convert.ToSingle(reader.Value);
						break;
					case "Y":
						reader.Read();
						flyOverNode.y = Convert.ToSingle(reader.Value);
						break;
					case "Z":
						reader.Read();
						flyOverNode.z = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return flyOverNode;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static BridgeScreenPosition ReadBridgeScreenPosition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			BridgeScreenPosition bridgeScreenPosition = new BridgeScreenPosition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "X":
						reader.Read();
						bridgeScreenPosition.x = Convert.ToSingle(reader.Value);
						break;
					case "Y":
						reader.Read();
						bridgeScreenPosition.y = Convert.ToSingle(reader.Value);
						break;
					case "Z":
						reader.Read();
						bridgeScreenPosition.z = Convert.ToSingle(reader.Value);
						break;
					case "ZOOM":
						reader.Read();
						bridgeScreenPosition.zoom = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return bridgeScreenPosition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static KampaiColor ReadKampaiColor(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			KampaiColor result = default(KampaiColor);
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "R":
						reader.Read();
						result.r = Convert.ToSingle(reader.Value);
						break;
					case "G":
						reader.Read();
						result.g = Convert.ToSingle(reader.Value);
						break;
					case "B":
						reader.Read();
						result.b = Convert.ToSingle(reader.Value);
						break;
					case "A":
						reader.Read();
						result.a = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return result;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static Reward ReadReward(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			Reward reward = new Reward();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "REQUIREDQUANTITY":
						reader.Read();
						reward.requiredQuantity = Convert.ToUInt32(reader.Value);
						break;
					case "PREMIUMREWARD":
						reader.Read();
						reward.premiumReward = Convert.ToUInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return reward;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MiniGameScoreReward ReadMiniGameScoreReward(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MiniGameScoreReward miniGameScoreReward = new MiniGameScoreReward();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "MINIGAMEID":
						reader.Read();
						miniGameScoreReward.MiniGameId = Convert.ToInt32(reader.Value);
						break;
					case "REWARDTABLE":
						reader.Read();
						miniGameScoreReward.rewardTable = PopulateList(reader, converters, ReadReward, miniGameScoreReward.rewardTable);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return miniGameScoreReward;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MiniGameScoreRange ReadMiniGameScoreRange(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MiniGameScoreRange miniGameScoreRange = new MiniGameScoreRange();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "MINIGAMEID":
						reader.Read();
						miniGameScoreRange.MiniGameId = Convert.ToInt32(reader.Value);
						break;
					case "SCORERANGEMAX":
						reader.Read();
						miniGameScoreRange.ScoreRangeMax = Convert.ToInt32(reader.Value);
						break;
					case "SCORERANGEMIN":
						reader.Read();
						miniGameScoreRange.ScoreRangeMin = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return miniGameScoreRange;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MasterPlanComponentRewardDefinition ReadMasterPlanComponentRewardDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MasterPlanComponentRewardDefinition masterPlanComponentRewardDefinition = new MasterPlanComponentRewardDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "REWARDITEMID":
						reader.Read();
						masterPlanComponentRewardDefinition.rewardItemId = Convert.ToInt32(reader.Value);
						break;
					case "REWARDQUANTITY":
						reader.Read();
						masterPlanComponentRewardDefinition.rewardQuantity = Convert.ToUInt32(reader.Value);
						break;
					case "GRINDREWARD":
						reader.Read();
						masterPlanComponentRewardDefinition.grindReward = Convert.ToUInt32(reader.Value);
						break;
					case "PREMIUMREWARD":
						reader.Read();
						masterPlanComponentRewardDefinition.premiumReward = Convert.ToUInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return masterPlanComponentRewardDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MasterPlanComponentTaskDefinition ReadMasterPlanComponentTaskDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MasterPlanComponentTaskDefinition masterPlanComponentTaskDefinition = new MasterPlanComponentTaskDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "REQUIREDITEMID":
						reader.Read();
						masterPlanComponentTaskDefinition.requiredItemId = Convert.ToInt32(reader.Value);
						break;
					case "REQUIREDQUANTITY":
						reader.Read();
						masterPlanComponentTaskDefinition.requiredQuantity = Convert.ToUInt32(reader.Value);
						break;
					case "SHOWWAYFINDER":
						reader.Read();
						masterPlanComponentTaskDefinition.ShowWayfinder = Convert.ToBoolean(reader.Value);
						break;
					case "TYPE":
						reader.Read();
						masterPlanComponentTaskDefinition.Type = ReadEnum<MasterPlanComponentTaskType>(reader);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return masterPlanComponentTaskDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static GhostFunctionDefinition ReadGhostFunctionDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			GhostFunctionDefinition ghostFunctionDefinition = new GhostFunctionDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "STARTTYPE":
						reader.Read();
						ghostFunctionDefinition.startType = ReadEnum<GhostComponentFunctionType>(reader);
						break;
					case "CLOSETYPE":
						reader.Read();
						ghostFunctionDefinition.closeType = ReadEnum<GhostFunctionCloseType>(reader);
						break;
					case "COMPONENTBUILDINGDEFID":
						reader.Read();
						ghostFunctionDefinition.componentBuildingDefID = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return ghostFunctionDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static KnuckleheadednessInfo ReadKnuckleheadednessInfo(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			KnuckleheadednessInfo knuckleheadednessInfo = new KnuckleheadednessInfo();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "KNUCKLEHEADDEDNESSMIN":
						reader.Read();
						knuckleheadednessInfo.KnuckleheaddednessMin = Convert.ToSingle(reader.Value);
						break;
					case "KNUCKLEHEADDEDNESSMAX":
						reader.Read();
						knuckleheadednessInfo.KnuckleheaddednessMax = Convert.ToSingle(reader.Value);
						break;
					case "KNUCKLEHEADDEDNESSSCALE":
						reader.Read();
						knuckleheadednessInfo.KnuckleheaddednessScale = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return knuckleheadednessInfo;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static AnimationAlternate ReadAnimationAlternate(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			AnimationAlternate animationAlternate = new AnimationAlternate();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "GROUPID":
						reader.Read();
						animationAlternate.GroupID = Convert.ToInt32(reader.Value);
						break;
					case "PERCENTCHANCE":
						reader.Read();
						animationAlternate.PercentChance = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return animationAlternate;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static CameraControlSettings ReadCameraControlSettings(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			CameraControlSettings cameraControlSettings = new CameraControlSettings();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "CUSTOMCAMERAPOSTIKI":
						reader.Read();
						cameraControlSettings.customCameraPosTiki = Convert.ToInt32(reader.Value);
						break;
					case "CUSTOMCAMERAPOSSTAGE":
						reader.Read();
						cameraControlSettings.customCameraPosStage = Convert.ToInt32(reader.Value);
						break;
					case "CUSTOMCAMERAPOSTOWNHALL":
						reader.Read();
						cameraControlSettings.customCameraPosTownHall = Convert.ToInt32(reader.Value);
						break;
					case "CUSTOMCAMERAPOSPARTYDEFAULT":
						reader.Read();
						cameraControlSettings.customCameraPosPartyDefault = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return cameraControlSettings;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static VFXAssetDefinition ReadVFXAssetDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			VFXAssetDefinition vFXAssetDefinition = new VFXAssetDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "LOCATION":
						reader.Read();
						vFXAssetDefinition.location = ReadLocation(reader, converters);
						break;
					case "PREFAB":
						reader.Read();
						vFXAssetDefinition.Prefab = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return vFXAssetDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MinionBenefit ReadMinionBenefit(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MinionBenefit minionBenefit = new MinionBenefit();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "LOCALIZEDKEY":
						reader.Read();
						minionBenefit.localizedKey = ReadString(reader, converters);
						break;
					case "ITEMICONID":
						reader.Read();
						minionBenefit.itemIconId = Convert.ToInt32(reader.Value);
						break;
					case "TYPE":
						reader.Read();
						minionBenefit.type = ReadEnum<Benefit>(reader);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return minionBenefit;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MinionBenefitLevel ReadMinionBenefitLevel(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MinionBenefitLevel minionBenefitLevel = new MinionBenefitLevel();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "DOUBLEDROPPERCENTAGE":
						reader.Read();
						minionBenefitLevel.doubleDropPercentage = Convert.ToSingle(reader.Value);
						break;
					case "DOUBLEDROPLEVEL":
						reader.Read();
						minionBenefitLevel.doubleDropLevel = Convert.ToInt32(reader.Value);
						break;
					case "PREMIUMDROPPERCENTAGE":
						reader.Read();
						minionBenefitLevel.premiumDropPercentage = Convert.ToSingle(reader.Value);
						break;
					case "PREMIUMDROPLEVEL":
						reader.Read();
						minionBenefitLevel.premiumDropLevel = Convert.ToInt32(reader.Value);
						break;
					case "RAREDROPPERCENTAGE":
						reader.Read();
						minionBenefitLevel.rareDropPercentage = Convert.ToSingle(reader.Value);
						break;
					case "RAREDROPLEVEL":
						reader.Read();
						minionBenefitLevel.rareDropLevel = Convert.ToInt32(reader.Value);
						break;
					case "TOKENSTOLEVEL":
						reader.Read();
						minionBenefitLevel.tokensToLevel = Convert.ToInt32(reader.Value);
						break;
					case "COSTUMEID":
						reader.Read();
						minionBenefitLevel.costumeId = Convert.ToInt32(reader.Value);
						break;
					case "IMAGE":
						reader.Read();
						minionBenefitLevel.image = ReadString(reader, converters);
						break;
					case "MASK":
						reader.Read();
						minionBenefitLevel.mask = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return minionBenefitLevel;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static ImageMaskCombo ReadImageMaskCombo(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			ImageMaskCombo result = default(ImageMaskCombo);
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "IMAGE":
						reader.Read();
						result.image = ReadString(reader, converters);
						break;
					case "MASK":
						reader.Read();
						result.mask = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return result;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static TransactionInstance ReadTransactionInstance(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			TransactionInstance transactionInstance = new TransactionInstance();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "ID":
						reader.Read();
						transactionInstance.ID = Convert.ToInt32(reader.Value);
						break;
					case "INPUTS":
						reader.Read();
						transactionInstance.Inputs = PopulateList(reader, converters, transactionInstance.Inputs);
						break;
					case "OUTPUTS":
						reader.Read();
						transactionInstance.Outputs = PopulateList(reader, converters, transactionInstance.Outputs);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return transactionInstance;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static QuestStepDefinition ReadQuestStepDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			QuestStepDefinition questStepDefinition = new QuestStepDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "TYPE":
						reader.Read();
						questStepDefinition.Type = ReadEnum<QuestStepType>(reader);
						break;
					case "ITEMAMOUNT":
						reader.Read();
						questStepDefinition.ItemAmount = Convert.ToInt32(reader.Value);
						break;
					case "ITEMDEFINITIONID":
						reader.Read();
						questStepDefinition.ItemDefinitionID = Convert.ToInt32(reader.Value);
						break;
					case "COSTUMEDEFINITIONID":
						reader.Read();
						questStepDefinition.CostumeDefinitionID = Convert.ToInt32(reader.Value);
						break;
					case "SHOWWAYFINDER":
						reader.Read();
						questStepDefinition.ShowWayfinder = Convert.ToBoolean(reader.Value);
						break;
					case "QUESTSTEPCOMPLETEPLAYERTRAININGCATEGORYITEMID":
						reader.Read();
						questStepDefinition.QuestStepCompletePlayerTrainingCategoryItemId = Convert.ToInt32(reader.Value);
						break;
					case "UPGRADELEVEL":
						reader.Read();
						questStepDefinition.UpgradeLevel = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return questStepDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static QuestChainStepDefinition ReadQuestChainStepDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			QuestChainStepDefinition questChainStepDefinition = new QuestChainStepDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "INTRO":
						reader.Read();
						questChainStepDefinition.Intro = ReadString(reader, converters);
						break;
					case "VOICE":
						reader.Read();
						questChainStepDefinition.Voice = ReadString(reader, converters);
						break;
					case "OUTRO":
						reader.Read();
						questChainStepDefinition.Outro = ReadString(reader, converters);
						break;
					case "XP":
						reader.Read();
						questChainStepDefinition.XP = Convert.ToInt32(reader.Value);
						break;
					case "GRIND":
						reader.Read();
						questChainStepDefinition.Grind = Convert.ToInt32(reader.Value);
						break;
					case "PREMIUM":
						reader.Read();
						questChainStepDefinition.Premium = Convert.ToInt32(reader.Value);
						break;
					case "TASKS":
						reader.Read();
						questChainStepDefinition.Tasks = PopulateList(reader, converters, ReadQuestChainTask, questChainStepDefinition.Tasks);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return questChainStepDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static QuestChainTask ReadQuestChainTask(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			QuestChainTask questChainTask = new QuestChainTask();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "TYPE":
						reader.Read();
						questChainTask.Type = ReadEnum<QuestChainTaskType>(reader);
						break;
					case "ITEM":
						reader.Read();
						questChainTask.Item = Convert.ToInt32(reader.Value);
						break;
					case "COUNT":
						reader.Read();
						questChainTask.Count = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return questChainTask;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static PlatformStoreSkuDefinition ReadPlatformStoreSkuDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			PlatformStoreSkuDefinition platformStoreSkuDefinition = new PlatformStoreSkuDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "APPLEAPPSTORE":
						reader.Read();
						platformStoreSkuDefinition.appleAppstore = ReadString(reader, converters);
						break;
					case "GOOGLEPLAY":
						reader.Read();
						platformStoreSkuDefinition.googlePlay = ReadString(reader, converters);
						break;
					case "DEFAULTSTORE":
						reader.Read();
						platformStoreSkuDefinition.defaultStore = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return platformStoreSkuDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static Vector3Serialize ReadVector3Serialize(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			Vector3Serialize vector3Serialize = new Vector3Serialize();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "X":
						reader.Read();
						vector3Serialize.x = Convert.ToInt32(reader.Value);
						break;
					case "Y":
						reader.Read();
						vector3Serialize.y = Convert.ToInt32(reader.Value);
						break;
					case "Z":
						reader.Read();
						vector3Serialize.z = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return vector3Serialize;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static SocialEventOrderDefinition ReadSocialEventOrderDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			SocialEventOrderDefinition socialEventOrderDefinition = new SocialEventOrderDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "ORDERID":
						reader.Read();
						socialEventOrderDefinition.OrderID = Convert.ToInt32(reader.Value);
						break;
					case "TRANSACTION":
						reader.Read();
						socialEventOrderDefinition.Transaction = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return socialEventOrderDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static TriggerRewardLayout ReadTriggerRewardLayout(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			TriggerRewardLayout triggerRewardLayout = new TriggerRewardLayout();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "INDEX":
						reader.Read();
						triggerRewardLayout.index = Convert.ToInt32(reader.Value);
						break;
					case "ITEMIDS":
						reader.Read();
						triggerRewardLayout.itemIds = PopulateListInt32(reader, triggerRewardLayout.itemIds);
						break;
					case "LAYOUT":
						reader.Read();
						triggerRewardLayout.layout = ReadEnum<TriggerRewardLayout.Layout>(reader);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return triggerRewardLayout;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static KampaiPendingTransaction ReadKampaiPendingTransaction(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			KampaiPendingTransaction kampaiPendingTransaction = new KampaiPendingTransaction();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "EXTERNALIDENTIFIER":
						reader.Read();
						kampaiPendingTransaction.ExternalIdentifier = ReadString(reader, converters);
						break;
					case "TRANSACTION":
						reader.Read();
						kampaiPendingTransaction.Transaction = ((converters.transactionDefinitionConverter == null) ? FastJSONDeserializer.Deserialize<TransactionDefinition>(reader, converters) : converters.transactionDefinitionConverter.ReadJson(reader, converters));
						break;
					case "TRANSACTIONINSTANCE":
						reader.Read();
						kampaiPendingTransaction.TransactionInstance = ReadTransactionInstance(reader, converters);
						break;
					case "STOREITEMDEFINITIONID":
						reader.Read();
						kampaiPendingTransaction.StoreItemDefinitionId = Convert.ToInt32(reader.Value);
						break;
					case "UTCTIMECREATED":
						reader.Read();
						kampaiPendingTransaction.UTCTimeCreated = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return kampaiPendingTransaction;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static UnlockedItem ReadUnlockedItem(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			UnlockedItem unlockedItem = new UnlockedItem();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "DEFID":
						reader.Read();
						unlockedItem.defID = Convert.ToInt32(reader.Value);
						break;
					case "QUANTITY":
						reader.Read();
						unlockedItem.quantity = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return unlockedItem;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static TrackedSale ReadTrackedSale(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			TrackedSale trackedSale = new TrackedSale();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "DEFID":
						reader.Read();
						trackedSale.defID = Convert.ToInt32(reader.Value);
						break;
					case "NUMBERPURCHASED":
						reader.Read();
						trackedSale.numberPurchased = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return trackedSale;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static SocialClaimRewardItem ReadSocialClaimRewardItem(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			SocialClaimRewardItem socialClaimRewardItem = new SocialClaimRewardItem();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "EVENTID":
						reader.Read();
						socialClaimRewardItem.eventID = Convert.ToInt32(reader.Value);
						break;
					case "CLAIMSTATE":
						reader.Read();
						socialClaimRewardItem.claimState = ReadEnum<SocialClaimRewardItem.ClaimState>(reader);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return socialClaimRewardItem;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static Player.HelpTipTrackingItem ReadHelpTipTrackingItem(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			Player.HelpTipTrackingItem result = default(Player.HelpTipTrackingItem);
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "TIPDIFINITIONID":
						reader.Read();
						result.tipDifinitionId = Convert.ToInt32(reader.Value);
						break;
					case "SHOWSCOUNT":
						reader.Read();
						result.showsCount = Convert.ToInt32(reader.Value);
						break;
					case "LASTSHOWNTIME":
						reader.Read();
						result.lastShownTime = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return result;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static QuestStep ReadQuestStep(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			QuestStep questStep = new QuestStep();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "STATE":
						reader.Read();
						questStep.state = ReadEnum<QuestStepState>(reader);
						break;
					case "AMOUNTCOMPLETED":
						reader.Read();
						questStep.AmountCompleted = Convert.ToInt32(reader.Value);
						break;
					case "AMOUNTREADY":
						reader.Read();
						questStep.AmountReady = Convert.ToInt32(reader.Value);
						break;
					case "TRACKEDID":
						reader.Read();
						questStep.TrackedID = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return questStep;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static OrderBoardTicket ReadOrderBoardTicket(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			OrderBoardTicket orderBoardTicket = new OrderBoardTicket();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "TRANSACTIONINST":
						reader.Read();
						orderBoardTicket.TransactionInst = ReadTransactionInstance(reader, converters);
						break;
					case "STARTGAMETIME":
						reader.Read();
						orderBoardTicket.StartGameTime = Convert.ToInt32(reader.Value);
						break;
					case "BOARDINDEX":
						reader.Read();
						orderBoardTicket.BoardIndex = Convert.ToInt32(reader.Value);
						break;
					case "ORDERNAMETABLEINDEX":
						reader.Read();
						orderBoardTicket.OrderNameTableIndex = Convert.ToInt32(reader.Value);
						break;
					case "STARTTIME":
						reader.Read();
						orderBoardTicket.StartTime = Convert.ToInt32(reader.Value);
						break;
					case "CHARACTERDEFINITIONID":
						reader.Read();
						orderBoardTicket.CharacterDefinitionId = Convert.ToInt32(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return orderBoardTicket;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static UserIdentity ReadUserIdentity(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			UserIdentity userIdentity = new UserIdentity();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "id":
						reader.Read();
						userIdentity.ID = ReadString(reader, converters);
						break;
					case "externalId":
						reader.Read();
						userIdentity.ExternalID = ReadString(reader, converters);
						break;
					case "userId":
						reader.Read();
						userIdentity.UserID = ReadString(reader, converters);
						break;
					case "type":
						reader.Read();
						userIdentity.Type = ReadEnum<IdentityType>(reader);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return userIdentity;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static SocialOrderProgress ReadSocialOrderProgress(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			SocialOrderProgress socialOrderProgress = new SocialOrderProgress();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "ORDERID":
						reader.Read();
						socialOrderProgress.OrderId = Convert.ToInt32(reader.Value);
						break;
					case "COMPLETEDBYUSERID":
						reader.Read();
						socialOrderProgress.CompletedByUserId = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return socialOrderProgress;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MasterPlanComponentReward ReadMasterPlanComponentReward(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MasterPlanComponentReward masterPlanComponentReward = new MasterPlanComponentReward();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "DEFINITION":
						reader.Read();
						masterPlanComponentReward.Definition = ReadMasterPlanComponentRewardDefinition(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return masterPlanComponentReward;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static MasterPlanComponentTask ReadMasterPlanComponentTask(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			MasterPlanComponentTask masterPlanComponentTask = new MasterPlanComponentTask();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "ISCOMPLETE":
						reader.Read();
						masterPlanComponentTask.isComplete = Convert.ToBoolean(reader.Value);
						break;
					case "EARNEDQUANTITY":
						reader.Read();
						masterPlanComponentTask.earnedQuantity = Convert.ToUInt32(reader.Value);
						break;
					case "DEFINITION":
						reader.Read();
						masterPlanComponentTask.Definition = ReadMasterPlanComponentTaskDefinition(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return masterPlanComponentTask;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static GachaConfig ReadGachaConfig(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			GachaConfig gachaConfig = new GachaConfig();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "GATCHAANIMATIONDEFINITIONS":
						reader.Read();
						gachaConfig.GatchaAnimationDefinitions = PopulateList(reader, converters, gachaConfig.GatchaAnimationDefinitions);
						break;
					case "DISTRIBUTIONTABLES":
						reader.Read();
						gachaConfig.DistributionTables = PopulateList(reader, converters, gachaConfig.DistributionTables);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return gachaConfig;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static TaskDefinition ReadTaskDefinition(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			TaskDefinition taskDefinition = new TaskDefinition();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "LEVELBANDS":
						reader.Read();
						taskDefinition.levelBands = PopulateList(reader, converters, taskDefinition.levelBands);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return taskDefinition;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static BucketAssignment ReadBucketAssignment(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			BucketAssignment bucketAssignment = new BucketAssignment();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "BUCKETID":
						reader.Read();
						bucketAssignment.BucketId = Convert.ToInt32(reader.Value);
						break;
					case "TIME":
						reader.Read();
						bucketAssignment.Time = Convert.ToSingle(reader.Value);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return bucketAssignment;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static PreloadableAsset ReadPreloadableAsset(JsonReader reader, JsonConverters converters = null)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			PreloadableAsset result = default(PreloadableAsset);
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
					switch (((string)reader.Value).ToUpper())
					{
					case "NAME":
						reader.Read();
						result.name = ReadString(reader, converters);
						break;
					case "TYPE":
						reader.Read();
						result.type = ReadString(reader, converters);
						break;
					default:
						reader.Skip();
						break;
					}
					break;
				case JsonToken.EndObject:
					return result;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static string ReadString(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			return Convert.ToString(reader.Value);
		}

		public static bool ReadBool(JsonReader reader, JsonConverters converters)
		{
			return Convert.ToBoolean(reader.Value);
		}

		public static Dictionary<string, Dictionary<string, string>> ReadDictionaryDictionaryString(JsonReader reader, JsonConverters converters)
		{
			return ReadDictionary(reader, converters, ReadDictionaryString);
		}

		public static Dictionary<string, string> ReadDictionaryString(JsonReader reader, JsonConverters converters)
		{
			return ReadDictionary(reader, converters, ReadString);
		}

		public static T ReadEnum<T>(JsonReader reader)
		{
			switch (reader.TokenType)
			{
			case JsonToken.PropertyName:
			case JsonToken.String:
				return (T)Enum.Parse(typeof(T), (string)reader.Value, true);
			case JsonToken.Integer:
				return (T)Enum.ToObject(typeof(T), reader.Value);
			default:
				throw new JsonSerializationException(string.Format("Unexpected can't read enum {0}. {1}", typeof(T), GetPositionInSource(reader)));
			}
		}

		public static Dictionary<string, string> ReadStringDictionary(JsonReader reader, JsonConverters converters = null)
		{
			return ReadDictionary(reader, converters, (JsonReader r, JsonConverters c) => (string)r.Value);
		}

		public static Dictionary<string, object> ReadDictionary(JsonReader reader)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			JObject jObject = JObject.Load(reader);
			JsonSerializer jsonSerializer = new JsonSerializer();
			return jsonSerializer.Deserialize<Dictionary<string, object>>(jObject.CreateReader());
		}

		public static Dictionary<string, object> ReadNestedDictionary(JsonReader reader, JsonConverters converters)
		{
			JObject token = JObject.Load(reader);
			return (Dictionary<string, object>)ReadNestedObject(token);
		}

		private static object ReadNestedObject(JToken token)
		{
			switch (token.Type)
			{
			case JTokenType.Object:
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				{
					foreach (JProperty item in token.Children<JProperty>())
					{
						dictionary.Add(item.Name, ReadNestedObject(item.Value));
					}
					return dictionary;
				}
			}
			case JTokenType.Array:
			{
				List<object> list = new List<object>();
				{
					foreach (JProperty item2 in token.Children<JProperty>())
					{
						list.Add(ReadNestedObject(item2.Value));
					}
					return list;
				}
			}
			default:
				return ((JValue)token).Value;
			}
		}

		public static Dictionary<string, T> ReadDictionary<T>(JsonReader reader, JsonConverters converters = null) where T : IFastJSONDeserializable, new()
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			Dictionary<string, T> dictionary = new Dictionary<string, T>();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string key = (string)reader.Value;
					reader.Read();
					T value = FastJSONDeserializer.Deserialize<T>(reader, converters);
					dictionary.Add(key, value);
					break;
				}
				case JsonToken.EndObject:
					return dictionary;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static Dictionary<string, T> ReadDictionary<T>(JsonReader reader, JsonConverters converters, Func<JsonReader, JsonConverters, T> valueReader)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			Dictionary<string, T> dictionary = new Dictionary<string, T>();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					string key = (string)reader.Value;
					reader.Read();
					T value = valueReader(reader, converters);
					dictionary.Add(key, value);
					break;
				}
				case JsonToken.EndObject:
					return dictionary;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static Dictionary<K, V> ReadDictionary<K, V>(JsonReader reader, JsonConverters converters, Func<JsonReader, JsonConverters, K> keyReader, Func<JsonReader, JsonConverters, V> valueReader)
		{
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			Dictionary<K, V> dictionary = new Dictionary<K, V>();
			EnsureToken(JsonToken.StartObject, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.PropertyName:
				{
					K key = keyReader(reader, converters);
					reader.Read();
					V value = valueReader(reader, converters);
					dictionary.Add(key, value);
					break;
				}
				case JsonToken.EndObject:
					return dictionary;
				default:
					throw new JsonSerializationException(string.Format("Unexpected token when deserializing object: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException("Unexpected end when deserializing object.");
		}

		public static List<List<int>> ReadListOfIntLists(JsonReader reader, JsonConverters converters)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			List<List<int>> list = new List<List<int>>();
			EnsureToken(JsonToken.StartArray, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.EndArray:
					return list;
				case JsonToken.Comment:
					continue;
				}
				List<int> item = PopulateListInt32(reader);
				list.Add(item);
			}
			throw new JsonSerializationException(string.Format("Unexpected end when deserializing list. {0}", GetPositionInSource(reader)));
		}

		public static List<T> PopulateList<T>(JsonReader reader, JsonConverters converters, Func<JsonReader, JsonConverters, T> elementReader, IEnumerable<T> existingValue = null)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			List<T> list = ((existingValue == null) ? new List<T>() : new List<T>(existingValue));
			EnsureToken(JsonToken.StartArray, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.EndArray:
					return list;
				case JsonToken.Comment:
					continue;
				}
				T item = elementReader(reader, converters);
				list.Add(item);
			}
			throw new JsonSerializationException(string.Format("Unexpected end when deserializing list. {0}", GetPositionInSource(reader)));
		}

		public static List<T> PopulateList<T>(JsonReader reader, JsonConverters converters, FastJsonConverter<T> converter, IEnumerable<T> existingValue = null) where T : class, IFastJSONDeserializable
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			List<T> list = ((existingValue == null) ? new List<T>() : new List<T>(existingValue));
			EnsureToken(JsonToken.StartArray, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.EndArray:
					return list;
				case JsonToken.Comment:
					continue;
				}
				T item = converter.ReadJson(reader, converters);
				list.Add(item);
			}
			throw new JsonSerializationException(string.Format("Unexpected end when deserializing list. {0}", GetPositionInSource(reader)));
		}

		public static List<T> PopulateList<T>(JsonReader reader, JsonConverters converters = null, IEnumerable<T> existingValue = null) where T : IFastJSONDeserializable, new()
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			if (reader.TokenType == JsonToken.None)
			{
				reader.Read();
			}
			List<T> list = ((existingValue == null) ? new List<T>() : new List<T>(existingValue));
			EnsureToken(JsonToken.StartArray, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.EndArray:
					return list;
				case JsonToken.Comment:
					continue;
				}
				T item = new T();
				item.Deserialize(reader, converters);
				list.Add(item);
			}
			throw new JsonSerializationException(string.Format("Unexpected end when deserializing list. {0}", GetPositionInSource(reader)));
		}

		public static List<string> PopulateListString(JsonReader reader, IEnumerable<string> existingValue = null)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			List<string> list = ((existingValue == null) ? new List<string>() : new List<string>(existingValue));
			EnsureToken(JsonToken.StartArray, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.EndArray:
					return list;
				case JsonToken.String:
				{
					string item = (string)reader.Value;
					list.Add(item);
					break;
				}
				default:
					throw new JsonSerializationException(string.Format("Unexpected element type on list when deserializiong string list: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException(string.Format("Unexpected end when deserializing string list. {0}", GetPositionInSource(reader)));
		}

		public static List<int> PopulateListInt32(JsonReader reader, IEnumerable<int> existingValue = null)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			List<int> list = ((existingValue == null) ? new List<int>() : new List<int>(existingValue));
			EnsureToken(JsonToken.StartArray, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.EndArray:
					return list;
				case JsonToken.Integer:
				{
					int item = Convert.ToInt32(reader.Value);
					list.Add(item);
					break;
				}
				default:
					throw new JsonSerializationException(string.Format("Unexpected element type on list when deserializiong int list: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException(string.Format("Unexpected end when deserializing string list. {0}", GetPositionInSource(reader)));
		}

		public static List<bool> PopulateListBoolean(JsonReader reader, IEnumerable<bool> existingValue = null)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			List<bool> list = ((existingValue == null) ? new List<bool>() : new List<bool>(existingValue));
			EnsureToken(JsonToken.StartArray, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.EndArray:
					return list;
				case JsonToken.Boolean:
				{
					bool item2 = Convert.ToBoolean(reader.Value);
					list.Add(item2);
					break;
				}
				case JsonToken.Integer:
				{
					bool item = Convert.ToBoolean(reader.Value);
					list.Add(item);
					break;
				}
				default:
					throw new JsonSerializationException(string.Format("Unexpected element type on list when deserializiong float list: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException(string.Format("Unexpected end when deserializing string list. {0}", GetPositionInSource(reader)));
		}

		public static List<float> PopulateListSingle(JsonReader reader, IEnumerable<float> existingValue = null)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			List<float> list = ((existingValue == null) ? new List<float>() : new List<float>(existingValue));
			EnsureToken(JsonToken.StartArray, reader);
			while (reader.Read())
			{
				switch (reader.TokenType)
				{
				case JsonToken.EndArray:
					return list;
				case JsonToken.Float:
				{
					float item2 = Convert.ToSingle(reader.Value);
					list.Add(item2);
					break;
				}
				case JsonToken.Integer:
				{
					float item = Convert.ToSingle(reader.Value);
					list.Add(item);
					break;
				}
				default:
					throw new JsonSerializationException(string.Format("Unexpected element type on list when deserializiong float list: {0}. {1}", reader.TokenType, GetPositionInSource(reader)));
				case JsonToken.Comment:
					break;
				}
			}
			throw new JsonSerializationException(string.Format("Unexpected end when deserializing string list. {0}", GetPositionInSource(reader)));
		}

		public static void EnsureToken(JsonToken token, JsonReader reader)
		{
			if (reader.TokenType != token)
			{
				throw new JsonSerializationException(string.Format("Expected token {0}. Encountered {1} instead. {2}", token, reader.TokenType, GetPositionInSource(reader)));
			}
		}

		public static T ReaderNotImplemented<T>(JsonReader reader, JsonConverters converters = null)
		{
			throw new JsonSerializationException("Reading of this entity is not implemented.");
		}

		public static string GetPositionInSource(JsonReader reader)
		{
			JsonTextReader jsonTextReader = reader as JsonTextReader;
			if (jsonTextReader != null)
			{
				return string.Format("Line number: {0}, Line position: {1}", jsonTextReader.LineNumber, jsonTextReader.LinePosition);
			}
			return "Line number: -, Line position: -";
		}

		public static Dictionary<ConfigurationDefinition.RateAppAfterEvent, bool> ReadRateAppTriggerConfig(JsonReader reader, JsonConverters converters)
		{
			return ReadDictionary(reader, converters, ReadRateAppAfterEvent, ReadBool);
		}

		public static ConfigurationDefinition.RateAppAfterEvent ReadRateAppAfterEvent(JsonReader reader, JsonConverters converters)
		{
			return ReadEnum<ConfigurationDefinition.RateAppAfterEvent>(reader);
		}

		public static KillSwitch ReadKillSwitch(JsonReader reader, JsonConverters converters)
		{
			return ReadEnum<KillSwitch>(reader);
		}
	}
}
