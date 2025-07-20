using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SimpleJSON;

public class NimbleBridge_MTXCatalogItem : SafeHandle
{
	public enum Type
	{
		UNKNOWN = 0,
		NONCONSUMABLE = 1,
		CONSUMABLE = 2
	}

	public override bool IsInvalid
	{
		get
		{
			return base.IsClosed || handle == IntPtr.Zero;
		}
	}

	private NimbleBridge_MTXCatalogItem()
		: base(IntPtr.Zero, true)
	{
	}

	internal NimbleBridge_MTXCatalogItem(IntPtr handle)
		: base(IntPtr.Zero, true)
	{
		SetHandle(handle);
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_MTXCatalogItem_Dispose(NimbleBridge_MTXCatalogItem wrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_MTXCatalogItem_getSku(NimbleBridge_MTXCatalogItem wrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_MTXCatalogItem_getTitle(NimbleBridge_MTXCatalogItem wrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_MTXCatalogItem_getDescription(NimbleBridge_MTXCatalogItem wrapper);

	[DllImport("NimbleCInterface")]
	private static extern float NimbleBridge_MTXCatalogItem_getPriceDecimal(NimbleBridge_MTXCatalogItem wrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_MTXCatalogItem_getPriceWithCurrencyAndFormat(NimbleBridge_MTXCatalogItem wrapper);

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_MTXCatalogItem_getItemType(NimbleBridge_MTXCatalogItem wrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_MTXCatalogItem_getMetaDataUrl(NimbleBridge_MTXCatalogItem wrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_MTXCatalogItem_getAdditionalInfo(NimbleBridge_MTXCatalogItem wrapper);

	protected override bool ReleaseHandle()
	{
		NimbleBridge_MTXCatalogItem_Dispose(this);
		return true;
	}

	public string GetSku()
	{
		return NimbleBridge_MTXCatalogItem_getSku(this);
	}

	public string GetTitle()
	{
		return NimbleBridge_MTXCatalogItem_getTitle(this);
	}

	public string GetDescription()
	{
		return NimbleBridge_MTXCatalogItem_getDescription(this);
	}

	public float GetPriceDecimal()
	{
		return NimbleBridge_MTXCatalogItem_getPriceDecimal(this);
	}

	public string GetPriceWithCurrencyAndFormat()
	{
		return NimbleBridge_MTXCatalogItem_getPriceWithCurrencyAndFormat(this);
	}

	public Type GetItemType()
	{
		return (Type)NimbleBridge_MTXCatalogItem_getItemType(this);
	}

	public string GetMetaDataUrl()
	{
		return NimbleBridge_MTXCatalogItem_getMetaDataUrl(this);
	}

	[Obsolete("Use GetAdditionalInfoDictionary instead")]
	public string GetAdditionalInfo()
	{
		return NimbleBridge_MTXCatalogItem_getAdditionalInfo(this);
	}

	public Dictionary<string, object> GetAdditionalInfoDictionary()
	{
		JSONNode jSONNode = JSON.Parse(NimbleBridge_MTXCatalogItem_getAdditionalInfo(this));
		return MarshalUtility.ConvertJsonToDictionary((JSONClass)jSONNode);
	}
}
