using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class GUIService : IGUIService
	{
		private sealed class GUICommand : IGUICommand
		{
			public GUIOperation operation { get; set; }

			public GUIPriority priority { get; private set; }

			public string prefab { get; private set; }

			public bool WorldCanvas { get; set; }

			public GUIArguments Args { get; set; }

			public string GUILabel { get; set; }

			public string skrimScreen { get; set; }

			public bool darkSkrim { get; set; }

			public float alphaAmt { get; set; }

			public SkrimBehavior skrimBehavior { get; set; }

			public bool disableSkrimButton { get; set; }

			public bool singleSkrimClose { get; set; }

			public bool genericPopupSkrim { get; set; }

			public ShouldShowPredicateDelegate ShouldShowPredicate { get; set; }

			public GUICommand(GUIOperation operation, string prefab, IKampaiLogger logger, string guiLabel)
			{
				this.operation = operation;
				priority = GUIPriority.Lowest;
				this.prefab = prefab;
				WorldCanvas = false;
				Args = new GUIArguments(logger);
				GUILabel = guiLabel;
				disableSkrimButton = false;
				singleSkrimClose = false;
				genericPopupSkrim = false;
			}

			public GUICommand(GUIOperation operation, GUIPriority priority, string prefab, IKampaiLogger logger)
			{
				this.operation = operation;
				this.priority = priority;
				this.prefab = prefab;
				WorldCanvas = false;
				Args = new GUIArguments(logger);
				GUILabel = prefab;
				disableSkrimButton = false;
				singleSkrimClose = false;
				genericPopupSkrim = false;
			}
		}

		public IKampaiLogger logger = LogManager.GetClassLogger("GUIService") as IKampaiLogger;

		private GameObject lastActiveInstance;

		private Dictionary<string, GameObject> instances = new Dictionary<string, GameObject>();

		private readonly Dictionary<string, AsyncOperation> asyncOperations = new Dictionary<string, AsyncOperation>();

		private Queue<IGUICommand> priorityQueue = new Queue<IGUICommand>();

		private GUIArguments overrides;

		[Inject(MainElement.UI_WORLDCANVAS)]
		public GameObject worldCanvas { get; set; }

		[Inject(MainElement.UI_GLASSCANVAS)]
		public GameObject glassCanvas { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public GUIServiceQueueEmptySignal guiServiceQueueEmptySignal { get; set; }

		public IGUICommand BuildCommand(GUIOperation operation, string prefab)
		{
			return new GUICommand(operation, prefab, logger, prefab);
		}

		public IGUICommand BuildCommand(GUIOperation operation, string prefab, string guiLabel)
		{
			return new GUICommand(operation, prefab, logger, guiLabel);
		}

		public IGUICommand BuildCommand(GUIOperation operation, GUIPriority priority, string prefab)
		{
			return new GUICommand(operation, priority, prefab, logger);
		}

		public GameObject Execute(GUIOperation operation, string prefab)
		{
			IGUICommand command = ((IGUIService)this).BuildCommand(operation, prefab);
			return ((IGUIService)this).Execute(command);
		}

		public GameObject Execute(GUIOperation operation, GUIPriority priority, string prefab)
		{
			IGUICommand command = ((IGUIService)this).BuildCommand(operation, priority, prefab);
			return ((IGUIService)this).Execute(command);
		}

		public GameObject Execute(IGUICommand command)
		{
			if (command == null)
			{
				logger.Error("GUICommand is null");
				return null;
			}
			EnsureOverrides();
			GUIArguments args = command.Args;
			if (args != null)
			{
				args.AddArguments(overrides);
			}
			else if (overrides.Count > 0)
			{
				command.Args = overrides;
			}
			GameObject result = null;
			switch (command.operation)
			{
			case GUIOperation.LoadStatic:
				result = LoadStatic(command);
				break;
			case GUIOperation.Load:
				result = Load(command);
				break;
			case GUIOperation.LoadUntrackedInstance:
				result = CreateNewInstance(command);
				break;
			case GUIOperation.Unload:
				Unload(command);
				break;
			case GUIOperation.Queue:
				Queue(command);
				break;
			case GUIOperation.AsyncLoad:
				AsyncLoad(command);
				break;
			default:
				logger.Error("Invalid GUIOperation");
				break;
			}
			return result;
		}

		private void AsyncLoad(IGUICommand command)
		{
			CreateCommandSkrim(command, true);
			string commandPrefab = command.prefab;
			string commandPrefabLabel = command.GUILabel;
			string text = commandPrefab + ((!DeviceCapabilities.IsTablet()) ? "_Phone" : "_Tablet");
			if (KampaiResources.FileExists(text))
			{
				commandPrefab = text;
			}
			if (string.IsNullOrEmpty(commandPrefab))
			{
				logger.Error("GUISettings.Path is empty");
			}
			if (asyncOperations.ContainsKey(commandPrefabLabel))
			{
				AsyncOperation asyncOperation = asyncOperations[commandPrefabLabel];
				if (asyncOperation != null && !asyncOperation.isDone)
				{
					return;
				}
				asyncOperations.Remove(commandPrefabLabel);
			}
			asyncOperations.Add(commandPrefabLabel, KampaiResources.LoadAsync(commandPrefab, routineRunner, delegate(UnityEngine.Object prefabObj)
			{
				if (!instances.ContainsKey(commandPrefabLabel))
				{
					GameObject gameObject = InstantiateGameObject(commandPrefab, commandPrefabLabel, prefabObj, command);
					if (gameObject != null)
					{
						instances[commandPrefabLabel] = gameObject;
					}
					if (gameObject.activeInHierarchy)
					{
						lastActiveInstance = gameObject;
					}
				}
			}));
		}

		private GameObject GetCachedObject(IGUICommand command)
		{
			string prefab = command.prefab;
			string gUILabel = command.GUILabel;
			if (instances.ContainsKey(gUILabel))
			{
				GameObject gameObject = instances[gUILabel];
				if (gameObject != null)
				{
					if (gameObject.activeSelf)
					{
						routineRunner.StartCoroutine(Initialize(gameObject, command.Args, prefab, gUILabel));
						return instances[gUILabel];
					}
					instances.Remove(gUILabel);
					UnityEngine.Object.Destroy(gameObject);
				}
				else
				{
					instances.Remove(gUILabel);
				}
			}
			return null;
		}

		private GameObject LoadStatic(IGUICommand command)
		{
			string prefab = command.prefab;
			string gUILabel = command.GUILabel;
			if (string.IsNullOrEmpty(prefab))
			{
				logger.Error("GUISettings.Path is empty");
				return null;
			}
			GameObject cachedObject = GetCachedObject(command);
			if (cachedObject != null)
			{
				return cachedObject;
			}
			cachedObject = CreateNewInstance(command);
			if (cachedObject != null)
			{
				instances[gUILabel] = cachedObject;
			}
			return cachedObject;
		}

		private GameObject CreateNewInstance(IGUICommand command)
		{
			string text = command.prefab;
			string gUILabel = command.GUILabel;
			string text2 = text + ((!DeviceCapabilities.IsTablet()) ? "_Phone" : "_Tablet");
			if (KampaiResources.FileExists(text2))
			{
				text = text2;
			}
			GameObject prefab = KampaiResources.Load<GameObject>(text);
			return InstantiateGameObject(text, gUILabel, prefab, command);
		}

		private GameObject InstantiateGameObject(string commandPrefab, string commandPrefabLabel, UnityEngine.Object prefab, IGUICommand command)
		{
			if (prefab == null)
			{
				logger.Error("Invalid GUISettings.Path: {0}", commandPrefab);
				return null;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab) as GameObject;
			if (gameObject == null)
			{
				logger.Error("Unable to create instance of {0}", commandPrefab);
				return null;
			}
			SkrimView component = gameObject.GetComponent<SkrimView>();
			if (component != null)
			{
				if (command.disableSkrimButton)
				{
					component.EnableSkrimButton(false);
				}
				component.singleSkrimClose = command.singleSkrimClose;
				component.genericPopupSkrim = command.genericPopupSkrim;
			}
			else
			{
				CreateCommandSkrim(command);
			}
			if (command.WorldCanvas)
			{
				gameObject.transform.parent = worldCanvas.transform;
			}
			else
			{
				gameObject.transform.SetParent(glassCanvas.transform, false);
			}
			CanvasGroup canvasGroup = gameObject.AddComponent<CanvasGroup>();
			canvasGroup.alpha = 0f;
			routineRunner.StartCoroutine(Initialize(gameObject, command.Args, commandPrefab, commandPrefabLabel));
			return gameObject;
		}

		private void CreateCommandSkrim(IGUICommand command, bool asyncLoad = false)
		{
			if (!string.IsNullOrEmpty(command.skrimScreen))
			{
				IGUICommand iGUICommand = BuildCommand((!asyncLoad) ? GUIOperation.Load : GUIOperation.AsyncLoad, "Skrim", command.skrimScreen);
				iGUICommand.singleSkrimClose = command.singleSkrimClose;
				iGUICommand.disableSkrimButton = command.disableSkrimButton;
				iGUICommand.genericPopupSkrim = command.genericPopupSkrim;
				GUIArguments args = iGUICommand.Args;
				args.Add(command.darkSkrim);
				args.Add(command.alphaAmt);
				args.Add(command.skrimBehavior);
				Execute(iGUICommand);
			}
		}

		private GameObject Load(IGUICommand command)
		{
			CreateCommandSkrim(command);
			GameObject gameObject = LoadStatic(command);
			if (gameObject == null)
			{
				return null;
			}
			if (gameObject.activeInHierarchy)
			{
				lastActiveInstance = gameObject;
			}
			return gameObject;
		}

		private void Unload(IGUICommand command)
		{
			string gUILabel = command.GUILabel;
			if (!instances.ContainsKey(gUILabel))
			{
				logger.Error("Unable to unload instance: {0}", gUILabel);
				return;
			}
			GameObject gameObject = instances[gUILabel];
			if (gameObject == lastActiveInstance)
			{
				lastActiveInstance = null;
			}
			UnityEngine.Object.Destroy(gameObject);
			instances.Remove(gUILabel);
			gameObject = null;
			Next();
		}

		private void Queue(IGUICommand command)
		{
			if (instances.ContainsKey(command.GUILabel))
			{
				return;
			}
			foreach (IGUICommand item in priorityQueue)
			{
				if (item.GUILabel == command.GUILabel)
				{
					return;
				}
			}
			priorityQueue.Enqueue(command);
			Next();
		}

		private void Next()
		{
			if (lastActiveInstance != null)
			{
				return;
			}
			if (priorityQueue.Count == 0)
			{
				guiServiceQueueEmptySignal.Dispatch();
				return;
			}
			IGUICommand iGUICommand = priorityQueue.Dequeue();
			if (iGUICommand == null)
			{
				return;
			}
			GUIOperation operation = iGUICommand.operation;
			if (operation != GUIOperation.Queue)
			{
				logger.Error("Invalid operation on the queue: {0}", iGUICommand.operation);
				return;
			}
			if (iGUICommand.ShouldShowPredicate == null || iGUICommand.ShouldShowPredicate())
			{
				Load(iGUICommand);
			}
			Next();
		}

		private IEnumerator Initialize(GameObject instance, GUIArguments args, string prefabName, string guiLabel)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
				if (tryInitializeMediator(instance, args, prefabName, guiLabel))
				{
					break;
				}
			}
			if (!(instance != null))
			{
				yield break;
			}
			CanvasGroup canvasGroup = instance.GetComponent<CanvasGroup>();
			if (canvasGroup != null)
			{
				if (!canvasGroup.blocksRaycasts)
				{
					canvasGroup.alpha = 1f;
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(canvasGroup);
				}
			}
		}

		private bool tryInitializeMediator(GameObject instance, GUIArguments args, string prefabName, string guiLabel)
		{
			if (instance == null)
			{
				return false;
			}
			KampaiMediator component = instance.GetComponent<KampaiMediator>();
			if (component != null)
			{
				component.PrefabName = prefabName;
				component.guiLabel = guiLabel;
				component.Initialize(args);
				return true;
			}
			EventMediator component2 = instance.GetComponent<EventMediator>();
			if (component2 != null)
			{
				return true;
			}
			return false;
		}

		private void EnsureOverrides()
		{
			if (overrides == null)
			{
				overrides = new GUIArguments(logger);
			}
		}

		public void AddToArguments(object arg)
		{
			EnsureOverrides();
			overrides.Add(arg);
		}

		public void RemoveFromArguments(Type arg)
		{
			EnsureOverrides();
			overrides.Remove(arg);
		}
	}
}
