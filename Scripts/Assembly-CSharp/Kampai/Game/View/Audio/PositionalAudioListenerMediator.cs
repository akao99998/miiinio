using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View.Audio
{
	public class PositionalAudioListenerMediator : Mediator
	{
		private const float CAMERA_HEIGHT_OFFSET = 5f;

		private Camera cameraAnchor;

		private bool characteAudioEnabled = true;

		private Transform audioListenerOriginalParent;

		private Transform newParent;

		[Inject]
		public PositionalAudioListenerView view { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mainCamera { get; set; }

		[Inject]
		public CameraUtils cameraUtils { get; set; }

		[Inject]
		public MoveAudioListenerSignal moveCharacterAudio { get; set; }

		public override void OnRegister()
		{
			audioListenerOriginalParent = base.transform.parent;
			cameraAnchor = mainCamera;
			moveCharacterAudio.AddListener(Toggle);
		}

		public override void OnRemove()
		{
			moveCharacterAudio.RemoveListener(Toggle);
		}

		private void Toggle(bool toggle, Transform newParent)
		{
			if (newParent != null)
			{
				this.newParent = newParent;
				base.transform.localPosition = Vector3.zero;
			}
			else
			{
				base.transform.parent = audioListenerOriginalParent;
			}
			characteAudioEnabled = toggle;
			Update();
		}

		private void Update()
		{
			float y = cameraAnchor.gameObject.transform.position.y - 5f;
			Vector3 vector = cameraUtils.CameraCenterRaycast(cameraAnchor);
			if (characteAudioEnabled)
			{
				view.UpdatePosition(new Vector3(vector.x, y, vector.z));
			}
			else if (newParent != null)
			{
				view.UpdatePosition(newParent.position);
			}
		}
	}
}
