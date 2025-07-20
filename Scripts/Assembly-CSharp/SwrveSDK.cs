using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Swrve;
using Swrve.Device;
using Swrve.Helpers;
using Swrve.Input;
using Swrve.Messaging;
using Swrve.REST;
using Swrve.ResourceManager;
using Swrve.Storage;
using SwrveMiniJSON;
using UnityEngine;

public class SwrveSDK
{
	protected class CoroutineReference<T>
	{
		private T val;

		public CoroutineReference()
		{
		}

		public CoroutineReference(T val)
		{
			this.val = val;
		}

		public T Value()
		{
			return val;
		}

		public void Value(T val)
		{
			this.val = val;
		}
	}

	public const string SdkVersion = "3.4.3";

	private const string Platform = "Unity ";

	private const float DefaultDPI = 160f;

	protected const string EventsSave = "Swrve_Events";

	protected const string InstallTimeEpochSave = "Swrve_JoinedDate";

	protected const string iOSdeviceTokenSave = "Swrve_iOSDeviceToken";

	protected const string GcmdeviceTokenSave = "Swrve_gcmDeviceToken";

	protected const string AbTestUserResourcesSave = "srcngt2";

	protected const string AbTestUserResourcesDiffSave = "rsdfngt2";

	protected const string DeviceIdSave = "Swrve_DeviceId";

	protected const string SeqNumSave = "Swrve_SeqNum";

	protected const string ResourcesCampaignTagSave = "cmpg_etag";

	protected const string ResourcesCampaignFlushFrequencySave = "swrve_cr_flush_frequency";

	protected const string ResourcesCampaignFlushDelaySave = "swrve_cr_flush_delay";

	private const string DeviceIdKey = "Swrve.deviceUniqueIdentifier";

	private const string EmptyJSONObject = "{}";

	private const float DefaultCampaignResourcesFlushFrenquency = 60f;

	private const float DefaultCampaignResourcesFlushRefreshDelay = 5f;

	public const string DefaultAutoShowMessagesTrigger = "Swrve.Messages.showAtSessionStart";

	private const string SwrveAndroidPushPluginPackageName = "com.swrve.unity.gcm.SwrveGcmDeviceRegistration";

	private const int DefaultDelayFirstMessage = 150;

	private const long DefaultMaxShows = 99999L;

	private const int DefaultMinDelay = 55;

	private const int GooglePlayPushPluginVersion = 3;

	private int gameId;

	private string apiKey;

	protected string userId;

	protected SwrveConfig config;

	public string Language;

	public SwrveResourceManager ResourceManager;

	public MonoBehaviour Container;

	public ISwrveInstallButtonListener GlobalInstallButtonListener;

	public ISwrveCustomButtonListener GlobalCustomButtonListener;

	public ISwrveMessageListener GlobalMessageListener;

	public ISwrvePushNotificationListener PushNotificationListener;

	public ISwrveTriggeredMessageListener TriggeredMessageListener;

	public Action ResourcesUpdatedCallback;

	public bool Initialised;

	public bool Destroyed;

	private string escapedUserId;

	private long installTimeEpoch;

	private string installTimeFormatted;

	private string lastPushEngagedId;

	private string gcmDeviceToken;

	private static AndroidJavaObject androidPlugin;

	private static bool androidPluginInitialized;

	private static bool androidPluginInitializedSuccessfully;

	private int deviceWidth;

	private int deviceHeight;

	private long lastSessionTick;

	private ICarrierInfo deviceCarrierInfo;

	protected StringBuilder eventBufferStringBuilder;

	protected string eventsPostString;

	protected string swrvePath;

	protected ISwrveStorage storage;

	protected IRESTClient restClient;

	private string eventsUrl;

	private string abTestResourcesDiffUrl;

	protected bool eventsConnecting;

	protected bool abTestUserResourcesDiffConnecting;

	protected string userResourcesRaw;

	protected Dictionary<string, Dictionary<string, string>> userResources;

	protected float campaignsAndResourcesFlushFrequency;

	protected float campaignsAndResourcesFlushRefreshDelay;

	protected string lastETag;

	protected long campaignsAndResourcesLastRefreshed;

	protected bool campaignsAndResourcesInitialized;

	private static readonly int CampaignAPIVersion = 1;

	private static readonly int CampaignEndpointVersion = 4;

	protected static readonly string CampaignsSave = "cmcc2";

	protected static readonly string CampaignsSettingsSave = "Swrve_CampaignsData";

	private static readonly string WaitTimeFormat = "HH\\:mm\\:ss zzz";

	protected static readonly string InstallTimeFormat = "yyyyMMdd";

	private string resourcesAndCampaignsUrl;

	protected string swrveTemporaryPath;

	protected bool campaignsConnecting;

	protected bool autoShowMessagesEnabled;

	protected bool assetsCurrentlyDownloading;

	protected HashSet<string> assetsOnDisk;

	protected List<SwrveCampaign> campaigns = new List<SwrveCampaign>();

	protected Dictionary<string, object> campaignSettings = new Dictionary<string, object>();

	protected Dictionary<string, string> gameStoreLinks = new Dictionary<string, string>();

	protected SwrveMessageFormat currentMessage;

	protected SwrveMessageFormat currentDisplayingMessage;

	protected SwrveOrientation currentOrientation;

	protected IInputManager inputManager = NativeInputManager.Instance;

	private string cdn = "http://swrve-content.s3.amazonaws.com/messaging/message_image/";

	private DateTime initialisedTime;

	private DateTime showMessagesAfterLaunch;

	private DateTime showMessagesAfterDelay;

	private long messagesLeftToShow;

	private int minDelayBetweenMessage;

	protected SwrveQAUser qaUser;

	private bool campaignAndResourcesCoroutineEnabled = true;

	private IEnumerator campaignAndResourcesCoroutineInstance;

	public int GameId
	{
		get
		{
			return gameId;
		}
	}

	public string ApiKey
	{
		get
		{
			return apiKey;
		}
	}

	public string UserId
	{
		get
		{
			return userId;
		}
	}

	public Color? DefaultBackgroundColor
	{
		get
		{
			return config.DefaultBackgroundColor;
		}
	}

	public void Init(MonoBehaviour container, int gameId, string apiKey)
	{
		Init(container, gameId, apiKey, new SwrveConfig());
	}

	public void Init(MonoBehaviour container, int gameId, string apiKey, string userId)
	{
		SwrveConfig swrveConfig = new SwrveConfig();
		swrveConfig.UserId = userId;
		Init(container, gameId, apiKey, swrveConfig);
	}

	public virtual void Init(MonoBehaviour container, int gameId, string apiKey, string userId, SwrveConfig config)
	{
		config.UserId = userId;
		Init(container, gameId, apiKey, config);
	}

	public virtual void Init(MonoBehaviour container, int gameId, string apiKey, SwrveConfig config)
	{
		Container = container;
		ResourceManager = new SwrveResourceManager();
		this.config = config;
		this.gameId = gameId;
		this.apiKey = apiKey;
		userId = config.UserId;
		Language = config.Language;
		lastSessionTick = GetSessionTime();
		initialisedTime = SwrveHelper.GetNow();
		campaignsAndResourcesInitialized = false;
		autoShowMessagesEnabled = true;
		assetsOnDisk = new HashSet<string>();
		assetsCurrentlyDownloading = false;
		if (string.IsNullOrEmpty(apiKey))
		{
			throw new Exception("The api key has not been specified.");
		}
		if (string.IsNullOrEmpty(userId))
		{
			userId = GetDeviceUniqueId();
		}
		if (!string.IsNullOrEmpty(userId))
		{
			PlayerPrefs.SetString("Swrve.deviceUniqueIdentifier", userId);
			PlayerPrefs.Save();
		}
		SwrveLog.Log("Your user id is: " + userId);
		escapedUserId = WWW.EscapeURL(userId);
		if (string.IsNullOrEmpty(Language))
		{
			Language = GetDeviceLanguage();
			if (string.IsNullOrEmpty(Language))
			{
				Language = config.DefaultLanguage;
			}
		}
		config.CalculateEndpoints(gameId);
		string contentServer = config.ContentServer;
		eventsUrl = config.EventsServer + "/1/batch";
		abTestResourcesDiffUrl = contentServer + "/api/1/user_resources_diff";
		resourcesAndCampaignsUrl = contentServer + "/api/1/user_resources_and_campaigns";
		swrvePath = GetSwrvePath();
		if (storage == null)
		{
			storage = CreateStorage();
		}
		storage.SetSecureFailedListener(delegate
		{
			NamedEventInternal("Swrve.signature_invalid", null, false);
		});
		restClient = CreateRestClient();
		eventBufferStringBuilder = new StringBuilder(config.MaxBufferChars);
		string savedInstallTimeEpoch = GetSavedInstallTimeEpoch();
		LoadData();
		InitUserResources();
		deviceCarrierInfo = new DeviceCarrierInfo();
		GetDeviceScreenInfo();
		Initialised = true;
		if (config.AutomaticSessionManagement)
		{
			QueueSessionStart();
			GenerateNewSessionInterval();
		}
		if (string.IsNullOrEmpty(savedInstallTimeEpoch))
		{
			NamedEvent("Swrve.first_session");
		}
		if (config.PushNotificationEnabled && !string.IsNullOrEmpty(config.GCMSenderId))
		{
			GooglePlayRegisterForPushNotification(Container, config.GCMSenderId);
		}
		QueueDeviceInfo();
		SendQueuedEvents();
		if (config.TalkEnabled)
		{
			if (string.IsNullOrEmpty(Language))
			{
				throw new Exception("Language needed to use Talk");
			}
			if (string.IsNullOrEmpty(config.AppStore))
			{
				config.AppStore = "google";
			}
			try
			{
				swrveTemporaryPath = GetSwrveTemporaryCachePath();
				LoadTalkData();
			}
			catch (Exception ex)
			{
				SwrveLog.LogError("Error while initializing " + ex);
			}
		}
		StartCampaignsAndResourcesTimer();
		DisableAutoShowAfterDelay();
	}

