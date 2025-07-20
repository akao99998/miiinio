using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kampai.Main;
using Kampai.Util;
using Kampai.Util.Audio;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class AnimEventHandler : MonoBehaviour
	{
		private enum AudioEventType
		{
			Default = 0,
			MinionState = 1,
			Looping = 2
		}

		protected const string MISSING_PARENT = "Missing parent for ";

		private PlayLocalAudioSignal audioSignal;

		private StopLocalAudioSignal stopAudioSignal;

		private PlayMinionStateAudioSignal minionStateAudioSignal;

		private StartLoopingAudioSignal startLoopingAudioSignal;

		private VFXScript siblingVFXScript;

		private Action<AnimEventHandler> binder;

		private static HashSet<string> loopingEvents = new HashSet<string>
		{
			"MasterChargingStation", "MasterIceMountain", "MasterSwampGas", "MasterCraftingTool", "MasterCraftingGadget", "MasterHotTub", "Play_leisure_firepit_fire_01", "Play_crafting_forge_01", "Play_crafting_gizmos_01", "Play_villainLair_component_thermalBattery_01",
			"Play_villainLair_component_ultravioletEmitter_01", "Play_villainLair_component_freqSpacer_01", "Play_crafting_dessert_01"
		};

		private static HashSet<string> minionStateEvents = new HashSet<string> { "Paired_Awareness_State", "Paired_Gacha_State", "Solo_Awareness_State", "Solo_Gacha_State", "Solo_Idle_State", "TownSquare_State", "Trio_Awareness_State", "Trio_Gacha_State" };

		private CustomFMOD_StudioEventEmitter localEmitter;

		private ActionableObject parentObject;

		private Dictionary<string, CustomFMOD_StudioEventEmitter> secondaryEmitters = new Dictionary<string, CustomFMOD_StudioEventEmitter>();

		private Dictionary<string, float> loopingEventParameters = new Dictionary<string, float>(1);

		private Signal stopBuildingAudioInIdleStateSignal;

		public bool IsStopBuildingAudioSignalSet
		{
			get
			{
				return stopBuildingAudioInIdleStateSignal != null;
			}
		}

		public bool mute { get; set; }

		public void SetStopBuildingAudioInIdleStateSignal(Signal signal)
		{
			if (stopBuildingAudioInIdleStateSignal != null)
			{
				stopBuildingAudioInIdleStateSignal.RemoveListener(OnStopAudio);
				stopBuildingAudioInIdleStateSignal = null;
			}
			if (signal != null)
			{
				stopBuildingAudioInIdleStateSignal = signal;
				stopBuildingAudioInIdleStateSignal.AddListener(OnStopAudio);
			}
		}

		public void Init(ActionableObject parent, CustomFMOD_StudioEventEmitter emitter, PlayLocalAudioSignal audioSignal, StopLocalAudioSignal stopAudioSignal, PlayMinionStateAudioSignal minionStateAudioSignal, StartLoopingAudioSignal startLoopingAudioSignal)
		{
			this.audioSignal = audioSignal;
			this.stopAudioSignal = stopAudioSignal;
			this.minionStateAudioSignal = minionStateAudioSignal;
			this.startLoopingAudioSignal = startLoopingAudioSignal;
			parentObject = parent;
			localEmitter = emitter;
		}

		public void Init(GameObject go, PlayLocalAudioSignal audioSignal, StopLocalAudioSignal stopAudioSignal, PlayMinionStateAudioSignal minionStateAudioSignal, StartLoopingAudioSignal startLoopingAudioSignal)
		{
			this.audioSignal = audioSignal;
			this.stopAudioSignal = stopAudioSignal;
			this.minionStateAudioSignal = minionStateAudioSignal;
			this.startLoopingAudioSignal = startLoopingAudioSignal;
			parentObject = null;
			localEmitter = GetAudioEmitter.Get(go, "LocalAudio");
		}

		public void OnDestroy()
		{
			if (stopBuildingAudioInIdleStateSignal != null)
			{
				stopBuildingAudioInIdleStateSignal.RemoveListener(OnStopAudio);
			}
		}

		public virtual void OnPlayAudio(AnimationEvent animationEvent)
		{
			if (mute)
			{
				return;
			}
			string stringParameter = animationEvent.stringParameter;
			if (stringParameter == null)
			{
				return;
			}
			string[] array = stringParameter.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			string clipName = array[0];
			string text = string.Empty;
			CustomFMOD_StudioEventEmitter emitter = localEmitter;
			if (array.Length > 1)
			{
				text = array[1];
				if (!secondaryEmitters.ContainsKey(text))
				{
					secondaryEmitters.Add(text, GetAudioEmitter.Get(base.gameObject, text));
				}
				emitter = secondaryEmitters[text];
			}
			AudioEventType eventType = GetEventType(clipName);
			if (audioSignal != null)
			{
				switch (eventType)
				{
				case AudioEventType.Default:
					HandleDefaultEventType(clipName, emitter);
					break;
				case AudioEventType.MinionState:
					HandleMinionStateEventType(animationEvent, clipName, emitter, text);
					break;
				case AudioEventType.Looping:
					HandleLoopingEventType(animationEvent, clipName, emitter);
					break;
				default:
					throw new InvalidEnumArgumentException("audioEventType", (int)eventType, typeof(AudioEventType));
				}
			}
		}

		public virtual void ShowProp(string propName)
		{
			parentObject.AddProp(propName, base.gameObject);
		}

		public virtual void HideProp(string propName)
		{
			parentObject.RemoveProp(propName);
		}

		private void HandleDefaultEventType(string clipName, CustomFMOD_StudioEventEmitter emitter)
		{
			stopAudioSignal.Dispatch(emitter);
			audioSignal.Dispatch(emitter, clipName, null);
		}

		private void HandleMinionStateEventType(AnimationEvent animationEvent, string clipName, CustomFMOD_StudioEventEmitter emitter, string emitterKey)
		{
			stopAudioSignal.Dispatch(emitter);
			float floatParameter = animationEvent.floatParameter;
			MinionStateAudioArgs minionStateAudioArgs = new MinionStateAudioArgs();
			minionStateAudioArgs.source = parentObject;
			minionStateAudioArgs.audioEvent = clipName;
			minionStateAudioArgs.emitterKey = emitterKey;
			minionStateAudioArgs.cueId = floatParameter;
			MinionStateAudioArgs type = minionStateAudioArgs;
			minionStateAudioSignal.Dispatch(type);
		}

		private void HandleLoopingEventType(AnimationEvent animationEvent, string clipName, CustomFMOD_StudioEventEmitter emitter)
		{
			loopingEventParameters["Cue"] = animationEvent.floatParameter;
			startLoopingAudioSignal.Dispatch(emitter, clipName, loopingEventParameters);
		}

		private AudioEventType GetEventType(string clipName)
		{
			if (loopingEvents.Contains(clipName))
			{
				return AudioEventType.Looping;
			}
			if (minionStateEvents.Contains(clipName))
			{
				return AudioEventType.MinionState;
			}
			return AudioEventType.Default;
		}

		public void SetSiblingVFXScript(VFXScript vfxScript)
		{
			siblingVFXScript = vfxScript;
		}

		public void OnStopAudio()
		{
			if (mute || stopAudioSignal == null)
			{
				return;
			}
			stopAudioSignal.Dispatch(localEmitter);
			foreach (KeyValuePair<string, CustomFMOD_StudioEventEmitter> secondaryEmitter in secondaryEmitters)
			{
				stopAudioSignal.Dispatch(secondaryEmitter.Value);
			}
		}

		public void StopAllFX()
		{
			foreach (Transform item in base.transform)
			{
				if (!(item.name == "fx"))
				{
					continue;
				}
				foreach (Transform item2 in item.transform)
				{
					ParticleSystem component = item2.gameObject.GetComponent<ParticleSystem>();
					if (component.isPlaying)
					{
						component.Stop();
					}
				}
			}
		}

		public void OnPlayFX(string fxName)
		{
			string[] array = fxName.Split(',');
			foreach (Transform item in base.transform)
			{
				if (!(item.name == "fx"))
				{
					continue;
				}
				foreach (Transform item2 in item.transform)
				{
					string[] array2 = array;
					foreach (string text in array2)
					{
						if (item2.name == text)
						{
							ParticleSystem component = item2.gameObject.GetComponent<ParticleSystem>();
							if (!component.isPlaying)
							{
								component.Play();
							}
						}
					}
				}
			}
		}

		public void AnimVFX(string state)
		{
			ResolveBindings();
			MinionObject component = GetComponent<MinionObject>();
			VFXScript vFXScript = null;
			vFXScript = ((!base.name.Contains("_LOD")) ? GetComponent<VFXScript>() : GetComponentInParent<VFXScript>());
			if (component != null)
			{
				component.AnimVFX(state);
			}
			else if (vFXScript != null)
			{
				vFXScript.AnimVFX(state);
			}
			if (siblingVFXScript != null)
			{
				siblingVFXScript.AnimVFX(state);
			}
		}

		private void ResolveBindings()
		{
			if (binder != null)
			{
				if (siblingVFXScript == null)
				{
					binder(this);
				}
				else
				{
					binder = null;
				}
			}
		}

		public void SetVFXScriptBinder(Action<AnimEventHandler> binder)
		{
			this.binder = binder;
		}
	}
}
