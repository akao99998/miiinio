using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Mignette;
using Kampai.Game.Mtx;
using Kampai.Game.Transaction;
using Kampai.Game.Trigger;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Splash;
using Kampai.UI.View;
using Newtonsoft.Json;
using Prime31;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.signal.impl;

namespace Kampai.Util
{
	public class DebugConsoleController
	{
		private readonly StringBuilder outBuilder = new StringBuilder();

		private readonly Dictionary<string, Tuple<DebugCommandAttribute, DebugCommand>> commands = new Dictionary<string, Tuple<DebugCommandAttribute, DebugCommand>>();

		private readonly IKampaiLogger logger = LogManager.GetClassLogger("DebugConsoleController") as IKampaiLogger;

		private readonly QuestScriptInstance consoleQuestScript = new QuestScriptInstance();

		private ZoomView zoomView;

		private Camera cameraComponent;

		private int uniqueID = 90000;

		private IList<TimedSocialEventDefinition> localSocialEvents;

		private IList<SocialEventInvitation> localInvitations;

		public readonly Signal CloseConsoleSignal = new Signal();

		public readonly Signal FlushSignal = new Signal();

		public readonly Signal EnableQuestDebugSignal = new Signal();

		public readonly Signal ToggleRightClickSignal = new Signal();

		private static bool showTransparent = true;

		private List<GameObject> HiddenTransparentObjects;

		private Dictionary<string, GameObject> hiddenGameObjectsMap;

		private HUDView hudView;

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IQuestScriptService questScriptService { get; set; }

		[Inject]
		public IVideoService videoService { get; set; }

		[Inject]
		public IMinionBuilder minionBuilder { get; set; }

		[Inject]
		public LogClientMetricsSignal metricsSignal { get; set; }

		[Inject]
		public ScheduleNotificationSignal notificationSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject(MainElement.CONTEXT)]
		public ICrossContextCapable mainContext { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public IEncryptionService encryptionService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject(SocialServices.GAMECENTER)]
		public ISocialService gamecenterService { get; set; }

		[Inject(SocialServices.GOOGLEPLAY)]
		public ISocialService googlePlayService { get; set; }