	public void SessionStart()
	{
		QueueSessionStart();
		SendQueuedEvents();
	}

	public void SessionEnd()
	{
		Dictionary<string, object> eventParameters = new Dictionary<string, object>();
		AppendEventToBuffer("session_end", eventParameters);
	}

	public virtual void NamedEvent(string name, Dictionary<string, string> payload = null)
	{
		NamedEventInternal(name, payload);
	}

	public void UserUpdate(Dictionary<string, string> attributes)
	{
		if (attributes != null && attributes.Count > 0)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("attributes", attributes);
			AppendEventToBuffer("user", dictionary);
		}
		else
		{
			SwrveLog.LogError("Invoked user update with no update attributes");
		}
	}

	public void Purchase(string item, string currency, int cost, int quantity)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("item", item);
		dictionary.Add("currency", currency);
		dictionary.Add("cost", cost);
		dictionary.Add("quantity", quantity);
		AppendEventToBuffer("purchase", dictionary);
	}

	public void Iap(int quantity, string productId, double productPrice, string currency)
	{
		IapRewards rewards = new IapRewards();
		Iap(quantity, productId, productPrice, currency, rewards);
	}

	public void Iap(int quantity, string productId, double productPrice, string currency, IapRewards rewards)
	{
		_Iap(quantity, productId, productPrice, currency, rewards, string.Empty, string.Empty, string.Empty, "unknown_store");
	}

	public void IapGooglePlay(string productId, double productPrice, string currency, string purchaseData, string dataSignature)
	{
		IapRewards rewards = new IapRewards();
		IapGooglePlay(productId, productPrice, currency, rewards, purchaseData, dataSignature);
	}

	public void IapGooglePlay(string productId, double productPrice, string currency, IapRewards rewards, string purchaseData, string dataSignature)
	{
		if (config.AppStore != "google")
		{
			throw new Exception("This function can only be called to validate IAP events from Google");
		}
		if (string.IsNullOrEmpty(purchaseData))
		{
			SwrveLog.LogError("IAP event not sent: purchase data cannot be empty for Google Play Store verification");
		}
		else if (string.IsNullOrEmpty(dataSignature))
		{
			SwrveLog.LogError("IAP event not sent: data signature cannot be empty for Google Play Store verification");
		}
		else
		{
			_Iap(1, productId, productPrice, currency, rewards, purchaseData, dataSignature, string.Empty, config.AppStore);
		}
	}

	public void CurrencyGiven(string givenCurrency, double amount)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("given_currency", givenCurrency);
		dictionary.Add("given_amount", amount);
		AppendEventToBuffer("currency_given", dictionary);
	}

	public bool SendQueuedEvents()
	{
		bool result = false;
		if (Initialised)
		{
			if (!eventsConnecting)
			{
				byte[] array = null;
				if (eventsPostString == null || eventsPostString.Length == 0)
				{
					eventsPostString = eventBufferStringBuilder.ToString();
					eventBufferStringBuilder.Length = 0;
				}
				if (eventsPostString.Length > 0)
				{
					long seconds = SwrveHelper.GetSeconds();
					array = PostBodyBuilder.Build(apiKey, gameId, userId, GetDeviceId(), GetAppVersion(), seconds, eventsPostString);
				}
				if (array != null)
				{
					eventsConnecting = true;
					SwrveLog.Log("Sending events to Swrve");
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					dictionary.Add("Content-Type", "application/json; charset=utf-8");
					dictionary.Add("Content-Length", array.Length.ToString());
					Dictionary<string, string> requestHeaders = dictionary;
					result = true;
					StartTask("PostEvents_Coroutine", PostEvents_Coroutine(requestHeaders, array));
				}
				else
				{
					eventsPostString = null;
				}
			}
			else
			{
				SwrveLog.LogWarning("Sending events already in progress");
			}
		}
		return result;
	}

	public void GetUserResources(Action<Dictionary<string, Dictionary<string, string>>, string> onResult, Action<Exception> onError)
	{
		if (Initialised)
		{
			if (userResources != null)
			{
				onResult(userResources, userResourcesRaw);
			}
			else
			{
				onResult(new Dictionary<string, Dictionary<string, string>>(), "[]");
			}
		}
	}

	public void GetUserResourcesDiff(Action<Dictionary<string, Dictionary<string, string>>, Dictionary<string, Dictionary<string, string>>, string> onResult, Action<Exception> onError)
	{
		if (Initialised && !abTestUserResourcesDiffConnecting)
		{
			abTestUserResourcesDiffConnecting = true;
			StringBuilder stringBuilder = new StringBuilder(abTestResourcesDiffUrl);
			stringBuilder.AppendFormat("?user={0}&api_key={1}&app_version={2}&joined={3}", escapedUserId, apiKey, WWW.EscapeURL(GetAppVersion()), installTimeEpoch);
			SwrveLog.Log("AB Test User Resources Diff request: " + stringBuilder.ToString());
			StartTask("GetUserResourcesDiff_Coroutine", GetUserResourcesDiff_Coroutine(stringBuilder.ToString(), onResult, onError, "rsdfngt2"));
		}
		else
		{
			string message = "Failed to initiate A/B test Diff GET request";
			SwrveLog.LogError(message);
			if (onError != null)
			{
				onError(new Exception(message));
			}
		}
	}

	public void LoadFromDisk()
	{
		LoadEventsFromDisk();
	}

	public void FlushToDisk(bool saveEventsBeingSent = false)
	{
		if (!Initialised || eventBufferStringBuilder == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string text = eventBufferStringBuilder.ToString();
		eventBufferStringBuilder.Length = 0;
		if (saveEventsBeingSent)
		{
			stringBuilder.Append(eventsPostString);
			eventsPostString = null;
			if (text.Length > 0)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(text);
			}
		}
		else
		{
			stringBuilder.Append(text);
		}
		try
		{
			string value = storage.Load("Swrve_Events", userId);
			if (!string.IsNullOrEmpty(value))
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(value);
			}
		}
		catch (Exception ex)
		{
			SwrveLog.LogWarning("Could not read events from cache (" + ex.ToString() + ")");
		}
		string data = stringBuilder.ToString();
		storage.Save("Swrve_Events", data, userId);
	}

	public string BasePath()
	{
		return swrvePath;
	}

	public Dictionary<string, string> GetDeviceInfo()
	{
		string deviceModel = GetDeviceModel();
		string operatingSystem = SystemInfo.operatingSystem;
		string value = "Android";
		float num = ((Screen.dpi != 0f) ? Screen.dpi : 160f);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("swrve.device_name", deviceModel);
		dictionary.Add("swrve.os", value);
		dictionary.Add("swrve.device_width", deviceWidth.ToString());
		dictionary.Add("swrve.device_height", deviceHeight.ToString());
		dictionary.Add("swrve.device_dpi", num.ToString());
		dictionary.Add("swrve.language", Language);
		dictionary.Add("swrve.os_version", operatingSystem);
		dictionary.Add("swrve.app_store", config.AppStore);
		dictionary.Add("swrve.sdk_version", "Unity 3.4.3");
		dictionary.Add("swrve.unity_version", Application.unityVersion);
		dictionary.Add("swrve.install_date", installTimeFormatted);
		Dictionary<string, string> dictionary2 = dictionary;
		string value2 = DateTimeOffset.Now.Offset.TotalSeconds.ToString();
		dictionary2["swrve.utc_offset_seconds"] = value2;
		if (!string.IsNullOrEmpty(gcmDeviceToken))
		{
			dictionary2["swrve.gcm_token"] = gcmDeviceToken;
		}
		string value3 = AndroidGetTimezone();
		if (!string.IsNullOrEmpty(value3))
		{
			dictionary2["swrve.timezone_name"] = value3;
		}
		ICarrierInfo carrierInfoProvider = GetCarrierInfoProvider();
		if (carrierInfoProvider != null)
		{
			string name = carrierInfoProvider.GetName();
			if (!string.IsNullOrEmpty(name))
			{
				dictionary2["swrve.sim_operator.name"] = name;
			}
			string isoCountryCode = carrierInfoProvider.GetIsoCountryCode();
			if (!string.IsNullOrEmpty(isoCountryCode))
			{
				dictionary2["swrve.sim_operator.iso_country_code"] = isoCountryCode;
			}
			string carrierCode = carrierInfoProvider.GetCarrierCode();
			if (!string.IsNullOrEmpty(carrierCode))
			{
				dictionary2["swrve.sim_operator.code"] = carrierCode;
			}
		}
		return dictionary2;
	}

	public void OnSwrvePause()
	{
		if (Initialised)
		{
			FlushToDisk();
			GenerateNewSessionInterval();
			if (config != null && config.AutoDownloadCampaignsAndResources)
			{
				StopCheckForCampaignAndResources();
			}
		}
	}

	public void OnSwrveResume()
	{
		if (Initialised)
		{
			LoadFromDisk();
			QueueDeviceInfo();
			long sessionTime = GetSessionTime();
			if (sessionTime >= lastSessionTick)
			{
				SessionStart();
			}
			else
			{
				SendQueuedEvents();
			}
			GenerateNewSessionInterval();
			StartCampaignsAndResourcesTimer();
			DisableAutoShowAfterDelay();
		}
	}

	public void OnSwrveDestroy()
	{
		if (!Destroyed)
		{
			Destroyed = true;
			if (Initialised)
			{
				FlushToDisk(true);
			}
			if (config != null && config.AutoDownloadCampaignsAndResources)
			{
				StopCheckForCampaignAndResources();
			}
		}
	}

	public List<SwrveCampaign> GetCampaigns()
	{
		return campaigns;
	}

	public void ButtonWasPressedByUser(SwrveButton button)
	{
		if (button == null)
		{
			return;
		}
		try
		{
			SwrveLog.Log(string.Concat("Button ", button.ActionType, ": ", button.Action, " game id: ", button.GameId));
			if (button.ActionType != SwrveActionType.Dismiss)
			{
				string text = "Swrve.Messages.Message-" + button.Message.Id + ".click";
				SwrveLog.Log("Sending click event: " + text);
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("name", button.Name);
				NamedEventInternal(text, dictionary);
			}
		}
		catch (Exception ex)
		{
			SwrveLog.LogError("Error while processing button click " + ex);
		}
	}

	public void MessageWasShownToUser(SwrveMessageFormat messageFormat)
	{
		try
		{
			SetMessageMinDelayThrottle();
			messagesLeftToShow--;
			SwrveCampaign campaign = messageFormat.Message.Campaign;
			if (campaign != null)
			{
				campaign.MessageWasShownToUser(messageFormat);
				SaveCampaignData(campaign);
			}
			string text = "Swrve.Messages.Message-" + messageFormat.Message.Id + ".impression";
			SwrveLog.Log("Sending view event: " + text);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("format", messageFormat.Name);
			dictionary.Add("orientation", messageFormat.Orientation.ToString());
			dictionary.Add("size", messageFormat.Size.X + "x" + messageFormat.Size.Y);
			NamedEventInternal(text, dictionary);
		}
		catch (Exception ex)
		{
			SwrveLog.LogError("Error while processing message impression " + ex);
		}
	}

	public bool IsMessageDispaying()
	{
		return currentMessage != null;
	}

	public string GetAppStoreLink(int gameId)
	{
		string value = null;
		if (gameStoreLinks != null)
		{
			gameStoreLinks.TryGetValue(gameId.ToString(), out value);
		}
		return value;
	}

	public SwrveMessage GetMessageForEvent(string eventName)
	{
		SwrveMessage swrveMessage = null;
		SwrveCampaign swrveCampaign = null;
		DateTime now = SwrveHelper.GetNow();
		Dictionary<int, string> dictionary = null;
		Dictionary<int, int> dictionary2 = null;
		SwrveLog.Log("Trying to get message for: " + eventName);
		if (campaigns != null)
		{
			if (campaigns.Count == 0)
			{
				NoMessagesWereShown(eventName, "No campaigns available");
				return null;
			}
			if (!string.Equals(eventName, "Swrve.Messages.showAtSessionStart", StringComparison.OrdinalIgnoreCase) && IsTooSoonToShowMessageAfterLaunch(now))
			{
				NoMessagesWereShown(eventName, "{App throttle limit} Too soon after launch. Wait until " + showMessagesAfterLaunch.ToString(WaitTimeFormat));
				return null;
			}
			if (IsTooSoonToShowMessageAfterDelay(now))
			{
				NoMessagesWereShown(eventName, "{App throttle limit} Too soon after last message. Wait until " + showMessagesAfterDelay.ToString(WaitTimeFormat));
				return null;
			}
			if (HasShowTooManyMessagesAlready())
			{
				NoMessagesWereShown(eventName, "{App throttle limit} Too many messages shown");
				return null;
			}
			if (qaUser != null)
			{
				dictionary = new Dictionary<int, string>();
				dictionary2 = new Dictionary<int, int>();
			}
			List<SwrveMessage> list = new List<SwrveMessage>();
			int num = int.MaxValue;
			List<SwrveMessage> list2 = new List<SwrveMessage>();
			IEnumerator<SwrveCampaign> enumerator = campaigns.GetEnumerator();
			SwrveOrientation deviceOrientation = GetDeviceOrientation();
			while (enumerator.MoveNext() && swrveMessage == null)
			{
				SwrveCampaign current = enumerator.Current;
				SwrveMessage messageForEvent = current.GetMessageForEvent(eventName, dictionary);
				if (messageForEvent == null)
				{
					continue;
				}
				if (messageForEvent.SupportsOrientation(deviceOrientation))
				{
					list.Add(messageForEvent);
					if (messageForEvent.Priority <= num)
					{
						num = messageForEvent.Priority;
						if (messageForEvent.Priority < num)
						{
							list2.Clear();
						}
						list2.Add(messageForEvent);
					}
				}
				else if (qaUser != null)
				{
					dictionary2.Add(current.Id, messageForEvent.Id);
					dictionary.Add(current.Id, "Message didn't support the current device orientation: " + deviceOrientation);
				}
			}
			list2.Shuffle();
			if (list2.Count > 0)
			{
				swrveMessage = list2[0];
				swrveCampaign = swrveMessage.Campaign;
			}
			if (qaUser != null && swrveCampaign != null && swrveMessage != null)
			{
				IEnumerator<SwrveMessage> enumerator2 = list.GetEnumerator();
				while (enumerator2.MoveNext())
				{
					SwrveMessage current2 = enumerator2.Current;
					if (current2 != swrveMessage)
					{
						int id = current2.Campaign.Id;
						if (!dictionary2.ContainsKey(id))
						{
							dictionary2.Add(id, current2.Id);
							dictionary.Add(id, "Campaign " + swrveCampaign.Id + " was selected for display ahead of this campaign");
						}
					}
				}
			}
		}
		if (qaUser != null)
		{
			qaUser.Trigger(eventName, swrveMessage, dictionary, dictionary2);
		}
		if (swrveMessage == null)
		{
			SwrveLog.LogWarning("Not showing message: no candidate messages for " + eventName);
		}
		else
		{
			Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
			dictionary3.Add("id", swrveMessage.Id.ToString());
			NamedEventInternal("Swrve.Messages.message_returned", dictionary3, false);
		}
		return swrveMessage;
	}

	public SwrveMessage GetMessageForId(int messageId)
	{
		SwrveMessage swrveMessage = null;
		IEnumerator<SwrveCampaign> enumerator = campaigns.GetEnumerator();
		while (enumerator.MoveNext() && swrveMessage == null)
		{
			SwrveCampaign current = enumerator.Current;
			swrveMessage = current.GetMessageForId(messageId);
			if (swrveMessage != null)
			{
				return swrveMessage;
			}
		}
		SwrveLog.LogWarning("Message with id " + messageId + " not found");
		return null;
	}

	public IEnumerator ShowMessageForEvent(string eventName, ISwrveInstallButtonListener installButtonListener = null, ISwrveCustomButtonListener customButtonListener = null, ISwrveMessageListener messageListener = null)
	{
		if (TriggeredMessageListener != null)
		{
			SwrveMessage message = GetMessageForEvent(eventName);
			if (message != null)
			{
				TriggeredMessageListener.OnMessageTriggered(message);
			}
		}
		else if (currentMessage == null)
		{
			SwrveMessage message2 = GetMessageForEvent(eventName);
			yield return Container.StartCoroutine(LaunchMessage(message2, installButtonListener, customButtonListener, messageListener));
		}
		TaskFinished("ShowMessageForEvent");
	}

	public void DismissMessage()
	{
		if (TriggeredMessageListener != null)
		{
			TriggeredMessageListener.DismissCurrentMessage();
			return;
		}
		try
		{
			if (currentMessage != null)
			{
				SetMessageMinDelayThrottle();
				currentMessage.Dismiss();
			}
		}
		catch (Exception ex)
		{
			SwrveLog.LogError("Error while dismissing a message " + ex);
		}
	}

	public virtual void RefreshUserResourcesAndCampaigns()
	{
		LoadResourcesAndCampaigns();
	}

	public void RegistrationIdReceived(string registrationId)
	{
		if (!string.IsNullOrEmpty(registrationId) && gcmDeviceToken != registrationId)
		{
			gcmDeviceToken = registrationId;
			storage.Save("Swrve_gcmDeviceToken", gcmDeviceToken);
			if (qaUser != null)
			{
				qaUser.UpdateDeviceInfo();
			}
			SendDeviceInfo();
		}
	}

	public void NotificationReceived(string notificationJson)
	{
		Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(notificationJson);
		if (androidPlugin != null && dictionary != null)
		{
			string pushId = GetPushId(dictionary);
			if (pushId != null)
			{
				androidPlugin.CallStatic("sdkAcknowledgeReceivedNotification", pushId);
			}
		}
		if (PushNotificationListener != null)
		{
			PushNotificationListener.OnNotificationReceived(dictionary);
		}
	}

	private string GetPushId(Dictionary<string, object> notification)
	{
		if (notification != null && notification.ContainsKey("_p"))
		{
			return notification["_p"].ToString();
		}
		SwrveLog.Log("Got unidentified notification");
		return null;
	}

	public void OpenedFromPushNotification(string notificationJson)
	{
		Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(notificationJson);
		string pushId = GetPushId(dictionary);
		SendPushNotificationEngagedEvent(pushId);
		if (pushId != null && androidPlugin != null)
		{
			androidPlugin.CallStatic("sdkAcknowledgeOpenedNotification", pushId);
		}
		if (PushNotificationListener != null)
		{
			PushNotificationListener.OnOpenedFromPushNotification(dictionary);
		}
	}

	private void QueueSessionStart()
	{
		Dictionary<string, object> eventParameters = new Dictionary<string, object>();
		AppendEventToBuffer("session_start", eventParameters);
	}

	private void NamedEventInternal(string name, Dictionary<string, string> payload = null, bool allowShowMessage = true)
	{
		if (payload == null)
		{
			payload = new Dictionary<string, string>();
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("name", name);
		dictionary.Add("payload", payload);
		AppendEventToBuffer("event", dictionary, allowShowMessage);
	}

	protected static string GetSwrvePath()
	{
		string text = Application.persistentDataPath;
		if (string.IsNullOrEmpty(text))
		{
			text = Application.temporaryCachePath;
			SwrveLog.Log("Swrve path (tried again): " + text);
		}
		return text;
	}

	protected static string GetSwrveTemporaryCachePath()
	{
		string text = Application.temporaryCachePath;
		if (text == null || text.Length == 0)
		{
			text = Application.persistentDataPath;
		}
		return text;
	}

	private void _Iap(int quantity, string productId, double productPrice, string currency, IapRewards rewards, string receipt, string receiptSignature, string transactionId, string appStore)
	{
		if (!_Iap_check_arguments(quantity, productId, productPrice, currency, appStore))
		{
			SwrveLog.LogError("ERROR: IAP event not sent because it received an illegal argument");
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("app_store", appStore);
		dictionary.Add("local_currency", currency);
		dictionary.Add("cost", productPrice);
		dictionary.Add("product_id", productId);
		dictionary.Add("quantity", quantity);
		dictionary.Add("rewards", rewards.getRewards());
		if (!string.IsNullOrEmpty(GetAppVersion()))
		{
			dictionary.Add("app_version", GetAppVersion());
		}
		if (appStore == "apple")
		{
			dictionary.Add("receipt", receipt);
			if (!string.IsNullOrEmpty(transactionId))
			{
				dictionary.Add("transaction_id", transactionId);
			}
		}
		else if (appStore == "google")
		{
			dictionary.Add("receipt", receipt);
			dictionary.Add("receipt_signature", receiptSignature);
		}
		else
		{
			dictionary.Add("receipt", receipt);
		}
		AppendEventToBuffer("iap", dictionary);
		if (config.AutoDownloadCampaignsAndResources)
		{
			CheckForCampaignsAndResourcesUpdates(false);
		}
	}

	protected virtual SwrveOrientation GetDeviceOrientation()
	{
		switch (Screen.orientation)
		{
		case ScreenOrientation.LandscapeLeft:
		case ScreenOrientation.LandscapeRight:
			return SwrveOrientation.Landscape;
		case ScreenOrientation.Portrait:
		case ScreenOrientation.PortraitUpsideDown:
			return SwrveOrientation.Portrait;
		default:
			if (Screen.height >= Screen.width)
			{
				return SwrveOrientation.Portrait;
			}
			return SwrveOrientation.Landscape;
		}
	}

	private bool _Iap_check_arguments(int quantity, string productId, double productPrice, string currency, string appStore)
	{
		if (string.IsNullOrEmpty(productId))
		{
			SwrveLog.LogError("IAP event illegal argument: productId cannot be empty");
			return false;
		}
		if (string.IsNullOrEmpty(currency))
		{
			SwrveLog.LogError("IAP event illegal argument: currency cannot be empty");
			return false;
		}
		if (string.IsNullOrEmpty(appStore))
		{
			SwrveLog.LogError("IAP event illegal argument: appStore cannot be empty");
			return false;
		}
		if (quantity <= 0)
		{
			SwrveLog.LogError("IAP event illegal argument: quantity must be greater than zero");
			return false;
		}
		if (productPrice < 0.0)
		{
			SwrveLog.LogError("IAP event illegal argument: productPrice must be greater than or equal to zero");
			return false;
		}
		return true;
	}

	private Dictionary<string, Dictionary<string, string>> ProcessUserResources(IList<object> userResources)
	{
		Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>();
		if (userResources != null)
		{
			IEnumerator<object> enumerator = userResources.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Dictionary<string, object> dictionary2 = (Dictionary<string, object>)enumerator.Current;
				string key = (string)dictionary2["uid"];
				dictionary.Add(key, NormalizeJson(dictionary2));
			}
		}
		return dictionary;
	}

	private Dictionary<string, string> NormalizeJson(Dictionary<string, object> json)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string key in json.Keys)
		{
			object obj = json[key];
			if (obj != null)
			{
				dictionary.Add(key, obj.ToString());
			}
		}
		return dictionary;
	}

	private IEnumerator GetUserResourcesDiff_Coroutine(string getRequest, Action<Dictionary<string, Dictionary<string, string>>, Dictionary<string, Dictionary<string, string>>, string> onResult, Action<Exception> onError, string saveCategory)
	{
		Exception wwwException = null;
		string abTestCandidate = null;
		string saveCategory2 = default(string);
		yield return Container.StartCoroutine(restClient.Get(getRequest, delegate(RESTResponse response)
		{
			if (response.Error == WwwDeducedError.NoError)
			{
				abTestCandidate = response.Body;
				SwrveLog.Log("AB Test result: " + abTestCandidate);
				storage.SaveSecure(saveCategory2, abTestCandidate, userId);
				TaskFinished("GetUserResourcesDiff_Coroutine");
			}
			else
			{
				wwwException = new Exception(response.Error.ToString());
				SwrveLog.LogError("AB Test request failed: " + response.Error);
				TaskFinished("GetUserResourcesDiff_Coroutine");
			}
		}));
		abTestUserResourcesDiffConnecting = false;
		if (wwwException != null || string.IsNullOrEmpty(abTestCandidate))
		{
			try
			{
				string loadedData = storage.LoadSecure(saveCategory, userId);
				if (string.IsNullOrEmpty(loadedData))
				{
					onError(wwwException);
				}
				else if (ResponseBodyTester.TestUTF8(loadedData, out abTestCandidate))
				{
					Dictionary<string, Dictionary<string, string>> userResourcesDiffNew2 = new Dictionary<string, Dictionary<string, string>>();
					Dictionary<string, Dictionary<string, string>> userResourcesDiffOld2 = new Dictionary<string, Dictionary<string, string>>();
					ProcessUserResourcesDiff(abTestCandidate, userResourcesDiffNew2, userResourcesDiffOld2);
					onResult(userResourcesDiffNew2, userResourcesDiffOld2, abTestCandidate);
				}
				else
				{
					onError(wwwException);
				}
			}
			catch (Exception ex)
			{
				Exception e = ex;
				SwrveLog.LogWarning("Could not read user resources diff from cache (" + e.ToString() + ")");
				onError(wwwException);
			}
		}
		if (!string.IsNullOrEmpty(abTestCandidate))
		{
			Dictionary<string, Dictionary<string, string>> userResourcesDiffNew = new Dictionary<string, Dictionary<string, string>>();
			Dictionary<string, Dictionary<string, string>> userResourcesDiffOld = new Dictionary<string, Dictionary<string, string>>();
			ProcessUserResourcesDiff(abTestCandidate, userResourcesDiffNew, userResourcesDiffOld);
			onResult(userResourcesDiffNew, userResourcesDiffOld, abTestCandidate);
		}
	}

	private void ProcessUserResourcesDiff(string abTestJson, Dictionary<string, Dictionary<string, string>> newResources, Dictionary<string, Dictionary<string, string>> oldResources)
	{
		IList<object> list = (List<object>)Json.Deserialize(abTestJson);
		if (list == null)
		{
			return;
		}
		IEnumerator<object> enumerator = list.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)enumerator.Current;
			string key = (string)dictionary["uid"];
			Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary["diff"];
			IEnumerator<string> enumerator2 = dictionary2.Keys.GetEnumerator();
			Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
			Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
			while (enumerator2.MoveNext())
			{
				Dictionary<string, string> dictionary5 = NormalizeJson((Dictionary<string, object>)dictionary2[enumerator2.Current]);
				dictionary3.Add(enumerator2.Current, dictionary5["new"]);
				dictionary4.Add(enumerator2.Current, dictionary5["old"]);
			}
			newResources.Add(key, dictionary3);
			oldResources.Add(key, dictionary4);
		}
	}

	private long GetInstallTimeEpoch()
	{
		string savedInstallTimeEpoch = GetSavedInstallTimeEpoch();
		if (!string.IsNullOrEmpty(savedInstallTimeEpoch))
		{
			long result = 0L;
			if (long.TryParse(savedInstallTimeEpoch, out result))
			{
				return result;
			}
		}
		long sessionTime = GetSessionTime();
		storage.Save("Swrve_JoinedDate", sessionTime.ToString(), userId);
		return sessionTime;
	}

	private string GetDeviceId()
	{
		string text = storage.Load("Swrve_DeviceId", userId);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		short num = (short)new System.Random().Next(32767);
		storage.Save("Swrve_DeviceId", num.ToString(), userId);
		return num.ToString();
	}

	private string getNextSeqNum()
	{
		string s = storage.Load("Swrve_SeqNum", userId);
		int result;
		object obj;
		if (int.TryParse(s, out result))
		{
			int num = ++result;
			obj = num.ToString();
		}
		else
		{
			obj = "1";
		}
		s = (string)obj;
		storage.Save("Swrve_SeqNum", s, userId);
		return s;
	}

	protected string GetDeviceLanguage()
	{
		string text = null;
		try
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("java.util.Locale"))
			{
				AndroidJavaObject androidJavaObject = androidJavaClass.CallStatic<AndroidJavaObject>("getDefault", new object[0]);
				text = androidJavaObject.Call<string>("getLanguage", new object[0]);
				string text2 = androidJavaObject.Call<string>("getCountry", new object[0]);
				if (!string.IsNullOrEmpty(text2))
				{
					text = text + "-" + text2;
				}
				string text3 = androidJavaObject.Call<string>("getVariant", new object[0]);
				if (!string.IsNullOrEmpty(text3))
				{
					text = text + "-" + text3;
				}
			}
		}
		catch (Exception ex)
		{
			SwrveLog.LogWarning("Couldn't get the device language, make sure you are running on an Android device: " + ex.ToString());
		}
		if (string.IsNullOrEmpty(text))
		{
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			string text4 = currentUICulture.TwoLetterISOLanguageName.ToLower();
			if (text4 != "iv")
			{
				text = text4;
			}
		}
		return text;
	}

	protected string GetSavedInstallTimeEpoch()
	{
		try
		{
			string text = storage.Load("Swrve_JoinedDate", userId);
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		catch (Exception ex)
		{
			SwrveLog.LogError("Couldn't obtain saved install time: " + ex.Message);
		}
		return null;
	}

	protected void InvalidateETag()
	{
		lastETag = string.Empty;
		storage.Remove("cmpg_etag", userId);
	}

	private void InitUserResources()
	{
		userResourcesRaw = storage.LoadSecure("srcngt2", userId);
		if (!string.IsNullOrEmpty(userResourcesRaw))
		{
			IList<object> list = (IList<object>)Json.Deserialize(userResourcesRaw);
			userResources = ProcessUserResources(list);
			NotifyUpdateUserResources();
		}
		else
		{
			InvalidateETag();
		}
	}

	private void NotifyUpdateUserResources()
	{
		if (userResources != null)
		{
			ResourceManager.SetResourcesFromJSON(userResources);
			if (ResourcesUpdatedCallback != null)
			{
				ResourcesUpdatedCallback();
			}
		}
	}

	private void LoadEventsFromDisk()
	{
		try
		{
			string value = storage.Load("Swrve_Events", userId);
			storage.Remove("Swrve_Events", userId);
			if (!string.IsNullOrEmpty(value))
			{
				if (eventBufferStringBuilder.Length != 0)
				{
					eventBufferStringBuilder.Insert(0, ",");
				}
				eventBufferStringBuilder.Insert(0, value);
			}
		}
		catch (Exception ex)
		{
			SwrveLog.LogWarning("Could not read events from cache (" + ex.ToString() + ")");
		}
	}

	private void LoadData()
	{
		LoadEventsFromDisk();
		installTimeEpoch = GetInstallTimeEpoch();
		installTimeFormatted = SwrveHelper.EpochToFormat(installTimeEpoch, InstallTimeFormat);
		lastETag = storage.Load("cmpg_etag", userId);
		string text = storage.Load("swrve_cr_flush_frequency", userId);
		if (!string.IsNullOrEmpty(text) && float.TryParse(text, out campaignsAndResourcesFlushFrequency))
		{
			campaignsAndResourcesFlushFrequency /= 1000f;
		}
		if (campaignsAndResourcesFlushFrequency == 0f)
		{
			campaignsAndResourcesFlushFrequency = 60f;
		}
		string text2 = storage.Load("swrve_cr_flush_delay", userId);
		if (!string.IsNullOrEmpty(text2) && float.TryParse(text2, out campaignsAndResourcesFlushRefreshDelay))
		{
			campaignsAndResourcesFlushRefreshDelay /= 1000f;
		}
		if (campaignsAndResourcesFlushRefreshDelay == 0f)
		{
			campaignsAndResourcesFlushRefreshDelay = 5f;
		}
	}

	protected string GetUniqueKey()
	{
		return apiKey + userId;
	}

	private string GetDeviceUniqueId()
	{
		string text = PlayerPrefs.GetString("Swrve.deviceUniqueIdentifier", null);
		if (string.IsNullOrEmpty(text))
		{
			text = GetRandomUUID();
		}
		return text;
	}

	private string GetRandomUUID()
	{
		try
		{
			Type type = Type.GetType("System.Guid");
			if (type != null)
			{
				MethodInfo method = type.GetMethod("NewGuid");
				if (method != null)
				{
					object obj = method.Invoke(null, null);
					if (obj != null)
					{
						string text = obj.ToString();
						if (!string.IsNullOrEmpty(text))
						{
							return text;
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			SwrveLog.LogWarning("Couldn't get random UUID: " + ex.ToString());
		}
		string text2 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		string text3 = string.Empty;
		for (int i = 0; i < 128; i++)
		{
			text3 += text2[UnityEngine.Random.Range(0, text2.Length - 1)];
		}
		return text3;
	}

	protected virtual IRESTClient CreateRestClient()
	{
		return new RESTClient();
	}

	protected virtual ISwrveStorage CreateStorage()
	{
		if (config.StoreDataInPlayerPrefs)
		{
			return new SwrvePlayerPrefsStorage();
		}
		return new SwrveFileStorage(swrvePath, GetUniqueKey());
	}

	private IEnumerator PostEvents_Coroutine(Dictionary<string, string> requestHeaders, byte[] eventsPostEncodedData)
	{
		byte[] eventsPostEncodedData2;
		yield return Container.StartCoroutine(restClient.Post(eventsUrl, eventsPostEncodedData, requestHeaders, delegate(RESTResponse response)
		{
			if (response.Error != WwwDeducedError.NetworkError)
			{
				ClearEventBuffer();
				eventsPostEncodedData2 = null;
			}
			eventsConnecting = false;
			TaskFinished("PostEvents_Coroutine");
		}));
	}

	protected virtual void ClearEventBuffer()
	{
		eventsPostString = null;
	}

	private void AppendEventToBuffer(string eventType, Dictionary<string, object> eventParameters, bool allowShowMessage = true)
	{
		eventParameters.Add("type", eventType);
		eventParameters.Add("seqnum", getNextSeqNum());
		eventParameters.Add("time", GetSessionTime());
		string text = Json.Serialize(eventParameters);
		string eventName = SwrveHelper.GetEventName(eventParameters);
		bool flag = eventBufferStringBuilder.Length + text.Length <= config.MaxBufferChars;
		if (flag || config.SendEventsIfBufferTooLarge)
		{
			if (!flag && config.SendEventsIfBufferTooLarge)
			{
				SendQueuedEvents();
			}
			if (eventBufferStringBuilder.Length > 0)
			{
				eventBufferStringBuilder.Append(',');
			}
			AppendEventToBuffer(text);
		}
		else
		{
			Debug.LogError("Could not append the event to the buffer. Please consider enabling SendEventsIfBufferTooLarge");
		}
		if (allowShowMessage && config.TalkEnabled)
		{
			StartTask("ShowMessageForEvent", ShowMessageForEvent(eventName, GlobalInstallButtonListener, GlobalCustomButtonListener, GlobalMessageListener));
		}
	}

	protected virtual void AppendEventToBuffer(string eventJson)
	{
		eventBufferStringBuilder.Append(eventJson);
	}

	protected virtual Coroutine StartTask(string tag, IEnumerator task)
	{
		return Container.StartCoroutine(task);
	}

	protected virtual void TaskFinished(string tag)
	{
	}

	private bool IsAlive()
	{
		return Container != null && !Destroyed;
	}

	protected virtual void GetDeviceScreenInfo()
	{
		deviceWidth = Screen.width;
		deviceHeight = Screen.height;
		if (deviceWidth > deviceHeight)
		{
			int num = deviceWidth;
			deviceWidth = deviceHeight;
			deviceHeight = num;
		}
	}

	private void QueueDeviceInfo()
	{
		Dictionary<string, string> deviceInfo = GetDeviceInfo();
		UserUpdate(deviceInfo);
	}

	private void SendDeviceInfo()
	{
		QueueDeviceInfo();
		SendQueuedEvents();
	}

	private IEnumerator WaitAndRefreshResourcesAndCampaigns_Coroutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		RefreshUserResourcesAndCampaigns();
	}

	private void CheckForCampaignsAndResourcesUpdates(bool invokedByTimer)
	{
		if (IsAlive())
		{
			if (SendQueuedEvents())
			{
				Container.StartCoroutine(WaitAndRefreshResourcesAndCampaigns_Coroutine(campaignsAndResourcesFlushRefreshDelay));
			}
			if (!invokedByTimer)
			{
				StopCheckForCampaignAndResources();
				StartCheckForCampaignsAndResources();
			}
		}
	}

	private void StartCheckForCampaignsAndResources()
	{
		if (campaignAndResourcesCoroutineInstance == null)
		{
			campaignAndResourcesCoroutineInstance = CheckForCampaignsAndResourcesUpdates_Coroutine();
			Container.StartCoroutine(campaignAndResourcesCoroutineInstance);
		}
		campaignAndResourcesCoroutineEnabled = true;
	}

	private void StopCheckForCampaignAndResources()
	{
		if (campaignAndResourcesCoroutineInstance != null)
		{
			Container.StopCoroutine(campaignAndResourcesCoroutineInstance);
			campaignAndResourcesCoroutineInstance = null;
		}
		campaignAndResourcesCoroutineEnabled = false;
	}

	private IEnumerator CheckForCampaignsAndResourcesUpdates_Coroutine()
	{
		yield return new WaitForSeconds(campaignsAndResourcesFlushFrequency);
		CheckForCampaignsAndResourcesUpdates(true);
		if (campaignAndResourcesCoroutineEnabled)
		{
			campaignAndResourcesCoroutineInstance = null;
			StartCheckForCampaignsAndResources();
		}
	}

	protected virtual long GetSessionTime()
	{
		return SwrveHelper.GetMilliseconds();
	}

	private void GenerateNewSessionInterval()
	{
		lastSessionTick = GetSessionTime() + config.NewSessionInterval * 1000;
	}

	public void Update()
	{
		if (currentDisplayingMessage != null && !currentMessage.Closing)
		{
			if (inputManager.GetMouseButtonDown(0))
			{
				ProcessButtonDown();
			}
			else if (inputManager.GetMouseButtonUp(0))
			{
				ProcessButtonUp();
			}
		}
	}

	public void OnGUI()
	{
		if (currentDisplayingMessage == null)
		{
			return;
		}
		SwrveOrientation deviceOrientation = GetDeviceOrientation();
		if (deviceOrientation != currentOrientation)
		{
			if (currentDisplayingMessage.Orientation != deviceOrientation)
			{
				if (currentDisplayingMessage.Message.SupportsOrientation(deviceOrientation))
				{
					StartTask("SwitchMessageOrienation", SwitchMessageOrienation(deviceOrientation));
				}
				else
				{
					currentDisplayingMessage.Rotate = true;
				}
			}
			else
			{
				currentDisplayingMessage.Rotate = false;
			}
		}
		int depth = GUI.depth;
		Matrix4x4 matrix = GUI.matrix;
		GUI.depth = 0;
		SwrveMessageRenderer.DrawMessage(currentMessage, Screen.width / 2 + currentMessage.Message.Position.X, Screen.height / 2 + currentMessage.Message.Position.Y);
		GUI.matrix = matrix;
		GUI.depth = depth;
		if (currentDisplayingMessage.MessageListener != null)
		{
			currentDisplayingMessage.MessageListener.OnShowing(currentDisplayingMessage);
		}
		if (currentMessage.Dismissed)
		{
			currentMessage = null;
			currentDisplayingMessage = null;
		}
		currentOrientation = deviceOrientation;
	}

	private IEnumerator SwitchMessageOrienation(SwrveOrientation newOrientation)
	{
		SwrveMessageFormat newFormat = currentMessage.Message.GetFormat(newOrientation);
		if (newFormat != null && newFormat != currentMessage)
		{
			SwrveMessageFormat oldFormat = currentMessage;
			CoroutineReference<bool> wereAllLoaded = new CoroutineReference<bool>(false);
			yield return StartTask("PreloadFormatAssets", PreloadFormatAssets(newFormat, wereAllLoaded));
			if (wereAllLoaded.Value())
			{
				currentMessage = (currentDisplayingMessage = newFormat);
				oldFormat.UnloadAssets();
			}
			else
			{
				SwrveLog.LogError("Could not switch orientation. Not all assets could be preloaded");
			}
			TaskFinished("SwitchMessageOrienation");
		}
	}

	private void ProcessButtonDown()
	{
		Vector3 mousePosition = inputManager.GetMousePosition();
		foreach (SwrveButton button in currentMessage.Buttons)
		{
			if (button.Rect.Contains(mousePosition))
			{
				button.Pressed = true;
			}
		}
	}

	private void ProcessButtonUp()
	{
		SwrveButton swrveButton = null;
		int num = currentMessage.Buttons.Count - 1;
		while (num >= 0 && swrveButton == null)
		{
			SwrveButton swrveButton2 = currentMessage.Buttons[num];
			Vector3 mousePosition = inputManager.GetMousePosition();
			if (swrveButton2.Rect.Contains(mousePosition) && swrveButton2.Pressed)
			{
				swrveButton = swrveButton2;
			}
			else
			{
				swrveButton2.Pressed = false;
			}
			num--;
		}
		if (swrveButton == null)
		{
			return;
		}
		SwrveLog.Log("Clicked button " + swrveButton.ActionType);
		ButtonWasPressedByUser(swrveButton);
		if (swrveButton.ActionType == SwrveActionType.Install)
		{
			string text = swrveButton.GameId.ToString();
			if (gameStoreLinks.ContainsKey(text))
			{
				string text2 = gameStoreLinks[text];
				if (!string.IsNullOrEmpty(text2))
				{
					bool flag = true;
					if (currentMessage.InstallButtonListener != null)
					{
						flag = currentMessage.InstallButtonListener.OnAction(text2);
					}
					if (flag)
					{
						Application.OpenURL(text2);
					}
				}
				else
				{
					SwrveLog.LogError("No app store url for game " + text);
				}
			}
			else
			{
				SwrveLog.LogError("Install button app store url empty!");
			}
		}
		else if (swrveButton.ActionType == SwrveActionType.Custom)
		{
			string action = swrveButton.Action;
			if (currentMessage.CustomButtonListener != null)
			{
				currentMessage.CustomButtonListener.OnAction(action);
			}
			else
			{
				SwrveLog.Log("No custom button listener, treating action as URL");
				if (!string.IsNullOrEmpty(action))
				{
					Application.OpenURL(action);
				}
			}
		}
		swrveButton.Pressed = false;
		DismissMessage();
	}

	protected void SetMessageMinDelayThrottle()
	{
		showMessagesAfterDelay = SwrveHelper.GetNow() + TimeSpan.FromSeconds(minDelayBetweenMessage);
	}

	private void AutoShowMessages()
	{
		if (!autoShowMessagesEnabled || !campaignsAndResourcesInitialized || campaigns == null || campaigns.Count == 0)
		{
			return;
		}
		foreach (SwrveCampaign campaign in campaigns)
		{
			if (!campaign.HasMessageForEvent("Swrve.Messages.showAtSessionStart"))
			{
				continue;
			}
			if (TriggeredMessageListener != null)
			{
				SwrveMessage messageForEvent = GetMessageForEvent("Swrve.Messages.showAtSessionStart");
				if (messageForEvent != null)
				{
					autoShowMessagesEnabled = false;
					TriggeredMessageListener.OnMessageTriggered(messageForEvent);
				}
			}
			else if (currentMessage == null)
			{
				SwrveMessage messageForEvent2 = GetMessageForEvent("Swrve.Messages.showAtSessionStart");
				if (messageForEvent2 != null)
				{
					autoShowMessagesEnabled = false;
					Container.StartCoroutine(LaunchMessage(messageForEvent2, GlobalInstallButtonListener, GlobalCustomButtonListener, GlobalMessageListener));
				}
			}
			break;
		}
	}

	private IEnumerator LaunchMessage(SwrveMessage message, ISwrveInstallButtonListener installButtonListener, ISwrveCustomButtonListener customButtonListener, ISwrveMessageListener messageListener)
	{
		if (message == null)
		{
			yield break;
		}
		SwrveOrientation currentOrientation = GetDeviceOrientation();
		SwrveMessageFormat selectedFormat = message.GetFormat(currentOrientation);
		if (selectedFormat != null)
		{
			CoroutineReference<bool> wereAllLoaded = new CoroutineReference<bool>(false);
			yield return StartTask("PreloadFormatAssets", PreloadFormatAssets(selectedFormat, wereAllLoaded));
			if (wereAllLoaded.Value())
			{
				ShowMessageFormat(selectedFormat, installButtonListener, customButtonListener, messageListener);
			}
			else
			{
				SwrveLog.LogError("Could not preload all the assets for message " + message.Id);
			}
		}
		else
		{
			SwrveLog.LogError("Could not get a format for the current orientation: " + currentOrientation);
		}
	}

	private void NoMessagesWereShown(string eventName, string reason)
	{
		SwrveLog.Log("Not showing message for " + eventName + ": " + reason);
		if (qaUser != null)
		{
			qaUser.TriggerFailure(eventName, reason);
		}
	}

	private IEnumerator PreloadFormatAssets(SwrveMessageFormat format, CoroutineReference<bool> wereAllLoaded)
	{
		SwrveLog.Log("Preloading format");
		bool allLoaded = true;
		foreach (SwrveImage image in format.Images)
		{
			if (image.Texture == null && !string.IsNullOrEmpty(image.File))
			{
				SwrveLog.Log("Preloading image file " + image.File);
				CoroutineReference<Texture2D> result2 = new CoroutineReference<Texture2D>();
				yield return StartTask("LoadAsset", LoadAsset(image.File, result2));
				if (result2.Value() != null)
				{
					image.Texture = result2.Value();
				}
				else
				{
					allLoaded = false;
				}
			}
		}
		foreach (SwrveButton button in format.Buttons)
		{
			if (button.Texture == null && !string.IsNullOrEmpty(button.Image))
			{
				SwrveLog.Log("Preloading button image " + button.Image);
				CoroutineReference<Texture2D> result = new CoroutineReference<Texture2D>();
				yield return StartTask("LoadAsset", LoadAsset(button.Image, result));
				if (result.Value() != null)
				{
					button.Texture = result.Value();
				}
				else
				{
					allLoaded = false;
				}
			}
		}
		wereAllLoaded.Value(allLoaded);
		TaskFinished("PreloadFormatAssets");
	}

	private bool HasShowTooManyMessagesAlready()
	{
		return messagesLeftToShow <= 0;
	}

	private bool IsTooSoonToShowMessageAfterLaunch(DateTime now)
	{
		return now < showMessagesAfterLaunch;
	}

	private bool IsTooSoonToShowMessageAfterDelay(DateTime now)
	{
		return now < showMessagesAfterDelay;
	}

	private SwrveMessageFormat ShowMessageFormat(SwrveMessageFormat format, ISwrveInstallButtonListener installButtonListener, ISwrveCustomButtonListener customButtonListener, ISwrveMessageListener messageListener)
	{
		currentMessage = format;
		format.MessageListener = messageListener;
		format.CustomButtonListener = customButtonListener;
		format.InstallButtonListener = installButtonListener;
		currentDisplayingMessage = currentMessage;
		currentOrientation = GetDeviceOrientation();
		SwrveMessageRenderer.InitMessage(currentDisplayingMessage);
		MessageWasShownToUser(currentDisplayingMessage);
		return format;
	}

	private IEnumerator LoadAsset(string fileName, CoroutineReference<Texture2D> texture)
	{
		string filePath = swrveTemporaryPath + "/" + fileName;
		WWW www = new WWW("file://" + filePath);
		yield return www;
		if (www != null && www.error == null)
		{
			Texture2D loadedTexture = www.texture;
			texture.Value(loadedTexture);
		}
		else
		{
			SwrveLog.LogError("Could not load asset with WWW " + filePath + ": " + www.error);
			if (CrossPlatformFile.Exists(filePath))
			{
				byte[] byteArray = CrossPlatformFile.ReadAllBytes(filePath);
				Texture2D loadedTexture2 = new Texture2D(4, 4);
				if (loadedTexture2.LoadImage(byteArray))
				{
					texture.Value(loadedTexture2);
				}
				else
				{
					SwrveLog.LogWarning("Could not load asset from I/O" + filePath);
				}
			}
			else
			{
				SwrveLog.LogError("The file " + filePath + " does not exist.");
			}
		}
		TaskFinished("LoadAsset");
	}

	protected virtual bool CheckAsset(string fileName)
	{
		if (CrossPlatformFile.Exists(swrveTemporaryPath + "/" + fileName))
		{
			return true;
		}
		return false;
	}

	protected virtual IEnumerator DownloadAsset(string fileName, CoroutineReference<Texture2D> texture)
	{
		string url = cdn + fileName;
		SwrveLog.Log("Downloading asset: " + url);
		WWW www = new WWW(url);
		yield return www;
		WwwDeducedError err = UnityWwwHelper.DeduceWwwError(www);
		if (www != null && err == WwwDeducedError.NoError && www.isDone)
		{
			Texture2D loadedTexture = www.texture;
			if (loadedTexture != null)
			{
				string filePath = swrveTemporaryPath + "/" + fileName;
				SwrveLog.Log("Saving to " + filePath);
				byte[] bytes2 = loadedTexture.EncodeToPNG();
				CrossPlatformFile.SaveBytes(filePath, bytes2);
				bytes2 = null;
				texture.Value(loadedTexture);
			}
		}
		TaskFinished("DownloadAsset");
	}

	protected IEnumerator DownloadAssets(List<string> assetsQueue)
	{
		assetsCurrentlyDownloading = true;
		foreach (string asset in assetsQueue)
		{
			if (!CheckAsset(asset))
			{
				CoroutineReference<Texture2D> resultTexture = new CoroutineReference<Texture2D>();
				yield return StartTask("DownloadAsset", DownloadAsset(asset, resultTexture));
				Texture2D texture = resultTexture.Value();
				if (texture != null)
				{
					assetsOnDisk.Add(asset);
					UnityEngine.Object.Destroy(texture);
				}
			}
			else
			{
				assetsOnDisk.Add(asset);
			}
		}
		assetsCurrentlyDownloading = false;
		AutoShowMessages();
		TaskFinished("DownloadAssets");
	}

	protected virtual void ProcessCampaigns(Dictionary<string, object> root)
	{
		List<SwrveCampaign> list = new List<SwrveCampaign>();
		List<string> list2 = new List<string>();
		try
		{
			if (root != null && root.ContainsKey("version"))
			{
				int @int = MiniJsonHelper.GetInt(root, "version");
				if (@int == CampaignAPIVersion)
				{
					cdn = (string)root["cdn_root"];
					Dictionary<string, object> dictionary = (Dictionary<string, object>)root["game_data"];
					foreach (string key in dictionary.Keys)
					{
						if (gameStoreLinks.ContainsKey(key))
						{
							gameStoreLinks.Remove(key);
						}
						Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary[key];
						if (dictionary2 != null && dictionary2.ContainsKey("app_store_url"))
						{
							object obj = dictionary2["app_store_url"];
							if (obj != null && obj is string)
							{
								gameStoreLinks.Add(key, (string)obj);
							}
						}
					}
					Dictionary<string, object> dictionary3 = (Dictionary<string, object>)root["rules"];
					int num = ((!dictionary3.ContainsKey("delay_first_message")) ? 150 : MiniJsonHelper.GetInt(dictionary3, "delay_first_message"));
					long num2 = ((!dictionary3.ContainsKey("max_messages_per_session")) ? 99999 : MiniJsonHelper.GetLong(dictionary3, "max_messages_per_session"));
					int num3 = ((!dictionary3.ContainsKey("min_delay_between_messages")) ? 55 : MiniJsonHelper.GetInt(dictionary3, "min_delay_between_messages"));
					DateTime now = SwrveHelper.GetNow();
					minDelayBetweenMessage = num3;
					messagesLeftToShow = num2;
					showMessagesAfterLaunch = initialisedTime + TimeSpan.FromSeconds(num);
					SwrveLog.Log("Game rules OK: Delay Seconds: " + num + " Max shows: " + num2);
					SwrveLog.Log("Time is " + now.ToString() + " show messages after " + showMessagesAfterLaunch.ToString());
					Dictionary<int, string> dictionary4 = null;
					if (root.ContainsKey("qa"))
					{
						Dictionary<string, object> dictionary5 = (Dictionary<string, object>)root["qa"];
						SwrveLog.Log("You are a QA user!");
						dictionary4 = new Dictionary<int, string>();
						qaUser = new SwrveQAUser(this, dictionary5);
						if (dictionary5.ContainsKey("campaigns"))
						{
							IList<object> list3 = (List<object>)dictionary5["campaigns"];
							for (int i = 0; i < list3.Count; i++)
							{
								Dictionary<string, object> dictionary6 = (Dictionary<string, object>)list3[i];
								int int2 = MiniJsonHelper.GetInt(dictionary6, "id");
								string text = (string)dictionary6["reason"];
								SwrveLog.Log("Campaign " + int2 + " not downloaded because: " + text);
								dictionary4.Add(int2, text);
							}
						}
					}
					else
					{
						qaUser = null;
					}
					IList<object> list4 = (List<object>)root["campaigns"];
					int j = 0;
					for (int count = list4.Count; j < count; j++)
					{
						Dictionary<string, object> campaignData = (Dictionary<string, object>)list4[j];
						SwrveCampaign swrveCampaign = SwrveCampaign.LoadFromJSON(this, campaignData, initialisedTime, swrveTemporaryPath);
						if (swrveCampaign.Messages.Count <= 0)
						{
							continue;
						}
						list2.AddRange(swrveCampaign.ListOfAssets());
						if (campaignSettings != null && (qaUser == null || !qaUser.ResetDevice))
						{
							if (campaignSettings.ContainsKey("Next" + swrveCampaign.Id))
							{
								int int3 = MiniJsonHelper.GetInt(campaignSettings, "Next" + swrveCampaign.Id);
								swrveCampaign.Next = int3;
							}
							if (campaignSettings.ContainsKey("Impressions" + swrveCampaign.Id))
							{
								int int4 = MiniJsonHelper.GetInt(campaignSettings, "Impressions" + swrveCampaign.Id);
								swrveCampaign.Impressions = int4;
							}
						}
						list.Add(swrveCampaign);
						if (qaUser != null)
						{
							dictionary4.Add(swrveCampaign.Id, null);
						}
					}
					if (qaUser != null)
					{
						qaUser.TalkSession(dictionary4);
					}
				}
			}
		}
		catch (Exception ex)
		{
			SwrveLog.LogError("Could not process campaigns: " + ex.ToString());
		}
		StartTask("DownloadAssets", DownloadAssets(list2));
		campaigns = new List<SwrveCampaign>(list);
	}

	private void LoadResourcesAndCampaigns()
	{
		try
		{
			if (campaignsConnecting)
			{
				return;
			}
			if (!config.AutoDownloadCampaignsAndResources)
			{
				if (campaignsAndResourcesLastRefreshed != 0L)
				{
					long sessionTime = GetSessionTime();
					if (sessionTime < campaignsAndResourcesLastRefreshed)
					{
						SwrveLog.Log("Request to retrieve campaign and user resource data was rate-limited.");
						return;
					}
				}
				campaignsAndResourcesLastRefreshed = GetSessionTime() + (long)(campaignsAndResourcesFlushFrequency * 1000f);
			}
			campaignsConnecting = true;
			float num = ((Screen.dpi != 0f) ? Screen.dpi : 160f);
			string deviceModel = GetDeviceModel();
			string operatingSystem = SystemInfo.operatingSystem;
			StringBuilder stringBuilder = new StringBuilder(resourcesAndCampaignsUrl).AppendFormat("?user={0}&api_key={1}&app_version={2}&joined={3}", escapedUserId, ApiKey, WWW.EscapeURL(GetAppVersion()), installTimeEpoch);
			if (config.TalkEnabled)
			{
				stringBuilder.AppendFormat("&version={0}&orientation={1}&language={2}&app_store={3}&device_width={4}&device_height={5}&device_dpi={6}&os_version={7}&device_name={8}", CampaignEndpointVersion, config.Orientation.ToString().ToLower(), Language, config.AppStore, deviceWidth, deviceHeight, num, WWW.EscapeURL(operatingSystem), WWW.EscapeURL(deviceModel));
			}
			if (!string.IsNullOrEmpty(lastETag))
			{
				stringBuilder.AppendFormat("&etag={0}", lastETag);
			}
			StartTask("GetCampaignsAndResources_Coroutine", GetCampaignsAndResources_Coroutine(stringBuilder.ToString()));
		}
		catch (Exception ex)
		{
			SwrveLog.LogError("Error while trying to get user resources and campaign data: " + ex);
		}
	}

	private string GetDeviceModel()
	{
		string text = SystemInfo.deviceModel;
		if (string.IsNullOrEmpty(text))
		{
			text = "ModelUnknown";
		}
		return text;
	}

	private IEnumerator GetCampaignsAndResources_Coroutine(string getRequest)
	{
		SwrveLog.Log("Campaigns and resources request: " + getRequest);
		yield return Container.StartCoroutine(restClient.Get(getRequest, delegate(RESTResponse response)
		{
			if (response.Error == WwwDeducedError.NoError)
			{
				string value = null;
				if (response.Headers != null)
				{
					response.Headers.TryGetValue("ETAG", out value);
					if (!string.IsNullOrEmpty(value))
					{
						lastETag = value;
						storage.Save("cmpg_etag", value, userId);
					}
				}
				if (!string.IsNullOrEmpty(response.Body))
				{
					Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(response.Body);
					if (dictionary != null)
					{
						if (dictionary.ContainsKey("flush_frequency"))
						{
							string @string = MiniJsonHelper.GetString(dictionary, "flush_frequency");
							if (!string.IsNullOrEmpty(@string) && float.TryParse(@string, out campaignsAndResourcesFlushFrequency))
							{
								campaignsAndResourcesFlushFrequency /= 1000f;
								storage.Save("swrve_cr_flush_frequency", @string, userId);
							}
						}
						if (dictionary.ContainsKey("flush_refresh_delay"))
						{
							string string2 = MiniJsonHelper.GetString(dictionary, "flush_refresh_delay");
							if (!string.IsNullOrEmpty(string2) && float.TryParse(string2, out campaignsAndResourcesFlushRefreshDelay))
							{
								campaignsAndResourcesFlushRefreshDelay /= 1000f;
								storage.Save("swrve_cr_flush_delay", string2, userId);
							}
						}
						if (dictionary.ContainsKey("user_resources"))
						{
							IList<object> obj = (IList<object>)dictionary["user_resources"];
							string data = Json.Serialize(obj);
							storage.SaveSecure("srcngt2", data, userId);
							userResources = ProcessUserResources(obj);
							userResourcesRaw = data;
							if (campaignsAndResourcesInitialized)
							{
								NotifyUpdateUserResources();
							}
						}
						if (config.TalkEnabled && dictionary.ContainsKey("campaigns"))
						{
							Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary["campaigns"];
							string cacheContent = Json.Serialize(dictionary2);
							SaveCampaignsCache(cacheContent);
							AutoShowMessages();
							ProcessCampaigns(dictionary2);
							StringBuilder stringBuilder = new StringBuilder();
							int i = 0;
							for (int count = campaigns.Count; i < count; i++)
							{
								SwrveCampaign swrveCampaign = campaigns[i];
								if (i != 0)
								{
									stringBuilder.Append(',');
								}
								stringBuilder.Append(swrveCampaign.Id);
							}
							Dictionary<string, string> payload = new Dictionary<string, string>
							{
								{
									"ids",
									stringBuilder.ToString()
								},
								{
									"count",
									(campaigns != null) ? campaigns.Count.ToString() : "0"
								}
							};
							NamedEventInternal("Swrve.Messages.campaigns_downloaded", payload);
						}
					}
				}
			}
			else
			{
				SwrveLog.LogError("Resources and campaigns request error: " + response.Error.ToString() + ":" + response.Body);
			}
			if (!campaignsAndResourcesInitialized)
			{
				campaignsAndResourcesInitialized = true;
				AutoShowMessages();
				NotifyUpdateUserResources();
			}
			campaignsConnecting = false;
			TaskFinished("GetCampaignsAndResources_Coroutine");
		}));
	}

	private void SaveCampaignsCache(string cacheContent)
	{
		try
		{
			if (cacheContent == null)
			{
				cacheContent = string.Empty;
			}
			storage.SaveSecure(CampaignsSave, cacheContent, userId);
		}
		catch (Exception ex)
		{
			SwrveLog.LogError("Error while saving campaigns to the cache " + ex);
		}
	}

	private void SaveCampaignData(SwrveCampaign campaign)
	{
		try
		{
			campaignSettings["Next" + campaign.Id] = campaign.Next;
			campaignSettings["Impressions" + campaign.Id] = campaign.Impressions;
			string data = Json.Serialize(campaignSettings);
			storage.Save(CampaignsSettingsSave, data, userId);
		}
		catch (Exception ex)
		{
			SwrveLog.LogError("Error while trying to save campaign settings " + ex);
		}
	}

	private void LoadTalkData()
	{
		try
		{
			string text = storage.Load(CampaignsSettingsSave, userId);
			string decodedString;
			if (text != null && text.Length != 0 && ResponseBodyTester.TestUTF8(text, out decodedString))
			{
				campaignSettings = (Dictionary<string, object>)Json.Deserialize(decodedString);
			}
		}
		catch (Exception)
		{
		}
		try
		{
			string text2 = storage.LoadSecure(CampaignsSave, userId);
			if (!string.IsNullOrEmpty(text2))
			{
				string decodedString2 = null;
				if (ResponseBodyTester.TestUTF8(text2, out decodedString2))
				{
					Dictionary<string, object> root = (Dictionary<string, object>)Json.Deserialize(decodedString2);
					ProcessCampaigns(root);
				}
				else
				{
					SwrveLog.Log("Failed to parse campaigns cache");
					InvalidateETag();
				}
			}
			else
			{
				InvalidateETag();
			}
		}
		catch (Exception ex2)
		{
			SwrveLog.LogWarning("Could not read campaigns from cache, using default (" + ex2.ToString() + ")");
			InvalidateETag();
		}
	}

	private void SendPushNotificationEngagedEvent(string pushId)
	{
		if (pushId != lastPushEngagedId)
		{
			lastPushEngagedId = pushId;
			string name = "Swrve.Messages.Push-" + pushId + ".engaged";
			NamedEvent(name);
			SwrveLog.Log("Got Swrve notification with ID " + pushId);
		}
	}

	protected int ConvertInt64ToInt32Hack(long val)
	{
		return (int)(val & 0xFFFFFFFFu);
	}

	protected virtual ICarrierInfo GetCarrierInfoProvider()
	{
		return deviceCarrierInfo;
	}

	private void GooglePlayRegisterForPushNotification(MonoBehaviour container, string senderId)
	{
		try
		{
			bool flag = false;
			gcmDeviceToken = storage.Load("Swrve_gcmDeviceToken");
			if (!androidPluginInitialized)
			{
				androidPluginInitialized = true;
				using (new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					string name = "com.swrve.unity.gcm.SwrveGcmDeviceRegistration".Replace(".", "/");
					if (AndroidJNI.FindClass(name).ToInt32() != 0)
					{
						androidPlugin = new AndroidJavaClass("com.swrve.unity.gcm.SwrveGcmDeviceRegistration");
						if (androidPlugin != null)
						{
							int num = androidPlugin.CallStatic<int>("getVersion", new object[0]);
							if (num != 3)
							{
								androidPlugin = null;
								throw new Exception("The version of the Swrve Android Push plugin is different. This Swrve SDK needs version " + 3);
							}
							androidPluginInitializedSuccessfully = true;
						}
					}
				}
			}
			if (androidPluginInitializedSuccessfully)
			{
				flag = androidPlugin.CallStatic<bool>("registerDevice", new object[3] { container.name, senderId, config.GCMPushNotificationTitle });
			}
			if (!flag)
			{
				SwrveLog.LogError("Could not communicate with the Swrve Android Push plugin. Have you copied all the jars to the directory?");
			}
		}
		catch (Exception ex)
		{
			SwrveLog.LogError("Could not retrieve the device Registration Id: " + ex.ToString());
		}
	}

	private string AndroidGetTimezone()
	{
		try
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("java.util.GregorianCalendar");
			return androidJavaObject.Call<AndroidJavaObject>("getTimeZone", new object[0]).Call<string>("getID", new object[0]);
		}
		catch (Exception ex)
		{
			SwrveLog.LogWarning("Couldn't get the device timezone, make sure you are running on an Android device: " + ex.ToString());
		}
		return null;
	}

	private string AndroidGetAppVersion()
	{
		try
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				string text = @static.Call<string>("getPackageName", new object[0]);
				return @static.Call<AndroidJavaObject>("getPackageManager", new object[0]).Call<AndroidJavaObject>("getPackageInfo", new object[2] { text, 0 }).Get<string>("versionName");
			}
		}
		catch (Exception ex)
		{
			SwrveLog.LogWarning("Couldn't get the device app version, make sure you are running on an Android device: " + ex.ToString());
		}
		return null;
	}

	public string GetAppVersion()
	{
		if (string.IsNullOrEmpty(config.AppVersion))
		{
			config.AppVersion = AndroidGetAppVersion();
		}
		return config.AppVersion;
	}

	private void SetInputManager(IInputManager inputManager)
	{
		this.inputManager = inputManager;
	}

	protected void StartCampaignsAndResourcesTimer()
	{
		if (config.AutoDownloadCampaignsAndResources)
		{
			RefreshUserResourcesAndCampaigns();
			StartCheckForCampaignsAndResources();
			Container.StartCoroutine(WaitAndRefreshResourcesAndCampaigns_Coroutine(campaignsAndResourcesFlushRefreshDelay));
		}
	}

	protected void DisableAutoShowAfterDelay()
	{
		Container.StartCoroutine(DisableAutoShowAfterDelay_Coroutine());
	}

	private IEnumerator DisableAutoShowAfterDelay_Coroutine()
	{
		yield return new WaitForSeconds(config.AutoShowMessagesMaxDelay);
		autoShowMessagesEnabled = false;
	}
}
