using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.Telemetry;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class QuestScriptController
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("QuestScriptController") as IKampaiLogger;

		private readonly Signal<int> timerDoneSignal = new Signal<int>();

		private QuestScriptInstance questScriptInstance;

		public readonly Signal ContinueSignal = new Signal();

		private readonly QuestDialogSetting currentDialogSetting = new QuestDialogSetting();

		private readonly SignalListener signalListener = new SignalListener();

		[Inject]
		public QuestScriptKernel kernel { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IVideoService videoService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public ITikiBarService tikiBarService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IOrderBoardService orderBoardService { get; set; }

		[Inject]
		public UIModel uiModel { get; set; }

		[Inject]
		public DeselectAllMinionsSignal deselectAllMinionsSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSoundSignal { get; set; }

		[Inject]
		public MoveBuildMenuSignal moveBuildMenuSignal { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		[Inject]
		public OpenBuildingMenuSignal openBuildingMenuSignal { get; set; }

		[Inject]
		public ShowTipsSignal showTipsSignal { get; set; }

		[Inject]
		public EnableCameraBehaviourSignal enableCameraBehaviourSignal { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraBehaviourSignal { get; set; }

		[Inject]
		public SelectMinionSignal selectMinionSignal { get; set; }

		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		[Inject]
		public SetLimitTikiBarWayFindersSignal setLimitTikiBarWayFindersSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPickService pickService { get; set; }

		[Inject]
		public IPlayerTrainingService playerTrainingService { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displayPlayerTrainingSignal { get; set; }

		[Inject]
		public UnlockMinionsSignal unlockMinionsSignal { get; set; }

		[Inject]
		public UnlockVillainLairSignal unlockVillainLairSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		public void Setup(QuestScriptInstance questScriptInstance)
		{
			this.questScriptInstance = questScriptInstance;
			kernel.AddSignalListener(signalListener);
		}

		public void Stop()
		{
			if (questScriptInstance == null)
			{
				logger.Info("QuestScriptController::Stop: Stopping even though it appears setup was never called. (null questScriptInstance)");
				return;
			}
			logger.Info("QuestScriptController::Stop: Stopping quest id {0}", questScriptInstance.QuestID);
			signalListener.Clear();
			kernel.RemoveSignalListener(signalListener);
		}

		private void nextInstruction()
		{
			ContinueSignal.Dispatch();
		}

		[QuestScriptAPI("wait")]
		public bool wait(IArgRetriever args, ReturnValueContainer ret)
		{
			float @float = args.GetFloat(1);
			routineRunner.StartCoroutine(waitFinish(@float, nextInstruction));
			return false;
		}

		private IEnumerator waitFinish(float seconds, Action onFinish)
		{
			yield return new WaitForSeconds(seconds);
			onFinish();
		}

		[QuestScriptAPI("waitTimer")]
		public bool waitTimer(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			int value = timeService.CurrentTime();
			timeEventService.AddEvent(questScriptInstance.QuestID, Convert.ToInt32(value), @int, timerDoneSignal);
			timerDoneSignal.AddListener(receive_waitTimerDone);
			return false;
		}

		private void receive_waitTimerDone(int questId)
		{
			timerDoneSignal.RemoveListener(receive_waitTimerDone);
			nextInstruction();
		}

		[QuestScriptAPI("seedPremiumCurrency")]
		public bool seedPremiumCurrency(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			playerService.AlterQuantity(StaticItem.PREMIUM_CURRENCY_ID, @int);
			uiContext.injectionBinder.GetInstance<SpawnDooberSignal>().Dispatch(Vector3.zero, DestinationType.PREMIUM, -1, false);
			return true;
		}

		[QuestScriptAPI("moveMenu")]
		public bool moveMenu(IArgRetriever args, ReturnValueContainer ret)
		{
			moveBuildMenuSignal.Dispatch(args.GetBoolean(1));
			return true;
		}

		[QuestScriptAPI("playVideo")]
		public bool playVideo(IArgRetriever args, ReturnValueContainer ret)
		{
			string @string = args.GetString(1);
			videoService.playVideo(@string, true, false);
			return true;
		}

		[QuestScriptAPI("deselectAllMinions")]
		public bool deselectAllMinions(IArgRetriever args, ReturnValueContainer ret)
		{
			deselectAllMinionsSignal.Dispatch();
			return true;
		}

		[QuestScriptAPI("getInstanceId")]
		public bool getInstanceId(IArgRetriever args, ReturnValueContainer ret)
		{
			int value = 0;
			int @int = args.GetInt(1);
			IList<Instance> instancesByDefinitionID = playerService.GetInstancesByDefinitionID(@int);
			if (instancesByDefinitionID != null && instancesByDefinitionID.Count > 0)
			{
				value = instancesByDefinitionID[0].ID;
			}
			ret.Set(value);
			return true;
		}

		[QuestScriptAPI("getDefinitionId")]
		public bool getDefinitionId(IArgRetriever args, ReturnValueContainer ret)
		{
			int value = 0;
			int @int = args.GetInt(1);
			Instance byInstanceId = playerService.GetByInstanceId<Instance>(@int);
			if (byInstanceId != null)
			{
				value = byInstanceId.Definition.ID;
			}
			ret.Set(value);
			return true;
		}

		[QuestScriptAPI("playSound")]
		public bool playSound(IArgRetriever args, ReturnValueContainer ret)
		{
			internalPlaySound(args.GetString(1));
			return true;
		}

		[QuestScriptAPI("hasSeenPlayerTraining")]
		public bool hasSeenPlayerTraining(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			ret.Set(playerTrainingService.HasSeen(@int, PlayerTrainingVisiblityType.GAME));
			return true;
		}

		[QuestScriptAPI("sendTelemetry")]
		public bool sendTelemetry(IArgRetriever args, ReturnValueContainer ret)
		{
			string @string = args.GetString(1);
			switch (@string)
			{
			default:
			{
				int num;
				if (num == 1)
				{
					string string3 = args.GetString(2);
					telemetryService.Send_Telemetry_EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL(questScriptInstance.QuestLocalizedKey + "." + string3, TelemetryAchievementType.QuestStep, 0, string.Empty);
				}
				else
				{
					ERROR(string.Format("Unknown telemetry event type at line {0} ({1})", "undefined", @string));
				}
				break;
			}
			case "tutorial":
			{
				string string2 = args.GetString(2);
				telemetryService.Send_Telemetry_EVT_USER_TUTORIAL_FUNNEL_EAL(questScriptInstance.QuestLocalizedKey, string2);
				break;
			}
			}
			return true;
		}

		[QuestScriptAPI("setDialogType")]
		public bool setDialogType(IArgRetriever args, ReturnValueContainer ret)
		{
			currentDialogSetting.type = QuestDialogType.NORMAL;
			return true;
		}

		[QuestScriptAPI("setCharacterImage")]
		public bool setCharacterImage(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			currentDialogSetting.definitionID = @int;
			return true;
		}

		[QuestScriptAPI("sitPhilAtBar")]
		public bool sitPhilAtBar(IArgRetriever args, ReturnValueContainer ret)
		{
			gameContext.injectionBinder.GetInstance<PhilSitAtBarSignal>().Dispatch(true);
			return true;
		}

		[QuestScriptAPI("setDialogSound")]
		public bool setDialogSound(IArgRetriever args, ReturnValueContainer ret)
		{
			string @string = args.GetString(1);
			if (@string.CompareTo("None") == 0)
			{
				currentDialogSetting.dialogSound = string.Empty;
			}
			else
			{
				currentDialogSetting.dialogSound = @string;
			}
			return true;
		}

		[QuestScriptAPI("closeAllDialogs")]
		public bool closeAllDialogs(IArgRetriever args, ReturnValueContainer ret)
		{
			uiContext.injectionBinder.GetInstance<CloseAllOtherMenuSignal>().Dispatch(null);
			return true;
		}

		[QuestScriptAPI("showDialog")]
		public bool showDialog(IArgRetriever args, ReturnValueContainer ret)
		{
			uiContext.injectionBinder.GetInstance<CloseAllOtherMenuSignal>().Dispatch(null);
			string @string = args.GetString(1);
			Tuple<int, int> type = new Tuple<int, int>(questScriptInstance.QuestID, questScriptInstance.QuestStepID);
			gameContext.injectionBinder.GetInstance<ShowDialogSignal>().Dispatch(@string, currentDialogSetting, type);
			return true;
		}

		[QuestScriptAPI("openGameDialog")]
		public bool openGameDialog(IArgRetriever args, ReturnValueContainer ret)
		{
			string @string = args.GetString(1);
			string string2 = args.GetString(2);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, @string);
			if (!string.IsNullOrEmpty(string2))
			{
				iGUICommand.skrimScreen = string2;
				iGUICommand.darkSkrim = false;
			}
			guiService.Execute(iGUICommand);
			return true;
		}

		[QuestScriptAPI("awardBuilding")]
		public bool awardBuilding(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			gameContext.injectionBinder.GetInstance<CreateBuildingInInventorySignal>().Dispatch(@int);
			return true;
		}

		[QuestScriptAPI("setBuildingState")]
		public bool setBuildingState(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			BuildingState int2 = (BuildingState)args.GetInt(2);
			gameContext.injectionBinder.GetInstance<BuildingChangeStateSignal>().Dispatch(@int, int2);
			return true;
		}

		[QuestScriptAPI("getBuildingState")]
		public bool getBuildingState(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			Building byInstanceId = playerService.GetByInstanceId<Building>(@int);
			if (byInstanceId != null)
			{
				ret.Set((int)byInstanceId.State);
			}
			else
			{
				ret.Set(-1);
			}
			return true;
		}

		[QuestScriptAPI("showWayFinder")]
		public bool showWayFinder(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			bool boolean = args.GetBoolean(2);
			WayFinderSettings wayFinderSettings = new WayFinderSettings(@int);
			if (boolean)
			{
				createWayFinderSignal.Dispatch(wayFinderSettings);
			}
			else
			{
				removeWayFinderSignal.Dispatch(wayFinderSettings.TrackedId);
			}
			return true;
		}

		[QuestScriptAPI("setLimitTikiBarWayFinders")]
		public bool setLimitTikiBarWayFinders(IArgRetriever args, ReturnValueContainer ret)
		{
			bool boolean = args.GetBoolean(1);
			setLimitTikiBarWayFindersSignal.Dispatch(boolean);
			return true;
		}

		[QuestScriptAPI("showSettingsMenuButton")]
		public bool showSettingsMenuButton(IArgRetriever args, ReturnValueContainer ret)
		{
			bool boolean = args.GetBoolean(1);
			uiContext.injectionBinder.GetInstance<ShowSettingsButtonSignal>().Dispatch(boolean);
			return true;
		}

		[QuestScriptAPI("showTraining")]
		public bool showTraining(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			displayPlayerTrainingSignal.Dispatch(@int, false, new Signal<bool>());
			return true;
		}

		[QuestScriptAPI("debug")]
		public bool debugFunction(IArgRetriever args, ReturnValueContainer ret)
		{
			Debug(args.GetString(1));
			return true;
		}

		private void Debug(string message)
		{
			logger.Log(KampaiLogLevel.Error, true, string.Format("Script Message: {0}", message));
		}

		private void SubStep()
		{
			nextInstruction();
		}

		[QuestScriptAPI("defaultTimeout")]
		public bool GetDefaultTimeout(IArgRetriever args, ReturnValueContainer ret)
		{
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(33301);
			int result = -1;
			string description = itemDefinition.Description;
			int.TryParse(description, out result);
			if (result < 0)
			{
				logger.Error("Failed to get default timeout from definitions!");
				result = 5;
			}
			ret.Set(result);
			return true;
		}

		[QuestScriptAPI("cameraDefault")]
		public bool cameraDefault(IArgRetriever args, ReturnValueContainer ret)
		{
			gameContext.injectionBinder.GetInstance<CameraAutoZoomSignal>().Dispatch(0.3f);
			return true;
		}

		[QuestScriptAPI("minionsSelected")]
		public bool minionsSelected(IArgRetriever args, ReturnValueContainer ret)
		{
			PickState pickState = pickService.GetPickState();
			IList<int> list = pickState.MinionsSelected;
			if (list.Count > 0)
			{
				foreach (int item in list)
				{
					ret.SetKey(item.ToString()).Set(true);
				}
			}
			else
			{
				ret.SetNil();
			}
			return true;
		}

		private void internalPlaySound(string soundEvent)
		{
			globalSoundSignal.Dispatch(soundEvent);
		}

		[QuestScriptAPI("waitSignal")]
		public bool waitSignal(IArgRetriever args, ReturnValueContainer ret)
		{
			string signalName = args.GetString(1);
			float @float = args.GetFloat(2);
			CompleteSignal completeSignal = new CompleteSignal();
			Signal timeoutSignal = new Signal();
			Action<string, bool> handleCallback = null;
			object @lock = new object();
			bool callbackExecuted = false;
			Action<string> onComplete = delegate(string name)
			{
				handleCallback(name, true);
			};
			Action onTimeout = delegate
			{
				handleCallback(signalName, false);
			};
			handleCallback = delegate(string name, bool retVal)
			{
				lock (@lock)
				{
					if (!callbackExecuted)
					{
						callbackExecuted = true;
						if (!retVal)
						{
							ret.SetNil();
						}
						completeSignal.RemoveListener(onComplete);
						timeoutSignal.RemoveListener(onTimeout);
						nextInstruction();
					}
				}
			};
			completeSignal.AddListener(onComplete);
			signalListener.ListenForSignal(signalName, completeSignal, ret);
			if (@float > 0f)
			{
				timeoutSignal.AddListener(onTimeout);
				Action onFinish = delegate
				{
					timeoutSignal.Dispatch();
				};
				routineRunner.StartCoroutine(waitFinish(@float, onFinish));
			}
			return false;
		}

		[QuestScriptAPI("waitAnySignal")]
		public bool waitAnySignal(IArgRetriever args, ReturnValueContainer ret)
		{
			int count = args.Length;
			List<string> signals = new List<string>();
			ReturnValueContainer nameRet = ret.PushIndex();
			ReturnValueContainer ret2 = ret.PushIndex();
			CompleteSignal completeSignal = new CompleteSignal();
			Action<string, bool> handleCallback = null;
			object @lock = new object();
			bool callbackExecuted = false;
			Action<string> onComplete = delegate(string name)
			{
				handleCallback(name, true);
			};
			handleCallback = delegate(string name, bool retVal)
			{
				lock (@lock)
				{
					if (!callbackExecuted)
					{
						callbackExecuted = true;
						if (!retVal)
						{
							ret.SetNil();
						}
						nameRet.Set(name);
						completeSignal.RemoveListener(onComplete);
						for (int j = 0; j < count; j++)
						{
							string name2 = signals[j];
							signalListener.StopListeningForSignal(name2);
						}
						nextInstruction();
					}
				}
			};
			completeSignal.AddListener(onComplete);
			for (int i = 1; i <= count; i++)
			{
				string @string = args.GetString(i);
				signals.Add(@string);
				signalListener.ListenForSignal(@string, completeSignal, ret2);
			}
			return false;
		}

		[QuestScriptAPI("checkCoppa")]
		public bool checkCoppa(IArgRetriever args, ReturnValueContainer ret)
		{
			ret.Set(coppaService.IsBirthdateKnown());
			return true;
		}

		[QuestScriptAPI("getPlayerLevel")]
		public bool getPlayerLevel(IArgRetriever args, ReturnValueContainer ret)
		{
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			ret.Set(quantity);
			return true;
		}

		[QuestScriptAPI("getPlayerItemQuantity")]
		public bool getPlayerItemQuantity(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			int quantityByDefinitionId = (int)playerService.GetQuantityByDefinitionId(@int);
			ret.Set(quantityByDefinitionId);
			return true;
		}

		[QuestScriptAPI("getMinionCount")]
		public bool getMinionCount(IArgRetriever args, ReturnValueContainer ret)
		{
			int minionCount = playerService.GetMinionCount();
			ret.Set(minionCount);
			return true;
		}

		[QuestScriptAPI("getPlayerQuantity")]
		public bool getPlayerQuantity(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			int countByDefinitionId = playerService.GetCountByDefinitionId(@int);
			ret.Set(countByDefinitionId);
			return true;
		}

		[QuestScriptAPI("alterPlayerQuantity")]
		public bool alterPlayerQuantity(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			int int2 = args.GetInt(2);
			StaticItem def = (StaticItem)@int;
			playerService.AlterQuantity(def, int2);
			return true;
		}

		[QuestScriptAPI("displayMinionLevelTokenDoober")]
		public bool displayMinionLevelTokenDoober(IArgRetriever args, ReturnValueContainer ret)
		{
			VillainLairEntranceBuilding byInstanceId = playerService.GetByInstanceId<VillainLairEntranceBuilding>(374);
			gameContext.injectionBinder.GetInstance<SpawnDooberSignal>().Dispatch(new Vector3(byInstanceId.Location.x, 0f, byInstanceId.Location.y), DestinationType.MINION_LEVEL_TOKEN, 31, true);
			return true;
		}

		[QuestScriptAPI("gotoInGame")]
		public bool gotoInGame(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			int int2 = args.GetInt(2);
			int int3 = args.GetInt(3);
			bool boolean = args.GetBoolean(4);
			gameContext.injectionBinder.GetInstance<GotoSignal>().Dispatch(new GotoArgument(@int, int2, int3, boolean));
			return true;
		}

		[QuestScriptAPI("getBuildingsOfType")]
		public bool getBuildingsOfType(IArgRetriever args, ReturnValueContainer ret)
		{
			BuildingType.BuildingTypeIdentifier @int = (BuildingType.BuildingTypeIdentifier)args.GetInt(1);
			IList<Building> instancesByType = playerService.GetInstancesByType<Building>();
			ret.SetEmptyArray();
			int i = 0;
			for (int count = instancesByType.Count; i < count; i++)
			{
				Building building = instancesByType[i];
				if (building.Definition.Type == @int)
				{
					ret.PushIndex().Set(building.ID);
				}
			}
			return true;
		}

		[QuestScriptAPI("isQuestComplete")]
		public bool isQuestComplete(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			ret.PushIndex().Set(questService.IsQuestCompleted(@int) ? 1 : 0);
			return true;
		}

		[QuestScriptAPI("isStuartQuesting")]
		public bool isStuartQuesting(IArgRetriever args, ReturnValueContainer ret)
		{
			StuartCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StuartCharacter>(70001);
			if (firstInstanceByDefinitionId == null)
			{
				ret.Set(0);
				return true;
			}
			if (firstInstanceByDefinitionId == null)
			{
				ret.Set(0);
				return true;
			}
			Prestige prestigeForSeatableCharacter = tikiBarService.GetPrestigeForSeatableCharacter(firstInstanceByDefinitionId);
			if (prestigeForSeatableCharacter.state != PrestigeState.InQueue)
			{
				ret.Set(1);
			}
			else
			{
				ret.Set(0);
			}
			return true;
		}

		[QuestScriptAPI("isLeisureMenuOpen")]
		public bool isLeisureMenuOpen(IArgRetriever args, ReturnValueContainer ret)
		{
			if (uiModel.LeisureMenuOpen)
			{
				ret.Set(1);
			}
			else
			{
				ret.Set(0);
			}
			return true;
		}

		[QuestScriptAPI("markVillainIslandAsUnlocked")]
		public bool markVillainIslandasUnlocked(IArgRetriever args, ReturnValueContainer ret)
		{
			gameContext.injectionBinder.GetInstance<EnableVillainIslandCollidersSignal>().Dispatch(false);
			return true;
		}

		[QuestScriptAPI("highlightStoreItem")]
		public bool highlightStoreItem(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			bool boolean = args.GetBoolean(2);
			uiContext.injectionBinder.GetInstance<OpenStoreHighlightItemSignal>().Dispatch(@int, boolean);
			return true;
		}

		[QuestScriptAPI("throbGoToButton")]
		public bool throbGoToButton(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.AddToArguments(new ThrobGotoButton());
			}
			else
			{
				guiService.RemoveFromArguments(typeof(ThrobGotoButton));
			}
			return true;
		}

		[QuestScriptAPI("throbPlacementButton")]
		public bool throbPlacementButton(IArgRetriever args, ReturnValueContainer ret)
		{
			bool boolean = args.GetBoolean(1);
			questService.SetPulseMoveBuildingAccept(boolean);
			return true;
		}

		[QuestScriptAPI("throbDeliverButton")]
		public bool throbDeliverButton(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.AddToArguments(new ThrobDeliverButton());
			}
			else
			{
				guiService.RemoveFromArguments(typeof(ThrobDeliverButton));
			}
			return true;
		}

		[QuestScriptAPI("enablePurchaseButton")]
		public bool enablePurchaseButton(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.RemoveFromArguments(typeof(DisablePurchaseButton));
			}
			else
			{
				guiService.AddToArguments(new DisablePurchaseButton());
			}
			return true;
		}

		[QuestScriptAPI("highlightCrafting")]
		public bool highlightCrafting(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.AddToArguments(new ThrobCraftingButton());
			}
			else
			{
				guiService.RemoveFromArguments(typeof(ThrobCraftingButton));
			}
			return true;
		}

		[QuestScriptAPI("highlightHarvest")]
		public bool highlightHarvest(IArgRetriever args, ReturnValueContainer ret)
		{
			bool boolean = args.GetBoolean(1);
			routineRunner.StartCoroutine(HarvestHighlight(boolean));
			return true;
		}

		private IEnumerator HarvestHighlight(bool highlight)
		{
			yield return null;
			yield return null;
			uiContext.injectionBinder.GetInstance<HighlightHarvestButtonSignal>().Dispatch(highlight);
		}

		[QuestScriptAPI("highlightFillOrder")]
		public bool highlightFillOrder(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				uiContext.injectionBinder.GetInstance<OrderBoardHighLightFillOrderSignal>().Dispatch();
			}
			return true;
		}

		[QuestScriptAPI("highlightTicket")]
		public bool highlightTicket(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.AddToArguments(new ThrobTicketButton());
			}
			else
			{
				guiService.RemoveFromArguments(typeof(ThrobTicketButton));
			}
			return true;
		}

		[QuestScriptAPI("setOrderBoardText")]
		public bool setOrderBoardText(IArgRetriever args, ReturnValueContainer ret)
		{
			string @string = args.GetString(1);
			routineRunner.StartCoroutine(TicketFTUEText(@string));
			return true;
		}

		private IEnumerator TicketFTUEText(string title)
		{
			yield return null;
			yield return null;
			uiContext.injectionBinder.GetInstance<SetFTUETextSignal>().Dispatch(title);
		}

		[QuestScriptAPI("showXPHighlight")]
		public bool showXPHighlight(IArgRetriever args, ReturnValueContainer ret)
		{
			bool boolean = args.GetBoolean(1);
			uiContext.injectionBinder.GetInstance<XPFTUEHighlightSignal>().Dispatch(boolean);
			return true;
		}

		[QuestScriptAPI("getMinions")]
		public bool getMinions(IArgRetriever args, ReturnValueContainer ret)
		{
			List<Minion> idleMinions = playerService.GetIdleMinions();
			for (int i = 0; i < idleMinions.Count; i++)
			{
				ReturnValueContainer returnValueContainer = ret.PushIndex();
				returnValueContainer.Set(idleMinions[i].ID);
			}
			return true;
		}

		[QuestScriptAPI("rewardPlayer")]
		public bool rewardPlayer(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			int int2 = args.GetInt(2);
			runDynamicTransaction(int2, @int);
			return true;
		}

		[QuestScriptAPI("addMinions")]
		public bool addMinions(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			if (@int > 0)
			{
				TransactionDefinition transactionDefinition = new TransactionDefinition();
				transactionDefinition.ID = int.MaxValue;
				transactionDefinition.Inputs = new List<QuantityItem>();
				transactionDefinition.Outputs = new List<QuantityItem>();
				transactionDefinition.Outputs.Add(new QuantityItem(5, (uint)@int));
				playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.QUEST_REWARD, null);
				unlockMinionsSignal.Dispatch();
			}
			return true;
		}

		[QuestScriptAPI("runTransaction")]
		public bool runTransaction(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			TransactionDefinition def = definitionService.Get<TransactionDefinition>(@int);
			playerService.RunEntireTransaction(def, TransactionTarget.NO_VISUAL, null);
			uiContext.injectionBinder.GetInstance<UpdateUIButtonsSignal>().Dispatch(false);
			return true;
		}

		private void runDynamicTransaction(int grind = 0, int xp = 0)
		{
			if (xp > 0 || grind > 0)
			{
				TransactionDefinition transactionDefinition = new TransactionDefinition();
				transactionDefinition.ID = int.MaxValue;
				transactionDefinition.Inputs = new List<QuantityItem>();
				transactionDefinition.Outputs = new List<QuantityItem>();
				if (grind > 0)
				{
					transactionDefinition.Outputs.Add(new QuantityItem(0, (uint)grind));
				}
				if (xp > 0)
				{
					transactionDefinition.Outputs.Add(new QuantityItem(2, (uint)xp));
				}
				playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.NO_VISUAL, null);
				IList<Instance> instancesByDefinitionID = playerService.GetInstancesByDefinitionID(3041);
				TikiBarBuilding tikiBarBuilding = instancesByDefinitionID[0] as TikiBarBuilding;
				Vector3 type = new Vector3(tikiBarBuilding.Location.x, 0f, tikiBarBuilding.Location.y);
				uiContext.injectionBinder.GetInstance<SpawnDooberSignal>().Dispatch(type, DestinationType.XP, -1, true);
			}
		}

		[QuestScriptAPI("seedVillainLairRepairGoods")]
		public bool seedVillainLairRepairGoods(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			TransactionTarget target = TransactionTarget.HARVEST;
			DestinationType type = DestinationType.STORAGE;
			KevinCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<KevinCharacter>(70003);
			GameObject instance = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.NAMED_CHARACTER_MANAGER);
			NamedCharacterManagerView component = instance.GetComponent<NamedCharacterManagerView>();
			NamedCharacterObject namedCharacterObject = component.Get(firstInstanceByDefinitionId.ID);
			Vector3 position = namedCharacterObject.gameObject.transform.position;
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(@int);
			playerService.RunEntireTransaction(transactionDefinition, target, null);
			for (int i = 0; i < transactionDefinition.Outputs.Count; i++)
			{
				int iD = transactionDefinition.Outputs[i].ID;
				if (iD != 0 && iD != 2 && iD != 1)
				{
					uiContext.injectionBinder.GetInstance<SpawnDooberSignal>().Dispatch(position, type, transactionDefinition.Outputs[i].ID, true);
				}
			}
			return true;
		}

		[QuestScriptAPI("unlockVillainLair")]
		public bool unlockVillainLair(IArgRetriever args, ReturnValueContainer ret)
		{
			VillainLairEntranceBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<VillainLairEntranceBuilding>(3132);
			unlockVillainLairSignal.Dispatch(firstInstanceByDefinitionId, 3137);
			return true;
		}

		[QuestScriptAPI("localPersistanceHasKey")]
		public bool localPersistanceHasKey(IArgRetriever args, ReturnValueContainer ret)
		{
			string @string = args.GetString(1);
			ret.Set(localPersistService.HasKey(@string));
			return true;
		}

		[QuestScriptAPI("moveMinion")]
		public bool moveMinion(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			float @float = args.GetFloat(2);
			float float2 = args.GetFloat(3);
			Vector3 value = default(Vector3);
			value.x = @float;
			value.y = 0f;
			value.z = float2;
			Boxed<Vector3> param = new Boxed<Vector3>(value);
			selectMinionSignal.Dispatch(@int, param, false);
			return true;
		}

		[QuestScriptAPI("openTikiHut")]
		public bool openTikiHut(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				gameContext.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.IN, BuildingZoomType.TIKIBAR));
			}
			else
			{
				gameContext.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.OUT, BuildingZoomType.TIKIBAR));
			}
			return true;
		}

		[QuestScriptAPI("taskMinion")]
		public bool taskMinion(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			int int2 = args.GetInt(2);
			GameObject instance = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.MINION_MANAGER);
			MinionManagerView component = instance.GetComponent<MinionManagerView>();
			MinionObject second = component.Get(@int);
			gameContext.injectionBinder.GetInstance<StartMinionTaskSignal>().Dispatch(new Tuple<int, MinionObject, int>(int2, second, timeService.CurrentTime()));
			return true;
		}

		[QuestScriptAPI("getTaskedMinionCount")]
		public bool getTaskedMinionCount(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			TaskableBuilding byInstanceId = playerService.GetByInstanceId<TaskableBuilding>(@int);
			int minionsInBuilding = byInstanceId.GetMinionsInBuilding();
			ret.Set(minionsInBuilding);
			return true;
		}

		[QuestScriptAPI("showMessageBox")]
		public bool showMessageBox(IArgRetriever args, ReturnValueContainer ret)
		{
			string @string = args.GetString(1);
			string string2 = localService.GetString(@string);
			popupMessageSignal.Dispatch(string2, PopupMessageType.NORMAL);
			return true;
		}

		[QuestScriptAPI("showTips")]
		public bool showTips(IArgRetriever args, ReturnValueContainer ret)
		{
			string @string = args.GetString(1);
			int @int = args.GetInt(2);
			IList<Minion> idleMinions = playerService.GetIdleMinions();
			int count = idleMinions.Count;
			if (count < @int)
			{
				return true;
			}
			showTipsSignal.Dispatch(@string);
			return true;
		}

		[QuestScriptAPI("getStoreItemIdFromDefinition")]
		public bool getStoreItemIdFromDefinition(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			StoreItemType value = StoreItemType.BaseResource;
			IList<StoreItemDefinition> all = definitionService.GetAll<StoreItemDefinition>();
			foreach (StoreItemDefinition item in all)
			{
				if (item.ReferencedDefID == @int)
				{
					value = item.Type;
				}
			}
			ret.Set((int)value);
			return true;
		}

		[QuestScriptAPI("moveStoreToMainMenu")]
		public bool moveStoreToMainMenu(IArgRetriever args, ReturnValueContainer ret)
		{
			uiContext.injectionBinder.GetInstance<MoveTabMenuSignal>().Dispatch(true);
			return true;
		}

		[QuestScriptAPI("enableCameraControls")]
		public bool enableCameraControls(IArgRetriever args, ReturnValueContainer ret)
		{
			enableCameraBehaviourSignal.Dispatch(7);
			return true;
		}

		[QuestScriptAPI("disableCameraControls")]
		public bool disableCameraControls(IArgRetriever args, ReturnValueContainer ret)
		{
			disableCameraBehaviourSignal.Dispatch(7);
			return true;
		}

		[QuestScriptAPI("panToBuildingAndOpen")]
		public bool panToBuildingAndOpen(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			if (@int > 0)
			{
				Building byInstanceId = playerService.GetByInstanceId<Building>(@int);
				if (byInstanceId != null)
				{
					BuildingManagerView component = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER).GetComponent<BuildingManagerView>();
					BuildingObject buildingObject = component.GetBuildingObject(@int);
					if (buildingObject != null)
					{
						openBuildingMenuSignal.Dispatch(buildingObject, byInstanceId);
					}
				}
			}
			return true;
		}

		[QuestScriptAPI("panCameraToInstance")]
		public bool panCameraToInstance(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			if (@int > 0)
			{
				float @float = args.GetFloat(2);
				PanInstructions panInstructions = new PanInstructions(@int);
				if (@float > 0f)
				{
					panInstructions.ZoomDistance = new Boxed<float>(@float);
				}
				gameContext.injectionBinder.GetInstance<CameraAutoMoveToInstanceSignal>().Dispatch(panInstructions, new Boxed<ScreenPosition>(new ScreenPosition()));
			}
			return true;
		}

		[QuestScriptAPI("panCameraToPosition")]
		public bool panCameraToPosition(IArgRetriever args, ReturnValueContainer ret)
		{
			float num = args.GetInt(1);
			float y = args.GetInt(2);
			float num2 = args.GetInt(3);
			if (num != 0f && num2 != 0f)
			{
				float @float = args.GetFloat(4);
				bool boolean = args.GetBoolean(5);
				gameContext.injectionBinder.GetInstance<CameraAutoMoveToPositionSignal>().Dispatch(new Vector3(num, y, num2), @float, boolean);
			}
			return true;
		}

		[QuestScriptAPI("cinematicCameraToBuilding")]
		public bool cinematicCameraToBuilding(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			if (@int != 0)
			{
				float @float = args.GetFloat(2);
				gameContext.injectionBinder.GetInstance<CameraCinematicMoveToBuildingSignal>().Dispatch(@int, @float);
			}
			return true;
		}

		[QuestScriptAPI("holdStill")]
		public bool holdStill(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			if (@int > 0)
			{
				SelectMinionState selectMinionState = new SelectMinionState();
				selectMinionState.runLocation = null;
				selectMinionState.triggerIncidentalAnimation = false;
				selectMinionState.muteStatus = false;
				selectMinionState.minionID = @int;
				gameContext.injectionBinder.GetInstance<AnimateSelectedMinionSignal>().Dispatch(selectMinionState);
			}
			return true;
		}

		[QuestScriptAPI("startMinionAnimation")]
		public bool startMinionAnimation(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			int int2 = args.GetInt(2);
			if (@int > 0 && int2 > 0)
			{
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(@int);
				MinionAnimationDefinition minionAnimationDefinition = definitionService.Get<MinionAnimationDefinition>(int2);
				if (byInstanceId != null && minionAnimationDefinition != null)
				{
					gameContext.injectionBinder.GetInstance<StartIncidentalAnimationSignal>().Dispatch(@int, int2);
				}
				else
				{
					logger.Error("Unable to start animation {0} {1}", @int, int2);
				}
			}
			return true;
		}

		[QuestScriptAPI("startFunUnlock")]
		public bool startFunUnlock(IArgRetriever args, ReturnValueContainer ret)
		{
			gameContext.injectionBinder.GetInstance<StartMinionPartyUnlockSequenceSignal>().Dispatch();
			return true;
		}

		[QuestScriptAPI("philStart")]
		public bool philStart(IArgRetriever args, ReturnValueContainer ret)
		{
			gameContext.injectionBinder.GetInstance<PhilGoToStartLocationSignal>().Dispatch();
			return true;
		}

		[QuestScriptAPI("philFixedSign")]
		public bool philFixedSign(IArgRetriever args, ReturnValueContainer ret)
		{
			gameContext.injectionBinder.GetInstance<PhilSignFixedSignal>().Dispatch();
			return true;
		}

		[QuestScriptAPI("stopMinionCamping")]
		public bool stopMinionCamping(IArgRetriever args, ReturnValueContainer ret)
		{
			gameContext.injectionBinder.GetInstance<StopMinionCampingSignal>().Dispatch();
			return true;
		}

		[QuestScriptAPI("displayCaptainTeaseModal")]
		public bool displayCaptainTeaseModal(IArgRetriever args, ReturnValueContainer ret)
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_MysteryMinionTeaserSelectionModal");
			iGUICommand.skrimScreen = "TSMTeaseSkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.disableSkrimButton = true;
			guiService.Execute(iGUICommand);
			return true;
		}

		[QuestScriptAPI("setCharacterAnimation")]
		public bool setCharacterAnimation(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			string @string = args.GetString(2);
			NamedCharacter byInstanceId = playerService.GetByInstanceId<NamedCharacter>(@int);
			if (byInstanceId is PhilCharacter)
			{
				gameContext.injectionBinder.GetInstance<AnimatePhilSignal>().Dispatch(@string);
			}
			else if (byInstanceId is KevinCharacter)
			{
				gameContext.injectionBinder.GetInstance<KevinGoToWelcomeHutSignal>().Dispatch(true);
			}
			return true;
		}

		[QuestScriptAPI("stageRepairCelebrate")]
		public bool stageRepairCelebrate(IArgRetriever args, ReturnValueContainer ret)
		{
			gameContext.injectionBinder.GetInstance<StuartTunesGuitarSignal>().Dispatch();
			return true;
		}

		[QuestScriptAPI("placeCharacterInBuilding")]
		public bool placeCharacterInBuilding(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			NamedCharacter byInstanceId = playerService.GetByInstanceId<NamedCharacter>(@int);
			if (byInstanceId is PhilCharacter)
			{
				gameContext.injectionBinder.GetInstance<PhilGoToTikiBarSignal>().Dispatch(true);
			}
			return true;
		}

		[QuestScriptAPI("setBuildMenuEnabled")]
		public bool setBuildMenuEnabled(IArgRetriever args, ReturnValueContainer ret)
		{
			bool boolean = args.GetBoolean(1);
			if (boolean)
			{
				uiModel.UIState &= ~UIModel.UIStateFlags.StoreButtonHiddenFromQuest;
			}
			else
			{
				uiModel.UIState |= UIModel.UIStateFlags.StoreButtonHiddenFromQuest;
			}
			uiContext.injectionBinder.GetInstance<SetBuildMenuEnabledSignal>().Dispatch(boolean);
			return true;
		}

		[QuestScriptAPI("setStorageMenuEnabled")]
		public bool setStorageMenuEnabled(IArgRetriever args, ReturnValueContainer ret)
		{
			bool boolean = args.GetBoolean(1);
			uiContext.injectionBinder.GetInstance<SetStorageMenuEnabledSignal>().Dispatch(boolean);
			return true;
		}

		[QuestScriptAPI("setOrderBoardMenuEnabled")]
		public bool setOrderBoardMenuEnabled(IArgRetriever args, ReturnValueContainer ret)
		{
			bool boolean = args.GetBoolean(1);
			orderBoardService.SetEnabled(boolean);
			return true;
		}

		[QuestScriptAPI("setIgnoreInstance")]
		public bool setIgnoreInstance(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			bool boolean = args.GetBoolean(2);
			pickService.SetIgnoreInstanceInput(@int, boolean);
			return true;
		}

		[QuestScriptAPI("dispatchSignal")]
		public bool DispatchSignal(IArgRetriever args, ReturnValueContainer ret)
		{
			string @string = args.GetString(1);
			Type type = Assembly.GetExecutingAssembly().GetType("Kampai.Game." + @string, false);
			if (type == null)
			{
				logger.Error("qs.dispatchSignal: Cannot dispatch signal {0}, cannot find type.", @string);
				return true;
			}
			object instance;
			try
			{
				instance = gameContext.injectionBinder.GetInstance(type);
			}
			catch (InjectionException)
			{
				logger.Error("qs.dispatchSignal: Cannot dispatch signal {0}, failed to get instance.", @string);
				return true;
			}
			Type baseType = type.BaseType;
			Type[] genericArguments = baseType.GetGenericArguments();
			if (genericArguments.Length > args.Length - 1)
			{
				logger.Error("qs.dispatchSignal: Cannot dispatch signal {0}, not enough args", @string);
				return true;
			}
			MethodInfo method = baseType.GetMethod("Dispatch", genericArguments);
			object[] array = new object[genericArguments.Length];
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				array[i] = args.Get(i + 2, genericArguments[i]);
			}
			method.Invoke(instance, array);
			return true;
		}

		[QuestScriptAPI("throbLockedButtons")]
		public bool throbLockedButtons(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.AddToArguments(new ThrobLockedButtons());
			}
			else
			{
				guiService.RemoveFromArguments(typeof(ThrobLockedButtons));
			}
			return true;
		}

		[QuestScriptAPI("throbRushButtons")]
		public bool throbRushButtons(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.AddToArguments(new ThrobRushButtons());
			}
			else
			{
				guiService.RemoveFromArguments(typeof(ThrobRushButtons));
			}
			return true;
		}

		[QuestScriptAPI("throbCallButtons")]
		public bool throbCallButtons(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.AddToArguments(new ThrobCallButtons());
			}
			else
			{
				guiService.RemoveFromArguments(typeof(ThrobCallButtons));
			}
			return true;
		}

		[QuestScriptAPI("throbCollectButton")]
		public bool throbCollectButton(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.AddToArguments(new ThrobCollectButton());
			}
			else
			{
				guiService.RemoveFromArguments(typeof(ThrobCollectButton));
			}
			return true;
		}

		[QuestScriptAPI("enableCallButtons")]
		public bool enableCallButtons(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.RemoveFromArguments(typeof(DisableCallButtons));
			}
			else
			{
				guiService.AddToArguments(new DisableCallButtons());
			}
			return true;
		}

		[QuestScriptAPI("enableRushButtons")]
		public bool enableRushButtons(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.RemoveFromArguments(typeof(DisableRushButtons));
			}
			else
			{
				guiService.AddToArguments(new DisableRushButtons());
			}
			return true;
		}

		[QuestScriptAPI("enableDeleteButton")]
		public bool enableDeleteButton(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.RemoveFromArguments(typeof(DisableDeleteOrderButton));
			}
			else
			{
				guiService.AddToArguments(new DisableDeleteOrderButton());
			}
			return true;
		}

		[QuestScriptAPI("enableLockedButton")]
		public bool enableLockedButton(IArgRetriever args, ReturnValueContainer ret)
		{
			if (args.GetBoolean(1))
			{
				guiService.RemoveFromArguments(typeof(DisableLockedButton));
			}
			else
			{
				guiService.AddToArguments(new DisableLockedButton());
			}
			return true;
		}

		[QuestScriptAPI("enableCabanas")]
		public bool enableCabanas(IArgRetriever args, ReturnValueContainer ret)
		{
			foreach (Instance item in playerService.GetInstancesByDefinition<CabanaBuildingDefinition>())
			{
				gameContext.injectionBinder.GetInstance<BuildingChangeStateSignal>().Dispatch(item.ID, BuildingState.Broken);
			}
			return true;
		}

		[QuestScriptAPI("cameraZoomToBeach")]
		public bool cameraZoomToBeach(IArgRetriever args, ReturnValueContainer ret)
		{
			gameContext.injectionBinder.GetInstance<CameraZoomBeachSignal>().Dispatch();
			return true;
		}

		[QuestScriptAPI("setFtueLevelCompleted")]
		public bool setFtueLevelCompleted(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			if (@int > 0)
			{
				playerService.SetHighestFtueCompleted(@int);
			}
			if (@int == 999999)
			{
				gameContext.injectionBinder.GetInstance<CheckTriggersSignal>().Dispatch(301);
			}
			return true;
		}

		[QuestScriptAPI("getFtueLevelCompleted")]
		public bool getFtueLevelCompleted(IArgRetriever args, ReturnValueContainer ret)
		{
			ret.Set(playerService.GetHighestFtueCompleted());
			return true;
		}

		[QuestScriptAPI("getFTUECompleteValue")]
		public bool getFTUECompleteValue(IArgRetriever args, ReturnValueContainer ret)
		{
			ret.Set(999999);
			return true;
		}

		[QuestScriptAPI("changeToPrestigeState")]
		public bool changeToPrestigeState(IArgRetriever args, ReturnValueContainer ret)
		{
			int @int = args.GetInt(1);
			PrestigeState targetState = (PrestigeState)(int)Enum.Parse(typeof(PrestigeState), args.GetString(2));
			IPrestigeService instance = gameContext.injectionBinder.GetInstance<IPrestigeService>();
			Prestige firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Prestige>(@int);
			instance.ChangeToPrestigeState(firstInstanceByDefinitionId, targetState);
			return true;
		}

		[QuestScriptAPI("liberateLeveledUpMinions")]
		public bool liberateLeveledUpMinions(IArgRetriever args, ReturnValueContainer ret)
		{
			int type = 1;
			if (args.Length > 1)
			{
				type = args.GetInt(1);
			}
			gameContext.injectionBinder.GetInstance<LiberateLeveledUpMinionsSignal>().Dispatch(type);
			return true;
		}

		[QuestScriptAPI("inEditor")]
		public bool inEditor(IArgRetriever args, ReturnValueContainer ret)
		{
			ret.Set(false);
			return true;
		}

		private void ERROR(string message)
		{
			logger.Log(KampaiLogLevel.Error, true, string.Format("SCRIPT ERROR[{0}] {1}", "undefined", message));
		}
	}
}
