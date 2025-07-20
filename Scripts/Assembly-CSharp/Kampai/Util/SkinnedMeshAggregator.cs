using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util
{
	public static class SkinnedMeshAggregator
	{
		private static Dictionary<string, Transform> bonesLookup = new Dictionary<string, Transform>(128);

		private static Stack<Transform> bonesStack = new Stack<Transform>(16);

		public static GameObject CreateAggregateObject(string name, string skeletonName, IEnumerable<string> meshNames, string targetLOD)
		{
			if (string.IsNullOrEmpty(skeletonName))
			{
				return null;
			}
			GameObject gameObject = new GameObject(name);
			AggregateSkinnedMeshes(gameObject.transform, skeletonName, meshNames, targetLOD);
			return gameObject;
		}

		private static void buildBonesLookup(Transform rootBone)
		{
			bonesStack.Push(rootBone);
			while (bonesStack.Count > 0)
			{
				Transform transform = bonesStack.Pop();
				if (!(transform == null))
				{
					bonesLookup[transform.name] = transform;
					for (int i = 0; i < transform.childCount; i++)
					{
						bonesStack.Push(transform.GetChild(i));
					}
				}
			}
		}

		public static void AggregateSkinnedMeshes(Transform parent, string skeletonName, IEnumerable<string> meshNames, string targetLOD)
		{
			GameObject original = KampaiResources.Load<GameObject>(string.Format("{0}_{1}", skeletonName, targetLOD));
			GameObject gameObject = Object.Instantiate(original);
			while (gameObject.transform.childCount > 0)
			{
				Transform child = gameObject.transform.GetChild(0);
				child.parent = parent;
				buildBonesLookup(child);
			}
			Animator animator = parent.gameObject.GetComponent<Animator>();
			if (animator == null)
			{
				animator = parent.gameObject.AddComponent<Animator>();
			}
			animator.avatar = gameObject.GetComponent<Animator>().avatar;
			AddSubModels(meshNames, parent, targetLOD);
			Object.Destroy(gameObject);
			bonesLookup.Clear();
		}

		public static void AddSubModels(IEnumerable<string> meshNames, Transform modelRoot, string targetLOD)
		{
			if (meshNames == null)
			{
				return;
			}
			foreach (string meshName in meshNames)
			{
				GameObject gameObject = Object.Instantiate(KampaiResources.Load(string.Format("{0}_{1}", meshName, targetLOD))) as GameObject;
				SkinnedMeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					AddSubModel(componentsInChildren[i], modelRoot);
				}
				Object.Destroy(gameObject);
			}
		}

		public static void AddSubModel(SkinnedMeshRenderer rendererToCopy, Transform modelRoot)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = CopyRenderer(rendererToCopy, modelRoot);
			skinnedMeshRenderer.bones = ReconstructBoneList(rendererToCopy, modelRoot);
			skinnedMeshRenderer.sharedMesh = rendererToCopy.sharedMesh;
			skinnedMeshRenderer.materials = rendererToCopy.materials;
		}

		private static SkinnedMeshRenderer CopyRenderer(SkinnedMeshRenderer rendererToCopy, Transform modelRoot)
		{
			GameObject gameObject = new GameObject(rendererToCopy.name);
			gameObject.transform.parent = modelRoot;
			gameObject.transform.ResetLocal();
			return gameObject.AddComponent<SkinnedMeshRenderer>();
		}

		private static Transform[] ReconstructBoneList(SkinnedMeshRenderer rendererToCopy, Transform modelRoot)
		{
			Transform[] array = new Transform[rendererToCopy.bones.Length];
			Transform[] bones = rendererToCopy.bones;
			for (int i = 0; i < bones.Length; i++)
			{
				Transform transform = FindBoneInHierarchy(bones[i].name, modelRoot);
				array[i] = transform;
			}
			return array;
		}

		private static Transform FindBoneInHierarchy(string boneName, Transform parentBone)
		{
			if (parentBone.name == boneName)
			{
				return parentBone;
			}
			Transform value;
			if (bonesLookup.TryGetValue(boneName, out value))
			{
				return value;
			}
			foreach (Transform item in parentBone)
			{
				value = FindBoneInHierarchy(boneName, item);
				if (value != null)
				{
					return value;
				}
			}
			return null;
		}
	}
}
