using Kampai.Main;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ShowWorldCanvasCommand : Command
	{
		private GoTween tween;

		private GoTweenConfig tweenConfig = new GoTweenConfig();

		[Inject]
		public bool show { get; set; }

		[Inject(MainElement.UI_WORLDCANVAS)]
		public GameObject worldCanvas { get; set; }

		public override void Execute()
		{
			if (show)
			{
				worldCanvas.SetActive(true);
				if (tween != null && tween.isValid())
				{
					tween.destroy();
				}
				tweenConfig.clearProperties();
				tweenConfig.clearEvents();
				tweenConfig.easeType = GoEaseType.CubicOut;
				tweenConfig.floatProp("alpha", 1f);
				tweenConfig.onComplete(delegate(AbstractGoTween thisTween)
				{
					thisTween.destroy();
				});
				tweenConfig.setIsTo();
				tween = new GoTween(worldCanvas.GetComponent<Canvas>(), 0.5f, tweenConfig);
				Go.addTween(tween);
			}
			else
			{
				if (tween != null && tween.isValid())
				{
					tween.destroy();
				}
				tweenConfig.clearProperties();
				tweenConfig.clearEvents();
				tweenConfig.easeType = GoEaseType.CubicOut;
				tweenConfig.floatProp("alpha", 0f);
				tweenConfig.onComplete(delegate(AbstractGoTween thisTween)
				{
					thisTween.destroy();
					worldCanvas.SetActive(false);
				});
				tweenConfig.setIsTo();
				tween = new GoTween(worldCanvas.GetComponent<Canvas>(), 0.5f, tweenConfig);
				Go.addTween(tween);
			}
		}
	}
}
