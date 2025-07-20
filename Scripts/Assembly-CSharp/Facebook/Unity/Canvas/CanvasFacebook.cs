using System;
using System.Collections.Generic;
using System.Globalization;
using Facebook.MiniJSON;

namespace Facebook.Unity.Canvas
{
	internal sealed class CanvasFacebook : FacebookBase, ICanvasFacebook, ICanvasFacebookImplementation, ICanvasFacebookResultHandler, IFacebook, IFacebookResultHandler, IPayFacebook
	{
		private class CanvasUIMethodCall<T> : MethodCall<T> where T : IResult
		{
			private CanvasFacebook canvasImpl;

			private string callbackMethod;

			public CanvasUIMethodCall(CanvasFacebook canvasImpl, string methodName, string callbackMethod)
				: base((FacebookBase)canvasImpl, methodName)
			{
				this.canvasImpl = canvasImpl;
				this.callbackMethod = callbackMethod;
			}

			public override void Call(MethodArguments args)
			{
				UI(base.MethodName, args, base.Callback);
			}

			private void UI(string method, MethodArguments args, FacebookDelegate<T> callback = null)
			{
				canvasImpl.canvasJSWrapper.DisableFullScreen();
				MethodArguments methodArguments = new MethodArguments(args);
				methodArguments.AddString("app_id", canvasImpl.appId);
				methodArguments.AddString("method", method);
				string text = canvasImpl.CallbackManager.AddFacebookDelegate(callback);
				canvasImpl.canvasJSWrapper.ExternalCall("FBUnity.ui", methodArguments.ToJsonString(), text, callbackMethod);
			}
		}

		internal const string MethodAppRequests = "apprequests";

		internal const string MethodFeed = "feed";

		internal const string MethodPay = "pay";

		internal const string MethodGameGroupCreate = "game_group_create";

		internal const string MethodGameGroupJoin = "game_group_join";

		internal const string CancelledResponse = "{\"cancelled\":true}";

		internal const string FacebookConnectURL = "https://connect.facebook.net";

		private const string AuthResponseKey = "authResponse";

		private string appId;

		private string appLinkUrl;

		private ICanvasJSWrapper canvasJSWrapper;

		public override bool LimitEventUsage { get; set; }

		public override string SDKName
		{
			get
			{
				return "FBJSSDK";
			}
		}

		public override string SDKVersion
		{
			get
			{
				return canvasJSWrapper.GetSDKVersion();
			}
		}

		public override string SDKUserAgent
		{
			get
			{
				FacebookUnityPlatform currentPlatform = Constants.CurrentPlatform;
				string productName;
				if (currentPlatform == FacebookUnityPlatform.WebGL || currentPlatform == FacebookUnityPlatform.WebPlayer)
				{
					productName = string.Format(CultureInfo.InvariantCulture, "FBUnity{0}", Constants.CurrentPlatform.ToString());
				}
				else
				{
					FacebookLogger.Warn("Currently running on uknown web platform");
					productName = "FBUnityWebUnknown";
				}
				return string.Format(CultureInfo.InvariantCulture, "{0} {1}", base.SDKUserAgent, Utilities.GetUserAgent(productName, FacebookSdkVersion.Build));
			}
		}

		public CanvasFacebook()
			: this(new CanvasJSWrapper(), new CallbackManager())
		{
		}

		public CanvasFacebook(ICanvasJSWrapper canvasJSWrapper, CallbackManager callbackManager)
			: base(callbackManager)
		{
			this.canvasJSWrapper = canvasJSWrapper;
		}

