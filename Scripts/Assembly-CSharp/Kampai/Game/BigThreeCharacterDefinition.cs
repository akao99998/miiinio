namespace Kampai.Game
{
	public class BigThreeCharacterDefinition : NamedCharacterDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1072;
			}
		}

		public override Instance Build()
		{
			return new BigThreeCharacter(this);
		}
	}
}
