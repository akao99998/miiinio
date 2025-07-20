using Kampai.Game.View;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.mediation.api;

namespace Kampai.Game
{
	public class MIBBuildingObjectView : BuildingObject, IView
	{
		private bool _requiresContext = true;

		protected bool registerWithContext = true;

		private IContext currentContext;

		public bool requiresContext
		{
			get
			{
				return _requiresContext;
			}
			set
			{
				_requiresContext = value;
			}
		}

		public bool registeredWithContext { get; set; }

		public virtual bool autoRegisterWithContext
		{
			get
			{
				return registerWithContext;
			}
			set
			{
				registerWithContext = value;
			}
		}

		protected void Awake()
		{
			KampaiView.BubbleToContextOnAwake(this, ref currentContext);
		}

		protected void Start()
		{
			KampaiView.BubbleToContextOnStart(this, ref currentContext);
		}

		protected void OnDestroy()
		{
			KampaiView.BubbleToContextOnDestroy(this, ref currentContext);
		}
	}
}
