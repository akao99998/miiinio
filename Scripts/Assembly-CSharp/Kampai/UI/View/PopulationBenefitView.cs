using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class PopulationBenefitView : KampaiView
	{
		public KampaiImage PopulationIcon;

		public Text Goal;

		public Text Benefit;

		public CanvasGroup lockedGroup;

		public float lockedAphaValue = 0.5f;

		private PopulationBenefitDefinition definition;

		private IDefinitionService definitionService;

		private ILocalizationService localizationService;

		private IPlayerService playerService;

		internal Signal updateSignal = new Signal();

		public int benefitDefinitionID { get; set; }

		public void Init(IDefinitionService definitionService, ILocalizationService localizationService, IPlayerService playerService)
		{
			this.definitionService = definitionService;
			this.localizationService = localizationService;
			this.playerService = playerService;
			UpdateView(false);
		}

		public void UpdateView(bool checkDoober)
		{
			if (definitionService != null)
			{
				definition = definitionService.Get<PopulationBenefitDefinition>(benefitDefinitionID);
				Goal.text = localizationService.GetString("MinionUpgradePopulationGoal", definition.numMinionsRequired, definition.minionLevelRequired + 1);
				Benefit.text = GetPopulationBenfitText();
				SetIcon();
				lockedGroup.alpha = (IsBenefitUnlocked(definition.ID) ? 1f : lockedAphaValue);
				if (checkDoober)
				{
					updateSignal.Dispatch();
				}
			}
		}

		public void SetIcon()
		{
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(definition.transactionDefinitionID);
			int count = transactionDefinition.Outputs.Count;
			if (count > 0)
			{
				UIUtils.SetItemIcon(PopulationIcon, definitionService.Get<DisplayableDefinition>(transactionDefinition.GetOutputItem(0).ID));
			}
		}

		public bool IsBenefitUnlocked(int definitionID)
		{
			MinionUpgradeBuilding byInstanceId = playerService.GetByInstanceId<MinionUpgradeBuilding>(375);
			return byInstanceId.processedPopulationBenefitDefinitionIDs.Contains(definitionID);
		}

		private string GetPopulationBenfitText()
		{
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(definition.transactionDefinitionID);
			int count = transactionDefinition.Outputs.Count;
			if (count <= 0)
			{
				return localizationService.GetString(definition.benefitDescriptionLocalizedKey);
			}
			object[] array = new object[count];
			for (int i = 0; i < count; i++)
			{
				QuantityItem quantityItem = transactionDefinition.Outputs[i];
				if (quantityItem.ID == 335)
				{
					array[i] = UIUtils.FormatTime(quantityItem.Quantity, localizationService);
				}
				else
				{
					array[i] = quantityItem.Quantity;
				}
			}
			return localizationService.GetString(definition.benefitDescriptionLocalizedKey, array);
		}
	}
}
