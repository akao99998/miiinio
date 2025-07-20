using System;
using System.Collections.Generic;
using SupersonicJSON;
using UnityEngine;

public class AndroidAgent : SupersonicIAgent
{
	private const string REWARD_AMOUNT = "reward_amount";

	private const string REWARD_NAME = "reward_name";

	private const string PLACEMENT_NAME = "placement_name";

	private static AndroidJavaObject _androidBridge;

	private static readonly string AndroidBridge = "com.supersonic.unity.androidbridge.AndroidBridge";

	public AndroidAgent()
	{
		Debug.Log("AndroidAgent ctr");
	}

	public void start()
	{
		Debug.Log("Android started");
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass(AndroidBridge))
		{
			_androidBridge = androidJavaClass.CallStatic<AndroidJavaObject>("getInstance", new object[0]);
		}
		_androidBridge.Call("setPluginData", "Unity", Supersonic.pluginVersion(), Supersonic.unityVersion());
		Debug.Log("Android started - ended");
	}

	public void reportAppStarted()
	{
		_androidBridge.Call("reportAppStarted");
	}

	public void onResume()
	{
		_androidBridge.Call("onResume");
	}

	public void onPause()
	{
		_androidBridge.Call("onPause");
	}

	public void setAge(int age)
	{
		_androidBridge.Call("setAge", age);
	}

	public void setGender(string gender)
	{
		_androidBridge.Call("setGender", gender);
	}

	public void setMediationSegment(string segment)
	{
		_androidBridge.Call("setMediationSegment", segment);
	}

	public void initRewardedVideo(string appKey, string userId)
	{
		_androidBridge.Call("initRewardedVideo", appKey, userId);
	}

	public void showRewardedVideo()
	{
		_androidBridge.Call("showRewardedVideo");
	}

	public void showRewardedVideo(string placementName)
	{
		_androidBridge.Call("showRewardedVideo", placementName);
	}

	public bool isRewardedVideoAvailable()
	{
		return _androidBridge.Call<bool>("isRewardedVideoAvailable", new object[0]);
	}

	public bool isRewardedVideoPlacementCapped(string placementName)
	{
		return _androidBridge.Call<bool>("isRewardedVideoPlacementCapped", new object[1] { placementName });
	}

	public string getAdvertiserId()
	{
		return _androidBridge.Call<string>("getAdvertiserId", new object[0]);
	}

	public void shouldTrackNetworkState(bool track)
	{
		_androidBridge.Call("shouldTrackNetworkState", track);
	}

	public void validateIntegration()
	{
		_androidBridge.Call("validateIntegration");
	}

	public SupersonicPlacement getPlacementInfo(string placementName)
	{
		string text = _androidBridge.Call<string>("getPlacementInfo", new object[1] { placementName });
		SupersonicPlacement result = null;
		if (text != null)
		{
			Dictionary<string, object> dictionary = Json.Deserialize(text) as Dictionary<string, object>;
			string pName = dictionary["placement_name"].ToString();
			string rName = dictionary["reward_name"].ToString();
			int rAmount = Convert.ToInt32(dictionary["reward_amount"].ToString());
			result = new SupersonicPlacement(pName, rName, rAmount);
		}
		return result;
	}

	public void initInterstitial(string appKey, string userId)
	{
		_androidBridge.Call("initInterstitial", appKey, userId);
	}

	public void loadInterstitial()
	{
		_androidBridge.Call("loadInterstitial");
	}

	public void showInterstitial()
	{
		_androidBridge.Call("showInterstitial");
	}

	public void showInterstitial(string placementName)
	{
		_androidBridge.Call("showInterstitial", placementName);
	}

	public bool isInterstitialReady()
	{
		return _androidBridge.Call<bool>("isInterstitialReady", new object[0]);
	}

	public bool isInterstitialPlacementCapped(string placementName)
	{
		return _androidBridge.Call<bool>("isInterstitialPlacementCapped", new object[1] { placementName });
	}

	public void initOfferwall(string appKey, string userId)
	{
		_androidBridge.Call("initOfferwall", appKey, userId);
	}

	public void showOfferwall()
	{
		_androidBridge.Call("showOfferwall");
	}

	public void getOfferwallCredits()
	{
		_androidBridge.Call("getOfferwallCredits");
	}

	public bool isOfferwallAvailable()
	{
		return _androidBridge.Call<bool>("isOfferwallAvailable", new object[0]);
	}
}
