using Kampai.Game;
using strange.extensions.command.impl;

public class DefinitionsFetchedCommand : Command
{
	public override void Execute()
	{
		DefinitionService.DeleteBinarySerialization();
	}
}
