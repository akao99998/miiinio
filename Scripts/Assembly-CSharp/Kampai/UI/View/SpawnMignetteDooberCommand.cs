using Kampai.Game.Mignette;
using Kampai.Util;
using UnityEngine;
using strange.extensions.pool.api;

namespace Kampai.UI.View
{
	public class SpawnMignetteDooberCommand : DooberCommand, IPoolable, IFastPooledCommand<MignetteHUDView, Vector3, int, bool>, IFastPooledCommandBase
	{
		private const float flyTime = 1.1f;

		private const float delayTime = 0.1f;

		private int numberOnDoober;

		private MignetteHUDView mignetteHUDView;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public MignetteGameModel mignetteModel { get; set; }

		[Inject]
		public MignetteDooberSpawnedSignal spawnedSignal { get; set; }

		public void Execute(MignetteHUDView _mignetteHUDView, Vector3 iconPosition, int numberOnDoober, bool fromWorldCanvas)
		{
			base.iconPosition = iconPosition;
			base.fromWorldCanvas = fromWorldCanvas;
			this.numberOnDoober = numberOnDoober;
			mignetteHUDView = _mignetteHUDView;
			GameObject gameObject = CreateTweenObject();
			Vector3 position = mignetteHUDView.DooberTarget.position;
			TweenToDestination(gameObject, position, 1.1f, DestinationType.MIGNETTE, 0.1f);
			spawnedSignal.Dispatch(gameObject);
		}

		private GameObject CreateTweenObject()
		{
			IGUICommand command = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "NumberedDoober");
			GameObject gameObject = guiService.Execute(command);
			NumberedDooberViewObject component = gameObject.GetComponent<NumberedDooberViewObject>();
			component.NumberLabel.text = string.Format("x {0}", numberOnDoober);
			if (mignetteModel.CollectableImage != null)
			{
				component.IconImage.sprite = mignetteModel.CollectableImage;
			}
			if (mignetteModel.CollectableImageMask != null)
			{
				component.IconImage.maskSprite = mignetteModel.CollectableImageMask;
			}
			RectTransform component2 = gameObject.GetComponent<RectTransform>();
			component2.anchoredPosition = GetScreenStartPosition();
			return gameObject;
		}
	}
}
