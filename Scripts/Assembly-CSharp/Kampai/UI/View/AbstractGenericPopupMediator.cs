using Kampai.Game;
using Kampai.Main;
using UnityEngine;

namespace Kampai.UI.View
{
	public abstract class AbstractGenericPopupMediator<T> : KampaiMediator where T : MonoBehaviour, IGenericPopupView
	{
		[Inject]
		public T view { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public DisplayItemPopupSignal popupSignal { get; set; }

		[Inject]
		public HideItemPopupSignal closeSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		public override void OnRegister()
		{
			T val = view;
			val.Init(localizationService);
			popupSignal.AddListener(ForceClose);
			closeSignal.AddListener(Close);
		}

		public override void OnRemove()
		{
			popupSignal.RemoveListener(ForceClose);
			closeSignal.RemoveListener(Close);
		}

		public override void Initialize(GUIArguments args)
		{
			soundFXSignal.Dispatch("Play_menu_popUp_02");
			ItemDefinition itemDef = args.Get<ItemDefinition>();
			Vector3 itemCenter = args.Get<Vector3>();
			Register(itemDef, itemCenter);
			RectTransform rectTransform = args.Get<RectTransform>();
			T val = view;
			val.gameObject.transform.parent = rectTransform.parent;
			T val2 = view;
			val2.gameObject.transform.SetAsLastSibling();
		}

		public virtual void Register(ItemDefinition itemDef, Vector3 itemCenter)
		{
			T val = view;
			val.Display(itemCenter);
		}

		public virtual void ForceClose(int ignore, RectTransform these, UIPopupType variables)
		{
			Close();
		}

		public virtual void Close()
		{
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			T val = view;
			val.Close(false);
		}

		public virtual void OnMenuClose()
		{
			Object.Destroy(base.gameObject);
		}
	}
}
