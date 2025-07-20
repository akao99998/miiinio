using System.Collections;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class QuestDialogMediator : UIStackMediator<QuestDialogView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("QuestDialogMediator") as IKampaiLogger;

		private QuestDialogSetting mySetting;

		private int currentQuestId;

		private int currentQuestStep;

		private IEnumerator pulseCoroutine;

		[Inject]
		public ShowPreviousQuestDialogSignal showPreviousDialog { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public PromptReceivedSignal promptReceivedSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public QuestDialogDismissedSignal questDismissedSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.Init(localizationService);
			base.view.QuestButton.ClickedSignal.AddListener(Close);
			showPreviousDialog.AddListener(ShowPreviousDialog);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.QuestButton.ClickedSignal.RemoveListener(Close);
			showPreviousDialog.RemoveListener(ShowPreviousDialog);
		}

		public override void Initialize(GUIArguments args)
		{
			string localizedKey = args.Get<string>();
			QuestDialogSetting setting = args.Get<QuestDialogSetting>();
			Tuple<int, int> tuple = args.Get<Tuple<int, int>>();
			ShowQuestDialog(localizedKey, setting, tuple);
			pulseCoroutine = BeginArrowPulse();
			StartCoroutine(pulseCoroutine);
		}

		internal void ShowPreviousDialog()
		{
			base.view.ShowPreviousDialog();
			if (mySetting.dialogSound.Length > 0)
			{
				soundFXSignal.Dispatch(mySetting.dialogSound);
			}
		}

		internal void ShowQuestDialog(string localizedKey, QuestDialogSetting setting, Tuple<int, int> tuple)
		{
			currentQuestId = tuple.Item1;
			currentQuestStep = tuple.Item2;
			base.gameObject.transform.SetAsLastSibling();
			string string2;
			if (setting.additionalStringParameter.Length != 0)
			{
				string @string = localizationService.GetString(setting.additionalStringParameter);
				string2 = localizationService.GetString(localizedKey, @string);
			}
			else
			{
				string2 = localizationService.GetString(localizedKey);
			}
			CheckSettingUpdate(setting);
			soundFXSignal.Dispatch("Play_menu_popUp_02");
			base.view.ShowDialog(string2, logger);
		}

		private void CheckSettingUpdate(QuestDialogSetting setting)
		{
			if (mySetting == null)
			{
				mySetting = new QuestDialogSetting();
			}
			if (mySetting.type != setting.type)
			{
				mySetting.type = setting.type;
			}
			if (mySetting.definitionID != setting.definitionID)
			{
				string dialogIcon = "icn_nav_story_mask";
				if (setting.definitionID != 0)
				{
					QuestResourceDefinition questResourceDefinition = prestigeService.DetermineQuestResourceDefinition(setting.definitionID, CharacterImageType.WayfinderIcon);
					if (questResourceDefinition != null && !string.IsNullOrEmpty(questResourceDefinition.maskPath))
					{
						dialogIcon = questResourceDefinition.maskPath;
					}
				}
				base.view.SetDialogIcon(dialogIcon);
				mySetting.definitionID = setting.definitionID;
			}
			if (mySetting.type == QuestDialogType.MINIONREWARD)
			{
				base.view.SetDialogIcon("icn_minionCall_mask");
			}
			else if (mySetting.type == QuestDialogType.NEWPRESTIGE)
			{
				base.view.SetDialogIcon("icn_nav_orderboard_mask");
			}
			mySetting.dialogSound = setting.dialogSound;
		}

		private IEnumerator BeginArrowPulse()
		{
			yield return new WaitForSeconds(3f);
			base.view.NextArrow.GetComponent<Animator>().CrossFade("anim_dialog_arrow", 0.25f);
		}

		protected override void OnCloseAllMenu(GameObject exception)
		{
		}

		protected override void Close()
		{
			if (pulseCoroutine != null)
			{
				StopCoroutine(pulseCoroutine);
				pulseCoroutine = null;
			}
			if (base.view.IsPageOver())
			{
				IGUICommand command = guiService.BuildCommand(GUIOperation.Unload, "screen_Dialog");
				guiService.Execute(command);
				questDismissedSignal.Dispatch();
				promptReceivedSignal.Dispatch(currentQuestId, currentQuestStep);
			}
			else
			{
				base.view.UpdateDialog();
			}
		}
	}
}
