using System.Collections.Generic;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;

namespace Kampai.Util
{
	public class KampaiView : MonoBehaviour, IView
	{
		private static Dictionary<int, IContext> s_contextCache = new Dictionary<int, IContext>();

		protected IContext currentContext;

		private bool m_requiresContext = true;

		protected bool registerWithContext = true;

		public bool requiresContext
		{
			get
			{
				return m_requiresContext;
			}
			set
			{
				m_requiresContext = value;
			}
		}

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

		public bool registeredWithContext { get; set; }

		public static void BubbleToContextOnAwake<T>(T view, ref IContext currentContext, bool addIfFound = false) where T : MonoBehaviour, IView
		{
			if (!(view == null) && view.autoRegisterWithContext && !view.registeredWithContext)
			{
				BubbleToContext(view, true, addIfFound, ref currentContext);
			}
		}

		public static void BubbleToContextOnStart<T>(T view, ref IContext currentContext) where T : MonoBehaviour, IView
		{
			if (!(view == null) && view.autoRegisterWithContext && !view.registeredWithContext)
			{
				BubbleToContext(view, true, true, ref currentContext);
			}
		}

		public static void BubbleToContextOnDestroy<T>(T view, ref IContext currentContext) where T : MonoBehaviour, IView
		{
			BubbleToContext(view, false, false, ref currentContext);
		}

		public static void ClearContextCache()
		{
			s_contextCache.Clear();
			s_contextCache = new Dictionary<int, IContext>();
		}

		protected virtual void Awake()
		{
			BubbleToContextOnAwake(this, ref currentContext);
		}

		protected virtual void Start()
		{
			BubbleToContextOnStart(this, ref currentContext);
		}

		protected virtual void OnDestroy()
		{
			BubbleToContextOnDestroy(this, ref currentContext);
		}

		public static void BubbleToContext<T>(T view, bool toAdd, bool finalTry, ref IContext currentContext) where T : MonoBehaviour, IView
		{
			if (currentContext != null)
			{
				AttachViewToContext(view, currentContext, toAdd, ref currentContext);
			}
			else
			{
				FindViewContext(view, toAdd, finalTry, ref currentContext);
			}
		}

		protected static void AttachViewToContext<T>(T view, IContext context, bool toAdd, ref IContext currentContext) where T : MonoBehaviour, IView
		{
			if (toAdd)
			{
				context.AddView(view);
				currentContext = context;
				view.registeredWithContext = true;
			}
			else
			{
				context.RemoveView(view);
			}
		}

		protected static int BubbleUpToContext<T>(T view, bool toAdd, GameObject viewGameObject, ref IContext currentContext) where T : MonoBehaviour, IView
		{
			int result = 0;
			Transform parent = viewGameObject.transform;
			while (parent.parent != null && result++ < 100)
			{
				parent = parent.parent;
				GameObject gameObject = parent.gameObject;
				IContext value;
				if (!Context.knownContexts.TryGetValue(gameObject, out value) && !s_contextCache.TryGetValue(gameObject.layer, out value))
				{
					continue;
				}
				AttachViewToContext(view, value, toAdd, ref currentContext);
				return -1;
			}
			return result;
		}

		protected static void FindViewContext<T>(T view, bool toAdd, bool finalTry, ref IContext currentContext) where T : MonoBehaviour, IView
		{
			GameObject gameObject = view.gameObject;
			int layer = gameObject.layer;
			if (s_contextCache.ContainsKey(layer))
			{
				currentContext = s_contextCache[layer];
				if (!toAdd || finalTry)
				{
					AttachViewToContext(view, currentContext, toAdd, ref currentContext);
				}
			}
			else if (!toAdd || finalTry)
			{
				int num = BubbleUpToContext(view, toAdd, gameObject, ref currentContext);
				if (layer != 0 && currentContext != null && !s_contextCache.ContainsKey(layer))
				{
					s_contextCache.Add(layer, currentContext);
				}
				if (view.requiresContext && finalTry && num != -1)
				{
					FinalTryToGetContext(view, num, ref currentContext);
				}
			}
		}

		protected static void FinalTryToGetContext<T>(T view, int numOfTries, ref IContext currentContext) where T : MonoBehaviour, IView
		{
			if (Context.firstContext != null)
			{
				AttachViewToContext(view, Context.firstContext, true, ref currentContext);
				return;
			}
			string arg = ((numOfTries == 100) ? "A view couldn't find a context. Loop limit reached." : "A view was added with no context. Views must be added into the hierarchy of their ContextView lest all hell break loose.");
			throw new MediationException(string.Format("{0}\nView: {1}", arg, view), MediationExceptionType.NO_CONTEXT);
		}
	}
}
