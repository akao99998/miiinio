using System.Collections;
using Kampai.Game;
using Kampai.Game.Transaction;
using UnityEngine;

namespace Kampai.UI.View
{
	public class BobPointsAtStuffWayFinderView : AbstractWayFinderView
	{
		private TransactionDefinition transactionDef;

		private bool _isMissingItem;

		private bool _initialSet;

		protected override string UIName
		{
			get
			{
				return "BobPointsAtStuffWayFinder";
			}
		}

		protected override string WayFinderDefaultIcon
		{
			get
			{
				return wayFinderDefinition.BobPointsAtStuffDefaultIcon;
			}
		}

		private void UpdateMissingItem(bool value)
		{
			if (_isMissingItem != value || !_initialSet)
			{
				_initialSet = true;
				_isMissingItem = value;
				if (_isMissingItem)
				{
					UpdateIcon(WayFinderDefaultIcon);
				}
				else
				{
					UpdateIcon(wayFinderDefinition.BobPointsAtStuffLandExpansionIcon);
				}
			}
		}

		internal void Init(ILandExpansionConfigService landExpansionService, IDefinitionService definitionService)
		{
			LandExpansionConfig expansionConfig = landExpansionService.GetExpansionConfig(playerService.GetTargetExpansion());
			transactionDef = definitionService.Get<TransactionDefinition>(expansionConfig.transactionId);
			StartCoroutine(CheckMissingItems());
		}

		private IEnumerator CheckMissingItems()
		{
			while (true)
			{
				UpdateMissingItem(playerService.IsMissingItemFromTransaction(transactionDef));
				yield return new WaitForSeconds(0.5f);
			}
		}

		public override Vector3 GetIndicatorPosition()
		{
			Vector3 indicatorPosition = base.GetIndicatorPosition();
			indicatorPosition.y += wayFinderDefinition.BobPointsAtStuffYWorldOffset;
			return indicatorPosition;
		}

		protected override bool OnCanUpdate()
		{
			if (zoomCameraModel.ZoomedIn)
			{
				return false;
			}
			return true;
		}
	}
}
