using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;

public static class DooberUtil
{
	public static void CheckForTween(TransactionDefinition transactionDef, List<GameObject> items, bool allowTweenToStorage, Camera uiCamera, SpawnDooberSignal tweenSignal, IDefinitionService definitionService)
	{
		int outputCount = transactionDef.GetOutputCount();
		int count = items.Count;
		int num = ((outputCount > count) ? count : outputCount);
		for (int i = 0; i < num; i++)
		{
			QuantityItem quantityItem = transactionDef.Outputs[i];
			RectTransform transform = items[i].transform as RectTransform;
			DetermineTweenToUse(quantityItem.ID, transform, allowTweenToStorage, uiCamera, tweenSignal, definitionService);
		}
	}

	public static void CheckForTween(TransactionDefinition transactionDef, List<KampaiImage> items, bool allowTweenToStorage, Camera uiCamera, SpawnDooberSignal tweenSignal, IDefinitionService definitionService)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < items.Count; i++)
		{
			list.Add(items[i].gameObject);
		}
		CheckForTween(transactionDef, list, allowTweenToStorage, uiCamera, tweenSignal, definitionService);
	}

	public static void CheckForTween(TransactionDefinition transactionDef, RectTransform transform, bool allowTweenToStorage, Camera uiCamera, SpawnDooberSignal tweenSignal, IDefinitionService definitionService, bool staggerTweens = false, IRoutineRunner routineRunner = null)
	{
		if (staggerTweens && routineRunner != null)
		{
			routineRunner.StartCoroutine(StaggerSingleTransformTweens(transactionDef, transform, allowTweenToStorage, uiCamera, tweenSignal, definitionService));
			return;
		}
		for (int i = 0; i < transactionDef.Outputs.Count; i++)
		{
			QuantityItem quantityItem = transactionDef.Outputs[i];
			DetermineTweenToUse(quantityItem.ID, transform, allowTweenToStorage, uiCamera, tweenSignal, definitionService);
		}
	}

	private static IEnumerator StaggerSingleTransformTweens(TransactionDefinition transactionDef, RectTransform transform, bool allowTweenToStorage, Camera uiCamera, SpawnDooberSignal tweenSignal, IDefinitionService definitionService)
	{
		for (int i = 0; i < transactionDef.Outputs.Count; i++)
		{
			if (transform == null)
			{
				break;
			}
			QuantityItem output = transactionDef.Outputs[i];
			DetermineTweenToUse(output.ID, transform, allowTweenToStorage, uiCamera, tweenSignal, definitionService);
			yield return new WaitForSeconds(0.5f);
		}
	}

	public static void CheckForTween(List<RewardSliderView> views, bool allowTweenToStorage, Camera uiCamera, SpawnDooberSignal tweenSignal, IDefinitionService definitionService)
	{
		for (int i = 0; i < views.Count; i++)
		{
			RectTransform transform = views[i].icon.transform as RectTransform;
			DetermineTweenToUse(views[i].ID, transform, allowTweenToStorage, uiCamera, tweenSignal, definitionService);
		}
	}

	private static void DetermineTweenToUse(int id, RectTransform transform, bool allowTweenToStorage, Camera uiCamera, SpawnDooberSignal tweenSignal, IDefinitionService definitionService)
	{
		DestinationType destinationType = GetDestinationType(id, definitionService);
		if (allowTweenToStorage || destinationType != DestinationType.STORAGE)
		{
			tweenSignal.Dispatch(uiCamera.WorldToScreenPoint(transform.position), destinationType, id, false);
		}
	}

	public static DestinationType GetDestinationType(int definitionID, IDefinitionService definitionService)
	{
		Definition definition = definitionService.Get<Definition>(definitionID);
		switch (definitionID)
		{
		case 0:
			return DestinationType.GRIND;
		case 1:
			return DestinationType.PREMIUM;
		case 2:
			return DestinationType.XP;
		case 5:
			return DestinationType.MINIONS;
		case 50:
			return DestinationType.MINION_LEVEL_TOKEN;
		default:
			if (definition is StickerDefinition)
			{
				return DestinationType.STICKER;
			}
			if (definition is BuffDefinition)
			{
				return DestinationType.BUFF;
			}
			if (definition is BuildingDefinition)
			{
				return DestinationType.STORE;
			}
			return DestinationType.STORAGE;
		}
	}
}
