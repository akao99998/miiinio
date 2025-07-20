using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	[RequireComponent(typeof(LayoutElement))]
	public class TSMQuantityItemView : MonoBehaviour
	{
		public LocalizeView m_amount;

		public KampaiImage m_image;

		public TSMQuantityItemView Init(IDisplayableDefinition displayableDefinition, uint amount, IKampaiLogger logger, ILocalizationService locale)
		{
			UIUtils.SetItemIcon(m_image, displayableDefinition);
			if (amount <= 1)
			{
				m_amount.gameObject.SetActive(false);
				return this;
			}
			m_amount.logger = logger;
			m_amount.service = locale;
			m_amount.Format("QuantityItemFormat", amount);
			return this;
		}

		public void Disable()
		{
			if (!(m_image == null))
			{
				m_image.Desaturate = 1f;
			}
		}
	}
}
