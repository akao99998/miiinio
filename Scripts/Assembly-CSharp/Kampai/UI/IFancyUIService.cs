using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.UI.View;
using UnityEngine;

namespace Kampai.UI
{
	public interface IFancyUIService
	{
		DummyCharacterObject CreateCharacter(DummyCharacterType type, DummyCharacterAnimationState startingState, Transform parent, Vector3 villainScale, Vector3 villainPositionOffset, int prestigeDefinitionID = 0, bool isHighLOD = true, bool isAudible = true, bool adjustMaterial = false);

		void SetKampaiImage(KampaiImage image, string iconPath, string maskPath);

		DummyCharacterObject BuildMinion(int minionId, DummyCharacterAnimationState startingState, Transform parent, bool isHighLOD = true, bool isAudible = true, int minionLevel = 0);

		BuildingObject CreateDummyBuildingObject(BuildingDefinition buildingDefinition, GameObject parent, out Building building, IList<MinionObject> minionsList = null, bool isAudible = true);

		void ReleaseBuildingObject(BuildingObject buildingObject, Building building, IList<MinionObject> minionsList = null);

		DummyCharacterType GetCharacterType(int prestigeDefinitionID);

		void SetStenciledShaderOnBuilding(GameObject buildingObject);
	}
}
