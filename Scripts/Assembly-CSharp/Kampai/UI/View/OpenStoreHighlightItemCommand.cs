using System.Collections.Generic;
using Kampai.Game;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class OpenStoreHighlightItemCommand : Command
	{
		[Inject]
		public int defId { get; set; }

		[Inject]
		public bool openStore { get; set; }

		[Inject]
		public IDefinitionService service { get; set; }

		[Inject]
		public MoveBuildMenuSignal moveBuildMenuSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public HighlightStoreItemSignal highlightStoreItemSignal { get; set; }

		public override void Execute()
		{
			IList<StoreItemDefinition> all = service.GetAll<StoreItemDefinition>();
			foreach (StoreItemDefinition item in all)
			{
				if (item.ReferencedDefID == defId)
				{
					if (openStore)
					{
						moveBuildMenuSignal.Dispatch(true);
						showStoreSignal.Dispatch(true);
						highlightStoreItemSignal.Dispatch(item, HighlightType.DRAG);
					}
					break;
				}
			}
		}
	}
}
