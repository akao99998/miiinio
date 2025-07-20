using Kampai.Game.Transaction;
using Kampai.Game.View;
using UnityEngine;

namespace Kampai.Game
{
	public interface IMasterPlanService
	{
		MasterPlan CurrentMasterPlan { get; }

		void Initialize();

		void CreateMasterPlanComponents(MasterPlan masterPlan);

		MasterPlan CreateNewMasterPlan();

		bool HasReceivedInitialRewardFromCurrentPlan();

		bool HasReceivedInitialRewardFromPlanDefinition(MasterPlanDefinition planDefinition);

		Vector3 GetComponentBuildingOffset(int buildingID);

		Vector3 GetComponentBuildingPosition(int buildingID);

		bool ForceNextMPDefinition(int defID);

		void SelectMasterPlanComponent(MasterPlanComponent component);

		void ProcessTransactionData(TransactionUpdateData data);

		void ProcessActiveComponent(MasterPlanComponentTaskType type, uint progress, int source = 0);

		void AddMasterPlanObject(MasterPlanObject obj);

		bool AllComponentsAreComplete(int masterPlanDefinitionID);

		int GetComponentCompleteCount(MasterPlanDefinition definition);

		void SetWayfinderState();

		MasterPlanComponent GetActiveComponentFromPlanDefinition(int masterPlanDefinitionID);
	}
}
