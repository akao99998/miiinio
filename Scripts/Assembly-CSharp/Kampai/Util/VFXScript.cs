using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util
{
	[Serializable]
	public class VFXScript : MonoBehaviour
	{
		private const string TRIGGER_PREFIX = "trigger_";

		public List<GameObject> state1FXs;

		public List<GameObject> state2FXs;

		public List<GameObject> state3FXs;

		public List<GameObject> state4FXs;

		public List<GameObject> state5FXs;

		public List<GameObject> state6FXs;

		public List<GameObject> state7FXs;

		public List<GameObject> state8FXs;

		public List<GameObject> state9FXs;

		public List<GameObject> state10FXs;

		public List<GameObject> state11FXs;

		public List<GameObject> state12FXs;

		public string state1ID;

		public string state2ID;

		public string state3ID;

		public string state4ID;

		public string state5ID;

		public string state6ID;

		public string state7ID;

		public string state8ID;

		public string state9ID;

		public string state10ID;

		public string state11ID;

		public string state12ID;

		private Dictionary<string, IList<ParticleSystem>> states;

		public void Init()
		{
			states = new Dictionary<string, IList<ParticleSystem>>();
			if (!string.IsNullOrEmpty(state1ID))
			{
				AddParticleSystems(state1ID, state1FXs);
			}
			if (!string.IsNullOrEmpty(state2ID))
			{
				AddParticleSystems(state2ID, state2FXs);
			}
			if (!string.IsNullOrEmpty(state3ID))
			{
				AddParticleSystems(state3ID, state3FXs);
			}
			if (!string.IsNullOrEmpty(state4ID))
			{
				AddParticleSystems(state4ID, state4FXs);
			}
			if (!string.IsNullOrEmpty(state5ID))
			{
				AddParticleSystems(state5ID, state5FXs);
			}
			if (!string.IsNullOrEmpty(state6ID))
			{
				AddParticleSystems(state6ID, state6FXs);
			}
			if (!string.IsNullOrEmpty(state7ID))
			{
				AddParticleSystems(state7ID, state7FXs);
			}
			if (!string.IsNullOrEmpty(state8ID))
			{
				AddParticleSystems(state8ID, state8FXs);
			}
			if (!string.IsNullOrEmpty(state9ID))
			{
				AddParticleSystems(state9ID, state9FXs);
			}
			if (!string.IsNullOrEmpty(state10ID))
			{
				AddParticleSystems(state10ID, state10FXs);
			}
			if (!string.IsNullOrEmpty(state11ID))
			{
				AddParticleSystems(state11ID, state11FXs);
			}
			if (!string.IsNullOrEmpty(state12ID))
			{
				AddParticleSystems(state12ID, state12FXs);
			}
		}

		private void AddParticleSystems(string stateName, IEnumerable<GameObject> parents)
		{
			foreach (GameObject parent in parents)
			{
				if (!(parent != null))
				{
					continue;
				}
				ParticleSystem[] components = parent.GetComponents<ParticleSystem>();
				foreach (ParticleSystem item in components)
				{
					if (!states.ContainsKey(stateName))
					{
						states.Add(stateName, new List<ParticleSystem>());
					}
					states[stateName].Add(item);
				}
			}
		}

		public void AnimVFX(string stateName)
		{
			TriggerState(stateName);
		}

		internal void TriggerState(string stateName)
		{
			if (stateName.StartsWith("trigger_"))
			{
				stateName = stateName.Substring("trigger_".Length);
				TriggerAnimVFX(stateName);
				return;
			}
			ICollection<ParticleSystem> collection = new List<ParticleSystem>();
			ICollection<ParticleSystem> collection2 = ((!states.ContainsKey(stateName)) ? new List<ParticleSystem>() : states[stateName]);
			foreach (string key in states.Keys)
			{
				if (key.Equals(stateName))
				{
					continue;
				}
				foreach (ParticleSystem item in states[key])
				{
					if (!collection2.Contains(item) && !collection.Contains(item))
					{
						collection.Add(item);
					}
				}
			}
			UpdateFX(collection2, collection);
		}

		private void TriggerAnimVFX(string key)
		{
			if (states.ContainsKey(key))
			{
				foreach (ParticleSystem item in states[key])
				{
					item.Play();
				}
				return;
			}
			Debug.LogError("No such AnimVFX trigger: " + key);
		}

		private void UpdateFX(IEnumerable<ParticleSystem> enabled, IEnumerable<ParticleSystem> disabled)
		{
			foreach (ParticleSystem item in enabled)
			{
				item.Play();
			}
			foreach (ParticleSystem item2 in disabled)
			{
				item2.Stop();
			}
		}
	}
}
