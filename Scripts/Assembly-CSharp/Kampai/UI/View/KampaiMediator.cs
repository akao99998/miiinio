using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public abstract class KampaiMediator : Mediator
	{
		public string PrefabName;

		public string guiLabel;

		public virtual void Initialize(GUIArguments args)
		{
		}

		public virtual void SetActive(bool isActive)
		{
			base.gameObject.SetActive(isActive);
		}

		protected virtual void Update()
		{
		}
	}
}
