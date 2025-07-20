using System;
using System.Collections.Generic;
using System.Linq;
using SupersonicJSON;
using UnityEngine;

public class SupersonicEvents : MonoBehaviour
{
	private const string ERROR_CODE = "error_code";

	private const string ERROR_DESCRIPTION = "error_description";

	private static event Action _onRewardedVideoInitSuccessEvent;

	public static event Action onRewardedVideoInitSuccessEvent
	{
		add
		{
			if (SupersonicEvents._onRewardedVideoInitSuccessEvent == null || !SupersonicEvents._onRewardedVideoInitSuccessEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onRewardedVideoInitSuccessEvent = (Action)Delegate.Combine(SupersonicEvents._onRewardedVideoInitSuccessEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onRewardedVideoInitSuccessEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onRewardedVideoInitSuccessEvent = (Action)Delegate.Remove(SupersonicEvents._onRewardedVideoInitSuccessEvent, value);
			}
		}
	}

	private static event Action<SupersonicError> _onRewardedVideoInitFailEvent;

	public static event Action<SupersonicError> onRewardedVideoInitFailEvent
	{
		add
		{
			if (SupersonicEvents._onRewardedVideoInitFailEvent == null || !SupersonicEvents._onRewardedVideoInitFailEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onRewardedVideoInitFailEvent = (Action<SupersonicError>)Delegate.Combine(SupersonicEvents._onRewardedVideoInitFailEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onRewardedVideoInitFailEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onRewardedVideoInitFailEvent = (Action<SupersonicError>)Delegate.Remove(SupersonicEvents._onRewardedVideoInitFailEvent, value);
			}
		}
	}

	private static event Action _onRewardedVideoAdOpenedEvent;

	public static event Action onRewardedVideoAdOpenedEvent
	{
		add
		{
			if (SupersonicEvents._onRewardedVideoAdOpenedEvent == null || !SupersonicEvents._onRewardedVideoAdOpenedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onRewardedVideoAdOpenedEvent = (Action)Delegate.Combine(SupersonicEvents._onRewardedVideoAdOpenedEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onRewardedVideoAdOpenedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onRewardedVideoAdOpenedEvent = (Action)Delegate.Remove(SupersonicEvents._onRewardedVideoAdOpenedEvent, value);
			}
		}
	}

	private static event Action _onRewardedVideoAdClosedEvent;

	public static event Action onRewardedVideoAdClosedEvent
	{
		add
		{
			if (SupersonicEvents._onRewardedVideoAdClosedEvent == null || !SupersonicEvents._onRewardedVideoAdClosedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onRewardedVideoAdClosedEvent = (Action)Delegate.Combine(SupersonicEvents._onRewardedVideoAdClosedEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onRewardedVideoAdClosedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onRewardedVideoAdClosedEvent = (Action)Delegate.Remove(SupersonicEvents._onRewardedVideoAdClosedEvent, value);
			}
		}
	}

	private static event Action _onVideoStartEvent;

	public static event Action onVideoStartEvent
	{
		add
		{
			if (SupersonicEvents._onVideoStartEvent == null || !SupersonicEvents._onVideoStartEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onVideoStartEvent = (Action)Delegate.Combine(SupersonicEvents._onVideoStartEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onVideoStartEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onVideoStartEvent = (Action)Delegate.Remove(SupersonicEvents._onVideoStartEvent, value);
			}
		}
	}

	private static event Action _onVideoEndEvent;

	public static event Action onVideoEndEvent
	{
		add
		{
			if (SupersonicEvents._onVideoEndEvent == null || !SupersonicEvents._onVideoEndEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onVideoEndEvent = (Action)Delegate.Combine(SupersonicEvents._onVideoEndEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onVideoEndEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onVideoEndEvent = (Action)Delegate.Remove(SupersonicEvents._onVideoEndEvent, value);
			}
		}
	}

	private static event Action<SupersonicPlacement> _onRewardedVideoAdRewardedEvent;

	public static event Action<SupersonicPlacement> onRewardedVideoAdRewardedEvent
	{
		add
		{
			if (SupersonicEvents._onRewardedVideoAdRewardedEvent == null || !SupersonicEvents._onRewardedVideoAdRewardedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onRewardedVideoAdRewardedEvent = (Action<SupersonicPlacement>)Delegate.Combine(SupersonicEvents._onRewardedVideoAdRewardedEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onRewardedVideoAdRewardedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onRewardedVideoAdRewardedEvent = (Action<SupersonicPlacement>)Delegate.Remove(SupersonicEvents._onRewardedVideoAdRewardedEvent, value);
			}
		}
	}

	private static event Action<bool> _onVideoAvailabilityChangedEvent;

	public static event Action<bool> onVideoAvailabilityChangedEvent
	{
		add
		{
			if (SupersonicEvents._onVideoAvailabilityChangedEvent == null || !SupersonicEvents._onVideoAvailabilityChangedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onVideoAvailabilityChangedEvent = (Action<bool>)Delegate.Combine(SupersonicEvents._onVideoAvailabilityChangedEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onVideoAvailabilityChangedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onVideoAvailabilityChangedEvent = (Action<bool>)Delegate.Remove(SupersonicEvents._onVideoAvailabilityChangedEvent, value);
			}
		}
	}

	private static event Action _onInterstitialInitSuccessEvent;

	public static event Action onInterstitialInitSuccessEvent
	{
		add
		{
			if (SupersonicEvents._onInterstitialInitSuccessEvent == null || !SupersonicEvents._onInterstitialInitSuccessEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialInitSuccessEvent = (Action)Delegate.Combine(SupersonicEvents._onInterstitialInitSuccessEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onInterstitialInitSuccessEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialInitSuccessEvent = (Action)Delegate.Remove(SupersonicEvents._onInterstitialInitSuccessEvent, value);
			}
		}
	}

	private static event Action<SupersonicError> _onInterstitialInitFailedEvent;

	public static event Action<SupersonicError> onInterstitialInitFailedEvent
	{
		add
		{
			if (SupersonicEvents._onInterstitialInitFailedEvent == null || !SupersonicEvents._onInterstitialInitFailedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialInitFailedEvent = (Action<SupersonicError>)Delegate.Combine(SupersonicEvents._onInterstitialInitFailedEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onInterstitialInitFailedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialInitFailedEvent = (Action<SupersonicError>)Delegate.Remove(SupersonicEvents._onInterstitialInitFailedEvent, value);
			}
		}
	}

	private static event Action _onInterstitialReadyEvent;

	public static event Action onInterstitialReadyEvent
	{
		add
		{
			if (SupersonicEvents._onInterstitialReadyEvent == null || !SupersonicEvents._onInterstitialReadyEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialReadyEvent = (Action)Delegate.Combine(SupersonicEvents._onInterstitialReadyEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onInterstitialReadyEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialReadyEvent = (Action)Delegate.Remove(SupersonicEvents._onInterstitialReadyEvent, value);
			}
		}
	}

	private static event Action<SupersonicError> _onInterstitialLoadFailedEvent;

	public static event Action<SupersonicError> onInterstitialLoadFailedEvent
	{
		add
		{
			if (SupersonicEvents._onInterstitialLoadFailedEvent == null || !SupersonicEvents._onInterstitialLoadFailedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialLoadFailedEvent = (Action<SupersonicError>)Delegate.Combine(SupersonicEvents._onInterstitialLoadFailedEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onInterstitialLoadFailedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialLoadFailedEvent = (Action<SupersonicError>)Delegate.Remove(SupersonicEvents._onInterstitialLoadFailedEvent, value);
			}
		}
	}

	private static event Action _onInterstitialOpenEvent;

	public static event Action onInterstitialOpenEvent
	{
		add
		{
			if (SupersonicEvents._onInterstitialOpenEvent == null || !SupersonicEvents._onInterstitialOpenEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialOpenEvent = (Action)Delegate.Combine(SupersonicEvents._onInterstitialOpenEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onInterstitialOpenEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialOpenEvent = (Action)Delegate.Remove(SupersonicEvents._onInterstitialOpenEvent, value);
			}
		}
	}

	private static event Action _onInterstitialCloseEvent;

	public static event Action onInterstitialCloseEvent
	{
		add
		{
			if (SupersonicEvents._onInterstitialCloseEvent == null || !SupersonicEvents._onInterstitialCloseEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialCloseEvent = (Action)Delegate.Combine(SupersonicEvents._onInterstitialCloseEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onInterstitialCloseEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialCloseEvent = (Action)Delegate.Remove(SupersonicEvents._onInterstitialCloseEvent, value);
			}
		}
	}

	private static event Action _onInterstitialShowSuccessEvent;

	public static event Action onInterstitialShowSuccessEvent
	{
		add
		{
			if (SupersonicEvents._onInterstitialShowSuccessEvent == null || !SupersonicEvents._onInterstitialShowSuccessEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialShowSuccessEvent = (Action)Delegate.Combine(SupersonicEvents._onInterstitialShowSuccessEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onInterstitialShowSuccessEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialShowSuccessEvent = (Action)Delegate.Remove(SupersonicEvents._onInterstitialShowSuccessEvent, value);
			}
		}
	}

	private static event Action<SupersonicError> _onInterstitialShowFailedEvent;

	public static event Action<SupersonicError> onInterstitialShowFailedEvent
	{
		add
		{
			if (SupersonicEvents._onInterstitialShowFailedEvent == null || !SupersonicEvents._onInterstitialShowFailedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialShowFailedEvent = (Action<SupersonicError>)Delegate.Combine(SupersonicEvents._onInterstitialShowFailedEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onInterstitialShowFailedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialShowFailedEvent = (Action<SupersonicError>)Delegate.Remove(SupersonicEvents._onInterstitialShowFailedEvent, value);
			}
		}
	}

	private static event Action _onInterstitialClickEvent;

	public static event Action onInterstitialClickEvent
	{
		add
		{
			if (SupersonicEvents._onInterstitialClickEvent == null || !SupersonicEvents._onInterstitialClickEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialClickEvent = (Action)Delegate.Combine(SupersonicEvents._onInterstitialClickEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onInterstitialClickEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onInterstitialClickEvent = (Action)Delegate.Remove(SupersonicEvents._onInterstitialClickEvent, value);
			}
		}
	}

	private static event Action _onOfferwallInitSuccessEvent;

	public static event Action onOfferwallInitSuccessEvent
	{
		add
		{
			if (SupersonicEvents._onOfferwallInitSuccessEvent == null || !SupersonicEvents._onOfferwallInitSuccessEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallInitSuccessEvent = (Action)Delegate.Combine(SupersonicEvents._onOfferwallInitSuccessEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onOfferwallInitSuccessEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallInitSuccessEvent = (Action)Delegate.Remove(SupersonicEvents._onOfferwallInitSuccessEvent, value);
			}
		}
	}

	private static event Action<SupersonicError> _onOfferwallInitFailEvent;

	public static event Action<SupersonicError> onOfferwallInitFailEvent
	{
		add
		{
			if (SupersonicEvents._onOfferwallInitFailEvent == null || !SupersonicEvents._onOfferwallInitFailEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallInitFailEvent = (Action<SupersonicError>)Delegate.Combine(SupersonicEvents._onOfferwallInitFailEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onOfferwallInitFailEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallInitFailEvent = (Action<SupersonicError>)Delegate.Remove(SupersonicEvents._onOfferwallInitFailEvent, value);
			}
		}
	}

	private static event Action _onOfferwallOpenedEvent;

	public static event Action onOfferwallOpenedEvent
	{
		add
		{
			if (SupersonicEvents._onOfferwallOpenedEvent == null || !SupersonicEvents._onOfferwallOpenedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallOpenedEvent = (Action)Delegate.Combine(SupersonicEvents._onOfferwallOpenedEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onOfferwallOpenedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallOpenedEvent = (Action)Delegate.Remove(SupersonicEvents._onOfferwallOpenedEvent, value);
			}
		}
	}

	private static event Action<SupersonicError> _onOfferwallShowFailEvent;

	public static event Action<SupersonicError> onOfferwallShowFailEvent
	{
		add
		{
			if (SupersonicEvents._onOfferwallShowFailEvent == null || !SupersonicEvents._onOfferwallShowFailEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallShowFailEvent = (Action<SupersonicError>)Delegate.Combine(SupersonicEvents._onOfferwallShowFailEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onOfferwallShowFailEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallShowFailEvent = (Action<SupersonicError>)Delegate.Remove(SupersonicEvents._onOfferwallShowFailEvent, value);
			}
		}
	}

	private static event Action _onOfferwallClosedEvent;

	public static event Action onOfferwallClosedEvent
	{
		add
		{
			if (SupersonicEvents._onOfferwallClosedEvent == null || !SupersonicEvents._onOfferwallClosedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallClosedEvent = (Action)Delegate.Combine(SupersonicEvents._onOfferwallClosedEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onOfferwallClosedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallClosedEvent = (Action)Delegate.Remove(SupersonicEvents._onOfferwallClosedEvent, value);
			}
		}
	}

	private static event Action<SupersonicError> _onGetOfferwallCreditsFailEvent;

	public static event Action<SupersonicError> onGetOfferwallCreditsFailEvent
	{
		add
		{
			if (SupersonicEvents._onGetOfferwallCreditsFailEvent == null || !SupersonicEvents._onGetOfferwallCreditsFailEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onGetOfferwallCreditsFailEvent = (Action<SupersonicError>)Delegate.Combine(SupersonicEvents._onGetOfferwallCreditsFailEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onGetOfferwallCreditsFailEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onGetOfferwallCreditsFailEvent = (Action<SupersonicError>)Delegate.Remove(SupersonicEvents._onGetOfferwallCreditsFailEvent, value);
			}
		}
	}

	private static event Action<Dictionary<string, object>> _onOfferwallAdCreditedEvent;

	public static event Action<Dictionary<string, object>> onOfferwallAdCreditedEvent
	{
		add
		{
			if (SupersonicEvents._onOfferwallAdCreditedEvent == null || !SupersonicEvents._onOfferwallAdCreditedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallAdCreditedEvent = (Action<Dictionary<string, object>>)Delegate.Combine(SupersonicEvents._onOfferwallAdCreditedEvent, value);
			}
		}
		remove
		{
			if (SupersonicEvents._onOfferwallAdCreditedEvent.GetInvocationList().Contains(value))
			{
				SupersonicEvents._onOfferwallAdCreditedEvent = (Action<Dictionary<string, object>>)Delegate.Remove(SupersonicEvents._onOfferwallAdCreditedEvent, value);
			}
		}
	}

	private void Awake()
	{
		base.gameObject.name = "SupersonicEvents";
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void onRewardedVideoInitSuccess(string empty)
	{
		if (SupersonicEvents._onRewardedVideoInitSuccessEvent != null)
		{
			SupersonicEvents._onRewardedVideoInitSuccessEvent();
		}
	}

	public void onRewardedVideoInitFail(string description)
	{
		if (SupersonicEvents._onRewardedVideoInitFailEvent != null)
		{
			Debug.Log("entered onRewardedVideoInitFail 1");
			SupersonicError errorFromErrorString = getErrorFromErrorString(description);
			SupersonicEvents._onRewardedVideoInitFailEvent(errorFromErrorString);
		}
	}

	public void onRewardedVideoAdOpened(string empty)
	{
		if (SupersonicEvents._onRewardedVideoAdOpenedEvent != null)
		{
			SupersonicEvents._onRewardedVideoAdOpenedEvent();
		}
	}

	public void onRewardedVideoAdClosed(string empty)
	{
		if (SupersonicEvents._onRewardedVideoAdClosedEvent != null)
		{
			SupersonicEvents._onRewardedVideoAdClosedEvent();
		}
	}

	public void onVideoStart(string empty)
	{
		if (SupersonicEvents._onVideoStartEvent != null)
		{
			SupersonicEvents._onVideoStartEvent();
		}
	}

	public void onVideoEnd(string empty)
	{
		if (SupersonicEvents._onVideoEndEvent != null)
		{
			SupersonicEvents._onVideoEndEvent();
		}
	}

	public void onRewardedVideoAdRewarded(string description)
	{
		if (SupersonicEvents._onRewardedVideoAdRewardedEvent != null)
		{
			SupersonicPlacement placementFromString = getPlacementFromString(description);
			SupersonicEvents._onRewardedVideoAdRewardedEvent(placementFromString);
		}
	}

	public void onVideoAvailabilityChanged(string stringAvailable)
	{
		bool obj = stringAvailable == "true";
		if (SupersonicEvents._onVideoAvailabilityChangedEvent != null)
		{
			SupersonicEvents._onVideoAvailabilityChangedEvent(obj);
		}
	}

	public void onInterstitialInitSuccess(string empty)
	{
		if (SupersonicEvents._onInterstitialInitSuccessEvent != null)
		{
			SupersonicEvents._onInterstitialInitSuccessEvent();
		}
	}

	public void onInterstitialInitFailed(string description)
	{
		if (SupersonicEvents._onInterstitialInitFailedEvent != null)
		{
			SupersonicError errorFromErrorString = getErrorFromErrorString(description);
			SupersonicEvents._onInterstitialInitFailedEvent(errorFromErrorString);
		}
	}

	public void onInterstitialReady()
	{
		if (SupersonicEvents._onInterstitialReadyEvent != null)
		{
			SupersonicEvents._onInterstitialReadyEvent();
		}
	}

	public void onInterstitialLoadFailed(string description)
	{
		if (SupersonicEvents._onInterstitialLoadFailedEvent != null)
		{
			SupersonicError errorFromErrorString = getErrorFromErrorString(description);
			SupersonicEvents._onInterstitialLoadFailedEvent(errorFromErrorString);
		}
	}

	public void onInterstitialOpen(string empty)
	{
		if (SupersonicEvents._onInterstitialOpenEvent != null)
		{
			SupersonicEvents._onInterstitialOpenEvent();
		}
	}

	public void onInterstitialClose(string empty)
	{
		if (SupersonicEvents._onInterstitialCloseEvent != null)
		{
			SupersonicEvents._onInterstitialCloseEvent();
		}
	}

	public void onInterstitialShowSuccess(string empty)
	{
		if (SupersonicEvents._onInterstitialShowSuccessEvent != null)
		{
			SupersonicEvents._onInterstitialShowSuccessEvent();
		}
	}

	public void onInterstitialShowFailed(string description)
	{
		if (SupersonicEvents._onInterstitialShowFailedEvent != null)
		{
			SupersonicError errorFromErrorString = getErrorFromErrorString(description);
			SupersonicEvents._onInterstitialShowFailedEvent(errorFromErrorString);
		}
	}

	public void onInterstitialClick(string empty)
	{
		if (SupersonicEvents._onInterstitialClickEvent != null)
		{
			SupersonicEvents._onInterstitialClickEvent();
		}
	}

	public void onOfferwallInitSuccess(string empty)
	{
		if (SupersonicEvents._onOfferwallInitSuccessEvent != null)
		{
			SupersonicEvents._onOfferwallInitSuccessEvent();
		}
	}

	public void onOfferwallInitFail(string description)
	{
		if (SupersonicEvents._onOfferwallInitFailEvent != null)
		{
			SupersonicError errorFromErrorString = getErrorFromErrorString(description);
			SupersonicEvents._onOfferwallInitFailEvent(errorFromErrorString);
		}
	}

	public void onOfferwallOpened(string empty)
	{
		if (SupersonicEvents._onOfferwallOpenedEvent != null)
		{
			SupersonicEvents._onOfferwallOpenedEvent();
		}
	}

	public void onOfferwallShowFail(string description)
	{
		if (SupersonicEvents._onOfferwallShowFailEvent != null)
		{
			SupersonicError errorFromErrorString = getErrorFromErrorString(description);
			SupersonicEvents._onOfferwallShowFailEvent(errorFromErrorString);
		}
	}

	public void onOfferwallClosed(string empty)
	{
		if (SupersonicEvents._onOfferwallClosedEvent != null)
		{
			SupersonicEvents._onOfferwallClosedEvent();
		}
	}

	public void onGetOfferwallCreditsFail(string description)
	{
		if (SupersonicEvents._onGetOfferwallCreditsFailEvent != null)
		{
			SupersonicError errorFromErrorString = getErrorFromErrorString(description);
			SupersonicEvents._onGetOfferwallCreditsFailEvent(errorFromErrorString);
		}
	}

	public void onOfferwallAdCredited(string json)
	{
		if (SupersonicEvents._onOfferwallAdCreditedEvent != null)
		{
			SupersonicEvents._onOfferwallAdCreditedEvent(Json.Deserialize(json) as Dictionary<string, object>);
		}
	}

	public SupersonicError getErrorFromErrorString(string description)
	{
		if (!string.IsNullOrEmpty(description))
		{
			Dictionary<string, object> dictionary = Json.Deserialize(description) as Dictionary<string, object>;
			if (dictionary != null)
			{
				int errCode = Convert.ToInt32(dictionary["error_code"].ToString());
				string errDescription = dictionary["error_description"].ToString();
				return new SupersonicError(errCode, errDescription);
			}
			return new SupersonicError(-1, string.Empty);
		}
		return new SupersonicError(-1, string.Empty);
	}

	public SupersonicPlacement getPlacementFromString(string jsonPlacement)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(jsonPlacement) as Dictionary<string, object>;
		int rAmount = Convert.ToInt32(dictionary["placement_reward_amount"].ToString());
		string rName = dictionary["placement_reward_name"].ToString();
		string pName = dictionary["placement_name"].ToString();
		return new SupersonicPlacement(pName, rName, rAmount);
	}
}
