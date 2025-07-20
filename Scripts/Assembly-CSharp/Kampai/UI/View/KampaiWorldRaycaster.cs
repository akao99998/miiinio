using System;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	[RequireComponent(typeof(Canvas))]
	public class KampaiWorldRaycaster : GraphicRaycaster, IView
	{
		private bool _requiresContext = true;

		protected bool registerWithContext = true;

		[Obsolete("Max still needs this for fixing latest version of Unity")]
		public override int priority
		{
			get
			{
				return 5;
			}
		}

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

		protected override void Awake()
		{
			Register();
		}

		protected override void Start()
		{
			Register();
		}

		protected override void OnDestroy()
		{
			bubbleToContext(this, false, false);
		}

		protected virtual void bubbleToContext(MonoBehaviour view, bool toAdd, bool finalTry)
		{
			int num = 0;
			Transform parent = view.gameObject.transform;
			while (parent.parent != null && num < 100)
			{
				num++;
				parent = parent.parent;
				GameObject key = parent.gameObject;
				IContext value;
				if (Context.knownContexts.TryGetValue(key, out value))
				{
					if (toAdd)
					{
						value.AddView(view);
						registeredWithContext = true;
					}
					else
					{
						value.RemoveView(view);
					}
					return;
				}
			}
			if (requiresContext && finalTry)
			{
				if (Context.firstContext == null)
				{
					string text = ((num != 100) ? "A view was added with no context. Views must be added into the hierarchy of their ContextView lest all hell break loose." : "A view couldn't find a context. Loop limit reached.");
					text = text + "\nView: " + view.ToString();
					throw new MediationException(text, MediationExceptionType.NO_CONTEXT);
				}
				Context.firstContext.AddView(view);
				registeredWithContext = true;
			}
		}

		private void Register()
		{
			if (autoRegisterWithContext && !registeredWithContext)
			{
				bubbleToContext(this, true, true);
			}
		}
	}
}
