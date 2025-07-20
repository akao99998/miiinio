using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class MoveBuildingWayFinderMediator : AbstractWayFinderMediator
	{
		[Inject]
		public MoveBuildingWayFinderView moveBuildingWayFinderView { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return moveBuildingWayFinderView;
			}
		}

		protected override void GoToClicked()
		{
			BuildingDefinitionObject targetObject = moveBuildingWayFinderView.GetTargetObject();
			ScreenPosition value = new ScreenPosition();
			gameContext.injectionBinder.GetInstance<CameraAutoMoveSignal>().Dispatch(targetObject.ResourceIconPosition, new Boxed<ScreenPosition>(value), new CameraMovementSettings(CameraMovementSettings.Settings.None, null, null), false);
		}
	}
}