		public void Init(string appId, bool cookie, bool logging, bool status, bool xfbml, string channelUrl, string authResponse, bool frictionlessRequests, string javascriptSDKLocale, bool loadDebugJSSDK, HideUnityDelegate hideUnityDelegate, InitDelegate onInitComplete)
		{
			if (canvasJSWrapper.IntegrationMethodJs == null)
			{
				throw new Exception("Cannot initialize facebook javascript");
			}
			base.Init(hideUnityDelegate, onInitComplete);
			canvasJSWrapper.ExternalEval(canvasJSWrapper.IntegrationMethodJs);
			this.appId = appId;
			bool flag = true;
			MethodArguments methodArguments = new MethodArguments();
			methodArguments.AddString("appId", appId);
			methodArguments.AddPrimative("cookie", cookie);
			methodArguments.AddPrimative("logging", logging);
			methodArguments.AddPrimative("status", status);
			methodArguments.AddPrimative("xfbml", xfbml);
			methodArguments.AddString("channelUrl", channelUrl);
			methodArguments.AddString("authResponse", authResponse);
			methodArguments.AddPrimative("frictionlessRequests", frictionlessRequests);
			methodArguments.AddString("version", FB.GraphApiVersion);
			canvasJSWrapper.ExternalCall("FBUnity.init", flag ? 1 : 0, "https://connect.facebook.net", javascriptSDKLocale, loadDebugJSSDK ? 1 : 0, methodArguments.ToJsonString(), status ? 1 : 0);
		}

		public override void LogInWithPublishPermissions(IEnumerable<string> permissions, FacebookDelegate<ILoginResult> callback)
		{
			canvasJSWrapper.DisableFullScreen();
			canvasJSWrapper.ExternalCall("FBUnity.login", permissions, base.CallbackManager.AddFacebookDelegate(callback));
		}

		public override void LogInWithReadPermissions(IEnumerable<string> permissions, FacebookDelegate<ILoginResult> callback)
		{
			canvasJSWrapper.DisableFullScreen();
			canvasJSWrapper.ExternalCall("FBUnity.login", permissions, base.CallbackManager.AddFacebookDelegate(callback));
		}

		public override void LogOut()
		{
			base.LogOut();
			canvasJSWrapper.ExternalCall("FBUnity.logout");
		}

		public override void AppRequest(string message, OGActionType? actionType, string objectId, IEnumerable<string> to, IEnumerable<object> filters, IEnumerable<string> excludeIds, int? maxRecipients, string data, string title, FacebookDelegate<IAppRequestResult> callback)
		{
			ValidateAppRequestArgs(message, actionType, objectId, to, filters, excludeIds, maxRecipients, data, title, callback);
			MethodArguments methodArguments = new MethodArguments();
			methodArguments.AddString("message", message);
			methodArguments.AddCommaSeparatedList("to", to);
			methodArguments.AddString("action_type", (!actionType.HasValue) ? null : actionType.ToString());
			methodArguments.AddString("object_id", objectId);
			methodArguments.AddList("filters", filters);
			methodArguments.AddList("exclude_ids", excludeIds);
			methodArguments.AddNullablePrimitive("max_recipients", maxRecipients);
			methodArguments.AddString("data", data);
			methodArguments.AddString("title", title);
			CanvasUIMethodCall<IAppRequestResult> canvasUIMethodCall = new CanvasUIMethodCall<IAppRequestResult>(this, "apprequests", "OnAppRequestsComplete");
			canvasUIMethodCall.Callback = callback;
			canvasUIMethodCall.Call(methodArguments);
		}

		public override void ActivateApp(string appId)
		{
			canvasJSWrapper.ExternalCall("FBUnity.activateApp");
		}

		public override void ShareLink(Uri contentURL, string contentTitle, string contentDescription, Uri photoURL, FacebookDelegate<IShareResult> callback)
		{
			MethodArguments methodArguments = new MethodArguments();
			methodArguments.AddUri("link", contentURL);
			methodArguments.AddString("name", contentTitle);
			methodArguments.AddString("description", contentDescription);
			methodArguments.AddUri("picture", photoURL);
			CanvasUIMethodCall<IShareResult> canvasUIMethodCall = new CanvasUIMethodCall<IShareResult>(this, "feed", "OnShareLinkComplete");
			canvasUIMethodCall.Callback = callback;
			canvasUIMethodCall.Call(methodArguments);
		}

