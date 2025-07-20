using Kampai.Game;
using Kampai.Main;
using UnityEngine;

namespace Kampai.UI.View
{
	public class HelpTipMediator : AbstractGenericPopupMediator<HelpTipView>
	{
		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		public override void Initialize(GUIArguments args)
		{
			RectTransform rectTransform = args.Get<RectTransform>();
			if (rectTransform == null)
			{
				OnMenuClose();
				return;
			}
			base.soundFXSignal.Dispatch("Play_menu_popUp_02");
			HelpTipDefinition tip = args.Get<HelpTipDefinition>();
			Vector3 itemCenter = args.Get<Vector3>();
			base.view.Init(base.localizationService);
			base.view.SetTip(tip);
			base.view.SetUICanvas(glassCanvas);
			base.view.Display(itemCenter);
			base.view.gameObject.transform.parent = rectTransform.parent;
			base.view.gameObject.transform.SetAsLastSibling();
		}

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
		}
	}
}
