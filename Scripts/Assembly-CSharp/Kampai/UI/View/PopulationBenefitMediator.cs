using System.Collections;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class PopulationBenefitMediator : Mediator
	{
		private Coroutine dooberCoroutine;

		[Inject]
		public PopulationBenefitView view { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public SpawnDooberSignal spawnDooberSignal { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public SpawnDooberModel dooberModel { get; set; }

		public override void OnRegister()
		{
			view.Init(definitionService, localizationService, playerService);
			view.updateSignal.AddListener(CheckTriggerDoober);
			CheckTriggerDoober();
		}

		public override void OnRemove()
		{
			view.updateSignal.RemoveListener(CheckTriggerDoober);
		}

		private void CheckTriggerDoober()
		{
			if (dooberCoroutine != null)
			{
				StopCoroutine(dooberCoroutine);
				dooberCoroutine = null;
			}
			dooberCoroutine = StartCoroutine(TriggerDoober());
		}

		private IEnumerator TriggerDoober()
		{
			yield return 0;
			if (view.benefitDefinitionID == dooberModel.PendingPopulationDoober)
			{
				PopulationBenefitDefinition benefitDefinition = definitionService.Get<PopulationBenefitDefinition>(view.benefitDefinitionID);
				TransactionDefinition transDef = definitionService.Get<TransactionDefinition>(benefitDefinition.transactionDefinitionID);
				DestinationType destType = ((transDef.Outputs[0].ID != 335) ? DestinationType.STORAGE_POPULATION_GOAL : DestinationType.TIMER_POPULATION_GOAL);
				spawnDooberSignal.Dispatch(uiCamera.WorldToScreenPoint(view.PopulationIcon.transform.position), destType, transDef.Outputs[0].ID, false);
				dooberModel.PendingPopulationDoober = 0;
			}
		}
	}
}
