using Kampai.Common.Service.Audio;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PlayMignetteMusicCommand : Command
	{
		public enum MusicEvent
		{
			Start = 0,
			Stop = 1
		}

		[Inject]
		public string audioSource { get; set; }

		[Inject]
		public MusicEvent musicEvent { get; set; }

		[Inject]
		public IFMODService fmodService { get; set; }

		public override void Execute()
		{
			string guid = fmodService.GetGuid(audioSource);
			string name = string.Format("/{0}", guid);
			GameObject gameObject = GameObject.Find(name);
			if (gameObject == null)
			{
				gameObject = new GameObject(guid);
			}
			MignetteMusicEmitter component = gameObject.GetComponent<MignetteMusicEmitter>();
			if (component == null && musicEvent == MusicEvent.Start)
			{
				component = gameObject.AddComponent<MignetteMusicEmitter>();
				if (!(component == null))
				{
					component.SetEventGUID(guid);
					component.StartEvent();
				}
			}
			else if (component != null)
			{
				component.StopEvent();
			}
		}
	}
}