		public override void FeedShare(string toId, Uri link, string linkName, string linkCaption, string linkDescription, Uri picture, string mediaSource, FacebookDelegate<IShareResult> callback)
		{
			MethodArguments methodArguments = new MethodArguments();
			methodArguments.AddString("to", toId);
			methodArguments.AddUri("link", link);
			methodArguments.AddString("name", linkName);
			methodArguments.AddString("caption", linkCaption);
			methodArguments.AddString("description", linkDescription);
			methodArguments.AddUri("picture", picture);
			methodArguments.AddString("source", mediaSource);
			CanvasUIMethodCall<IShareResult> canvasUIMethodCall = new CanvasUIMethodCall<IShareResult>(this, "feed", "OnShareLinkComplete");
			canvasUIMethodCall.Callback = callback;
			canvasUIMethodCall.Call(methodArguments);
		}

		public void Pay(string product, string action, int quantity, int? quantityMin, int? quantityMax, string requestId, string pricepointId, string testCurrency, FacebookDelegate<IPayResult> callback)
		{
			PayImpl(product, action, quantity, quantityMin, quantityMax, requestId, pricepointId, testCurrency, callback);
		}

		public override void GameGroupCreate(string name, string description, string privacy, FacebookDelegate<IGroupCreateResult> callback)
		{
			MethodArguments methodArguments = new MethodArguments();
			methodArguments.AddString("name", name);
			methodArguments.AddString("description", description);
			methodArguments.AddString("privacy", privacy);
			methodArguments.AddString("display", "async");
			CanvasUIMethodCall<IGroupCreateResult> canvasUIMethodCall = new CanvasUIMethodCall<IGroupCreateResult>(this, "game_group_create", "OnGroupCreateComplete");
			canvasUIMethodCall.Callback = callback;
			canvasUIMethodCall.Call(methodArguments);
		}

		public override void GameGroupJoin(string id, FacebookDelegate<IGroupJoinResult> callback)
		{
			MethodArguments methodArguments = new MethodArguments();
			methodArguments.AddString("id", id);
			methodArguments.AddString("display", "async");
			CanvasUIMethodCall<IGroupJoinResult> canvasUIMethodCall = new CanvasUIMethodCall<IGroupJoinResult>(this, "game_group_join", "OnJoinGroupComplete");
			canvasUIMethodCall.Callback = callback;
			canvasUIMethodCall.Call(methodArguments);
		}

		public override void GetAppLink(FacebookDelegate<IAppLinkResult> callback)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("url", appLinkUrl);
			Dictionary<string, object> dictionary2 = dictionary;
			callback(new AppLinkResult(new ResultContainer(dictionary2)));
			appLinkUrl = string.Empty;
		}

		public override void AppEventsLogEvent(string logEvent, float? valueToSum, Dictionary<string, object> parameters)
		{
			canvasJSWrapper.ExternalCall("FBUnity.logAppEvent", logEvent, valueToSum, Json.Serialize(parameters));
		}

		public override void AppEventsLogPurchase(float logPurchase, string currency, Dictionary<string, object> parameters)
		{
			canvasJSWrapper.ExternalCall("FBUnity.logPurchase", logPurchase, currency, Json.Serialize(parameters));
		}

		public override void OnLoginComplete(ResultContainer result)
		{
			FormatAuthResponse(result, delegate(ResultContainer formattedResponse)
			{
				OnAuthResponse(new LoginResult(formattedResponse));
			});
		}

		public override void OnGetAppLinkComplete(ResultContainer message)
		{
			throw new NotImplementedException();
		}

		public void OnFacebookAuthResponseChange(string responseJsonData)
		{
			OnFacebookAuthResponseChange(new ResultContainer(responseJsonData));
		}

		public void OnFacebookAuthResponseChange(ResultContainer resultContainer)
		{
			FormatAuthResponse(resultContainer, delegate(ResultContainer formattedResponse)
			{
				LoginResult loginResult = new LoginResult(formattedResponse);
				AccessToken.CurrentAccessToken = loginResult.AccessToken;
			});
		}

		public void OnPayComplete(string responseJsonData)
		{
			OnPayComplete(new ResultContainer(responseJsonData));
		}

		public void OnPayComplete(ResultContainer resultContainer)
		{
			PayResult result = new PayResult(resultContainer);
			base.CallbackManager.OnFacebookResponse(result);
		}

		public override void OnAppRequestsComplete(ResultContainer resultContainer)
		{
			AppRequestResult result = new AppRequestResult(resultContainer);
			base.CallbackManager.OnFacebookResponse(result);
		}

