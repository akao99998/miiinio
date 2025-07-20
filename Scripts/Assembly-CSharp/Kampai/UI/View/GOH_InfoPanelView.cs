using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class GOH_InfoPanelView : KampaiView
	{
		public MinionSlotModal minionSlot;

		public ScrollableButtonView gohPanelButton;

		public GameObject selectedGroup;

		public Text minionName;

		[Header("")]
		public GameObject lockedGroup;

		public KampaiImage lockedCharacterIcon;

		public Text lockedDescription;

		[Header("")]
		public GameObject cooldownGroup;

		public Text cooldownDescription;

		public ScrollableButtonView cooldownRushButton;

		public Text cooldownRushCost;

		[Header("")]
		public GameObject availableGroup;

		public Text characterAvailDesc;

		[Header("")]
		public Text buffTypeAmount;

		public KampaiImage buffTypeIcon;

		public Text buffDuration;

		internal int prestigeDefID;

		internal bool isLocked;

		internal int cooldown;

		internal int myIndex;

		internal bool initiallySelected;

		private DummyCharacterObject dummyCharacterObject;

		private float origZPosition;

		private bool minionAtOrigPosition;

		private GOHCardClickedSignal IAmClicked;

		internal Signal rushCallBack;

		public void Init(string name, GOHCardClickedSignal gohClickedSignal)
		{
			cooldownRushButton.EnableDoubleConfirm();
			SetName(name);
			SetIAmClickedSignal(gohClickedSignal);
		}

		internal void CreateAnimatedCharacter(IFancyUIService fancyUIService)
		{
			DummyCharacterType characterType = fancyUIService.GetCharacterType(prestigeDefID);
			dummyCharacterObject = fancyUIService.CreateCharacter(characterType, DummyCharacterAnimationState.Happy, minionSlot.transform, minionSlot.VillainScale, minionSlot.VillainPositionOffset, prestigeDefID);
			origZPosition = (minionSlot.transform as RectTransform).anchoredPosition3D.z;
			minionAtOrigPosition = true;
		}

		internal void SetName(string name)
		{
			minionName.text = name;
		}

		internal void SetIAmClickedSignal(GOHCardClickedSignal signal)
		{
			IAmClicked = signal;
		}

		internal void SetBuffInfo(string currentModifier, Sprite buffMaskIcon, string duration)
		{
			buffTypeAmount.text = currentModifier;
			buffTypeIcon.maskSprite = buffMaskIcon;
			buffDuration.text = duration;
		}

		internal void SetCharacterLocked(Sprite mask, string text)
		{
			lockedGroup.SetActive(true);
			lockedCharacterIcon.maskSprite = mask;
			lockedDescription.text = text;
		}

		internal void SetCharacterInCooldown(string cooldownText, string rushCost)
		{
			cooldownGroup.SetActive(true);
			cooldownDescription.text = cooldownText;
			cooldownRushCost.text = rushCost;
		}

		internal void SetCharacterAvailable()
		{
			availableGroup.SetActive(true);
		}

		internal void SetAvailabilityText(string availText)
		{
			characterAvailDesc.text = availText;
		}

		public void RegisterClicked()
		{
			IndicateSelected(true);
			IAmClicked.Dispatch(myIndex, !isLocked && cooldown == 0);
		}

		public void IndicateSelected(bool selected)
		{
			selectedGroup.SetActive(selected);
			if (dummyCharacterObject == null)
			{
				return;
			}
			if (selected)
			{
				if (minionAtOrigPosition)
				{
					RectTransform rectTransform = minionSlot.transform as RectTransform;
					rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, rectTransform.anchoredPosition3D.z + -900f);
					minionAtOrigPosition = false;
				}
			}
			else if (!minionAtOrigPosition)
			{
				RectTransform rectTransform2 = minionSlot.transform as RectTransform;
				rectTransform2.anchoredPosition3D = new Vector3(rectTransform2.anchoredPosition3D.x, rectTransform2.anchoredPosition3D.y, origZPosition);
				minionAtOrigPosition = true;
			}
		}

		public void RushCharacterCooldown()
		{
			cooldownGroup.SetActive(false);
			cooldown = 0;
			SetCharacterAvailable();
		}

		public void DestroyDummyObject()
		{
			if (dummyCharacterObject != null)
			{
				dummyCharacterObject.RemoveCoroutine();
				Object.Destroy(dummyCharacterObject.gameObject);
			}
		}
	}
}
