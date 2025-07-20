using System;
using UnityEngine;

namespace Swrve.Device
{
	public class DeviceCarrierInfo : ICarrierInfo
	{
		private AndroidJavaObject androidTelephonyManager;

		public DeviceCarrierInfo()
		{
			try
			{
				using (AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("android.content.Context"))
				{
					using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
					{
						using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
						{
							string @static = androidJavaClass2.GetStatic<string>("TELEPHONY_SERVICE");
							androidTelephonyManager = androidJavaObject.Call<AndroidJavaObject>("getSystemService", new object[1] { @static });
						}
					}
				}
			}
			catch (Exception ex)
			{
				SwrveLog.LogWarning("Couldn't get access to TelephonyManager: " + ex.ToString());
			}
		}

		private string AndroidGetTelephonyManagerAttribute(string method)
		{
			if (androidTelephonyManager != null)
			{
				try
				{
					return androidTelephonyManager.Call<string>(method, new object[0]);
				}
				catch (Exception ex)
				{
					SwrveLog.LogWarning("Problem accessing the TelephonyManager - " + method + ": " + ex.ToString());
				}
			}
			return null;
		}

		public string GetName()
		{
			return AndroidGetTelephonyManagerAttribute("getSimOperatorName");
		}

		public string GetIsoCountryCode()
		{
			return AndroidGetTelephonyManagerAttribute("getSimCountryIso");
		}

		public string GetCarrierCode()
		{
			return AndroidGetTelephonyManagerAttribute("getSimOperator");
		}
	}
}