		public override void OnShareLinkComplete(ResultContainer resultContainer)
		{
			ShareResult result = new ShareResult(resultContainer);
			base.CallbackManager.OnFacebookResponse(result);
		}

		public override void OnGroupCreateComplete(ResultContainer resultContainer)
		{
			GroupCreateResult result = new GroupCreateResult(resultContainer);
			base.CallbackManager.OnFacebookResponse(result);
		}

		public override void OnGroupJoinComplete(ResultContainer resultContainer)
		{
			GroupJoinResult result = new GroupJoinResult(resultContainer);
			base.CallbackManager.OnFacebookResponse(result);
		}

		public void OnUrlResponse(string url)
		{
			appLinkUrl = url;
		}

		private static void FormatAuthResponse(ResultContainer result, Utilities.Callback<ResultContainer> callback)
		{
			if (result.ResultDictionary == null)
			{
				callback(result);
				return;
			}
			IDictionary<string, object> value;
			if (result.ResultDictionary.TryGetValue<IDictionary<string, object>>("authResponse", out value))
			{
				result.ResultDictionary.Remove("authResponse");
				foreach (KeyValuePair<string, object> item in value)
				{
					result.ResultDictionary[item.Key] = item.Value;
				}
			}
			if (result.ResultDictionary.ContainsKey(LoginResult.AccessTokenKey) && !result.ResultDictionary.ContainsKey(LoginResult.PermissionsKey))
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("fields", "permissions");
				dictionary.Add("access_token", (string)result.ResultDictionary[LoginResult.AccessTokenKey]);
				Dictionary<string, string> formData = dictionary;
				FacebookDelegate<IGraphResult> callback2 = delegate(IGraphResult r)
				{
					IDictionary<string, object> value2;
					if (r.ResultDictionary != null && r.ResultDictionary.TryGetValue<IDictionary<string, object>>("permissions", out value2))
					{
						IList<string> list = new List<string>();
						IList<object> value3;
						if (value2.TryGetValue<IList<object>>("data", out value3))
						{
							foreach (object item2 in value3)
							{
								IDictionary<string, object> dictionary2 = item2 as IDictionary<string, object>;
								if (dictionary2 != null)
								{
									string value4;
									if (dictionary2.TryGetValue<string>("status", out value4) && value4.Equals("granted", StringComparison.InvariantCultureIgnoreCase))
									{
										string value5;
										if (dictionary2.TryGetValue<string>("permission", out value5))
										{
											list.Add(value5);
										}
										else
										{
											FacebookLogger.Warn("Didn't find permission name");
										}
									}
									else
									{
										FacebookLogger.Warn("Didn't find status in permissions result");
									}
								}
								else
								{
									FacebookLogger.Warn("Failed to case permission dictionary");
								}
							}
						}
						else
						{
							FacebookLogger.Warn("Failed to extract data from permissions");
						}
						result.ResultDictionary[LoginResult.PermissionsKey] = list.ToCommaSeparateList();
					}
					else
					{
						FacebookLogger.Warn("Failed to load permissions for access token");
					}
					callback(result);
				};
				FB.API("me", HttpMethod.GET, callback2, formData);
			}
			else
			{
				callback(result);
			}
		}

		private void PayImpl(string product, string action, int quantity, int? quantityMin, int? quantityMax, string requestId, string pricepointId, string testCurrency, FacebookDelegate<IPayResult> callback)
		{
			MethodArguments methodArguments = new MethodArguments();
			methodArguments.AddString("product", product);
			methodArguments.AddString("action", action);
			methodArguments.AddPrimative("quantity", quantity);
			methodArguments.AddNullablePrimitive("quantity_min", quantityMin);
			methodArguments.AddNullablePrimitive("quantity_max", quantityMax);
			methodArguments.AddString("request_id", requestId);
			methodArguments.AddString("pricepoint_id", pricepointId);
			methodArguments.AddString("test_currency", testCurrency);
			CanvasUIMethodCall<IPayResult> canvasUIMethodCall = new CanvasUIMethodCall<IPayResult>(this, "pay", "OnPayComplete");
			canvasUIMethodCall.Callback = callback;
			canvasUIMethodCall.Call(methodArguments);
		}
	}
}
