using Kampai.Game;
using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Util
{
	public interface IMinionBuilder
	{
		MinionObject BuildMinion(CostumeItemDefinition costume, string animatorStateMachine, GameObject parent = null, bool showShadow = true);

		void SetLOD(TargetPerformance targetPerformance);

		TargetPerformance GetLOD();

		void RebuildMinion(GameObject minion);
	}
}