		[Inject(MainElement.CAMERA)]
		public GameObject cameraGO { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public PurchaseLandExpansionSignal purchaseLandExpansionSignal { get; set; }

		[Inject]
		public EndSaleSignal endSaleSignal { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public ILandExpansionConfigService landExpansionConfigService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public LoadAnimatorStateInfoSignal loadAnimatorStateInfoSignal { get; set; }

		[Inject]
		public UnloadAnimatorStateInfoSignal unloadAnimatorStateInfoSignal { get; set; }

		[Inject]
		public ABTestSignal ABTestSignal { get; set; }

		[Inject]
		public IConfigurationsService configService { get; set; }

		[Inject]
		public SavePlayerSignal savePlayerSignal { get; set; }

		[Inject]
		public SocialLoginSignal socialLoginSignal { get; set; }

		[Inject]
		public SocialInitSignal socialInitSignal { get; set; }

		[Inject]
		public SocialLogoutSignal socialLogoutSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject(QuestRunnerLanguage.Lua)]
		public IQuestScriptRunner luaRunner { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public UnlinkAccountSignal unlinkAccountSignal { get; set; }

		[Inject]
		public UpdatePlayerDLCTierSignal playerDLCTierSignal { get; set; }

		[Inject]
		public MignetteGameModel mignetteGameModel { get; set; }

		[Inject]
		public MignetteCollectionService mignetteCollectionService { get; set; }

		[Inject]
		public BuildingCooldownCompleteSignal cooldownCompleteSignal { get; set; }

		[Inject]
		public BuildingCooldownUpdateViewSignal cooldownUpdateViewSignal { get; set; }

		[Inject]
		public SpawnMignetteDooberSignal spawnDooberSignal { get; set; }

		[Inject]
		public ChangeMignetteScoreSignal changeScoreSignal { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public ShowClientUpgradeDialogSignal showClientUpgradeDialogSignal { get; set; }

		[Inject]
		public ShowForcedClientUpgradeScreenSignal showForcedClientUpgradeScreenSignal { get; set; }

		[Inject]
		public CompositeBuildingPieceAddedSignal compositeBuildingPieceAddedSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public SocialOrderBoardCompleteSignal socialOrderBoardCompleteSignal { get; set; }

		[Inject]
		public CreateWayFinderSignal createWayFinderSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		[Inject]
		public RepairBuildingSignal repairBuildingSignal { get; set; }

		[Inject]
		public SetupPushNotificationsSignal setupPushNotificationsSignal { get; set; }

		[Inject]
		public DebugKeyHitSignal debugKeyHitSignal { get; set; }

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public ReconcileSalesSignal reconcileSalesSignal { get; set; }

		[Inject]
		public RushDialogConfirmationSignal dialogConfirmedSignal { get; set; }

		[Inject]
		public OpenBuildingMenuSignal openBuildingMenuSignal { get; set; }

		[Inject]
		public IAchievementService achievementService { get; set; }

		[Inject]
		public AddFPSCounterSignal addFPSCounterSignal { get; set; }

		[Inject]
		public PurchaseLandExpansionSignal purchaseSignal { get; set; }

		[Inject]
		public IMIBService mibService { get; set; }

		[Inject]
		public IOrderBoardService orderBoardService { get; set; }

		[Inject]
		public AwardLevelSignal awardLevelSignal { get; set; }

		[Inject]
		public IPrestigeService characterService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public OpenUpSellModalSignal openUpSellModalSignal { get; set; }

		[Inject]
		public UnlockVillainLairSignal unlockVillainLairSignal { get; set; }

		[Inject]
		public IPartyFavorAnimationService partyFavorAnimationService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public IMasterPlanQuestService masterPlanQuestService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public ISupersonicService supersonicService { get; set; }

		[Inject]
		public IRewardedAdService rewardedAdService { get; set; }

		[Inject]
		public ResetRewardedAdLimitSignal resetRewardedAdLimitSignal { get; set; }

		[Inject]
		public GameLoadedModel gameLoadedModel { get; set; }

		public DebugConsoleController()
		{
			try
			{
				Type typeFromHandle = typeof(DebugConsoleController);
				MethodInfo[] methods = typeFromHandle.GetMethods(BindingFlags.Instance | BindingFlags.Public);
				int i = 0;
				for (int num = methods.Length; i < num; i++)
				{
					MethodInfo methodInfo = methods[i];
					object[] customAttributes = methodInfo.GetCustomAttributes(typeof(DebugCommandAttribute), false);
					if (customAttributes.Length < 1)
					{
						continue;
					}
					DebugCommand second = null;
					try
					{
						second = Delegate.CreateDelegate(typeof(DebugCommand), methodInfo) as DebugCommand;
					}
					catch (ArgumentException ex)
					{
						outBuilder.AppendFormat("Failed to grab command method {0}: {1}", methodInfo.Name, ex.Message);
					}
					int j = 0;
					for (int num2 = customAttributes.Length; j < num2; j++)
					{
						DebugCommandAttribute debugCommandAttribute = customAttributes[j] as DebugCommandAttribute;
						string text = debugCommandAttribute.Name;
						if (text == null)
						{
							text = methodInfo.Name.ToLower();
						}
						commands.Add(text, new Tuple<DebugCommandAttribute, DebugCommand>(debugCommandAttribute, second));
					}
				}
			}
			catch (Exception ex2)
			{
				logger.Error("{0}, {1}, {2}", ex2.ToString(), ex2.Message, ex2.StackTrace);
				throw;
			}
		}

		[PostConstruct]
		public void Init()
		{
			zoomView = cameraGO.GetComponent<ZoomView>();
			cameraComponent = cameraGO.GetComponent<Camera>();
			debugKeyHitSignal.AddListener(DebugKeyHit);
		}

		public DebugCommandError GetCommand(string[] args, out DebugCommand command)
		{
			int i = 0;
			for (int num = args.Length; i < num; i++)
			{
				string key = string.Join(" ", args, 0, i + 1);
				Tuple<DebugCommandAttribute, DebugCommand> value;
				commands.TryGetValue(key, out value);
				if (value != null)
				{
					if (value.Item1.RequiresAllArgs && args.Length - i - 1 < value.Item1.Args.Length)
					{
						command = null;
						return DebugCommandError.NotEnoughArguments;
					}
					command = value.Item2;
					return DebugCommandError.NoError;
				}
			}
			command = null;
			return DebugCommandError.NotFound;
		}

		public string GetOutput()
		{
			string result = outBuilder.ToString();
			outBuilder.Length = 0;
			return result;
		}

		[DebugCommand]
		public void unlockLairPortal(string[] args)
		{
			VillainLairEntranceBuilding byInstanceId = playerService.GetByInstanceId<VillainLairEntranceBuilding>(374);
			if (byInstanceId.State == BuildingState.Inaccessible)
			{
				byInstanceId.SetState(BuildingState.Broken);
				repairBuildingSignal.Dispatch(byInstanceId);
				outBuilder.AppendLine("Villain Lair Portal has been repaired, use debug command 'unlockLairPortal' again to simulate having the neccessary upgraded minions.");
			}
			else if (!byInstanceId.IsUnlocked)
			{
				unlockVillainLairSignal.Dispatch(byInstanceId, 3137);
				outBuilder.AppendLine("Villain Lair Portal should now be unlocked.");
			}
			else
			{
				outBuilder.AppendLine("could not unlock the villain lair portal...building state is not inaccessible or already unlocked through debug.");
			}
		}

		[DebugCommand(Args = new string[] { "command_name" })]
		[DebugCommand(Name = "?")]
		public void Help(string[] args)
		{
			outBuilder.AppendLine("commands:");
			string text = string.Join(" ", args, 1, args.Length - 1);
			Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
			foreach (string key in commands.Keys)
			{
				if (key.StartsWith(text))
				{
					string commandName;
					string subcommandName;
					SplitCommandKey(key, text, out commandName, out subcommandName);
					List<string> value;
					dictionary.TryGetValue(commandName, out value);
					if (value == null)
					{
						value = new List<string>();
						dictionary.Add(commandName, value);
					}
					if (subcommandName != null)
					{
						value.Add(subcommandName);
					}
				}
			}
			int count = dictionary.Count;
			int num = 0;
			foreach (KeyValuePair<string, List<string>> item in dictionary.OrderBy((KeyValuePair<string, List<string>> key) => key.Key))
			{
				outBuilder.Append(item.Key);
				if (item.Value.Count < 1)
				{
					OutputArguments(commands[item.Key].Item1.Args);
				}
				else
				{
					OutputSubcommands(item.Value);
				}
				if (num++ < count - 1)
				{
					outBuilder.Append(" | ");
				}
			}
			outBuilder.AppendLine("\n Use help {{command_name}} to see more details of a command.");
		}

		[DebugCommand]
		public void FTUELevel(string[] args)
		{
			int highestFtueCompleted = playerService.GetHighestFtueCompleted();
			outBuilder.Append("FtueLevel: " + highestFtueCompleted + " - " + (FtueLevel)highestFtueCompleted);
		}

		[DebugCommand]
		public void Help2(string[] args)
		{
			string text = "quantity items:\n    ";
			int num = 0;
			foreach (KeyValuePair<int, Definition> allDefinition in definitionService.GetAllDefinitions())
			{
				if (allDefinition.Value.LocalizedKey != null)
				{
					text += string.Format("{0}  |  ", allDefinition.Value.LocalizedKey.Replace(' ', '_').ToLower());
					num++;
					if (num > 4)
					{
						text += "\n    ";
						num = 0;
					}
				}
			}
			outBuilder.Append(text);
		}

		[DebugCommand]
		public void Exit(string[] args)
		{
			CloseConsoleSignal.Dispatch();
		}

		[DebugCommand]
		public void AutoOrderBoard(string[] args)
		{
			OrderBoardBuildingObjectView orderBoardBuildingObjectView = UnityEngine.Object.FindObjectOfType<OrderBoardBuildingObjectView>();
			if (orderBoardBuildingObjectView != null)
			{
				Building byInstanceId = playerService.GetByInstanceId<Building>(orderBoardBuildingObjectView.ID);
				if (byInstanceId != null)
				{
					openBuildingMenuSignal.Dispatch(orderBoardBuildingObjectView, byInstanceId);
				}
			}
			DebugButton debugButton = UnityEngine.Object.FindObjectOfType<DebugButton>();
			if (debugButton != null)
			{
				debugButton.OnClick(null);
			}
			int result = 1;
			if (args.Length > 1)
			{
				int.TryParse(args[1], out result);
			}
			routineRunner.StartCoroutine(AutoOrderBoardLoop(result));
		}

		[DebugCommand(Args = new string[] { "Scope" })]
		public void Hindsight(string[] args)
		{
			if (args.Length < 2)
			{
				outBuilder.AppendLine("Mising argument Scope");
				return;
			}
			HindsightCampaign.Scope scope = HindsightCampaign.Scope.unknown;
			try
			{
				scope = (HindsightCampaign.Scope)(int)Enum.Parse(typeof(HindsightCampaign.Scope), args[1]);
			}
			catch (Exception ex)
			{
				outBuilder.AppendFormat("Failed to convert scope: {0}\n", args[1]);
				outBuilder.AppendFormat("Exception: {0}", ex.Message);
				return;
			}
			if (scope == HindsightCampaign.Scope.unknown)
			{
				outBuilder.AppendFormat("Scope is unknown");
			}
			else
			{
				mainContext.injectionBinder.GetInstance<DisplayHindsightContentSignal>().Dispatch(scope);
			}
		}

		private IEnumerator AutoOrderBoardLoop(int orderCount)
		{
			yield return new WaitForSeconds(2.5f);
			GameObject go = GameObject.Find("btn_FillOrder_normal");
			if (go == null)
			{
				yield return null;
			}
			FillOrderButtonView fillOrderButtonView = go.GetComponent<FillOrderButtonView>();
			if (fillOrderButtonView == null)
			{
				yield return null;
			}
			for (int i = 0; i < orderCount; i++)
			{
				OrderBoardTicketView[] obtView = UnityEngine.Object.FindObjectsOfType<OrderBoardTicketView>();
				if (obtView.Length > 0)
				{
					obtView[UnityEngine.Random.Range(0, obtView.Length)].TicketButton.ClickedSignal.Dispatch();
				}
				yield return new WaitForSeconds(0.1f);
				if (fillOrderButtonView.gameObject.activeInHierarchy)
				{
					fillOrderButtonView.OnClickEvent();
				}
				yield return new WaitForSeconds(2f);
			}
		}

		[DebugCommand]
		public void Auth(string[] args)
		{
			outBuilder.AppendLine("unsupported");
		}

		[DebugCommand]
		public void Stream(string[] args)
		{
			videoService.playVideo("https://archive.org/download/Pbtestfilemp4videotestmp4/video_test.mp4", true, true);
		}

		[DebugCommand]
		public void Lod(string[] args)
		{
			outBuilder.Append(string.Join(" ", args) + " " + minionBuilder.GetLOD());
		}

		[DebugCommand]
		public void Throw(string[] args)
		{
			logger.Log(KampaiLogLevel.Error, true, "Throwing...");
			if (args.Length < 2 || args[1].Equals("null"))
			{
				string text = null;
				text.IndexOf("a");
				return;
			}
			if (args[1].Equals("bind"))
			{
				gameContext.injectionBinder.Bind<object>().ToValue(this).ToName("DEBUG");
				gameContext.injectionBinder.Bind<object>().ToValue(42).ToName("DEBUG");
				gameContext.injectionBinder.GetInstance<object>("DEBUG");
				return;
			}
			if (args[1].Equals("index"))
			{
				string text2 = args[args.Length + 100];
				logger.Info(text2);
				return;
			}
			throw new Exception();
		}

		[DebugCommand]
		public void Crash(string[] args)
		{
			logger.Log(KampaiLogLevel.Error, true, "Crashing...");
			Native.Crash();
		}

		[DebugCommand]
		public void HardCrash(string[] args)
		{
			logger.Log(KampaiLogLevel.Error, true, "Crashing hard...");
			IBuildingUtilities buildingUtilities = null;
			Debug.Log(buildingUtilities.AvailableLandSpaceCount().ToString());
		}

		[DebugCommand]
		public void Health(string[] args)
		{
			metricsSignal.Dispatch(false);
		}

		[DebugCommand]
		public void Notify(string[] args)
		{
			NotificationDefinition notificationDefinition = new NotificationDefinition();
			notificationDefinition.ID = 11;
			notificationDefinition.Seconds = 30;
			notificationDefinition.Title = "Oh";
			notificationDefinition.Text = "Debug console notification";
			notificationDefinition.Type = NotificationType.DebugConsole.ToString();
			notificationSignal.Dispatch(notificationDefinition);
		}

		[DebugCommand]
		public void LocalNote(string[] args)
		{
			int result;
			NotificationDefinition definition;
			if (args.Length < 2)
			{
				outBuilder.AppendLine("Need arg");
			}
			else if (int.TryParse(args[1], out result) && definitionService.TryGet<NotificationDefinition>(result, out definition))
			{
				definition.Seconds = 30;
				notificationSignal.Dispatch(definition);
			}
			else
			{
				outBuilder.Append("No such notification ID");
			}
		}

		[DebugCommand]
		public void Telemetry(string[] args)
		{
			telemetryService.Send_Telemetry_EVT_GAME_ERROR_GAMEPLAY("error", "test", true);
		}

		[DebugCommand]
		public void Fps(string[] args)
		{
			ToggleFPSGraphSignal instance = uiContext.injectionBinder.GetInstance<ToggleFPSGraphSignal>();
			instance.Dispatch();
		}

		[DebugCommand]
		public void Quality(string[] args)
		{
			outBuilder.Append(QualitySettings.names[QualitySettings.GetQualityLevel()]);
		}

		[DebugCommand]
		public void ap(string[] args)
		{
			Time.timeScale = 1f - Time.timeScale;
		}

		[DebugCommand]
		public void ToggleHUD(string[] args)
		{
			if (hudView == null)
			{
				hudView = UnityEngine.Object.FindObjectOfType<HUDView>();
			}
			if (hudView != null)
			{
				hudView.gameObject.SetActive(!hudView.gameObject.activeSelf);
			}
			else
			{
				logger.Error("Can't find HUD view");
			}
		}

		[DebugCommand]
		public void ToggleAnimators(string[] args)
		{
			Animator[] array = UnityEngine.Object.FindObjectsOfType<Animator>();
			foreach (Animator animator in array)
			{
				animator.enabled = false;
			}
		}

		[DebugCommand]
		public void FindByType(string[] args)
		{
			if (args.Length < 2)
			{
				outBuilder.AppendLine("Must include type name to search");
				return;
			}
			Type typeByName = GetTypeByName(args[1]);
			if (typeByName == null)
			{
				outBuilder.AppendLine(string.Format("Could not find type {0}", args[1]));
				return;
			}
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeByName);
			if (array == null)
			{
				return;
			}
			outBuilder.AppendLine(string.Format("Found {0} object", array.Length));
			UnityEngine.Object[] array2 = array;
			foreach (UnityEngine.Object @object in array2)
			{
				GameObject gameObject = GameObject.Find(@object.name);
				if (gameObject != null)
				{
					outBuilder.AppendLine(string.Format("Name: {0}, activeSelf: {1}", @object.name, gameObject.activeSelf));
				}
				else
				{
					outBuilder.AppendLine(string.Format("Name: {0}", @object.name));
				}
			}
		}

		private Type GetTypeByName(string typeName)
		{
			Type type = null;
			string[] array = new string[5]
			{
				string.Empty,
				"Kampai",
				"UnityEngine",
				"UnityEngine.UI",
				"UnityEngine.Rendering"
			};
			int num = 0;
			while (type == null && num < array.Length)
			{
				string text = array[num++];
				string text2 = (string.IsNullOrEmpty(text) ? string.Empty : (text + ".")) + typeName;
				string text3 = (string.IsNullOrEmpty(text) ? string.Empty : ("," + text));
				type = Type.GetType(text2 + text3);
			}
			return type;
		}

		[DebugCommand]
		public void ToggleActive(string[] args)
		{
			if (args.Length < 2)
			{
				return;
			}
			GameObject value = null;
			if (hiddenGameObjectsMap == null)
			{
				hiddenGameObjectsMap = new Dictionary<string, GameObject>();
			}
			bool flag = hiddenGameObjectsMap.TryGetValue(args[1], out value);
			if (value == null)
			{
				value = GameObject.Find(args[1]);
			}
			if (!(value == null))
			{
				if (!flag)
				{
					hiddenGameObjectsMap.Add(args[1], value);
				}
				value.SetActive(!value.activeSelf);
			}
		}

		[DebugCommand]
		public void ToggleTransparent(string[] args)
		{
			showTransparent = !showTransparent;
			if (!showTransparent)
			{
				HiddenTransparentObjects = new List<GameObject>();
				Renderer[] array = UnityEngine.Object.FindObjectsOfType<Renderer>();
				foreach (Renderer renderer in array)
				{
					if (renderer.material.renderQueue >= 3000)
					{
						renderer.gameObject.SetActive(false);
						HiddenTransparentObjects.Add(renderer.gameObject);
					}
				}
			}
			if (!showTransparent)
			{
				return;
			}
			foreach (GameObject hiddenTransparentObject in HiddenTransparentObjects)
			{
				hiddenTransparentObject.SetActive(true);
			}
		}

		[DebugCommand]
		public void HideShader(string[] args)
		{
			if (args.Length <= 0)
			{
				return;
			}
			Renderer[] array = UnityEngine.Object.FindObjectsOfType<Renderer>();
			foreach (Renderer renderer in array)
			{
				if (renderer.material.shader.name.ToLower().Contains(args[1].ToLower()))
				{
					renderer.gameObject.SetActive(false);
				}
			}
		}

		[DebugCommand]
		public void System(string[] args)
		{
			Resolution currentResolution = Screen.currentResolution;
			outBuilder.AppendLine("\t Model: " + SystemInfo.deviceModel);
			outBuilder.AppendLine("\t resolution: " + currentResolution.width + "x" + currentResolution.height + ", " + currentResolution.refreshRate);
			outBuilder.AppendLine("\t processor count: " + SystemInfo.processorCount);
			outBuilder.AppendLine("\t ram: " + SystemInfo.systemMemorySize);
			outBuilder.AppendLine("\t vram: " + SystemInfo.graphicsMemorySize);
			outBuilder.AppendLine("\t shader level: " + SystemInfo.graphicsShaderLevel);
			outBuilder.AppendLine("\t gpu vendor: " + SystemInfo.graphicsDeviceVendor);
			outBuilder.AppendLine("\t gpu name: " + SystemInfo.graphicsDeviceName);
		}

		[DebugCommand]
		public void User(string[] args)
		{
			string data = localPersistService.GetData("UserID");
			outBuilder.AppendLine(string.Join(" ", args) + " -> UserID: " + data);
			string plainText = localPersistService.GetData("AnonymousID");
			encryptionService.TryDecrypt(plainText, "Kampai!", out plainText);
			outBuilder.AppendLine("\tAnonynousID: " + plainText);
			string userID = facebookService.userID;
			outBuilder.AppendLine("\tFacebookID: " + userID);
			string synergyId = NimbleBridge_SynergyIdManager.GetComponent().GetSynergyId();
			outBuilder.AppendLine("\tSynergyID: " + synergyId);
			string sdkVersion = NimbleBridge_Base.GetSdkVersion();
			outBuilder.AppendLine("\tNimbleSDK: " + sdkVersion);
			string userID2 = googlePlayService.userID;
			outBuilder.AppendLine("\tGooglePlayID: " + userID2);
		}

		[DebugCommand]
		public void GC(string[] args)
		{
		}

		[DebugCommand]
		public void CBO(string[] args)
		{
			Ray ray = new Ray(cameraGO.transform.position, cameraGO.transform.forward);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, 512))
			{
				Vector3 center = hitInfo.collider.gameObject.GetComponent<BuildingObject>().Center;
				outBuilder.AppendLine("Offset: " + (cameraGO.transform.position - center).ToString() + " Zoom: " + zoomView.GetCurrentPercentage());
				return;
			}
			ray = new Ray(cameraGO.transform.position + new Vector3(2f, 0f, 2f), cameraGO.transform.forward);
			if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, 512))
			{
				Vector3 center2 = hitInfo.collider.gameObject.GetComponent<BuildingObject>().Center;
				outBuilder.AppendLine("Offset: " + (cameraGO.transform.position - center2).ToString() + " Zoom: " + zoomView.GetCurrentPercentage());
			}
			else
			{
				outBuilder.AppendLine("Unable to determine building");
			}
		}

		[DebugCommand]
		public void locTest(string[] args)
		{
			int time = 10;
			int time2 = 75;
			int time3 = 12345;
			int time4 = 1234567;
			int num = 7500;
			int num2 = 777333;
			int num3 = 123456789;
			float num4 = 1f;
			float num5 = 55.55f;
			float num6 = 1234567.9f;
			ILocalizationService instance = gameContext.injectionBinder.GetInstance<ILocalizationService>();
			outBuilder.AppendLine("---  country is " + instance.GetCountry() + " & lang is " + Native.GetDeviceLanguage());
			outBuilder.AppendLine("time1: " + UIUtils.FormatTime(time, instance));
			outBuilder.AppendLine("time2: " + UIUtils.FormatTime(time2, instance));
			outBuilder.AppendLine("time3: " + UIUtils.FormatTime(time3, instance));
			outBuilder.AppendLine("time4: " + UIUtils.FormatTime(time4, instance));
			outBuilder.AppendLine("date: " + UIUtils.FormatDate(timeService.CurrentTime(), instance));
			outBuilder.AppendLine(num + " = " + UIUtils.FormatLargeNumber(num));
			outBuilder.AppendLine(num2 + " = " + UIUtils.FormatLargeNumber(num2));
			outBuilder.AppendLine(num3 + " = " + UIUtils.FormatLargeNumber(num3));
			outBuilder.AppendLine(string.Format("{0:0.00##}", num4) + " = " + num4.ToString("R", instance.CultureInfo));
			outBuilder.AppendLine(string.Format("{0:0.00##}", num5) + " = " + num5.ToString("R", instance.CultureInfo));
			outBuilder.AppendLine(string.Format("{0:0.00##}", num6) + " = " + num6.ToString("R", instance.CultureInfo));
			outBuilder.AppendLine("-----------");
		}

		[DebugCommand]
		public void tubes(string[] args)
		{
			int result = 3;
			if (args.Length > 1 && int.TryParse(args[1], out result) && (result > 3 || result < 0))
			{
				result = 3;
			}
			Exit(new string[0]);
			routineRunner.StartCoroutine(CallForTubes(result));
		}

		private IEnumerator CallForTubes(int tubeCount)
		{
			yield return new WaitForSeconds(1f);
			for (int i = 0; i < tubeCount; i++)
			{
				gameContext.injectionBinder.GetInstance<CreateInnerTubeSignal>().Dispatch(i);
			}
		}

		[DebugCommand(Name = "ua")]
		public void UpdateAchievement(string[] args)
		{
			int result;
			int result2;
			if (int.TryParse(args[1], out result) && int.TryParse(args[2], out result2))
			{
				achievementService.UpdateIncrementalAchievement(result, result2);
			}
		}

		[DebugCommand(Name = "ra")]
		public void ResetAchievements(string[] args)
		{
			IList<Achievement> instancesByType = playerService.GetInstancesByType<Achievement>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				playerService.Remove(instancesByType[i]);
			}
		}

		[DebugCommand]
		public void AddMinion(string[] args)
		{
			int minionsToAdd;
			if (args.Length == 1)
			{
				minionsToAdd = 1;
			}
			else
			{
				int result;
				if (!int.TryParse(args[1], out result))
				{
					result = 1;
				}
				minionsToAdd = result;
			}
			AddMinions(minionsToAdd);
		}

		private void AddMinions(int minionsToAdd)
		{
			for (int i = 0; i < minionsToAdd; i++)
			{
				int id = UnityEngine.Random.Range(601, 607);
				MinionDefinition def = definitionService.Get<MinionDefinition>(id);
				Minion minion = new Minion(def);
				playerService.Add(minion);
				gameContext.injectionBinder.GetInstance<CreateMinionSignal>().Dispatch(minion);
			}
		}

		[DebugCommand(Name = "addingredients", Args = new string[] { "ingredients_id", "amount" }, RequiresAllArgs = true)]
		public void AddIngredients(string[] args)
		{
			int result;
			uint result2;
			if (int.TryParse(args[1], out result) && uint.TryParse(args[2], out result2))
			{
				TransactionDefinition transactionDefinition = new TransactionDefinition();
				transactionDefinition.Inputs = new List<QuantityItem>();
				transactionDefinition.Outputs = new List<QuantityItem>();
				QuantityItem item = new QuantityItem(result, result2);
				transactionDefinition.Outputs.Add(item);
				playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.INGREDIENT, TransactionCallback);
			}
		}

		[DebugCommand]
		public void LevelUp(string[] args)
		{
			int num;
			if (args.Length == 1)
			{
				num = 1;
			}
			else
			{
				int result;
				if (!int.TryParse(args[1], out result))
				{
					result = 1;
				}
				num = result - (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			}
			if (num < 0)
			{
				outBuilder.AppendLine("You can't Level Down silly.");
				return;
			}
			for (int i = 0; i < num; i++)
			{
				Level();
			}
			Action<int, int> callback = delegate
			{
				gameContext.injectionBinder.GetInstance<EnableCameraBehaviourSignal>().Dispatch(3);
			};
			gameContext.injectionBinder.GetInstance<PromptReceivedSignal>().AddOnce(callback);
			uiContext.injectionBinder.GetInstance<SetXPSignal>().Dispatch();
		}

		private void Level()
		{
			playerService.AlterQuantity(StaticItem.LEVEL_ID, 1);
			TransactionDefinition rewardTransaction = RewardUtil.GetRewardTransaction(definitionService, playerService);
			awardLevelSignal.Dispatch(rewardTransaction);
			characterService.UpdateEligiblePrestigeList();
			gameContext.injectionBinder.GetInstance<GetNewQuestSignal>().Dispatch();
			telemetryService.Send_Telemetry_EVT_GP_LEVEL_PROMOTION();
			playerDurationService.MarkLevelUpUTC();
			gameContext.injectionBinder.GetInstance<UpdateMarketplaceRepairStateSignal>().Dispatch();
			gameContext.injectionBinder.GetInstance<UpdateForSaleSignsSignal>().Dispatch();
			if (playerService.GetHighestFtueCompleted() >= 999999)
			{
				reconcileSalesSignal.Dispatch(0);
			}
		}

		[DebugCommand]
		public void PlayerTraining(string[] args)
		{
			if (args.Length == 1)
			{
				outBuilder.AppendLine("Invalid call.  You need to specify a PlayerTrainingDefinition ID");
				return;
			}
			int result;
			if (!int.TryParse(args[1], out result))
			{
				outBuilder.AppendLine("Invalid call.  You need to specify a PlayerTrainingDefinition ID");
				return;
			}
			PlayerTrainingDefinition playerTrainingDefinition = definitionService.Get<PlayerTrainingDefinition>(result);
			if (playerTrainingDefinition == null)
			{
				outBuilder.AppendLine("Invalid ID.  You need to specify a PlayerTrainingDefinition ID");
			}
			else
			{
				gameContext.injectionBinder.GetInstance<DisplayPlayerTrainingSignal>().Dispatch(result, true, new Signal<bool>());
			}
		}

		[DebugCommand]
		public void Experience(string[] args)
		{
			int result;
			if (!int.TryParse(args[1], out result))
			{
				result = 50;
			}
			playerService.CreateAndRunCustomTransaction(2, result, TransactionTarget.NO_VISUAL);
		}

		[DebugCommand(Name = "quantity", Args = new string[] { "getQuantity [def id]" }, RequiresAllArgs = true)]
		public void quantity(string[] args)
		{
			int result = 0;
			if (args.Length > 0 && int.TryParse(args[1], out result))
			{
				outBuilder.AppendFormat("quantity of {0} is {1}", result, playerService.GetQuantityByDefinitionId(result));
			}
			else
			{
				outBuilder.AppendFormat("Unable to parse {0}", args[1]);
			}
		}

		[DebugCommand(Name = "availableland")]
		public void AvailableLandSpaceCount(string[] args)
		{
			IBuildingUtilities instance = gameContext.injectionBinder.GetInstance<IBuildingUtilities>();
			if (instance == null)
			{
				outBuilder.AppendFormat("Couldn't find type {0}", typeof(IBuildingUtilities));
				return;
			}
			outBuilder.AppendFormat("AvailableLandSpaceCount: {0}", instance.AvailableLandSpaceCount());
			outBuilder.AppendFormat("Total Saved To Player AvailableLandSpaceCount: {0}", playerService.GetQuantity(StaticItem.TOTAL_AVAILABLE_LAND_SPACE));
		}

		[DebugCommand]
		public void PlaySound(string[] args)
		{
			gameContext.injectionBinder.GetInstance<PlayGlobalSoundFXSignal>().Dispatch(args[1]);
		}

		[DebugCommand(Name = "showfps", Args = new string[] { "sampleSize" }, RequiresAllArgs = true)]
		[DebugCommand(Name = "hidefps")]
		public void ToggleFps(string[] args)
		{
			bool flag = args[0] == "showfps";
			int result = 0;
			if (flag)
			{
				if (int.TryParse(args[1], out result))
				{
					addFPSCounterSignal.Dispatch(true, result);
				}
				else
				{
					outBuilder.AppendFormat("Invalid sampleSize {0} must be an integer value", args[1]);
				}
			}
			else
			{
				addFPSCounterSignal.Dispatch(false, 0);
			}
		}

		[DebugCommand]
		public void AddPartyFavorItem(string[] args)
		{
			int result;
			if (args.Length < 2)
			{
				outBuilder.AppendLine(string.Format("Invalid Party Favor Id {0}", args));
			}
			else if (int.TryParse(args[1], out result))
			{
				Definition definition = definitionService.Get(result);
				PartyFavorAnimationDefinition partyFavorAnimationDefinition = definition as PartyFavorAnimationDefinition;
				if (partyFavorAnimationDefinition != null)
				{
					playerService.CreateAndRunCustomTransaction(partyFavorAnimationDefinition.UnlockId, 1, TransactionTarget.NO_VISUAL);
					return;
				}
				PartyFavorAnimationItemDefinition partyFavorAnimationItemDefinition = definition as PartyFavorAnimationItemDefinition;
				if (partyFavorAnimationItemDefinition != null)
				{
					playerService.CreateAndRunCustomTransaction(partyFavorAnimationItemDefinition.ID, 1, TransactionTarget.NO_VISUAL);
					return;
				}
				outBuilder.AppendLine(string.Format("Invalid Definition Type {0}", definition));
				outBuilder.AppendLine("Enter in a Definition id of type PartyFavorAnimationItemDefinition or PartyFavorAnimationDefinition");
			}
			else
			{
				outBuilder.AppendLine(string.Format("Invalid Argument for Party Favor Id {0}", args[1]));
			}
		}

		[DebugCommand]
		public void ListActivePartyFavors(string[] args)
		{
			List<PartyFavorAnimationDefinition> all = definitionService.GetAll<PartyFavorAnimationDefinition>();
			if (all == null)
			{
				return;
			}
			int num = 0;
			foreach (PartyFavorAnimationDefinition item in all)
			{
				if (playerService.GetQuantityByDefinitionId(item.UnlockId) != 0)
				{
					outBuilder.AppendLine(string.Format("{0}: Name {2}, id {1}, item id: {3}, animation id: {4}", ++num, item.ID, item.LocalizedKey, item.ItemID, item.AnimationID));
					break;
				}
			}
		}

		[DebugCommand]
		public void PartyPoints(string[] args)
		{
			int result = 0;
			if (args.Length > 1 && int.TryParse(args[1], out result))
			{
				playerService.CreateAndRunCustomTransaction(2, result, TransactionTarget.NO_VISUAL);
				uiContext.injectionBinder.GetInstance<SetXPSignal>().Dispatch();
				return;
			}
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			bool flag = playerService.IsMinionPartyUnlocked();
			outBuilder.AppendLine("Minon Party is currently " + ((!flag) ? "Disabled" : "Enabled"));
			outBuilder.AppendLine("You currently have " + minionPartyInstance.CurrentPartyPoints + " party points.");
			outBuilder.AppendLine("You need " + minionPartyInstance.CurrentPartyPointsRequired + " (or another action) to raise Party Meter Tier again.");
		}

		[DebugCommand(Name = "showpp")]
		[DebugCommand(Name = "hidepp")]
		public void DisplayPartyPoints(string[] args)
		{
			if (args[0] == "showpp")
			{
				GameObject gameObject = new GameObject("PartyPoints");
				gameObject.transform.parent = glassCanvas.transform;
				gameObject.layer = 5;
				Text text = gameObject.AddComponent<Text>();
				text.transform.localPosition = Vector3.zero;
				text.rectTransform.localScale = Vector3.one;
				text.font = KampaiResources.Load<Font>("HelveticaLTStd-Cond");
				text.fontSize = 32;
				text.rectTransform.offsetMin = new Vector2(-50f, 0f - (float)Screen.height / 2f);
				text.rectTransform.offsetMax = new Vector2(50f, 100f - (float)Screen.height / 2f);
				routineRunner.StartCoroutine(PartyPointsUpdate(text));
			}
			else
			{
				GameObject gameObject2 = GameObject.Find("PartyPoints");
				Text component = gameObject2.GetComponent<Text>();
				routineRunner.StopCoroutine(PartyPointsUpdate(component));
				UnityEngine.Object.Destroy(gameObject2);
			}
		}

		private IEnumerator PartyPointsUpdate(Text textCmp)
		{
			while (true)
			{
				yield return null;
				textCmp.text = playerService.GetQuantity(StaticItem.XP_ID).ToString();
			}
		}

		[DebugCommand]
		public void StopSaving(string[] args)
		{
			savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, string.Empty, true));
			PlayerPrefs.SetInt("Debug.StopSaving", 1);
			outBuilder.AppendLine("Last save complete. Turning off remote save. Use the StartSaving command to enable saving again.");
		}

		[DebugCommand]
		public void StartSaving(string[] args)
		{
			PlayerPrefs.DeleteKey("Debug.StopSaving");
			outBuilder.AppendLine("Turning on remote save.");
		}

		[DebugCommand]
		public void StartParty(string[] args)
		{
			gameContext.injectionBinder.GetInstance<StartMinionPartyIntroSignal>().Dispatch();
		}

		[DebugCommand]
		public void EndParty(string[] args)
		{
			int result = 0;
			if (args.Length > 1 && int.TryParse(args[1], out result))
			{
				gameContext.injectionBinder.GetInstance<EndPartyBuffTimerSignal>().Dispatch(result);
			}
			else
			{
				gameContext.injectionBinder.GetInstance<EndPartyBuffTimerSignal>().Dispatch(1);
			}
		}

		[DebugCommand]
		public void HelpMe(string[] args)
		{
			mainContext.injectionBinder.GetInstance<OpenHelpSignal>().Dispatch(HelpType.ONLINE_HELP);
		}

		[DebugCommand(Name = "rc")]
		public void RefreshCatalog(string[] args)
		{
			currencyService.RefreshCatalog();
		}

		[DebugCommand]
		public void LandExpand(string[] args)
		{
			TransactionDefinition transactionDefinition = new TransactionDefinition();
			transactionDefinition.Inputs = new List<QuantityItem>();
			transactionDefinition.Outputs = new List<QuantityItem>();
			transactionDefinition.ID = int.MaxValue;
			PurchasedLandExpansion byInstanceId = playerService.GetByInstanceId<PurchasedLandExpansion>(354);
			IList<QuantityItem> outputs = transactionDefinition.Outputs;
			if (args.Length < 2)
			{
				foreach (LandExpansionBuilding allExpansionBuilding in landExpansionService.GetAllExpansionBuildings())
				{
					LandExpansionConfig expansionConfig = landExpansionConfigService.GetExpansionConfig(allExpansionBuilding.ExpansionID);
					QuantityItem item = new QuantityItem(expansionConfig.ID, 1u);
					if (!outputs.Contains(item) && !byInstanceId.HasPurchased(expansionConfig.expansionId))
					{
						outputs.Add(item);
					}
				}
			}
			else
			{
				for (int i = 1; i < args.Length; i++)
				{
					string text = args[i];
					int result;
					if (int.TryParse(text, out result))
					{
						LandExpansionConfig definition;
						if (definitionService.TryGet<LandExpansionConfig>(result, out definition))
						{
							if (byInstanceId.PurchasedExpansions.Contains(definition.expansionId))
							{
								outBuilder.AppendFormat("Already owned: {0}\n", result);
							}
							else
							{
								outputs.Add(new QuantityItem(definition.ID, 1u));
							}
						}
						else
						{
							outBuilder.AppendFormat("Not a land expansion config: {0}\n", result);
						}
					}
					else
					{
						outBuilder.AppendFormat("Not a number: {0}\n", text);
					}
				}
			}
			QuantityItemTriggerRewardDefinition quantityItemTriggerRewardDefinition = new QuantityItemTriggerRewardDefinition();
			quantityItemTriggerRewardDefinition.transaction = transactionDefinition.ToInstance();
			quantityItemTriggerRewardDefinition.RewardPlayer(gameContext);
			outBuilder.AppendFormat("Purchased {0} expansions\n", outputs.Count);
		}

		[DebugCommand]
		public void ClearDebris(string[] args)
		{
			CleanupDebrisSignal instance = gameContext.injectionBinder.GetInstance<CleanupDebrisSignal>();
			List<DebrisBuilding> instancesByType = playerService.GetInstancesByType<DebrisBuilding>();
			foreach (DebrisBuilding item in instancesByType)
			{
				instance.Dispatch(item.ID, false);
			}
		}

		[DebugCommand]
		public void AddBuddy(string[] args)
		{
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(40002);
			playerService.AssignNextInstanceId(prestigeDefinition);
			Prestige prestige = new Prestige(prestigeDefinition);
			prestige.state = PrestigeState.Prestige;
			prestige.CurrentPrestigePoints = 100;
			prestigeService.AddPrestige(prestige);
		}

		[DebugCommand]
		public void CreateLocalSocialEventInvitations(string[] args)
		{
			if (localInvitations == null)
			{
				localInvitations = new List<SocialEventInvitation>();
			}
			SocialEventInvitation socialEventInvitation = new SocialEventInvitation();
			socialEventInvitation.EventID = 101;
			socialEventInvitation.Team = new SocialTeamInvitationView();
			socialEventInvitation.Team.TeamID = 12421342345234346L;
			socialEventInvitation.inviter = new UserIdentity();
			socialEventInvitation.inviter.ExternalID = "1374474932861011";
			socialEventInvitation.inviter.ID = "1374474932861011";
			socialEventInvitation.inviter.Type = IdentityType.facebook;
			localInvitations.Add(socialEventInvitation);
		}

		public void OnGetSocialEventStateResponse(SocialTeamResponse response, ErrorResponse error)
		{
			if (response.UserEvent != null && !response.UserEvent.RewardClaimed && response.Team.OrderProgress.Count == timedSocialEventService.GetCurrentSocialEvent().Orders.Count)
			{
				uiContext.injectionBinder.GetInstance<ShowSocialPartyRewardSignal>().Dispatch(timedSocialEventService.GetCurrentSocialEvent().ID);
			}
			else if (response.Team != null)
			{
				uiContext.injectionBinder.GetInstance<ShowSocialPartyFillOrderSignal>().Dispatch(0);
			}
			else if (response.UserEvent != null && response.UserEvent.Invitations != null && response.UserEvent.Invitations.Count > 0 && facebookService.isLoggedIn)
			{
				uiContext.injectionBinder.GetInstance<ShowSocialPartyInviteAlertSignal>().Dispatch();
			}
			else
			{
				uiContext.injectionBinder.GetInstance<ShowSocialPartyStartSignal>().Dispatch();
			}
		}

		[DebugCommand]
		public void CheckTriggers(string[] args)
		{
			TSMCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<TSMCharacter>(70008);
			if (firstInstanceByDefinitionId == null)
			{
				logger.Error("Failed to find TSM Character in player inventory");
			}
			else
			{
				gameContext.injectionBinder.GetInstance<CheckTriggersSignal>().Dispatch(firstInstanceByDefinitionId.ID);
			}
		}

		[DebugCommand]
		public void ShowSocialPartyFlow(string[] args)
		{
			if (timedSocialEventService.GetCurrentSocialEvent() != null)
			{
				parseSocialEventInvitiations();
				Signal<SocialTeamResponse, ErrorResponse> signal = new Signal<SocialTeamResponse, ErrorResponse>();
				signal.AddListener(OnGetSocialEventStateResponse);
				timedSocialEventService.GetSocialEventState(timedSocialEventService.GetCurrentSocialEvent().ID, signal);
			}
		}

		[DebugCommand]
		public void AddStageBuilding(string[] args)
		{
			Location location = new Location(125, 158);
			StageBuildingDefinition stageBuildingDefinition = new StageBuildingDefinition();
			stageBuildingDefinition = definitionService.Get<StageBuildingDefinition>(3054);
			Building building = new StageBuilding(stageBuildingDefinition);
			building.ID = 356568568;
			building.Location = location;
			building.SetState(BuildingState.Idle);
			playerService.Add(building);
			CreateInventoryBuildingSignal instance = gameContext.injectionBinder.GetInstance<CreateInventoryBuildingSignal>();
			instance.Dispatch(building, location);
		}

		[DebugCommand]
		public void NutRush(string[] args)
		{
			EtceteraAndroid.showCustomWebView("http://192.168.64.190/~bbangerter/nut_rush/index.htm", true, false);
		}

		[DebugCommand]
		public void SprintClub(string[] args)
		{
			EtceteraAndroid.showCustomWebView("http://192.168.64.190/~bbangerter/sprint_club_nitro/index.htm", true, false);
		}

		[DebugCommand(Name = "createwayfinder", Args = new string[] { "trackedId" }, RequiresAllArgs = true)]
		[DebugCommand(Name = "removewayfinder", Args = new string[] { "trackedId" }, RequiresAllArgs = true)]
		public void CreateOrRemoveWayFinder(string[] args)
		{
			bool flag = args[0] == "createwayfinder";
			int num = int.Parse(args[1]);
			if (flag)
			{
				createWayFinderSignal.Dispatch(new WayFinderSettings(num));
			}
			else
			{
				removeWayFinderSignal.Dispatch(num);
			}
		}

		[DebugCommand(Name = "add", Args = new string[] { "quantity_item", "ammount" }, RequiresAllArgs = true)]
		[DebugCommand(Name = "remove", Args = new string[] { "quantity_item", "ammount" }, RequiresAllArgs = true)]
		public void AddOrRemove(string[] args)
		{
			bool add = args[0] == "add";
			int result = 0;
			if (!int.TryParse(args[1], out result) && !IdOf(args[1], out result))
			{
				outBuilder.AppendLine(string.Join(" ", args) + " -> DEFINITION NOT FOUND");
			}
			else
			{
				ProcessTransaction(args[2], result, add);
			}
		}

		[DebugCommand]
		public void Anim(string[] args)
		{
			if (args[1].Equals("on"))
			{
				loadAnimatorStateInfoSignal.Dispatch();
			}
			else if (args[1].Equals("off"))
			{
				unloadAnimatorStateInfoSignal.Dispatch();
			}
		}

		[DebugCommand]
		public void Transaction(string[] args)
		{
			int result = 0;
			if (int.TryParse(args[1], out result))
			{
				ProcessTransaction(result, true);
			}
			mainContext.injectionBinder.GetInstance<SetGrindCurrencySignal>().Dispatch();
			mainContext.injectionBinder.GetInstance<SetPremiumCurrencySignal>().Dispatch();
			mainContext.injectionBinder.GetInstance<SetStorageCapacitySignal>().Dispatch();
		}

		[DebugCommand]
		public void Grant(string[] args)
		{
			int result = 0;
			if (int.TryParse(args[1], out result))
			{
				ProcessTransaction(result, false);
			}
			mainContext.injectionBinder.GetInstance<SetGrindCurrencySignal>().Dispatch();
			mainContext.injectionBinder.GetInstance<SetPremiumCurrencySignal>().Dispatch();
			mainContext.injectionBinder.GetInstance<SetStorageCapacitySignal>().Dispatch();
		}

		[DebugCommand(Name = "set lod")]
		public void SetLod(string[] args)
		{
			if (args[2].Equals("low"))
			{
				minionBuilder.SetLOD(TargetPerformance.LOW);
			}
			else if (args[2].Equals("medium"))
			{
				minionBuilder.SetLOD(TargetPerformance.MED);
			}
			else if (args[2].Equals("high"))
			{
				minionBuilder.SetLOD(TargetPerformance.HIGH);
			}
			outBuilder.AppendLine(string.Join(" ", args) + " -> LOD SET " + minionBuilder.GetLOD());
			mainContext.injectionBinder.GetInstance<ReloadGameSignal>().Dispatch();
		}

		[DebugCommand(Name = "set config")]
		public void SetConfig(string[] args)
		{
			if (args[2] != null)
			{
				ABTestCommand.GameMetaData gameMetaData = new ABTestCommand.GameMetaData();
				gameMetaData.configurationVariant = args[2];
				gameMetaData.debugConsoleTest = true;
				ABTestSignal.Dispatch(gameMetaData);
			}
		}

		[DebugCommand(Name = "set shaderlod")]
		public void SetShaderLod(string[] args)
		{
			if (args.Length > 2)
			{
				int result = -1;
				int.TryParse(args[2], out result);
				if (result < 0)
				{
					result = int.MaxValue;
				}
				Shader.globalMaximumLOD = result;
			}
			outBuilder.AppendLine(string.Format("Shader LOD: {0}", Shader.globalMaximumLOD));
		}

		[DebugCommand(Name = "list pf")]
		public void ListAvailablePartyFavors(string[] args)
		{
			List<int> availablePartyFavorItems = partyFavorAnimationService.GetAvailablePartyFavorItems();
			outBuilder.AppendLine("Count: " + availablePartyFavorItems.Count);
			foreach (int item in availablePartyFavorItems)
			{
				outBuilder.AppendLine("  ID: " + item);
			}
			outBuilder.AppendLine("\n");
		}

		[DebugCommand(Name = "trigger pf", Args = new string[] { "[id id]" })]
		public void TriggerPartyFavorIncidental(string[] args)
		{
			int result = 0;
			if (int.TryParse(args[2], out result))
			{
				partyFavorAnimationService.PlayRandomIncidentalAnimation(result);
			}
		}

		[DebugCommand(Name = "set definition", Args = new string[] { "[id id]|[variants variants]" })]
		public void SetDefinition(string[] args)
		{
			bool flag = false;
			ABTestCommand.GameMetaData gameMetaData = new ABTestCommand.GameMetaData();
			gameMetaData.debugConsoleTest = true;
			for (int i = 2; i < args.Length; i += 2)
			{
				if (i + 1 < args.Length && !string.IsNullOrEmpty(args[i]) && !string.IsNullOrEmpty(args[i + 1]))
				{
					string text = args[i];
					string text2 = args[i + 1];
					switch (text)
					{
					case "id":
						gameMetaData.definitionId = text2;
						flag = true;
						break;
					case "variants":
						gameMetaData.definitionVariants = text2;
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				ABTestSignal.Dispatch(gameMetaData);
			}
		}

		[DebugCommand(Name = "set pref")]
		public void SetPref(string[] args)
		{
			if (args.Length != 5)
			{
				outBuilder.AppendLine("set pref: Too little arguments.");
				return;
			}
			string text = args[2];
			string text2 = args[3];
			string text3 = args[4];
			string empty = string.Empty;
			if (text.Equals("int"))
			{
				PlayerPrefs.SetInt(text2, int.Parse(text3));
				empty = PlayerPrefs.GetInt(text2).ToString();
			}
			else if (text.Equals("float"))
			{
				PlayerPrefs.SetFloat(text2, float.Parse(text3));
				empty = PlayerPrefs.GetFloat(text2).ToString();
			}
			else
			{
				PlayerPrefs.SetString(text2, text3);
				empty = PlayerPrefs.GetString(text2);
			}
			PlayerPrefs.Save();
			outBuilder.AppendLine(text2 + " = " + empty);
		}

		[DebugCommand(Name = "get config")]
		public void GetConfig(string[] args)
		{
			outBuilder.AppendLine("Config URL: " + configService.GetConfigURL());
			outBuilder.AppendLine("Definition URL: " + configService.GetConfigurations().definitions);
		}

		[DebugCommand(Name = "get pref")]
		public void GetPref(string[] args)
		{
			string key = args[2];
			if (PlayerPrefs.HasKey(key))
			{
				string empty = string.Empty;
				float @float = PlayerPrefs.GetFloat(key, float.NaN);
				int @int = PlayerPrefs.GetInt(key, int.MaxValue);
				empty = ((!@float.Equals(float.NaN)) ? @float.ToString() : (@int.Equals(int.MaxValue) ? PlayerPrefs.GetString(key) : @int.ToString()));
				outBuilder.AppendLine(empty);
			}
			else
			{
				outBuilder.AppendLine("Key not found");
			}
		}

		[DebugCommand(Name = "get shaders")]
		public void GetShaders(string[] args)
		{
			HashSet<string> hashSet = new HashSet<string>();
			Renderer[] array = Resources.FindObjectsOfTypeAll<Renderer>();
			foreach (Renderer renderer in array)
			{
				Material[] materials = renderer.materials;
				foreach (Material material in materials)
				{
					if (material.shader != null && !string.IsNullOrEmpty(material.shader.name))
					{
						hashSet.Add(material.shader.name);
					}
				}
			}
			outBuilder.AppendLine("Shaders:");
			foreach (string item in hashSet)
			{
				outBuilder.AppendLine("\t" + item);
			}
			outBuilder.AppendLine(string.Format("Found {0} shaders", hashSet.Count));
		}

		[DebugCommand]
		public void Enable(string[] args)
		{
			outBuilder.AppendLine(string.Join(" ", args) + " TO DO ??? not implemented");
		}

		[DebugCommand(Name = "load local")]
		public void LoadLocal(string[] args)
		{
			string data = args[2];
			localPersistService.PutData("LoadMode", "local");
			localPersistService.PutData("LocalID", data);
			SceneManager.LoadScene("Main");
		}

		[DebugCommand(Name = "load file")]
		public void LoadFile(string[] args)
		{
			string data = args[2];
			localPersistService.PutData("LoadMode", "file");
			localPersistService.PutData("LocalFileName", data);
			SceneManager.LoadScene("Main");
		}

		[DebugCommand(Name = "load remote")]
		public void LoadRemote(string[] args)
		{
			localPersistService.PutData("LoadMode", "remote");
			SceneManager.LoadScene("Main");
		}

		[DebugCommand(Name = "zeroselltime", Args = new string[] { "on/off" }, RequiresAllArgs = true)]
		public void ZeroSellTime(string[] args)
		{
			if (args[1].Equals("on"))
			{
				localPersistService.PutData("ZeroSellTime", "true");
			}
			else
			{
				localPersistService.PutData("ZeroSellTime", "false");
			}
		}

		[DebugCommand(Name = "minstrikecost", Args = new string[] { "item_id", "cost" }, RequiresAllArgs = true)]
		public void MinStrikeCost(string[] args)
		{
			int itemID = int.Parse(args[1]);
			int price = int.Parse(args[2]);
			marketplaceService.SetMinStrikePrice(itemID, price);
		}

		[DebugCommand(Name = "maxstrikecost", Args = new string[] { "item_id", "cost" }, RequiresAllArgs = true)]
		public void MaxStrikeCost(string[] args)
		{
			int itemID = int.Parse(args[1]);
			int price = int.Parse(args[2]);
			marketplaceService.SetMaxStrikePrice(itemID, price);
		}

		[DebugCommand(Args = new string[] { "corruptionId", "client/server" })]
		public void CorruptMe(string[] args)
		{
			string text = args[1];
			SaveLocation first = ((args.Length < 3 || args[2].StartsWith("s")) ? SaveLocation.REMOTE_NOSANITY : SaveLocation.REMOTE);
			if ("5622" == text)
			{
				PurchasedLandExpansion byInstanceId = playerService.GetByInstanceId<PurchasedLandExpansion>(354);
				byInstanceId.AdjacentExpansions.Clear();
				byInstanceId.PurchasedExpansions.Clear();
			}
			else
			{
				outBuilder.AppendLine("Invalid corruption id");
			}
			savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(first, string.Empty, false));
		}

		[DebugCommand(Args = new string[] { "local/remote" })]
		public void Save(string[] args)
		{
			string text = args[1];
			string second = string.Empty;
			SaveLocation first = SaveLocation.REMOTE;
			if (text == "local")
			{
				second = args[2];
				first = SaveLocation.LOCAL;
			}
			savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(first, second, false));
		}

		[DebugCommand(Name = "delete local")]
		public void DeleteLocal(string[] args)
		{
			localPersistService.DeleteAll();
		}

		[DebugCommand(Name = "fb init")]
		public void FBInit(string[] args)
		{
			socialInitSignal.Dispatch(facebookService);
		}

		[DebugCommand(Name = "fb login")]
		public void FBLogin(string[] args)
		{
			facebookService.LoginSource = "Debug Console";
			socialLoginSignal.Dispatch(facebookService, new Boxed<Action>(null));
		}

		[DebugCommand(Name = "fb logout")]
		public void FBLogout(string[] args)
		{
			socialLogoutSignal.Dispatch(facebookService);
		}

		[DebugCommand(Name = "camera position")]
		public void CameraPosition(string[] args)
		{
			outBuilder.AppendLine(cameraGO.transform.position.ToString());
		}

		[DebugCommand(Name = "camera tilt")]
		public void CameraTilt(string[] args)
		{
			outBuilder.AppendLine(cameraGO.transform.eulerAngles.x.ToString());
		}

		[DebugCommand(Name = "camera fov")]
		public void CameraFOV(string[] args)
		{
			outBuilder.AppendLine(cameraComponent.fieldOfView.ToString());
		}

		[DebugCommand(Name = "camera initial zoom")]
		public void CameraInitialZoom(string[] args)
		{
			outBuilder.AppendLine(zoomView.InitialFraction.ToString());
		}

		[DebugCommand(Name = "synergy login")]
		public void SynergyLogin(string[] args)
		{
			string userSynergyId = args[2];
			logger.Debug("Logging into synergy");
			NimbleBridge_SynergyIdManager.GetComponent().Login(userSynergyId, "test");
		}

		[DebugCommand(Name = "purchase a")]
		public void PurchaseApprove(string[] args)
		{
			PurchaseApproval(true);
		}

		[DebugCommand(Name = "purchase d")]
		public void PurchaseDeny(string[] args)
		{
			PurchaseApproval(false);
		}

		[DebugCommand(Name = "map view enable")]
		public void MapEnable(string[] args)
		{
			SetMap(true);
		}

		[DebugCommand(Name = "map view disable")]
		public void MapDisable(string[] args)
		{
			SetMap(false);
		}

		[DebugCommand(Name = "grid enable")]
		public void GridEnable(string[] args)
		{
			gameContext.injectionBinder.GetInstance<PopulateEnvironmentSignal>().Dispatch(true);
		}

		[DebugCommand(Name = "grid disable")]
		public void GridDisable(string[] args)
		{
			gameContext.injectionBinder.GetInstance<PopulateEnvironmentSignal>().Dispatch(false);
		}

		[DebugCommand(Args = new string[] { "id" })]
		public void StartQuest(string[] args)
		{
			QuestDefinition def = definitionService.Get<QuestDefinition>(int.Parse(args[1]));
			Quest quest = new Quest(def);
			quest.Initialize();
			questService.RemoveQuest(quest.GetActiveDefinition().ID);
			questService.AddQuest(quest);
			questScriptService.StartQuestScript(quest, true);
		}

		[DebugCommand]
		public void Lua(string[] args)
		{
			string scriptText = string.Join(" ", args, 1, args.Length - 1);
			luaRunner.Stop();
			luaRunner.Start(consoleQuestScript, scriptText, "THE ALMIGHTY DEBUG CONSOLE", null);
		}

		[DebugCommand]
		public void LuaFile(string[] args)
		{
			luaRunner.Stop();
			TextAsset textAsset = Resources.Load<TextAsset>(args[1]);
			luaRunner.Start(consoleQuestScript, textAsset.text, args[1], null);
		}

		[DebugCommand(Args = new string[] { "id" })]
		public void ShowQuest(string[] args)
		{
			QuestDefinition questDefinition = definitionService.Get<QuestDefinition>(int.Parse(args[1]));
			Dictionary<int, IQuestController> questMap = questService.GetQuestMap();
			IQuestController questController;
			if (questMap.ContainsKey(questDefinition.ID))
			{
				questController = questMap[questDefinition.ID];
			}
			else
			{
				Quest quest = new Quest(questDefinition);
				quest.Initialize();
				questController = questService.AddQuest(quest);
			}
			questController.Debug_SetQuestToInProgressIfNotAlready();
			uiContext.injectionBinder.GetInstance<ShowQuestPanelSignal>().Dispatch(questController.ID);
		}

		[DebugCommand(Name = "dlc list")]
		public void DLCList(string[] args)
		{
			outBuilder.AppendLine("DLC Packs:");
			DirectoryInfo directoryInfo = new DirectoryInfo(GameConstants.DLC_PATH);
			int num = 0;
			FileInfo[] files = directoryInfo.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				outBuilder.AppendLine("\t" + fileInfo.Name);
				num++;
			}
			outBuilder.AppendLine(string.Format("Found {0} bundles", num));
		}

		[DebugCommand(Name = "dlc backup")]
		public void CheckDlcBackupFlag(string[] args)
		{
			outBuilder.AppendLine("iOS only!");
		}

		[DebugCommand(Name = "dlc clear")]
		public void DLCClear(string[] args)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(GameConstants.DLC_PATH);
			FileInfo[] files = directoryInfo.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				fileInfo.Delete();
			}
			SceneManager.LoadScene("Initialize");
		}

		[DebugCommand(Name = "dlc quality")]
		public void DLCQuality(string[] args)
		{
			outBuilder.AppendLine("DLC Download Quality = " + dlcService.GetDownloadQualityLevel());
			outBuilder.AppendLine("DLC Display Quality = " + dlcService.GetDisplayQualityLevel());
			outBuilder.AppendLine("DLC Tier = " + playerService.GetQuantity(StaticItem.TIER_ID));
		}

		[DebugCommand(Name = "dlc setquality")]
		public void DLCSetQuality(string[] args)
		{
			string value = args[2];
			TargetPerformance targetPerformance = (TargetPerformance)(int)Enum.Parse(typeof(TargetPerformance), value, true);
			if (targetPerformance != TargetPerformance.UNKNOWN && targetPerformance != TargetPerformance.UNSUPPORTED)
			{
				localPersistService.PutData("FORCE_LOD", targetPerformance.ToString());
				mainContext.injectionBinder.GetInstance<ReloadGameSignal>().Dispatch();
			}
		}

		[DebugCommand(Name = "dlc tier")]
		public void DLCTier(string[] args)
		{
			outBuilder.AppendLine("Player's DLC Tier = " + playerService.GetQuantity(StaticItem.TIER_ID));
			outBuilder.AppendLine("Player's Tier Gate = " + playerService.GetQuantity(StaticItem.TIER_GATE_ID));
			outBuilder.AppendLine("Highest Tier Downloaded  = " + gameContext.injectionBinder.GetInstance<DLCModel>().HighestTierDownloaded);
		}

		[DebugCommand(Name = "dlc preinstall")]
		public void DLCPreInstall(string[] args)
		{
			string value = File.ReadAllText(GameConstants.PREINSTALL_JSON_PATH);
			PreinstallBundles preinstallBundles = JsonConvert.DeserializeObject<PreinstallBundles>(value);
			foreach (string bundle in preinstallBundles.Bundles)
			{
				outBuilder.AppendLine(bundle);
			}
		}

		[DebugCommand(Args = new string[] { "fb/gc/gp" })]
		public void Unlink(string[] args)
		{
			Dictionary<string, IdentityType> dictionary = new Dictionary<string, IdentityType>();
			dictionary.Add("fb", IdentityType.facebook);
			dictionary.Add("gc", IdentityType.gamecenter);
			dictionary.Add("gp", IdentityType.googleplay);
			Dictionary<string, IdentityType> dictionary2 = dictionary;
			IdentityType value;
			if (dictionary2.TryGetValue(args[1], out value))
			{
				unlinkAccountSignal.Dispatch(value);
			}
			else
			{
				outBuilder.AppendLine("Invalid identity type!");
			}
		}

		[DebugCommand(Args = new string[] { "displaymib (true|false)" })]
		public void DisplayMIB(string[] args)
		{
			bool type = Convert.ToBoolean(args[1]);
			gameContext.injectionBinder.GetInstance<DisplayMIBBuildingSignal>().Dispatch(type);
		}

		[DebugCommand(Args = new string[] { "setmibreturning (true|false)" })]
		public void SetMIBReturning(string[] args)
		{
			if (Convert.ToBoolean(args[1]))
			{
				mibService.SetReturningKey();
			}
			else
			{
				mibService.ClearReturningKey();
			}
		}

		[DebugCommand(Name = "resetadlimits")]
		public void ResetRewardedAdLimits(string[] args)
		{
			logger.Info("Debug console: reset rewarded ad limits");
			resetRewardedAdLimitSignal.Dispatch();
		}

		[DebugCommand(Name = "ssrv", Args = new string[] { "placement id" }, RequiresAllArgs = false)]
		public void SupersonicRewardedVideo(string[] args)
		{
			string text = ((args.Length <= 1) ? string.Empty : args[1]);
			logger.Info("Debug console: try to show rewarded video, placement: '{0}'", text);
			supersonicService.ShowRewardedVideo(text);
		}

		[DebugCommand(Name = "sso")]
		public void SupersonicOfferwall(string[] args)
		{
			logger.Info("Debug console: show offerwall");
			supersonicService.ShowOfferwall();
		}

		[DebugCommand(Name = "adstatus")]
		public void RewardedAdStatus(string[] args)
		{
			RewardedAdService rewardedAdService = this.rewardedAdService as RewardedAdService;
			if (rewardedAdService == null)
			{
				outBuilder.AppendFormat("Not supported type of rewardedAdService: {0}", this.rewardedAdService.GetType());
			}
			else
			{
				outBuilder.AppendLine(rewardedAdService.GetPlacementsReport());
			}
		}

		[DebugCommand(Args = new string[] { "def id" }, RequiresAllArgs = true)]
		public void CreatePrestige(string[] args)
		{
			int result = 0;
			int.TryParse(args[1], out result);
			Prestige prestige = prestigeService.GetPrestige(result, false);
			if (prestige != null)
			{
				outBuilder.AppendFormat("Prestige already exists for def id {0}: {1}\n", result, prestige);
				return;
			}
			PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(result);
			if (prestigeDefinition == null)
			{
				outBuilder.AppendLine(string.Join(" ", args) + " -> DEFINITION NOT FOUND");
				return;
			}
			outBuilder.AppendLine("Creating new prestige from def id " + result);
			prestige = new Prestige(prestigeDefinition);
			prestigeService.AddPrestige(prestige);
			prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Prestige);
			outBuilder.AppendFormat("Prestige created: {0}\n", prestige);
		}

		[DebugCommand(Args = new string[] { "instance id", "target state" }, RequiresAllArgs = true)]
		[DebugCommand(Name = "pc", Args = new string[] { "instance id", "target state" }, RequiresAllArgs = true)]
		public void PrestigeCharacter(string[] args)
		{
			int result = 0;
			int.TryParse(args[1], out result);
			Prestige byInstanceId = playerService.GetByInstanceId<Prestige>(result);
			if (byInstanceId == null)
			{
				outBuilder.AppendFormat("Prestige with id {0} not found!\n", result);
				return;
			}
			PrestigeState prestigeState;
			try
			{
				prestigeState = (PrestigeState)(int)Enum.Parse(typeof(PrestigeState), args[2], true);
			}
			catch (ArgumentException)
			{
				outBuilder.AppendFormat("{0} is not a quest state.\n", args[2]);
				return;
			}
			if (byInstanceId.state == PrestigeState.Prestige && prestigeState == PrestigeState.Prestige)
			{
				int num = byInstanceId.CurrentPrestigeLevel + 1;
				outBuilder.AppendFormat("Adding 1 to prestige level -> CurrentPrestigeLevel is now {0}\n", num);
				prestigeService.ChangeToPrestigeState(byInstanceId, PrestigeState.Prestige, num);
			}
			else
			{
				outBuilder.AppendFormat("Changing prestige from {0} state to {1} state.\n", byInstanceId.state, prestigeState);
				prestigeService.ChangeToPrestigeState(byInstanceId, prestigeState);
			}
		}

		[DebugCommand(Name = "lsp")]
		public void ListPrestige(string[] args)
		{
			List<Prestige> instancesByType = playerService.GetInstancesByType<Prestige>();
			int i = 0;
			for (int count = instancesByType.Count; i < count; i++)
			{
				Prestige prestige = instancesByType[i];
				int num = 0;
				int num2 = 0;
				if (prestige.Definition.PrestigeLevelSettings != null)
				{
					num = prestige.CurrentPrestigeLevel;
					num2 = prestige.NeededPrestigePoints;
				}
				outBuilder.AppendFormat("{0}: ID:{1} State:{2} Level:{3} Points:{4} Needed:{5}\n", prestige.Definition.LocalizedKey, prestige.ID, num, Enum.GetName(typeof(PrestigeState), prestige.state), prestige.CurrentPrestigePoints, num2);
			}
		}

		[DebugCommand(Args = new string[] { "character id" })]
		public void UnlockCharacter(string[] args)
		{
			int result = 0;
			int.TryParse(args[1], out result);
			if (result != 0)
			{
				Prestige prestige = prestigeService.GetPrestige(result);
				if (prestige == null)
				{
					outBuilder.AppendLine(string.Join(" ", args) + " -> CHARACTER NOT FOUND");
				}
				else if (prestige.state != PrestigeState.Taskable && prestige.state == PrestigeState.Questing)
				{
					prestigeService.ChangeToPrestigeState(prestige, PrestigeState.Taskable);
				}
			}
		}

		[DebugCommand(Args = new string[] { "fatal id", "fatal code" })]
		public void Fatal(string[] args)
		{
			FatalCode code = (FatalCode)0;
			int result = 0;
			StringBuilder stringBuilder = new StringBuilder();
			if (args.Length > 1)
			{
				int result2 = 0;
				int.TryParse(args[1], out result2);
				code = (FatalCode)result2;
				if (args.Length > 2)
				{
					int.TryParse(args[2], out result);
				}
				for (int i = 4; i <= args.Length; i++)
				{
					stringBuilder.Append(args[i - 1]).Append(" ");
				}
			}
			string text = stringBuilder.ToString();
			if (string.IsNullOrEmpty(text))
			{
				logger.Fatal(code, result);
			}
			else
			{
				logger.Fatal(code, result, text);
			}
		}

		[DebugCommand(Args = new string[] { "points" })]
		public void AddMignetteScore(string[] args)
		{
			int result2;
			int result3;
			if (args.Length == 2 && mignetteGameModel.IsMignetteActive)
			{
				int result;
				if (int.TryParse(args[1], out result))
				{
					changeScoreSignal.Dispatch(result);
					spawnDooberSignal.Dispatch(glassCanvas.transform.GetComponentInChildren<MignetteHUDView>(), new Vector3(250f, 250f, 0f), result, true);
					return;
				}
			}
			else if (args.Length == 3 && int.TryParse(args[1], out result2) && int.TryParse(args[2], out result3))
			{
				MignetteScoreTriggerRewardDefinition mignetteScoreTriggerRewardDefinition = new MignetteScoreTriggerRewardDefinition();
				mignetteScoreTriggerRewardDefinition.Points = result2;
				mignetteScoreTriggerRewardDefinition.MignetteBuildingId = result3;
				mignetteScoreTriggerRewardDefinition.RewardPlayer(gameContext);
				return;
			}
			outBuilder.AppendLine("Usage: addmignettescore <points> [building id]");
		}

		[DebugCommand]
		public void AwardNextPrize(string[] args)
		{
			if (!mignetteGameModel.IsMignetteActive)
			{
				outBuilder.AppendLine(args[0] + "-> Can only be called while mignette is active");
				return;
			}
			RewardCollection activeCollectionForMignette = mignetteCollectionService.GetActiveCollectionForMignette(mignetteGameModel.BuildingId, false);
			int pointTotalForNextReward = activeCollectionForMignette.GetPointTotalForNextReward();
			changeScoreSignal.Dispatch(pointTotalForNextReward);
			spawnDooberSignal.Dispatch(glassCanvas.transform.GetComponentInChildren<MignetteHUDView>(), new Vector3(250f, 250f, 0f), pointTotalForNextReward, true);
		}

		[DebugCommand(Args = new string[] { "version" })]
		public void Version(string[] args)
		{
			int result;
			if (args.Length > 1 && int.TryParse(args[1], out result))
			{
				localPersistService.PutDataInt("OverrideVersion", result);
				localPersistService.PutData("OverrideVersionPersistState", "keep");
				outBuilder.AppendLine("New version will be active on app restart.");
			}
			else
			{
				outBuilder.AppendLine("Usage: version 1234");
			}
		}

		[DebugCommand(Name = "upgrade", Args = new string[] { "f/o" })]
		public void UpgradeClient(string[] args)
		{
			switch (args[1])
			{
			case "f":
				showForcedClientUpgradeScreenSignal.Dispatch();
				break;
			case "o":
				showClientUpgradeDialogSignal.Dispatch();
				break;
			default:
				outBuilder.Append("Usage: update f / update o");
				break;
			}
		}

		[DebugCommand(Name = "idof", Args = new string[] { "item_name" }, RequiresAllArgs = true)]
		public void IdOfCommand(string[] args)
		{
			int id;
			if (!IdOf(args[1], out id))
			{
				outBuilder.AppendLine(string.Join(" ", args) + " -> DEFINITION NOT FOUND");
			}
			else
			{
				outBuilder.AppendLine(id.ToString());
			}
		}

		[DebugCommand]
		public void UnlockSticker(string[] args)
		{
			int result;
			if (int.TryParse(args[1], out result))
			{
				playerService.CreateAndRunCustomTransaction(result, 1, TransactionTarget.NO_VISUAL);
			}
		}

		[DebugCommand]
		public void CustomCamera(string[] args)
		{
			int result;
			if (int.TryParse(args[1], out result))
			{
				gameContext.injectionBinder.GetInstance<CameraMoveToCustomPositionSignal>().Dispatch(result, new Boxed<Action>(RestoreCamera));
			}
		}

		private void RestoreCamera()
		{
			gameContext.injectionBinder.GetInstance<EnableCameraBehaviourSignal>().Dispatch(1);
			gameContext.injectionBinder.GetInstance<EnableCameraBehaviourSignal>().Dispatch(2);
		}

		[DebugCommand]
		public void AddToTotemPole(string[] args)
		{
			foreach (CompositeBuildingPieceDefinition item in definitionService.GetAll<CompositeBuildingPieceDefinition>())
			{
				if (playerService.GetFirstInstanceByDefinitionId<CompositeBuildingPiece>(item.ID) != null)
				{
					continue;
				}
				playerService.CreateAndRunCustomTransaction(item.ID, 1, TransactionTarget.NO_VISUAL);
				CompositeBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<CompositeBuilding>(item.BuildingDefinitionID);
				compositeBuildingPieceAddedSignal.Dispatch(firstInstanceByDefinitionId);
				break;
			}
		}

		private IEnumerator UpdateDLCTier()
		{
			yield return new WaitForSeconds(1f);
			playerDLCTierSignal.Dispatch();
		}

		private void ExpansionTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (!pct.Success || pct.GetPendingTransaction().Outputs == null)
			{
				return;
			}
			foreach (QuantityItem output in pct.GetPendingTransaction().Outputs)
			{
				Definition definition = definitionService.Get<Definition>(output.ID);
				LandExpansionConfig landExpansionConfig = definition as LandExpansionConfig;
				if (landExpansionConfig != null)
				{
					purchaseSignal.Dispatch(landExpansionConfig.expansionId, true);
				}
			}
			routineRunner.StartCoroutine(UpdateDLCTier());
		}

		[DebugCommand]
		public void UM(string[] args)
		{
			playerService.AddUpsellToPurchased(50002);
			PackDefinition packDefinition = definitionService.Get<PackDefinition>(50002);
			TransactionDefinition def = packDefinition.TransactionDefinition.ToDefinition();
			playerService.RunEntireTransaction(def, TransactionTarget.LAND_EXPANSION, ExpansionTransactionCallback);
			int num = 1;
			ICollection<int> animatingBuildingIDs = playerService.GetAnimatingBuildingIDs();
			foreach (int item in animatingBuildingIDs)
			{
				Building byInstanceId = playerService.GetByInstanceId<Building>(item);
				MignetteBuilding mignetteBuilding = byInstanceId as MignetteBuilding;
				if (mignetteBuilding != null)
				{
					byInstanceId.SetState(BuildingState.Idle);
					if (mignetteBuilding.GetMinionSlotsOwned() > num)
					{
						num = mignetteBuilding.GetMinionSlotsOwned();
					}
					outBuilder.Append(mignetteBuilding.Definition.LocalizedKey + "(" + item + ") set to Idle\n");
				}
			}
		}

		[DebugCommand]
		public void SetCooldown(string[] args)
		{
			int result;
			if (!int.TryParse(args[1], out result))
			{
				return;
			}
			Building byInstanceId = playerService.GetByInstanceId<Building>(mignetteGameModel.BuildingId);
			MignetteBuilding mignetteBuilding = byInstanceId as MignetteBuilding;
			if (mignetteBuilding != null)
			{
				int cooldown = mignetteBuilding.GetCooldown();
				int num = cooldown - result;
				if ((float)num < 0f)
				{
					num = 0;
					result = cooldown;
				}
				timeEventService.RushEvent(byInstanceId.ID);
				byInstanceId.StateStartTime = timeService.CurrentTime() - num;
				byInstanceId.SetState(BuildingState.Cooldown);
				int num2 = mignetteBuilding.GetCooldown() / 10;
				for (int i = 0; i < 10; i++)
				{
					timeEventService.AddEvent(byInstanceId.ID, byInstanceId.StateStartTime, i * num2, cooldownUpdateViewSignal);
				}
				timeEventService.AddEvent(byInstanceId.ID, byInstanceId.StateStartTime, mignetteBuilding.GetCooldown(), cooldownCompleteSignal);
				outBuilder.Append("Altered building:" + byInstanceId.Definition.ID + " cooldown to " + result);
			}
		}

		[DebugCommand]
		public void ShowSales(string[] args)
		{
			List<Sale> instancesByType = playerService.GetInstancesByType<Sale>();
			foreach (Sale item in instancesByType)
			{
				outBuilder.AppendFormat("Sale ID {0} [{1}]: Started - {2} Finished - {3} Viewed - {4} Purchased - {5} TimeRemaining - {6} StartDate - {7} EndDate - {8}\n", item.ID, item.Definition.ID, item.Started, item.Finished, item.Viewed, item.Purchased, timeEventService.GetTimeRemaining(item.ID), item.Definition.UTCStartDate, item.Definition.UTCEndDate);
			}
		}

		[DebugCommand]
		public void ResetPurchasedPack(string[] args)
		{
			int result = 0;
			if (!int.TryParse(args[1], out result))
			{
				Help(args);
			}
			else
			{
				playerService.ClearPurchasedUpsells(result);
			}
		}

		[DebugCommand]
		public void TriggerGacha(string[] args)
		{
			MinionState[] states = new MinionState[3]
			{
				MinionState.Idle,
				MinionState.Selectable,
				MinionState.WaitingOnMagnetFinger
			};
			List<Minion> minions = playerService.GetMinions(true, states);
			HashSet<int> hashSet = new HashSet<int>();
			for (int i = 0; i < 3; i++)
			{
				hashSet.Add(minions[i].ID);
			}
			Boxed<Vector3> center = new Boxed<Vector3>(new Vector3(150f, 0f, 150f));
			MinionAnimationInstructions type = new MinionAnimationInstructions(hashSet, center);
			gameContext.injectionBinder.GetInstance<StartGroupGachaSignal>().Dispatch(type);
		}

		[DebugCommand]
		public void TsmTrigger(string[] args)
		{
			int result = 0;
			if (!int.TryParse(args[1], out result))
			{
				Help(args);
				return;
			}
			IGUIService instance = uiContext.injectionBinder.GetInstance<IGUIService>();
			IGUICommand iGUICommand = instance.BuildCommand(GUIOperation.Queue, "popup_TSM_Gift_Upsell");
			iGUICommand.skrimScreen = "ProceduralTaskSkrim";
			iGUICommand.darkSkrim = true;
			IList<TriggerDefinition> triggerDefinitions = definitionService.GetTriggerDefinitions();
			IList<TriggerInstance> triggers = playerService.GetTriggers();
			TriggerInstance triggerInstance = null;
			foreach (TriggerInstance item in triggers)
			{
				if (item.ID == result)
				{
					triggerInstance = item;
					break;
				}
			}
			if (triggerInstance == null)
			{
				foreach (TriggerDefinition item2 in triggerDefinitions)
				{
					if (item2.ID == result)
					{
						triggerInstance = item2.Build();
						break;
					}
				}
				if (triggerInstance == null)
				{
					Help(args);
					return;
				}
			}
			iGUICommand.Args.Add(typeof(TriggerInstance), triggerInstance);
			instance.Execute(iGUICommand);
		}

		[DebugCommand]
		public void ShowUpsellModal(string[] args)
		{
			int result = 0;
			PackDefinition definition;
			if (!int.TryParse(args[1], out result))
			{
				Help(args);
			}
			else if (definitionService.TryGet<PackDefinition>(result, out definition))
			{
				openUpSellModalSignal.Dispatch(definition, "Upsell", false);
			}
			else
			{
				outBuilder.AppendLine("Invalid upsell or salepack id");
			}
		}

		[DebugCommand]
		public void ResetTriggerReward(string[] args)
		{
			int result = 0;
			if (!int.TryParse(args[1], out result))
			{
				Help(args);
				return;
			}
			IGUIService instance = uiContext.injectionBinder.GetInstance<IGUIService>();
			IGUICommand iGUICommand = instance.BuildCommand(GUIOperation.LoadUntrackedInstance, "popup_TSM_Gift_Upsell");
			iGUICommand.skrimScreen = "ProceduralTaskSkrim";
			iGUICommand.darkSkrim = true;
			IList<TriggerInstance> triggers = playerService.GetTriggers();
			TriggerInstance triggerInstance = null;
			foreach (TriggerInstance item in triggers)
			{
				if (item.ID == result)
				{
					triggerInstance = item;
					break;
				}
			}
			if (triggerInstance == null)
			{
				Help(args);
			}
			else
			{
				triggerInstance.RecievedRewardIds.Clear();
			}
		}

		[DebugCommand(Args = new string[] { "Duration", "Percent off", "[Optional]i|in {itemId quantity}...", "o|out {itemId quantity}...", "s|sku" }, RequiresAllArgs = false)]
		public void TriggerSales(string[] args)
		{
			List<SalePackDefinition> all = definitionService.GetAll<SalePackDefinition>();
			SalePackDefinition salePackDefinition = all.LastOrDefault();
			if (salePackDefinition == null)
			{
				return;
			}
			int result2;
			if (args.Length == 3)
			{
				int result;
				if (int.TryParse(args[1], out result) && int.TryParse(args[2], out result2))
				{
					SalePackDefinition salePackDefinition2 = definitionService.Get<SalePackDefinition>(result);
					if (salePackDefinition2 != null)
					{
						salePackDefinition2.UTCStartDate = timeService.CurrentTime() + 2;
						salePackDefinition2.UTCEndDate = timeService.CurrentTime() + 2 + result2;
						salePackDefinition2.Duration = result2;
						salePackDefinition2.ID = result + all.Count + timeService.AppTime();
					}
				}
				return;
			}
			bool flag = false;
			if (args.Length < 5)
			{
				return;
			}
			for (int i = 0; i < args.Length; i++)
			{
				if (!(args[i].ToLower() != "s") || !(args[i].ToLower() != "sku"))
				{
					flag = true;
					break;
				}
			}
			int result3;
			if (!int.TryParse(args[1], out result2) || !int.TryParse(args[2], out result3))
			{
				return;
			}
			int num = 3;
			List<QuantityItem> list = new List<QuantityItem>();
			List<QuantityItem> list2 = new List<QuantityItem>();
			int result4;
			uint result5;
			if ((!flag && num < args.Length && args[num].ToLower() == "i") || args[num].ToLower() == "in")
			{
				num++;
				outBuilder.AppendLine("Upsell inputs are:");
				while (num + 1 < args.Length && !(args[num].ToLower() == "o") && !(args[num].ToLower() == "out") && int.TryParse(args[num++], out result4) && uint.TryParse(args[num++], out result5))
				{
					list.Add(new QuantityItem(result4, result5));
					outBuilder.AppendLine(new QuantityItem(result4, result5).ToString());
				}
				if (list.Count == 0)
				{
					outBuilder.AppendLine("Invalid input Count, remove to make the upsell free");
					return;
				}
				outBuilder.AppendLine();
			}
			if ((num < args.Length && args[num].ToLower() == "o") || args[num].ToLower() == "out")
			{
				num++;
				outBuilder.AppendLine("Upsell outputs are:");
				while (num + 1 < args.Length && int.TryParse(args[num++], out result4) && uint.TryParse(args[num++], out result5))
				{
					list2.Add(new QuantityItem(result4, result5));
					outBuilder.AppendLine(new QuantityItem(result4, result5).ToString());
				}
				if (list2.Count == 0)
				{
					outBuilder.AppendLine("Invalid output Count");
					return;
				}
				outBuilder.AppendLine();
				SalePackDefinition salePackDefinition3 = new SalePackDefinition();
				salePackDefinition3.ID = salePackDefinition.ID + all.Count + timeService.AppTime();
				salePackDefinition3.Type = SalePackType.Upsell;
				salePackDefinition3.UTCStartDate = timeService.CurrentTime() + 2;
				salePackDefinition3.UTCEndDate = timeService.CurrentTime() + 2 + result2;
				salePackDefinition3.PercentagePer100 = result3;
				salePackDefinition3.Duration = result2;
				salePackDefinition3.BannerAd = "UpSellTimeLeft";
				salePackDefinition3.ExclusiveItemList = new List<int>(205);
				salePackDefinition3.CurrencyImageID = 0;
				TransactionDefinition transactionDefinition = new TransactionDefinition();
				if (flag)
				{
					salePackDefinition3.TransactionType = UpsellTransactionType.Cash;
					salePackDefinition3.LocalizedKey = "UpSellTitle";
					salePackDefinition3.Description = "MTXUpSellHeadline";
					salePackDefinition3.PlatformStoreSku = new List<PlatformStoreSkuDefinition>();
					PlatformStoreSkuDefinition platformStoreSkuDefinition = new PlatformStoreSkuDefinition();
					platformStoreSkuDefinition.appleAppstore = "com.ea.mtx.870581";
					platformStoreSkuDefinition.googlePlay = "870582";
					platformStoreSkuDefinition.defaultStore = "SKU_STARTER_PACK";
					PlatformStoreSkuDefinition item = platformStoreSkuDefinition;
					salePackDefinition3.PlatformStoreSku.Add(item);
					salePackDefinition3.ActiveSKUIndex = 0;
				}
				else if (list.Count >= 1)
				{
					salePackDefinition3.TransactionType = ((result3 <= 0) ? UpsellTransactionType.ItemPurchase : UpsellTransactionType.ItemDiscount);
				}
				else
				{
					salePackDefinition3.BannerAd = "Gift";
					salePackDefinition3.TransactionType = UpsellTransactionType.Free;
				}
				transactionDefinition.Inputs = list;
				transactionDefinition.Outputs = list2;
				salePackDefinition3.TransactionDefinition = transactionDefinition.ToInstance();
				playerService.AssignNextInstanceId(salePackDefinition3.TransactionDefinition);
				definitionService.Add(salePackDefinition3);
				definitionService.Add(salePackDefinition3.TransactionDefinition.ToDefinition());
				reconcileSalesSignal.Dispatch(0);
			}
			else
			{
				outBuilder.AppendLine("Missing output variables");
				FlushSignal.Dispatch();
			}
		}

		[DebugCommand]
		public void TriggerSales2(string[] args)
		{
			int result;
			int result2;
			if (int.TryParse(args[1], out result) && int.TryParse(args[2], out result2))
			{
				SalePackDefinition definition;
				if (!definitionService.TryGet<SalePackDefinition>(result, out definition))
				{
					Debug.LogError("Unable to find: " + result);
				}
				if (definition != null)
				{
					SalePackDefinition salePackDefinition = definition;
					salePackDefinition.ID = definition.ID + timeService.AppTime();
					salePackDefinition.Type = SalePackType.Upsell;
					int num = 2;
					salePackDefinition.UTCStartDate = timeService.CurrentTime() + num;
					salePackDefinition.UTCEndDate = timeService.CurrentTime() + num + result2;
					salePackDefinition.Duration = result2;
					playerService.AssignNextInstanceId(salePackDefinition.TransactionDefinition);
					definitionService.Add(salePackDefinition);
					definitionService.Add(salePackDefinition.TransactionDefinition.ToDefinition());
					reconcileSalesSignal.Dispatch(0);
				}
			}
		}

		[DebugCommand]
		public void ReconcileSales(string[] args)
		{
			reconcileSalesSignal.Dispatch(0);
		}

		[DebugCommand]
		public void TestAppStoreOpen(string[] args)
		{
			string text = "com.ea.gp.pets";
			Native.OpenAppStoreLink("market://details?id=" + text);
		}

		[DebugCommand]
		public void TriggerRedemptionSale(string[] args)
		{
			int result;
			if (int.TryParse(args[1], out result))
			{
				SalePackDefinition salePackDefinition = definitionService.Get<SalePackDefinition>(result);
				if (salePackDefinition != null)
				{
					ReceiptValidationResult validationResult = new ReceiptValidationResult(salePackDefinition.SKU, string.Empty, string.Empty, ReceiptValidationResult.Code.SUCCESS);
					playerService.addPendingRemption(validationResult);
					openUpSellModalSignal.Dispatch(salePackDefinition, "REDEMPTION", true);
				}
			}
		}

		[DebugCommand]
		public void BuyStarterPack(string[] args)
		{
			SalePackDefinition salePackDefinition = definitionService.Get<SalePackDefinition>(50001);
			TransactionArg transactionArg = new TransactionArg();
			transactionArg.IsFromPremiumSource = true;
			playerService.RunEntireTransaction(salePackDefinition.TransactionDefinition.ToDefinition(), TransactionTarget.CURRENCY, null, transactionArg);
			gameContext.injectionBinder.GetInstance<UnlockMinionsSignal>().Dispatch();
			Sale firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Sale>(salePackDefinition.ID);
			firstInstanceByDefinitionId.Purchased = true;
			gameContext.injectionBinder.GetInstance<EndSaleSignal>().Dispatch(firstInstanceByDefinitionId.ID);
		}

		[DebugCommand]
		public void RemoveSale(string[] args)
		{
			int result;
			if (int.TryParse(args[1], out result))
			{
				gameContext.injectionBinder.GetInstance<EndSaleSignal>().Dispatch(result);
			}
		}

		[DebugCommand]
		public void ResetRateMyApp(string[] args)
		{
			localPersistService.PutDataPlayer("RateApp", "Enabled");
			uiContext.injectionBinder.GetInstance<ShouldRateAppSignal>().Dispatch(ConfigurationDefinition.RateAppAfterEvent.LevelUp);
		}

		[DebugCommand]
		public void TestRateMyApp(string[] args)
		{
			string dataPlayer = localPersistService.GetDataPlayer("RateApp");
			if (!(dataPlayer == "Disabled"))
			{
				uiContext.injectionBinder.GetInstance<ShouldRateAppSignal>().Dispatch(ConfigurationDefinition.RateAppAfterEvent.LevelUp);
			}
		}

		[DebugCommand]
		public void SpeedUpTimers(string[] args)
		{
			int result;
			if (int.TryParse(args[1], out result))
			{
				result *= 60;
				timeEventService.SpeedUpTimers(result);
			}
		}

		[DebugCommand(Args = new string[] { "multiplier|reset" })]
		public void ScaleTimers(string[] args)
		{
			if (args.Length > 1)
			{
				if (args[1].ToLower().Equals("reset"))
				{
					timeEventService.TimerScale = 0f;
					outBuilder.Append("Time scaling reset. Will take effect the next time an event is created.");
					return;
				}
				float result = 0f;
				if (float.TryParse(args[1], out result) && (double)result > 0.01)
				{
					timeEventService.TimerScale = result;
					outBuilder.AppendFormat("All new event timers will be scaled by {0}", result);
				}
				else
				{
					outBuilder.AppendFormat("The multiplier {0} is not a real number greater than 0.01", args[1]);
				}
			}
			else
			{
				outBuilder.Append("usage: \"scaletimers <multiplier>\" where <multiplier> is a real number greater than 0.01");
				outBuilder.Append("You must set the scale before creating a new event. Use \"scaletimers reset\" to reset.");
			}
		}

		[DebugCommand]
		public void SpeedTime(string[] args)
		{
			if (args.Length < 2)
			{
				Time.timeScale = 1f;
				outBuilder.AppendLine("Invalid time argument, reseting to default speed of 1.0");
				return;
			}
			float result = 1f;
			if (float.TryParse(args[1], out result))
			{
				Time.timeScale = result;
				return;
			}
			Time.timeScale = 1f;
			outBuilder.AppendLine("Invalid time argument, reseting to default speed of 1.0");
		}

		[DebugCommand]
		public void TriggerConfirmation(string[] args)
		{
			Signal<bool> signal = new Signal<bool>();
			signal.AddListener(delegate(bool result)
			{
				outBuilder.Append("User Selected: " + result);
				FlushSignal.Dispatch();
			});
			PopupConfirmationSetting type = new PopupConfirmationSetting("popupConfirmationDefaultTitle", "popupConfirmationDefaultTitle", "img_char_Min_FeedbackChecklist01", signal);
			gameContext.injectionBinder.GetInstance<QueueConfirmationSignal>().Dispatch(type);
		}

		[DebugCommand]
		public void SMTM(string[] args)
		{
			playerService.CreateAndRunCustomTransaction(0, 90000, TransactionTarget.CURRENCY);
			playerService.CreateAndRunCustomTransaction(1, 90000, TransactionTarget.CURRENCY);
		}

		[DebugCommand]
		public void ST(string[] args)
		{
			TriggerInstance activeTrigger = gameContext.injectionBinder.GetInstance<ITriggerService>().ActiveTrigger;
			if (activeTrigger != null)
			{
				outBuilder.AppendLine("ID: " + activeTrigger.Definition.ID + " " + activeTrigger.Definition.ToString());
			}
			else
			{
				outBuilder.AppendLine("No trigger exists");
			}
		}

		[DebugCommand]
		public void TriggerState(string[] args)
		{
			int result;
			if (args.Length > 1 && int.TryParse(args[1], out result))
			{
				TriggerDefinition triggerDefinition = definitionService.Get<TriggerDefinition>(result);
				ShowTriggerState(triggerDefinition);
				return;
			}
			IList<TriggerInstance> triggers = playerService.GetTriggers();
			IList<TriggerDefinition> all = definitionService.GetAll<TriggerDefinition>();
			foreach (TriggerDefinition item in all)
			{
				if (IsTriggerAboutToActivate(item, triggers))
				{
					ShowTriggerState(item);
				}
			}
		}

		private bool IsTriggerAboutToActivate(TriggerDefinition triggerDef, IList<TriggerInstance> triggerInstances)
		{
			if (triggerDef.IsTriggered(gameContext))
			{
				foreach (TriggerInstance triggerInstance in triggerInstances)
				{
					if (triggerInstance.Definition.ID == triggerDef.ID && triggerInstance.StartGameTime > 0)
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private void ShowTriggerState(TriggerDefinition triggerDefinition)
		{
			outBuilder.AppendLine("ID: " + triggerDefinition.ID.ToString() + " Priority: " + triggerDefinition.priority + " CanTrigger: " + triggerDefinition.IsTriggered(gameContext));
			triggerDefinition.PrintTriggerConditions(gameContext, outBuilder);
		}

		[DebugCommand(Name = "dcn", Args = new string[] { "featured" })]
		public void DCN(string[] args)
		{
			switch (args[1])
			{
			case "featured":
				gameContext.injectionBinder.GetInstance<DCNFeaturedSignal>().Dispatch();
				break;
			case "reset":
				localPersistService.DeleteKey("DCNStore");
				gameContext.injectionBinder.GetInstance<DCNMaybeShowContentSignal>().Dispatch();
				break;
			case "show":
				localPersistService.DeleteKey("DCNStoreDoNotShow");
				break;
			case "fake":
			{
				localPersistService.DeleteKey("DCNStore");
				DCNService dCNService = gameContext.injectionBinder.GetInstance<IDCNService>() as DCNService;
				dCNService.SetFeaturedContent(1, "http://www.ea.com/");
				gameContext.injectionBinder.GetInstance<DCNMaybeShowContentSignal>().Dispatch();
				break;
			}
			}
		}

		[DebugCommand(Args = new string[] { "minion ID", "level" }, RequiresAllArgs = false)]
		public void LevelMinion(string[] args)
		{
			try
			{
				if (args.Length == 2)
				{
					int id = Convert.ToInt32(args[1]);
					Minion byInstanceId = playerService.GetByInstanceId<Minion>(id);
					outBuilder.Append(string.Format("Minion: {0} Level: {1}", byInstanceId.ID, byInstanceId.Level));
				}
				else if (args.Length == 3)
				{
					MinionBenefitLevelBandDefintion minionBenefitLevelBandDefintion = definitionService.Get<MinionBenefitLevelBandDefintion>(89898);
					int num = Convert.ToInt32(args[1]);
					Minion byInstanceId2 = playerService.GetByInstanceId<Minion>(num);
					byInstanceId2.Level = Mathf.Clamp(Convert.ToInt32(args[2]), 0, minionBenefitLevelBandDefintion.minionBenefitLevelBands.Count - 1);
					outBuilder.Append(string.Format("Minion: {0} Level: {1}", byInstanceId2.ID, byInstanceId2.Level));
					byInstanceId2.Level--;
					gameContext.injectionBinder.GetInstance<MinionUpgradeSignal>().Dispatch(num, 0u);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.Message);
				outBuilder.Append("Error: [" + args[0] + "]: " + ex.Message);
			}
		}

		[DebugCommand(Name = "inventory", Args = new string[] { "typename" }, RequiresAllArgs = true)]
		public void Inventory(string[] args)
		{
			if (args[1] == "*")
			{
				args[1] = "Instance";
			}
			Type type = null;
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			foreach (Type type2 in types)
			{
				if (type2.Name.ToLower() == args[1].ToLower())
				{
					type = type2;
					break;
				}
			}
			if (type == null)
			{
				outBuilder.AppendFormat("Type {0} not found", args[1]);
				return;
			}
			MethodInfo methodInfo = (from m in playerService.GetType().GetMethods()
				where m.Name == "GetInstancesByType"
				select new
				{
					Method = m,
					Params = m.GetParameters(),
					Args = m.GetGenericArguments()
				} into x
				where x.Params.Length == 0 && x.Args.Length == 1
				select x.Method).First();
			MethodInfo methodInfo2 = methodInfo.MakeGenericMethod(type);
			if (methodInfo2 == null)
			{
				outBuilder.AppendFormat("Method GetInstancesByType<{0}> not found", type);
				return;
			}
			IList list = methodInfo2.Invoke(playerService, null) as IList;
			foreach (object item2 in list)
			{
				Instance instance = item2 as Instance;
				Item item = item2 as Item;
				if (item != null)
				{
					outBuilder.AppendFormat("{0} [{1}] <{2}>: {3} {4}\n", item.ID, item.Definition.ID, item.Definition.LocalizedKey, item.ToString(), item.Quantity);
				}
				else if (instance != null)
				{
					outBuilder.AppendFormat("{0} [{1}] <{2}>: {3}\n", instance.ID, instance.Definition.ID, instance.Definition.LocalizedKey, instance.ToString());
				}
				else
				{
					outBuilder.AppendFormat("{0}\n", item2.ToString());
				}
			}
		}

		[DebugCommand(Name = "get item")]
		public void GetItem(string[] args)
		{
			int result = 0;
			if (!int.TryParse(args[2], out result) && !IdOf(args[2], out result))
			{
				outBuilder.AppendLine(string.Join(" ", args) + " -> DEFINITION NOT FOUND");
				return;
			}
			foreach (Instance item2 in playerService.GetInstancesByDefinitionID(result))
			{
				Item item = item2 as Item;
				if (item != null)
				{
					outBuilder.AppendFormat("{0} [{1}] <{2}>: {3} {4}\n", item.ID, item.Definition.ID, item.Definition.LocalizedKey, item.ToString(), item.Quantity);
				}
				else if (item2 != null)
				{
					outBuilder.AppendFormat("{0} [{1}] <{2}>: {3}\n", item2.ID, item2.Definition.ID, item2.Definition.LocalizedKey, item2.ToString());
				}
				else
				{
					outBuilder.AppendLine(string.Join(" ", args) + " -> ITEM NOT FOUND");
				}
			}
		}

		[DebugCommand]
		public void StuartShow(string[] args)
		{
			socialOrderBoardCompleteSignal.Dispatch();
		}

		[DebugCommand]
		public void AddPhilToTikibar(string[] args)
		{
			gameContext.injectionBinder.GetInstance<PhilSitAtBarSignal>().Dispatch(true);
		}

		[DebugCommand]
		public void EnableBuildMenu(string[] args)
		{
			bool type = args.Length <= 1 || !(args[1] == "off");
			uiContext.injectionBinder.GetInstance<SetBuildMenuEnabledSignal>().Dispatch(type);
		}

		[DebugCommand]
		public void InitPN(string[] args)
		{
			setupPushNotificationsSignal.Dispatch();
		}

		[DebugCommand(Name = "pgtcooldown", Args = new string[] { "seconds" }, RequiresAllArgs = true)]
		public void pgtcooldown(string[] args)
		{
			int result;
			if (int.TryParse(args[1], out result) && result > 0)
			{
				TSMCharacter firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<TSMCharacter>(70008);
				firstInstanceByDefinitionId.Definition.CooldownInSeconds = result;
				outBuilder.Append("Altered travelling sales minion's cooldown to " + result);
			}
			else
			{
				outBuilder.Append("Bad cooldown: " + args[1]);
			}
		}

		[DebugCommand]
		public void CompleteQuest(string[] args)
		{
			int result;
			if (int.TryParse(args[1], out result))
			{
				QuestDefinition def = definitionService.Get<QuestDefinition>(result);
				Quest quest = new Quest(def);
				quest.Initialize();
				quest.state = QuestState.Complete;
				questService.AddQuest(quest);
				gameContext.injectionBinder.GetInstance<GetNewQuestSignal>().Dispatch();
			}
		}

		[DebugCommand]
		public void FinishQuest(string[] args)
		{
			int result;
			if (int.TryParse(args[1], out result))
			{
				IQuestController questController = questService.GetQuestMap()[result];
				questController.GoToQuestState(QuestState.Harvestable);
			}
		}

		[DebugCommand]
		public void FinishQuests(string[] args)
		{
			List<Quest> instancesByType = playerService.GetInstancesByType<Quest>();
			foreach (Quest item in instancesByType)
			{
				if (item.state == QuestState.RunningTasks)
				{
					IQuestController questController = questService.GetQuestMap()[item.GetActiveDefinition().ID];
					questController.GoToQuestState(QuestState.Harvestable);
				}
			}
		}

		[DebugCommand]
		public void FinishTasks(string[] args)
		{
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan != null)
			{
				MasterPlanComponent activeComponentFromPlanDefinition = masterPlanService.GetActiveComponentFromPlanDefinition(currentMasterPlan.Definition.ID);
				if (activeComponentFromPlanDefinition != null && activeComponentFromPlanDefinition.State == MasterPlanComponentState.InProgress)
				{
					FinishTasks(activeComponentFromPlanDefinition);
				}
			}
		}

		private void FinishTasks(MasterPlanComponent component)
		{
			foreach (MasterPlanComponentTask task in component.tasks)
			{
				task.isComplete = true;
			}
			component.State = MasterPlanComponentState.TasksComplete;
			Quest questByInstanceId = masterPlanQuestService.GetQuestByInstanceId(component.ID);
			gameContext.injectionBinder.GetInstance<UpdateQuestWorldIconsSignal>().Dispatch(questByInstanceId);
			gameContext.injectionBinder.GetInstance<MasterPlanComponentTaskUpdatedSignal>().Dispatch(component);
			uiContext.injectionBinder.GetInstance<CloseQuestBookSignal>().Dispatch();
		}

		[DebugCommand]
		public void CompleteComponents(string[] args)
		{
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan != null)
			{
				IList<Instance> instancesByDefinition = playerService.GetInstancesByDefinition<MasterPlanComponentDefinition>();
				for (int i = 0; i < instancesByDefinition.Count; i++)
				{
					MasterPlanComponent masterPlanComponent = instancesByDefinition[i] as MasterPlanComponent;
					if (masterPlanComponent.State == MasterPlanComponentState.InProgress)
					{
						FinishTasks(masterPlanComponent);
						continue;
					}
					foreach (MasterPlanComponentTask task in masterPlanComponent.tasks)
					{
						task.isComplete = true;
					}
					masterPlanComponent.State = MasterPlanComponentState.Complete;
				}
				if (instancesByDefinition.Count > 0)
				{
					outBuilder.AppendLine(string.Format("Components Complete for plan {0}", currentMasterPlan.Definition.ID));
					return;
				}
			}
			outBuilder.AppendLine("Error in completing components.  Is the current master plan null?  Or are components null?");
		}

		[DebugCommand]
		public void NextMP(string[] args)
		{
			int result;
			if (int.TryParse(args[1], out result))
			{
				if (masterPlanService.ForceNextMPDefinition(result))
				{
					outBuilder.AppendLine("SUCCESS: the next master plan will be from definition " + result);
					outBuilder.AppendLine("Note that this will NOT affect the playlist order: it will resume at the completion of this master plan.");
				}
				else
				{
					outBuilder.AppendLine("FAILURE: " + result + " is not a valid MasterPlanDefinition ID.  No action taken.");
				}
			}
			else
			{
				outBuilder.AppendLine("FAILURE: invalid input.  Please supply a valid Master Plan Definition ID.  No action taken.");
			}
		}

		[DebugCommand]
		public void MPInfo(string[] args)
		{
			outBuilder.AppendLine("----------Current Master Plan Information:----------");
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			VillainLairDefinition villainLairDefinition = definitionService.Get<VillainLairDefinition>(StaticItem.VILLAIN_LAIR_DEFINITION_ID);
			if (currentMasterPlan == null)
			{
				outBuilder.AppendLine("\tNo master plan exists.");
				outBuilder.AppendLine("----------End of Current Plan Information.----------");
				return;
			}
			outBuilder.AppendLine(string.Format("\tMasterPlanDefinition ID: {0}\n\tReward transaction ID: {1}", currentMasterPlan.Definition.ID, currentMasterPlan.Definition.RewardTransactionID));
			for (int i = 0; i < currentMasterPlan.Definition.ComponentDefinitionIDs.Count; i++)
			{
				MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(currentMasterPlan.Definition.ComponentDefinitionIDs[i]);
				PlatformDefinition platformDefinition = villainLairDefinition.Platforms[i];
				if (firstInstanceByDefinitionId != null)
				{
					outBuilder.AppendLine(string.Format("\tComponent[{0}] defID: {1} state: {2}", i, currentMasterPlan.Definition.ComponentDefinitionIDs[i], firstInstanceByDefinitionId.State));
					int num = currentMasterPlan.Definition.CompBuildingDefinitionIDs[i];
					MasterPlanComponentBuilding firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(num);
					if (firstInstanceByDefinitionId2 != null)
					{
						outBuilder.AppendLine(string.Format("\t\tBuilding ID {0} in state:{1},\n\t\tbuilding prefab: {2}", num, firstInstanceByDefinitionId2.State, firstInstanceByDefinitionId2.Definition.Prefab));
					}
					else
					{
						outBuilder.AppendLine(string.Format("\t\tBuilding def id {0} is not yet built.", num));
						outBuilder.AppendLine(string.Format("\t\tbuilding prefab: {0}", definitionService.Get<MasterPlanComponentBuildingDefinition>().Prefab));
					}
					outBuilder.AppendLine(string.Format("\t\tPlatform base location is {0},{1}, offset is {2}, with final position: {3}", platformDefinition.placementLocation.x, platformDefinition.placementLocation.y, platformDefinition.offset, masterPlanService.GetComponentBuildingPosition(num)));
				}
				else
				{
					outBuilder.AppendLine(string.Format("\tComponent[{0}]: defID: {1} is null.", i, currentMasterPlan.Definition.ComponentDefinitionIDs[i]));
				}
			}
			PlatformDefinition platformDefinition2 = villainLairDefinition.Platforms[villainLairDefinition.Platforms.Count - 1];
			int buildingDefID = currentMasterPlan.Definition.BuildingDefID;
			MasterPlanComponentBuilding firstInstanceByDefinitionId3 = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(buildingDefID);
			if (firstInstanceByDefinitionId3 != null)
			{
				outBuilder.AppendLine(string.Format("\tMasterPlan Building id {0} is in state {1}", firstInstanceByDefinitionId3.Definition.ID, firstInstanceByDefinitionId3.State));
			}
			else
			{
				outBuilder.AppendLine(string.Format("\tMasterPlan Buiding id {0} is not yet built.", buildingDefID));
			}
			outBuilder.AppendLine(string.Format("\t\tMasterPlan building base location is {0},{1}, offset is {2}, with final position: {3}", platformDefinition2.placementLocation.x, platformDefinition2.placementLocation.y, platformDefinition2.offset, masterPlanService.GetComponentBuildingPosition(buildingDefID)));
			BuildingDefinition buildingDefinition = definitionService.Get<BuildingDefinition>(currentMasterPlan.Definition.LeavebehindBuildingDefID);
			outBuilder.AppendLine(string.Format("\t\tLeaveBehind is {0} buildingID: {1}", localizationService.GetString(buildingDefinition.LocalizedKey), currentMasterPlan.Definition.LeavebehindBuildingDefID));
			outBuilder.AppendLine(string.Format("\n----------End of Info for Current Plan {0} ----------", currentMasterPlan.Definition.ID));
		}

		[DebugCommand]
		public void BackButton(string[] args)
		{
			UIModel instance = uiContext.injectionBinder.GetInstance<UIModel>();
			if (instance.UIOpen)
			{
				Action action = instance.RemoveTopUI();
				if (action != null)
				{
					action();
				}
			}
		}

		[DebugCommand]
		public void ShowQuests(string[] args)
		{
			List<Quest> instancesByType = playerService.GetInstancesByType<Quest>();
			foreach (Quest item in instancesByType)
			{
				outBuilder.AppendFormat("Quest ID {0}: State - {1} DefID - {2} LineID - {3} SurfaceType - {4} SurfaceID - {5}\n", item.ID, item.state.ToString(), item.GetActiveDefinition().ID, item.GetActiveDefinition().QuestLineID, item.GetActiveDefinition().SurfaceType.ToString(), item.GetActiveDefinition().SurfaceID);
			}
		}

		[DebugCommand]
		public void ShowQuestLines(string[] args)
		{
			Dictionary<int, QuestLine> questLines = questService.GetQuestLines();
			foreach (KeyValuePair<int, QuestLine> item in questLines)
			{
				QuestLine value = item.Value;
				outBuilder.AppendFormat("First Quest Id - {0} Last QuestId - {1} State - {2} \n", value.Quests[value.Quests.Count - 1].ID, value.Quests[0].ID, value.state.ToString());
			}
		}

		[DebugCommand]
		public void ShowQuestDebug(string[] args)
		{
			EnableQuestDebugSignal.Dispatch();
		}

		[DebugCommand]
		public void ToggleRight(string[] args)
		{
			ToggleRightClickSignal.Dispatch();
		}

		[DebugCommand(Args = new string[] { "delay" })]
		public void Disconnect(string[] args)
		{
			float result = 0f;
			if (args.Length > 1 && !float.TryParse(args[1], out result))
			{
				result = 0f;
			}
			routineRunner.StartTimer("DebugDisconnectID", result, networkConnectionLostSignal.Dispatch);
		}

		[DebugCommand]
		public void ResetMignetteScore(string[] args)
		{
			mignetteCollectionService.ResetMignetteProgress();
		}

		[DebugCommand(Name = "showhud", Args = new string[] { "on/off" }, RequiresAllArgs = true)]
		public void ShowHUD(string[] args)
		{
			bool type = args[1] == "on";
			uiContext.injectionBinder.GetInstance<ShowHUDSignal>().Dispatch(type);
		}

		[DebugCommand]
		public void HideWayfinders(string[] args)
		{
			uiContext.injectionBinder.GetInstance<HideAllWayFindersSignal>().Dispatch();
		}

		[DebugCommand]
		public void RandomFlyOver(string[] args)
		{
			int result = 0;
			if (args.Length > 1)
			{
				int.TryParse(args[1], out result);
			}
			gameContext.injectionBinder.GetInstance<RandomFlyOverSignal>().Dispatch(result);
		}

		[DebugCommand]
		public void ClearVideo(string[] args)
		{
			PlayerPrefs.SetString("VideoCache", "INVALID");
			PlayerPrefs.SetInt("intro_video_played", 0);
		}

		[DebugCommand]
		public void ToggleSellTimers(string[] args)
		{
			marketplaceService.IsDebugMode = !marketplaceService.IsDebugMode;
		}

		[DebugCommand]
		public void MarketplaceMultiplier(string[] args)
		{
			float result = 1f;
			if (args.Length > 1)
			{
				float.TryParse(args[1], out result);
			}
			marketplaceService.DebugMultiplier = result;
			outBuilder.Append("Marketplace Multiplier set to " + result);
		}

		[DebugCommand]
		public void MPSelect(string[] args)
		{
			if (args.Length > 1)
			{
				if (args[1].ToLower().Equals("reset"))
				{
					marketplaceService.DebugSelectedItem = 0;
					outBuilder.Append("Marketplace Selected Item reset to normal");
					return;
				}
				int result = 0;
				int.TryParse(args[1], out result);
				Definition definition;
				if (definitionService.TryGet<Definition>(result, out definition))
				{
					marketplaceService.DebugSelectedItem = result;
					RefreshSaleItemsSignalArgs refreshSaleItemsSignalArgs = new RefreshSaleItemsSignalArgs();
					refreshSaleItemsSignalArgs.RefreshItems = true;
					refreshSaleItemsSignalArgs.StopSpinning = false;
					RefreshSaleItemsSignalArgs type = refreshSaleItemsSignalArgs;
					gameContext.injectionBinder.GetInstance<RefreshSaleItemsSignal>().Dispatch(type);
					outBuilder.Append("Marketplace Selected Item set to " + result);
				}
				else
				{
					outBuilder.Append("Item with given id not found: " + result);
				}
			}
			else
			{
				outBuilder.Append("usage: \"mpselect 12345\" to select item 12345 or \"mpselect reset\" to work normally");
			}
		}

		[DebugCommand(Args = new string[] { "on/off" })]
		public void MPFacebook(string[] args)
		{
			if (args.Length > 1 && (args[1].Equals("on") || args[1].Equals("off")))
			{
				marketplaceService.DebugFacebook = args[1].Equals("on");
				outBuilder.Append(string.Format("Marketplace Facebook features turned {0}", (!marketplaceService.DebugFacebook) ? "off" : "on"));
				gameContext.injectionBinder.GetInstance<UpdateMarketplaceSlotStateSignal>().Dispatch();
			}
			else
			{
				outBuilder.Append("usage: \"mpfacebook <on|off>\" to switch facebook features on/off");
			}
		}

		[DebugCommand(Name = "killswitch", Args = new string[] { "clear", "[type]", "[type] on/off" }, RequiresAllArgs = false)]
		public void Killswitch(string[] args)
		{
			if (args.Length > 1)
			{
				if (args[1].ToUpper() == "CLEAR")
				{
					configurationsService.ClearAllKillswitchOverrides();
					outBuilder.Append("CLEARED All killswitch overrides.");
					return;
				}
				try
				{
					KillSwitch killSwitch = (KillSwitch)(int)Enum.Parse(typeof(KillSwitch), args[1].ToUpper());
					if (args.Length > 2)
					{
						switch (args[2].ToLower())
						{
						case "on":
							configurationsService.OverrideKillswitch(killSwitch, true);
							break;
						case "off":
							configurationsService.OverrideKillswitch(killSwitch, false);
							break;
						}
					}
					outBuilder.Append(killSwitch.ToString() + " killswitch state:" + configurationsService.isKillSwitchOn(killSwitch));
					return;
				}
				catch (Exception ex)
				{
					logger.Info(ex.ToString());
				}
			}
			outBuilder.Append("Invalid killswitch type in second param. Valid types: ");
			foreach (int value in Enum.GetValues(typeof(KillSwitch)))
			{
				outBuilder.Append(((KillSwitch)value).ToString() + ", ");
			}
			outBuilder.Append(" Enter 'killswitch type on/off' to set an override, or 'killswitch clear' to disable all overrides.");
		}

		[DebugCommand(Name = "clone", Args = new string[] { "User ID" }, RequiresAllArgs = true)]
		public void Clone(string[] args)
		{
			try
			{
				long userId = Convert.ToInt64(args[1]);
				savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, string.Empty, true));
				routineRunner.StartCoroutine(ClonePlayer(userId));
			}
			catch (Exception ex)
			{
				logger.Error(ex.Message);
				outBuilder.Append("Error: [" + args[0] + "]: " + ex.Message);
			}
		}

		[DebugCommand(Name = "liberate", Args = new string[] { "Minimum Level" }, RequiresAllArgs = false)]
		public void LiberateLeveledUpMinions(string[] args)
		{
			int type = 1;
			if (args.Length > 1)
			{
				type = int.Parse(args[1]);
			}
			gameContext.injectionBinder.GetInstance<LiberateLeveledUpMinionsSignal>().Dispatch(type);
		}

		[DebugCommand(Args = new string[] { "Order ID" }, RequiresAllArgs = false)]
		public void SocialOrder(string[] args)
		{
			if (args.Length > 1)
			{
				int result;
				if (int.TryParse(args[1], out result))
				{
					SocialOrderTriggerRewardDefinition socialOrderTriggerRewardDefinition = new SocialOrderTriggerRewardDefinition();
					socialOrderTriggerRewardDefinition.OrderId = result;
					socialOrderTriggerRewardDefinition.RewardPlayer(gameContext);
				}
				else
				{
					outBuilder.Append("Bad order ID: " + args[1]);
				}
				return;
			}
			TimedSocialEventDefinition currentSocialEvent = timedSocialEventService.GetCurrentSocialEvent();
			if (currentSocialEvent == null)
			{
				outBuilder.Append("No social order available.\n");
				return;
			}
			SocialTeamResponse socialEventStateCached = timedSocialEventService.GetSocialEventStateCached(currentSocialEvent.ID);
			if (socialEventStateCached == null || socialEventStateCached.Team == null)
			{
				outBuilder.Append("No social team available.\n");
				return;
			}
			foreach (SocialEventOrderDefinition order in currentSocialEvent.Orders)
			{
				string text = null;
				foreach (SocialOrderProgress item in socialEventStateCached.Team.OrderProgress)
				{
					if (item.OrderId == order.OrderID)
					{
						text = item.CompletedByUserId;
						break;
					}
				}
				outBuilder.Append(string.Format("Order {0} : {1}\n", order.OrderID, (!string.IsNullOrEmpty(text)) ? ("COMPLETED BY " + text) : "OPEN"));
			}
		}

		private IEnumerator ClonePlayer(long userId)
		{
			yield return new WaitForSeconds(1f);
			gameContext.injectionBinder.GetInstance<CloneUserSignal>().Dispatch(userId);
		}

		[DebugCommand(Args = new string[] { "Source Environment", "User ID" }, RequiresAllArgs = true)]
		public void CloneFrom(string[] args)
		{
			try
			{
				string sourceEnvironment = args[1].ToLower();
				long userId = Convert.ToInt64(args[2]);
				savePlayerSignal.Dispatch(new Tuple<SaveLocation, string, bool>(SaveLocation.REMOTE, string.Empty, true));
				routineRunner.StartCoroutine(ClonePlayer(sourceEnvironment, userId));
			}
			catch (Exception ex)
			{
				logger.Error(ex.Message);
				outBuilder.Append("Error: [" + args[0] + "]: " + ex.Message);
			}
		}

		private IEnumerator ClonePlayer(string sourceEnvironment, long userId)
		{
			yield return new WaitForSeconds(1f);
			gameContext.injectionBinder.GetInstance<CloneUserFromEnvSignal>().Dispatch(sourceEnvironment, userId);
		}

		[DebugCommand]
		public void Stats(string[] args)
		{
			if (gameLoadedModel.coldStartTime > 0)
			{
				outBuilder.AppendFormat("Last Start Time: {0}", ((float)gameLoadedModel.coldStartTime / 1000f).ToString());
			}
		}

		private void DebugKeyHit(DebugArgument arg)
		{
			switch (arg)
			{
			case DebugArgument.RUSH_ALL_QUESTS:
				FinishQuests(new string[0]);
				break;
			case DebugArgument.RUSH_ALL_TASKS:
				FinishTasks(new string[0]);
				break;
			case DebugArgument.BACK_BUTTON:
				BackButton(new string[0]);
				break;
			}
		}

		private void SplitCommandKey(string key, string argString, out string commandName, out string subcommandName)
		{
			int num = -1;
			if (argString.Length > 0)
			{
				if (key.Length > argString.Length)
				{
					num = key.IndexOf(' ', argString.Length + 1);
				}
			}
			else
			{
				num = key.IndexOf(" ");
			}
			if (num != -1)
			{
				commandName = key.Substring(0, num);
				int num2 = key.IndexOf(' ', num + 1);
				if (num2 == -1)
				{
					num2 = key.Length;
				}
				subcommandName = key.Substring(num + 1, num2 - num - 1);
			}
			else
			{
				commandName = key;
				subcommandName = null;
			}
		}

		private void OutputSubcommands(List<string> subcommands)
		{
			if (subcommands == null)
			{
				return;
			}
			outBuilder.Append(" <");
			int i = 0;
			for (int count = subcommands.Count; i < count; i++)
			{
				outBuilder.Append(subcommands[i]);
				if (i < count - 1)
				{
					outBuilder.Append(",");
				}
			}
			outBuilder.Append("> ");
		}

		private void OutputArguments(IEnumerable arguments)
		{
			if (arguments == null)
			{
				return;
			}
			foreach (string argument in arguments)
			{
				outBuilder.Append(" {");
				outBuilder.Append(argument);
				outBuilder.Append("} ");
			}
		}

		private void ProcessTransaction(string input, int id, bool add)
		{
			uint result = 0u;
			if (uint.TryParse(input, out result))
			{
				QuantityItem item = new QuantityItem(id, result);
				TransactionDefinition transactionDefinition = new TransactionDefinition();
				transactionDefinition.ID = uniqueID++;
				if (add)
				{
					transactionDefinition.Outputs = new List<QuantityItem>();
					transactionDefinition.Outputs.Add(item);
				}
				else
				{
					transactionDefinition.Inputs = new List<QuantityItem>();
					transactionDefinition.Inputs.Add(item);
				}
				playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.HARVEST, TransactionCallback);
			}
		}

		private void ProcessTransaction(int id, bool processInputs)
		{
			if (processInputs)
			{
				playerService.RunEntireTransaction(id, TransactionTarget.HARVEST, TransactionCallback);
				return;
			}
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(id).CopyTransaction();
			if (transactionDefinition.Inputs.Count() > 0)
			{
				transactionDefinition.Inputs.Clear();
			}
			playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.HARVEST, TransactionCallback);
		}

		private void TransactionCallback(PendingCurrencyTransaction pending)
		{
			if (pending.Success)
			{
				ExpansionTransactionCallback(pending);
				outBuilder.AppendLine("TRANSACTION COMPLETE");
			}
			else
			{
				outBuilder.AppendLine("TRANSACTION FAILED");
			}
			FlushSignal.Dispatch();
		}

		private void PurchaseApproval(bool approve)
		{
			IList<KampaiPendingTransaction> pendingTransactions = playerService.GetPendingTransactions();
			if (pendingTransactions.Count > 0)
			{
				string externalIdentifier = pendingTransactions[0].ExternalIdentifier;
				StoreItemDefinition storeItemDefinition = definitionService.Get<StoreItemDefinition>(pendingTransactions[0].StoreItemDefinitionId);
				if (approve)
				{
					outBuilder.AppendLine(string.Format("Approving {0} - {1}", externalIdentifier, storeItemDefinition.LocalizedKey));
					currencyService.PurchaseSucceededAndValidatedCallback(externalIdentifier);
				}
				else
				{
					outBuilder.AppendLine(string.Format("Denying {0} - {1}", externalIdentifier, storeItemDefinition.LocalizedKey));
					currencyService.PurchaseCanceledCallback(externalIdentifier, uint.MaxValue);
				}
			}
			else
			{
				outBuilder.AppendLine("No pending transactions.");
			}
		}

		private void SetMap(bool enabled)
		{
			EnableBuildingAnimatorsSignal instance = gameContext.injectionBinder.GetInstance<EnableBuildingAnimatorsSignal>();
			EnableAllMinionRenderersSignal instance2 = gameContext.injectionBinder.GetInstance<EnableAllMinionRenderersSignal>();
			instance.Dispatch(!enabled);
			instance2.Dispatch(!enabled);
		}

		private bool IdOf(string itemName, out int id)
		{
			foreach (KeyValuePair<int, Definition> allDefinition in definitionService.GetAllDefinitions())
			{
				if (allDefinition.Value.LocalizedKey != null && itemName.ToLower().Equals(allDefinition.Value.LocalizedKey.Replace(' ', '_').ToLower()))
				{
					id = allDefinition.Key;
					return true;
				}
			}
			id = 0;
			return false;
		}

		private bool parseSocialEventDefinitions()
		{
			int num = timeService.CurrentTime();
			foreach (TimedSocialEventDefinition localSocialEvent in localSocialEvents)
			{
				int startTime = localSocialEvent.StartTime;
				int finishTime = localSocialEvent.FinishTime;
				if (num >= startTime && num < finishTime)
				{
					return true;
				}
			}
			return false;
		}

		private bool parseSocialEventInvitiations()
		{
			if (localInvitations != null)
			{
				localPersistService.GetData("UserID");
				foreach (SocialEventInvitation localInvitation in localInvitations)
				{
					if (localInvitation.EventID == timedSocialEventService.GetCurrentSocialEvent().ID)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
