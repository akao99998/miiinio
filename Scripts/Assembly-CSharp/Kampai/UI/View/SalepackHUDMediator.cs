using System.Collections;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class SalepackHUDMediator : EventMediator
	{
		private SalePackDefinition m_salePackDefinition;

		private bool m_isValidItem = true;

		[Inject]
		public SalepackHUDView view { get; set; }

		[Inject]
		public OpenUpSellModalSignal openUpSellModalSignal { get; set; }

		[Inject]
		public AppPauseSignal pauseSignal { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public RemoveSalePackSignal removeSalePackSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public PickControllerModel pickModel { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			view.Init();
			view.closeSignal.AddListener(Close);
			view.SalePackButton.ClickedSignal.AddListener(OnSalePackButtonClicked);
			pauseSignal.AddListener(OnPause);
			SetSalePackItemRoutines();
		}

		public override void OnRemove()
		{
			view.closeSignal.RemoveListener(Close);
			pauseSignal.RemoveListener(OnPause);
			view.SalePackButton.ClickedSignal.RemoveListener(OnSalePackButtonClicked);
			Close();
		}

		private void Close()
		{
			m_isValidItem = true;
		}

		private void OnPause()
		{
			Close();
		}

		private void OnEnable()
		{
			if (view != null)
			{
				SetSalePackItemRoutines();
			}
		}

		private void SetSalePackItemRoutines()
		{
			if (view.SalePackItem != null)
			{
				m_salePackDefinition = view.SalePackItem.Definition;
				StartCoroutine(UpdateSaleTime());
			}
		}

		internal void OnSalePackButtonClicked()
		{
			if (!pickModel.PanningCameraBlocked && !pickModel.ZoomingCameraBlocked && !zoomCameraModel.ZoomInProgress)
			{
				openUpSellModalSignal.Dispatch(m_salePackDefinition, "HUD", false);
			}
		}

		internal IEnumerator UpdateSaleTime()
		{
			while (m_isValidItem)
			{
				if (view == null || view.SalePackItem == null)
				{
					m_isValidItem = false;
					continue;
				}
				if (view.SalePackItem.Purchased || view.SalePackItem.Finished)
				{
					removeSalePackSignal.Dispatch(view.SalePackItem.ID);
					m_isValidItem = false;
				}
				int saleTime = timeEventService.GetTimeRemaining(view.SalePackItem.ID);
				if (saleTime > 0)
				{
					string saleTimeStr = UIUtils.FormatTime(saleTime, localizationService);
					view.ItemText.text = string.Format("{0}", saleTimeStr);
				}
				yield return new WaitForSeconds(1f);
			}
		}
	}
}
