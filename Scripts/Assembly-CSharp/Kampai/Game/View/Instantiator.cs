using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.impl;
using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class Instantiator : strange.extensions.mediation.impl.View
	{
		public enum InstatiationEvent
		{
			OnAwake = 0,
			OnStart = 1
		}

		private IKampaiLogger logger = LogManager.GetClassLogger("Instantiator") as IKampaiLogger;

		public InstatiationEvent instatiationEvent;

		public string PrefabName = string.Empty;

		private bool isInstantiated;

		private static GameObject gameRoot;

		protected override void Awake()
		{
			RegisterWithContext();
			if (!string.IsNullOrEmpty(PrefabName) && instatiationEvent == InstatiationEvent.OnAwake)
			{
				Instantiate();
			}
		}

		protected override void Start()
		{
			if (!string.IsNullOrEmpty(PrefabName) && instatiationEvent == InstatiationEvent.OnStart)
			{
				Instantiate();
			}
		}

		private void Instantiate()
		{
			if (isInstantiated)
			{
				return;
			}
			isInstantiated = true;
			GameObject gameObject = KampaiResources.Load<GameObject>(PrefabName);
			if (gameObject == null)
			{
				if (logger != null)
				{
					logger.Error("Unable to load: {0}", PrefabName);
				}
				return;
			}
			GameObject gameObject2 = Object.Instantiate(gameObject);
			if (gameObject2 != null)
			{
				gameObject2.transform.parent = base.transform;
				gameObject2.transform.localPosition = Vector3.zero;
			}
		}

		private void RegisterWithContext()
		{
			if (!registeredWithContext)
			{
				ContextView component = GetGameRoot().GetComponent<ContextView>();
				if (component == null)
				{
					throw new MediationException("Game Root missing ContextView", MediationExceptionType.NO_CONTEXT);
				}
				if (component.context != null)
				{
					component.context.AddView(this);
				}
			}
		}

		private static GameObject GetGameRoot()
		{
			if (gameRoot != null)
			{
				return gameRoot;
			}
			gameRoot = GameObject.Find("/Game Root");
			if (gameRoot == null)
			{
				throw new MediationException("Could not find Game Root", MediationExceptionType.NO_CONTEXT);
			}
			return gameRoot;
		}
	}
}
