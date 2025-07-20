using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public interface IScaffoldingPart
	{
		GameObject GameObject { get; }

		void Init(Building building, IKampaiLogger logger, IDefinitionService definitionService);
	}
}
