using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	internal sealed class CoordinatedAnimation : MonoBehaviour, Identifiable
	{
		private AnimationDefinition Def;

		private IKampaiLogger logger;

		private Transform[] routes;

		private GameObject GO;

		private Object src;

		private VFXScript vfxScript;

		public int ID
		{
			get
			{
				return Def.ID;
			}
			set
			{
			}
		}

		internal void Init(PartyFavorAnimationDefinition definition, Transform parent, Vector3 centerPoint, Vector3 lookPoint, IKampaiLogger log)
		{
			Def = definition;
			Init(definition.Prefab, 1, parent, centerPoint, lookPoint, log);
		}

		internal void Init(GachaAnimationDefinition definition, Transform parent, Vector3 centerPoint, Vector3 lookPoint, IKampaiLogger log)
		{
			Def = definition;
			Init(definition.Prefab, definition.Minions, parent, centerPoint, lookPoint, log);
		}

		private void Init(string prefab, int count, Transform parent, Vector3 centerPoint, Vector3 lookPoint, IKampaiLogger log)
		{
			logger = log;
			src = KampaiResources.Load(prefab);
			if (src == null)
			{
				logger.Fatal(FatalCode.AN_UNABLE_TO_LOAD_PREFAB);
			}
			GO = Object.Instantiate(src) as GameObject;
			if (GO == null)
			{
				logger.Fatal(FatalCode.AN_UNABLE_TO_LOAD_PREFAB);
			}
			Transform transform = GO.transform;
			transform.parent = parent;
			transform.position = centerPoint;
			transform.LookAt(lookPoint);
			routes = new Transform[count];
			for (int i = 0; i < count; i++)
			{
				Transform transform2 = transform.Find("route" + i);
				if (transform2 == null)
				{
					logger.Fatal(FatalCode.AN_NO_SUCH_ROUTE, "Minion {0} in coordinated animation {1}", i, ID);
				}
				routes[i] = transform2;
			}
			vfxScript = GO.GetComponent<VFXScript>();
			if (vfxScript != null)
			{
				vfxScript.Init();
			}
		}

		public VFXScript GetVFXScript()
		{
			return vfxScript;
		}

		public Transform[] GetRoutingSlots()
		{
			return routes;
		}

		public void OnDestroy()
		{
			if (GO != null)
			{
				Object.Destroy(GO);
			}
		}
	}
}
