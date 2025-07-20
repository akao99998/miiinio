using System.Collections;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using Kampai.Util.Graphics;
using UnityEngine;

public class GhostComponentFadeHelperObject : MonoBehaviour
{
	public GhostBuildingDisplayType ghostDisplayType;

	private float fadeTime = 0.5f;

	private float openDuration;

	private float BuildingFadeToAmt = 0.6f;

	private GoTween buildingTween;

	private MaterialModifier materialModifier;

	private Renderer[] buildingRenderers;

	private PopupMessageSignal popupMessageSignal;

	private CloseAllMessageDialogs closeAllDialogsSignal;

	private PlayGlobalSoundFXSignal sfxSignal;

	private IGhostComponentService ghostService;

	private bool autoFadeOut;

	private Coroutine isClosing;

	private IEnumerator fadeCoroutine;

	public BuildingObject buildingObject { get; set; }

	public float FadeAlpha
	{
		get
		{
			return (materialModifier == null) ? 0f : materialModifier.GetFadeAlpha();
		}
		set
		{
			if (materialModifier != null)
			{
				materialModifier.SetFadeAlpha(value);
			}
		}
	}

	public void SetupAndDisplay(IGhostComponentService ghostService, BuildingObject obj, GhostBuildingDisplayType displayType, bool fadeIn = true)
	{
		ghostDisplayType = displayType;
		this.ghostService = ghostService;
		buildingObject = obj;
		SetListeners();
		buildingRenderers = base.gameObject.GetComponentsInChildren<Renderer>();
		materialModifier = new MaterialModifier(buildingRenderers);
		if (fadeIn)
		{
			materialModifier.SetFadeAlpha(0f);
			FadeIn();
		}
		else
		{
			materialModifier.SetFadeAlpha(1f);
		}
	}

	public void SetupAndAutoFadeWithMessage(float fadeTime, float openTime, PopupMessageSignal popupMessageSignal, CloseAllMessageDialogs closeAllDialogsSignal, IGhostComponentService ghostService, GhostBuildingDisplayType displayType, BuildingObject obj)
	{
		ghostDisplayType = displayType;
		buildingObject = obj;
		this.fadeTime = fadeTime;
		openDuration = openTime;
		this.popupMessageSignal = popupMessageSignal;
		this.closeAllDialogsSignal = closeAllDialogsSignal;
		this.ghostService = ghostService;
		SetListeners();
		buildingRenderers = base.gameObject.GetComponentsInChildren<Renderer>();
		materialModifier = new MaterialModifier(buildingRenderers);
		materialModifier.SetFadeAlpha(0f);
		autoFadeOut = true;
		FadeIn();
	}

	private void SetListeners()
	{
		if (popupMessageSignal != null)
		{
			popupMessageSignal.AddListener(CloseEarly);
		}
		if (closeAllDialogsSignal != null)
		{
			closeAllDialogsSignal.AddListener(CloseDialogReceived);
		}
	}

	private void RemoveListeners()
	{
		if (popupMessageSignal != null)
		{
			popupMessageSignal.RemoveListener(CloseEarly);
		}
		if (closeAllDialogsSignal != null)
		{
			closeAllDialogsSignal.RemoveListener(CloseDialogReceived);
		}
	}

	public void TriggerFTUEDropAnimation(string controllerName, PlayGlobalSoundFXSignal soundFXSignal)
	{
		sfxSignal = soundFXSignal;
		Animator animator = base.gameObject.GetComponent<Animator>();
		if (animator == null)
		{
			animator = base.gameObject.AddComponent<Animator>();
		}
		animator.runtimeAnimatorController = KampaiResources.Load<RuntimeAnimatorController>(controllerName);
	}

	private void CloseDialogReceived()
	{
		if (autoFadeOut)
		{
			if (buildingTween != null && isClosing == null)
			{
				buildingTween.complete();
				buildingTween.destroy();
			}
			StartFadeOut(false);
		}
	}

	public void PlayFTUEFX_Drop()
	{
		if (sfxSignal != null)
		{
			sfxSignal.Dispatch("Play_componentFall_woosh_01");
		}
	}

	public void PlayFTUEFX_Hit()
	{
		if (sfxSignal != null)
		{
			sfxSignal.Dispatch("Play_componentFall_metalThud_01");
		}
	}

	public void StartFadeOut(bool immediate)
	{
		if (immediate)
		{
			CleanupBuilding();
			return;
		}
		if (isClosing != null && fadeCoroutine != null)
		{
			if ((int)openDuration == 0)
			{
				return;
			}
			StopCoroutine(fadeCoroutine);
		}
		openDuration = 0f;
		fadeCoroutine = FadeOut();
		isClosing = StartCoroutine(fadeCoroutine);
	}

	private void FadeIn()
	{
		if (buildingTween != null && buildingTween.isValid())
		{
			buildingTween.destroy();
		}
		if (materialModifier == null)
		{
			return;
		}
		buildingTween = Go.to(this, fadeTime, new GoTweenConfig().floatProp("FadeAlpha", BuildingFadeToAmt).onComplete(delegate
		{
			if (autoFadeOut)
			{
				fadeCoroutine = FadeOut();
				isClosing = StartCoroutine(fadeCoroutine);
			}
		}));
	}

	private IEnumerator FadeOut()
	{
		yield return new WaitForSeconds(openDuration);
		openDuration = 0f;
		if (buildingTween != null && buildingTween.isValid())
		{
			buildingTween.destroy();
		}
		if (materialModifier != null)
		{
			buildingTween = Go.to(this, fadeTime, new GoTweenConfig().floatProp("FadeAlpha", 0f).onComplete(delegate
			{
				CleanupBuilding();
			}));
		}
	}

	private void CloseEarly(string s, PopupMessageType messageType)
	{
		CleanupBuilding();
	}

	private void CleanupBuilding()
	{
		if (fadeCoroutine != null)
		{
			StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}
		RemoveListeners();
		if (buildingTween != null)
		{
			buildingTween.destroy();
			buildingTween = null;
		}
		if (materialModifier != null)
		{
			materialModifier.Destroy();
			materialModifier = null;
		}
		ghostService.GhostBuildingAutoRemoved(buildingObject.DefinitionID, this);
		Object.Destroy(base.gameObject);
	}
}
