using System.Collections;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class RewardedAdPickRewardModalMediator : UIStackMediator<RewardedAdPickRewardModalView>
	{
		private const float DOOBER_ANIMATION_TIME_SEC = 1f;

		private string prefabName;

		private AdPlacementInstance adPlacementInstance;

		private QuantityItem[] items;

		private int selectedIndex = -1;

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IRewardedAdService rewardedAdService { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public SpawnDooberSignal spawnDooberSignal { get; set; }

		[Inject(MainElement.UI_DOOBER_CANVAS)]
		public GameObject dooberCanvas { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public SpawnDooberModel spawnDooberModel { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		public override void OnRegister()
		{
			base.closeAllOtherMenuSignal.Dispatch(null);
			base.OnRegister();
			base.view.Init(localService, definitionService, randomService);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.CollectButton.onClick.AddListener(OnCollect);
			int num = 0;
			Button[] boxes = base.view.Boxes;
			foreach (Button button in boxes)
			{
				int index_ = num++;
				button.onClick.AddListener(delegate
				{
					OnBox(index_);
				});
			}
		}

		public override void OnRemove()
		{
			base.OnRemove();
			CleanupListeners();
		}

		private void CleanupListeners()
		{
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.CollectButton.onClick.RemoveListener(OnCollect);
			Button[] boxes = base.view.Boxes;
			foreach (Button button in boxes)
			{
				button.onClick.RemoveAllListeners();
			}
		}

		public override void Initialize(GUIArguments args)
		{
			prefabName = args.Get<string>();
			items = args.Get<QuantityItem[]>();
			adPlacementInstance = args.Get<AdPlacementInstance>();
			uiModel.DisableBack = true;
		}

		protected override void Close()
		{
			uiModel.DisableBack = false;
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			base.view.Close();
		}

		private void OnMenuClose()
		{
			hideSkrimSignal.Dispatch("RewardedAdPickReward");
			guiService.Execute(GUIOperation.Unload, prefabName);
		}

		private void OnCollect()
		{
			if (!base.view.IsAnimationPlaying("Close"))
			{
				TransactionDefinition transactionDefinition = new TransactionDefinition();
				transactionDefinition.Inputs = new List<QuantityItem>();
				transactionDefinition.Outputs = new List<QuantityItem> { items[0] };
				TransactionDefinition transactionDefinition2 = transactionDefinition;
				rewardedAdService.RewardPlayer(transactionDefinition2, adPlacementInstance);
				telemetryService.Send_Telemetry_EVT_AD_INTERACTION(adPlacementInstance.Definition.Name, transactionDefinition2.Outputs, adPlacementInstance.RewardPerPeriodCount);
				ItemDefinition itemDefinition;
				int rewardAmount;
				if (RewardedAdUtil.GetFirstItemDefintion(transactionDefinition2.Outputs, out itemDefinition, out rewardAmount, definitionService))
				{
					SpawnRewardDoober(transactionDefinition2, itemDefinition, rewardAmount);
				}
				base.view.Close();
			}
		}

		private void OnBox(int index)
		{
			if (selectedIndex == -1)
			{
				selectedIndex = index;
				base.view.Select(selectedIndex, items);
			}
		}

		private void SpawnRewardDoober(TransactionDefinition rewardTransaction, ItemDefinition rewardItem, int rewardAmount)
		{
			GameObject original = KampaiResources.Load<GameObject>("rewardedAdReward");
			GameObject gameObject = Object.Instantiate(original);
			gameObject.transform.SetParent(dooberCanvas.transform, false);
			Transform parent = base.view.Boxes[selectedIndex].transform.parent;
			spawnDooberModel.rewardedAdDooberSpawnLocation = parent.position;
			gameObject.transform.position = parent.position;
			RewardedAdRewardView component = gameObject.GetComponent<RewardedAdRewardView>();
			component.Init(rewardItem, rewardAmount);
			routineRunner.StartCoroutine(ProcessDoobers(component, rewardTransaction));
		}

		private IEnumerator ProcessDoobers(RewardedAdRewardView view, TransactionDefinition rewardTransaction)
		{
			yield return new WaitForSeconds(1f);
			spawnDooberModel.RewardedAdDooberMode = true;
			DooberUtil.CheckForTween(rewardTransaction, new List<KampaiImage> { view.RewardItemImage }, true, uiCamera, spawnDooberSignal, definitionService);
			spawnDooberModel.RewardedAdDooberMode = false;
			yield return new WaitForEndOfFrame();
			Object.Destroy(view.gameObject);
		}
	}
}
