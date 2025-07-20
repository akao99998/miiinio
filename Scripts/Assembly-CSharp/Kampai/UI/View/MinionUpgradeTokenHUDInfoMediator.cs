using Kampai.Game;
using Kampai.Main;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class MinionUpgradeTokenHUDInfoMediator : Mediator
	{
		[Inject]
		public MinionUpgradeTokenHUDInfoView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public TokenDooberCompleteSignal tokenDooberCompleteSignal { get; set; }

		[Inject]
		public TokenDooberHasBeenSpawnedSignal tokenDooberHasBeenSpawnedSignal { get; set; }

		[Inject]
		public SetHUDTokenAmountSignal setHUDTokenAmountSignal { get; set; }

		public override void OnRegister()
		{
			view.Init();
			tokenDooberCompleteSignal.AddListener(RecievedToken);
			tokenDooberHasBeenSpawnedSignal.AddListener(SlideIn);
			setHUDTokenAmountSignal.AddListener(UpdateAmount);
			UpdateAmount();
		}

		public override void OnRemove()
		{
			tokenDooberCompleteSignal.RemoveListener(RecievedToken);
			tokenDooberHasBeenSpawnedSignal.RemoveListener(SlideIn);
			setHUDTokenAmountSignal.RemoveListener(UpdateAmount);
		}

		private void UpdateAmount()
		{
			int quantityByDefinitionId = (int)playerService.GetQuantityByDefinitionId(50);
			string @string = localizationService.GetString("QuantityItemFormat", quantityByDefinitionId);
			view.SetText(@string);
		}

		private void SlideIn()
		{
			view.animator.Play("Open");
		}

		private void RecievedToken()
		{
			UpdateAmount();
			SlideOut();
		}

		private void SlideOut()
		{
			view.animator.Play("Close");
		}
	}
}
