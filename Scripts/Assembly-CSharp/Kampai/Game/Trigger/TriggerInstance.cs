using System;
using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public abstract class TriggerInstance<TDefinition> : IIsTriggerable, IGameTimeTracker, TriggerInstance, IComparable<TriggerInstance>, IEquatable<TriggerInstance>, IComparable<TriggerInstance<TDefinition>>, IEquatable<TriggerInstance<TDefinition>>, IFastJSONDeserializable, IFastJSONSerializable where TDefinition : TriggerDefinition
	{
		private IList<int> m_recievedRewardIds = new List<int>();

		TriggerDefinition TriggerInstance.Definition
		{
			get
			{
				return Definition;
			}
		}

		public int ID
		{
			get
			{
				int result;
				if (Definition == null)
				{
					result = -1;
				}
				else
				{
					TDefinition definition = Definition;
					result = definition.ID;
				}
				return result;
			}
		}

		public IList<int> RecievedRewardIds
		{
			get
			{
				return m_recievedRewardIds;
			}
			set
			{
				m_recievedRewardIds = value;
			}
		}

		public int StartGameTime { get; set; }

		public TDefinition Definition { get; protected set; }

		protected TriggerInstance(TDefinition definition)
		{
			Definition = definition;
			if (definition != null)
			{
			}
		}

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
			default:
			{
				int num;
				if (num == 1)
				{
					reader.Read();
					StartGameTime = Convert.ToInt32(reader.Value);
					break;
				}
				return false;
			}
			case "RECIEVEDREWARDIDS":
				reader.Read();
				RecievedRewardIds = ReaderUtil.PopulateListInt32(reader, RecievedRewardIds);
				break;
			}
			return true;
		}

		public virtual void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			SerializeProperties(writer);
			writer.WriteEndObject();
		}

		protected virtual void SerializeProperties(JsonWriter writer)
		{
			FastTriggerInstanceSerializationHelper.SerializeTriggerInstanceData(writer, this);
		}

		public void OnDefinitionHotSwap(TriggerDefinition definition)
		{
			Definition = definition as TDefinition;
		}

		public abstract void RewardPlayer(IPlayerService playerService);

		public virtual bool IsTriggered(ICrossContextCapable gameContext)
		{
			if (StartGameTime > 0)
			{
				return false;
			}
			TDefinition definition = Definition;
			return definition.IsTriggered(gameContext);
		}

		public override string ToString()
		{
			return string.Format("{0}, ID: {1}, Start GameTime: {2}, {3}", base.ToString(), ID, StartGameTime, Definition);
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			TriggerInstance<TDefinition> triggerInstance = obj as TriggerInstance<TDefinition>;
			return !object.ReferenceEquals(null, triggerInstance) && CompareTo(triggerInstance) == 0;
		}

		public override int GetHashCode()
		{
			return new { Definition, StartGameTime, ID }.GetHashCode();
		}

		public bool Equals(TriggerInstance<TDefinition> obj)
		{
			return obj != null && Equals((object)obj);
		}

		public bool Equals(TriggerInstance obj)
		{
			return obj != null && Equals((object)obj);
		}

		public int CompareTo(TriggerInstance other)
		{
			if (other == null)
			{
				return 1;
			}
			TDefinition definition = Definition;
			int num = definition.CompareTo(other.Definition);
			if (num != 0)
			{
				return num;
			}
			int num2 = other.StartGameTime.CompareTo(StartGameTime);
			if (num2 != 0)
			{
				return num2;
			}
			return ID.CompareTo(other.ID);
		}

		public virtual int CompareTo(TriggerInstance<TDefinition> other)
		{
			return (other == null) ? 1 : CompareTo((TriggerInstance)other);
		}
	}
	[RequiresJsonConverter]
	[Serializer("FastTriggerInstanceSerializationHelper.SerializeTriggerInstanceData")]
	public interface TriggerInstance : IIsTriggerable, IGameTimeTracker, IComparable<TriggerInstance>, IEquatable<TriggerInstance>, IFastJSONDeserializable, IFastJSONSerializable
	{
		int ID { get; }

		IList<int> RecievedRewardIds { get; set; }

		TriggerDefinition Definition { get; }

		void OnDefinitionHotSwap(TriggerDefinition definition);
	}
}
