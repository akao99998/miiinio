using Kampai.Game;
using UnityEngine;

namespace Kampai.UI.View.UpSell
{
	public class UpsellPriceMarkdownView : MonoBehaviour
	{
		public LocalizeView originalPriceText;

		public LocalizeView percentOffText;

		public LocalizeView percentOffBanner;

		private PackDefinition m_packDefinition;

		private IDefinitionService m_definitionService;

		private ICurrencyService m_currencyService;

		private PremiumCurrencyItemDefinition m_currencyItemDefinition;

		public void Init(PackDefinition packDefinition, IDefinitionService definitionService, ICurrencyService currencyService)
		{
			m_packDefinition = packDefinition;
			m_definitionService = definitionService;
			m_currencyService = currencyService;
			SetupMTXDescription();
		}

		private void SetupMTXDescription()
		{
			if (percentOffText == null)
			{
				return;
			}
			if (originalPriceText != null && m_packDefinition.TransactionType == UpsellTransactionType.Cash)
			{
				if (m_definitionService.TryGet<PremiumCurrencyItemDefinition>(m_packDefinition.CurrencyImageID, out m_currencyItemDefinition))
				{
					originalPriceText.text = m_currencyService.GetPriceWithCurrencyAndFormat(m_currencyItemDefinition.SKU);
				}
				else
				{
					originalPriceText.gameObject.SetActive(false);
				}
				percentOffText.text = m_currencyService.GetPriceWithCurrencyAndFormat(m_packDefinition.SKU);
			}
			percentOffBanner.text = string.Format("{0}%", m_packDefinition.PercentagePer100);
		}
	}
}
