using Kampai.Game.View;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public interface NamedCharacter : Character, Instance, IFastJSONDeserializable, IFastJSONSerializable, Identifiable, Prestigable
	{
		new NamedCharacterDefinition Definition { get; }

		[JsonIgnore]
		bool Created { get; set; }

		NamedCharacterObject Setup(GameObject go);
	}
	public abstract class NamedCharacter<T> : Character<T>, Character, NamedCharacter, Instance, IFastJSONDeserializable, IFastJSONSerializable, Identifiable, Prestigable where T : NamedCharacterDefinition
	{
		NamedCharacterDefinition NamedCharacter.Definition
		{
			get
			{
				return base.Definition;
			}
		}

		[JsonIgnore]
		public bool Created { get; set; }

		protected NamedCharacter(T def)
			: base(def)
		{
		}

		public abstract NamedCharacterObject Setup(GameObject go);
	}
}
