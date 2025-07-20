using System.IO;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class PlatformTriggerConditionDefinition : TriggerConditionDefinition
	{
		private Boxed<RuntimePlatform> testValue;

		public override int TypeCode
		{
			get
			{
				return 1162;
			}
		}

		public string Platform { get; set; }

		public override TriggerConditionType.Identifier type
		{
			get
			{
				return TriggerConditionType.Identifier.Platform;
			}
		}

		public PlatformTriggerConditionDefinition()
		{
		}

		public PlatformTriggerConditionDefinition(RuntimePlatform testValue)
		{
			this.testValue = new Boxed<RuntimePlatform>(testValue);
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			BinarySerializationUtil.WriteString(writer, Platform);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			Platform = BinarySerializationUtil.ReadString(reader);
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "PLATFORM":
				reader.Read();
				Platform = ReaderUtil.ReadString(reader, converters);
				return true;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}, Operator: {1}, Type: {2}, Platform: {3}", GetType(), base.conditionOp, type, Platform);
		}

		public override bool IsTriggered(ICrossContextCapable gameContext)
		{
			IKampaiLogger kampaiLogger = LogManager.GetClassLogger("PlatformTriggerConditionDefinition") as IKampaiLogger;
			if (string.IsNullOrEmpty(Platform))
			{
				kampaiLogger.Error("No platform.");
				return false;
			}
			RuntimePlatform runtimePlatform = ((testValue != null) ? testValue.Value : Application.platform);
			string text = StringUtil.UnifiedPlatformName(runtimePlatform);
			if (text == null)
			{
				kampaiLogger.Error("Unknown platform {0}", runtimePlatform);
				return false;
			}
			return TestOperator(Platform.ToLower(), text.ToLower());
		}
	}
}
