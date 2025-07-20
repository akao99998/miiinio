using FMOD;
using FMOD.Studio;
using UnityEngine;

namespace Kampai.Game
{
	public class MignetteMusicEmitter : MonoBehaviour
	{
		private EventInstance eventInstance;

		private static bool isShuttingDown;

		public virtual void Update()
		{
			if (eventInstance == null || !eventInstance.isValid() || HasFinished())
			{
				Object.Destroy(base.gameObject);
			}
		}

		public bool IsValid()
		{
			return eventInstance != null && eventInstance.isValid();
		}

		public bool HasFinished()
		{
			if (!IsValid())
			{
				return true;
			}
			return getPlaybackState() == PLAYBACK_STATE.STOPPED;
		}

		public PLAYBACK_STATE getPlaybackState()
		{
			if (!IsValid())
			{
				return PLAYBACK_STATE.STOPPED;
			}
			PLAYBACK_STATE state = PLAYBACK_STATE.STOPPED;
			if (ERRCHECK(eventInstance.getPlaybackState(out state)) == RESULT.OK)
			{
				return state;
			}
			return PLAYBACK_STATE.STOPPED;
		}

		public void SetEventGUID(string guid)
		{
			if (!string.IsNullOrEmpty(guid))
			{
				eventInstance = FMOD_StudioSystem.instance.GetEvent(guid);
			}
		}

		public void StartEvent()
		{
			if (IsValid())
			{
				ERRCHECK(eventInstance.start());
			}
		}

		public void StopEvent()
		{
			if (IsValid())
			{
				ERRCHECK(eventInstance.stop(STOP_MODE.ALLOWFADEOUT));
			}
		}

		private void OnApplicationQuit()
		{
			isShuttingDown = true;
		}

		private void OnDestroy()
		{
			if (!isShuttingDown && eventInstance != null && eventInstance.isValid())
			{
				if (getPlaybackState() != PLAYBACK_STATE.STOPPED)
				{
					ERRCHECK(eventInstance.stop(STOP_MODE.IMMEDIATE));
				}
				ERRCHECK(eventInstance.release());
				eventInstance = null;
			}
		}

		private RESULT ERRCHECK(RESULT result)
		{
			UnityUtil.ERRCHECK(result);
			return result;
		}
	}
}
