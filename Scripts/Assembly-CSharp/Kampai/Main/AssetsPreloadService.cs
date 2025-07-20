using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Main
{
	internal sealed class AssetsPreloadService : IAssetsPreloadService
	{
		private readonly Dictionary<string, Type> KNOWN_TYPES = new Dictionary<string, Type>
		{
			{
				"UnityEngine.Camera",
				typeof(Camera)
			},
			{
				"UnityEngine.AnimationClip",
				typeof(AnimationClip)
			},
			{
				"UnityEngine.ParticleSystem",
				typeof(ParticleSystem)
			},
			{
				"UnityEngine.ParticleSystemRenderer",
				typeof(ParticleSystemRenderer)
			},
			{
				"UnityEngine.Mesh",
				typeof(Mesh)
			},
			{
				"UnityEngine.Avatar",
				typeof(Avatar)
			},
			{
				"UnityEngine.Animator",
				typeof(Animator)
			},
			{
				"UnityEngine.SkinnedMeshRenderer",
				typeof(SkinnedMeshRenderer)
			},
			{
				"UnityEngine.Shader",
				typeof(Shader)
			},
			{
				"UnityEngine.LODGroup",
				typeof(LODGroup)
			},
			{
				"UnityEngine.MeshRenderer",
				typeof(MeshRenderer)
			},
			{
				"UnityEngine.MeshFilter",
				typeof(MeshFilter)
			},
			{
				"UnityEngine.BoxCollider",
				typeof(BoxCollider)
			},
			{
				"UnityEngine.SphereCollider",
				typeof(SphereCollider)
			},
			{
				"UnityEngine.TextAsset",
				typeof(TextAsset)
			},
			{
				"UnityEngine.AnimatorOverrideController",
				typeof(AnimatorOverrideController)
			},
			{
				"UnityEngine.MeshCollider",
				typeof(MeshCollider)
			},
			{
				"UnityEngine.Rigidbody",
				typeof(Rigidbody)
			},
			{
				"UnityEngine.LineRenderer",
				typeof(LineRenderer)
			},
			{
				"UnityEngine.CapsuleCollider",
				typeof(CapsuleCollider)
			},
			{
				"UnityEngine.TrailRenderer",
				typeof(TrailRenderer)
			},
			{
				"UnityEngine.Animation",
				typeof(Animation)
			},
			{
				"UnityEngine.MonoBehaviour",
				typeof(MonoBehaviour)
			},
			{
				"UnityEngine.GameObject",
				typeof(GameObject)
			},
			{
				"UnityEngine.Transform",
				typeof(Transform)
			},
			{
				"UnityEngine.RectTransform",
				typeof(RectTransform)
			},
			{
				"UnityEngine.CanvasRenderer",
				typeof(CanvasRenderer)
			},
			{
				"UnityEngine.Font",
				typeof(Font)
			},
			{
				"UnityEngine.Material",
				typeof(Material)
			},
			{
				"UnityEngine.Texture2D",
				typeof(Texture2D)
			},
			{
				"UnityEngine.Sprite",
				typeof(Sprite)
			},
			{
				"UnityEngine.Object",
				typeof(UnityEngine.Object)
			},
			{
				"UnityEngine.RuntimeAnimatorController",
				typeof(RuntimeAnimatorController)
			}
		};

		private List<PreloadableAsset> assetsQueue = new List<PreloadableAsset>(512);

		private IEnumerator integrationCoroutine;

		private int integrationStepLength = 150;

		private IKampaiLogger logger = LogManager.GetClassLogger("AssetsPreloadService") as IKampaiLogger;

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		public void AddAssetToPreloadQueue(PreloadableAsset asset)
		{
			assetsQueue.Add(asset);
			if (integrationCoroutine == null)
			{
				integrationCoroutine = IntegratePreloadQueue();
				routineRunner.StartCoroutine(integrationCoroutine);
			}
		}

		public void PreloadAllAssets()
		{
			TextAsset textAsset = KampaiResources.Load<TextAsset>("PreloadedAssetsList");
			if (textAsset == null)
			{
				logger.Info("Assets preload list was not found. This message is harmful only to the load time.");
				return;
			}
			try
			{
				using (MemoryStream stream = new MemoryStream(textAsset.bytes))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						using (JsonTextReader reader2 = new JsonTextReader(reader))
						{
							AssetsPreloadList assetsPreloadList = FastJSONDeserializer.Deserialize<AssetsPreloadList>(reader2);
							List<PreloadableAsset> assetsList = assetsPreloadList.AssetsList;
							for (int num = assetsList.Count - 1; num >= 0; num--)
							{
								AddAssetToPreloadQueue(assetsList[num]);
							}
						}
					}
				}
			}
			catch (JsonSerializationException ex)
			{
				logger.Error("AssetsPreloadList Json Parse Err: {0}", ex);
			}
			catch (JsonReaderException ex2)
			{
				logger.Error("AssetsPreloadList Json Read Err: {0}", ex2);
			}
			catch (Exception ex3)
			{
				logger.Error("AssetsPreloadList load error: {0}", ex3);
			}
		}

		public void StopAssetsPreload()
		{
			if (integrationCoroutine != null)
			{
				assetsQueue.Clear();
				routineRunner.StopCoroutine(integrationCoroutine);
				integrationCoroutine = null;
				logger.Info("Preload has stopped");
			}
		}

		public void SetIntegrationStepLength(int msec)
		{
			integrationStepLength = msec;
		}

		private IEnumerator IntegratePreloadQueue()
		{
			yield return null;
			Stopwatch sw = Stopwatch.StartNew();
			while (assetsQueue.Count > 0)
			{
				if (sw.ElapsedMilliseconds > integrationStepLength)
				{
					yield return null;
					if (assetsQueue.Count == 0)
					{
						integrationCoroutine = null;
						yield break;
					}
					sw.Reset();
					sw.Start();
				}
				int idx = assetsQueue.Count - 1;
				PreloadableAsset assetInfo = assetsQueue[idx];
				assetsQueue.RemoveAt(idx);
				Type assetType;
				KNOWN_TYPES.TryGetValue(assetInfo.type, out assetType);
				if (assetType != null)
				{
					KampaiResources.Load(assetInfo.name, assetType);
					logger.Info("Preload asset '{0}'", assetInfo.name);
				}
				else
				{
					logger.Info("Failed to preload asset '{0}': type '{1}' is unknown", assetInfo.name, assetInfo.type);
				}
			}
			integrationCoroutine = null;
		}
	}
}
