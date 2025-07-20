using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class BuddyWelcomePanelView : PopupMenuView
	{
		public LocalizeView WelcomeTitle;

		public LocalizeView Name;

		public float FadeOutTime = 2f;

		public Vector3 height = new Vector3(0f, 1.1f, 0f);

		private IPositionService positionService;

		private CharacterObject characterObject;

		public bool Initialized { get; set; }

		public void SetUpInjections(IPositionService positionService)
		{
			this.positionService = positionService;
		}

		public void SetUpCharacterObject(CharacterObject characterObject)
		{
			this.characterObject = characterObject;
		}

		public void Init(string title, string name)
		{
			base.Init();
			WelcomeTitle.LocKey = title;
			Name.LocKey = name;
		}

		internal void OnUpdatePosition(PositionData positionData)
		{
			base.gameObject.transform.position = positionData.WorldPositionInUI;
			base.gameObject.transform.localPosition = VectorUtils.ZeroZ(base.gameObject.transform.localPosition);
		}

		internal void LateUpdate()
		{
			if (Initialized && !(characterObject == null))
			{
				PositionData positionData = ((!(characterObject is VillainView)) ? positionService.GetPositionData(characterObject.GetIndicatorPosition() + height) : positionService.GetPositionData(characterObject.GetIndicatorPosition() + GameConstants.UI.VILLAIN_UI_OFFSET));
				OnUpdatePosition(positionData);
			}
		}
	}
}
