using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.mediation.api;

namespace Kampai.Game.View
{
	public abstract class NamedMinionView : NamedCharacterObject, IView
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
			if (autoRegisterWithContext && !registeredWithContext)
			{
				KampaiView.BubbleToContext(this, true, false, ref currentContext);
			}
		}

		protected void Start()
		{
			if (autoRegisterWithContext && !registeredWithContext)
			{
				KampaiView.BubbleToContext(this, true, true, ref currentContext);
			}
		}

		protected void OnDestroy()
		{
			KampaiView.BubbleToContext(this, false, false, ref currentContext);
		}
	}
}
