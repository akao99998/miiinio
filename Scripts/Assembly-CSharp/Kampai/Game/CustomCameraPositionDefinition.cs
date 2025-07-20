using System;
using System.IO;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public class CustomCameraPositionDefinition : Definition
	{
		public override int TypeCode
		{
			get
			{
				return 1071;
			}
		}

		public float xPos { get; set; }

		public float yPos { get; set; }

		public float zPos { get; set; }

		public float xRotation { get; set; }

		public float yRotation { get; set; }

		public float zRotation { get; set; }

		public float FOV { get; set; }

		public float nearClip { get; set; }

		public float farClip { get; set; }

		public bool enableCameraControl { get; set; }

		public string panSound { get; set; }

		public float duration { get; set; }

		public CustomCameraPositionDefinition()
		{
			duration = 1f;
		}

		public override void Write(BinaryWriter writer)
		{
			base.Write(writer);
			writer.Write(xPos);
			writer.Write(yPos);
			writer.Write(zPos);
			writer.Write(xRotation);
			writer.Write(yRotation);
			writer.Write(zRotation);
			writer.Write(FOV);
			writer.Write(nearClip);
			writer.Write(farClip);
			writer.Write(enableCameraControl);
			BinarySerializationUtil.WriteString(writer, panSound);
			writer.Write(duration);
		}

		public override void Read(BinaryReader reader)
		{
			base.Read(reader);
			xPos = reader.ReadSingle();
			yPos = reader.ReadSingle();
			zPos = reader.ReadSingle();
			xRotation = reader.ReadSingle();
			yRotation = reader.ReadSingle();
			zRotation = reader.ReadSingle();
			FOV = reader.ReadSingle();
			nearClip = reader.ReadSingle();
			farClip = reader.ReadSingle();
			enableCameraControl = reader.ReadBoolean();
			panSound = BinarySerializationUtil.ReadString(reader);
			duration = reader.ReadSingle();
		}

		protected override bool DeserializeProperty(string propertyName, JsonReader reader, JsonConverters converters)
		{
			switch (propertyName)
			{
			case "XPOS":
				reader.Read();
				xPos = Convert.ToSingle(reader.Value);
				break;
			case "YPOS":
				reader.Read();
				yPos = Convert.ToSingle(reader.Value);
				break;
			case "ZPOS":
				reader.Read();
				zPos = Convert.ToSingle(reader.Value);
				break;
			case "XROTATION":
				reader.Read();
				xRotation = Convert.ToSingle(reader.Value);
				break;
			case "YROTATION":
				reader.Read();
				yRotation = Convert.ToSingle(reader.Value);
				break;
			case "ZROTATION":
				reader.Read();
				zRotation = Convert.ToSingle(reader.Value);
				break;
			case "FOV":
				reader.Read();
				FOV = Convert.ToSingle(reader.Value);
				break;
			case "NEARCLIP":
				reader.Read();
				nearClip = Convert.ToSingle(reader.Value);
				break;
			case "FARCLIP":
				reader.Read();
				farClip = Convert.ToSingle(reader.Value);
				break;
			case "ENABLECAMERACONTROL":
				reader.Read();
				enableCameraControl = Convert.ToBoolean(reader.Value);
				break;
			case "PANSOUND":
				reader.Read();
				panSound = ReaderUtil.ReadString(reader, converters);
				break;
			case "DURATION":
				reader.Read();
				duration = Convert.ToSingle(reader.Value);
				break;
			default:
				return base.DeserializeProperty(propertyName, reader, converters);
			}
			return true;
		}
	}
}
