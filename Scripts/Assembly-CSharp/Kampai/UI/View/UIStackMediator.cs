using UnityEngine;
using strange.extensions.mediation.api;

namespace Kampai.UI.View
{
	public abstract class UIStackMediator<T> : KampaiMediator where T : MonoBehaviour, IView
	{
		[Inject]
		public T view { get; set; }

		[Inject]
		public UIAddedSignal uiAddedSignal { get; set; }

		[Inject]
		public UIRemovedSignal uiRemovedSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllOtherMenuSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			RegisterView();
			closeAllOtherMenuSignal.AddListener(OnCloseAllMenu);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			RemoveView();
			closeAllOtherMenuSignal.RemoveListener(OnCloseAllMenu);
		}

		protected virtual void OnEnable()
		{
			RegisterView();
		}

		protected virtual void OnDisable()
		{
			RemoveView();
		}

		protected virtual GameObject GetViewGameObject()
		{
			T val = view;
			return val.gameObject;
		}

		protected virtual void OnCloseAllMenu(GameObject exception)
		{
			T val = view;
			if (exception != val.gameObject)
			{
				Close();
			}
		}

		protected abstract void Close();

		private void RegisterView()
		{
			if (view != null)
			{
				uiAddedSignal.Dispatch(GetViewGameObject(), Close);
			}
		}

		private void RemoveView()
		{
			uiRemovedSignal.Dispatch(GetViewGameObject());
		}
	}
}
