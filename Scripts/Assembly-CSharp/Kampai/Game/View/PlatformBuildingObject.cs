using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class PlatformBuildingObject : MonoBehaviour, IScaffoldingPart
	{
		public GameObject GameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		public void Init(Building building, IKampaiLogger logger, IDefinitionService definitionService)
		{
		}
	}
}
