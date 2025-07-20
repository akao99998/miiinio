using Elevation.Logging;
using Kampai.Util;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class ResourceIconPanelMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ResourceIconPanelMediator") as IKampaiLogger;

		[Inject]
		public ResourceIconPanelView View { get; set; }

		[Inject]
		public CreateResourceIconSignal CreateResourceIconSignal { get; set; }

		[Inject]
		public RemoveResourceIconSignal RemoveResourceIconSignal { get; set; }

		[Inject]
		public RemoveResourceIconsByTrackedIdSignal RemoveResourceIconsByTrackedIdSignal { get; set; }

		[Inject]
		public UpdateResourceIconCountSignal UpdateResourceIconCountSignal { get; set; }

		[Inject]
		public ShowAllResourceIconsSignal ShowAllResourceIconsSignal { get; set; }

		[Inject]
		public HideAllResourceIconsSignal HideAllResourceIconsSignal { get; set; }

		public override void OnRegister()
		{
			View.Init(logger);
			CreateResourceIconSignal.AddListener(CreateResourceIcon);
			RemoveResourceIconSignal.AddListener(RemoveResourceIcon);
			RemoveResourceIconsByTrackedIdSignal.AddListener(RemoveResourceIconsByTrackedId);
			UpdateResourceIconCountSignal.AddListener(UpdateResourceIconCount);
			ShowAllResourceIconsSignal.AddListener(ShowAllResourceIcons);
			HideAllResourceIconsSignal.AddListener(HideAllResourceIcons);
		}

		public override void OnRemove()
		{
			View.Cleanup();
			CreateResourceIconSignal.RemoveListener(CreateResourceIcon);
			RemoveResourceIconSignal.RemoveListener(RemoveResourceIcon);
			RemoveResourceIconsByTrackedIdSignal.RemoveListener(RemoveResourceIconsByTrackedId);
			UpdateResourceIconCountSignal.RemoveListener(UpdateResourceIconCount);
			ShowAllResourceIconsSignal.RemoveListener(ShowAllResourceIcons);
			HideAllResourceIconsSignal.RemoveListener(HideAllResourceIcons);
		}

		private void CreateResourceIcon(ResourceIconSettings resourceIconSettings)
		{
			View.CreateResourceIcon(resourceIconSettings);
		}

		private void RemoveResourceIcon(Tuple<int, int> tuple)
		{
			View.RemoveResourceIcon(tuple.Item1, tuple.Item2);
		}

		private void RemoveResourceIconsByTrackedId(int trackedId)
		{
			View.RemoveResourceIcon(trackedId);
		}

		private void UpdateResourceIconCount(Tuple<int, int> tuple, int count)
		{
			View.UpdateResourceIconCount(tuple.Item1, tuple.Item2, count);
		}

		private void HideAllResourceIcons()
		{
			View.HideAllResourceIcons();
		}

		private void ShowAllResourceIcons()
		{
			View.ShowAllResourceIcons();
		}
	}
}
