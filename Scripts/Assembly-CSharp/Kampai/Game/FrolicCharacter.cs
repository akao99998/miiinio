using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.Game
{
	public interface FrolicCharacter : Character, NamedCharacter, Instance, IFastJSONDeserializable, IFastJSONSerializable, Identifiable, Prestigable
	{
		[JsonIgnore]
		FloatLocation CurrentFrolicLocation { get; set; }

		new FrolicCharacterDefinition Definition { get; }
	}
	public abstract class FrolicCharacter<T> : NamedCharacter<T>, Character, FrolicCharacter, NamedCharacter, Instance, IFastJSONDeserializable, IFastJSONSerializable, Identifiable, Prestigable where T : FrolicCharacterDefinition
	{
		FrolicCharacterDefinition FrolicCharacter.Definition
		{
			get
			{
				return base.Definition;
			}
		}

		[JsonIgnore]
		public FloatLocation CurrentFrolicLocation { get; set; }

		protected FrolicCharacter(T def)
			: base(def)
		{
		}
	}
}
