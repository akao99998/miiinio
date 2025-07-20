using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.api;

namespace Kampai.Game.View
{
	public class OrderBoardBuildingObjectView : BuildingObject, IView
	{
		public OrderBoard orderBoard;

		private OrderBoardBuildingTicketsView ticketViews;

		private bool ticketsReady;

		private bool _requiresContext = true;

		protected bool registerWithContext = true;

		private IContext currentContext;

		public bool requiresContext
		{
			get
			{
				return _requiresContext;
			}
			set
			{
				_requiresContext = value;
			}
		}

		public bool registeredWithContext { get; set; }

		public virtual bool autoRegisterWithContext
		{
			get
			{
				return registerWithContext;
			}
			set
			{
				registerWithContext = value;
			}
		}

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			orderBoard = building as OrderBoard;
			ticketViews = base.gameObject.GetComponent<OrderBoardBuildingTicketsView>();
			ticketsReady = false;
		}

		internal void ClearBoard()
		{
			if (ticketViews == null)
			{
				return;
			}
			if (!ticketsReady)
			{
				ticketsReady = ticketViews.IsOrderboardSetupCorrectly();
				if (!ticketsReady)
				{
					logger.Error("Tickets are not assign on the prefab. Please make sure that they do.");
					return;
				}
			}
			ticketViews.DisableTickets();
		}

		public void ToggleHitbox(bool enable)
		{
			Collider[] components = GetComponents<Collider>();
			foreach (Collider collider in components)
			{
				collider.enabled = enable;
			}
		}

		internal void SetTicketState(int ticketIndex, OrderBoardTicketState state)
		{
			if (ticketViews == null)
			{
				return;
			}
			if (!ticketsReady)
			{
				ticketsReady = ticketViews.IsOrderboardSetupCorrectly();
				if (!ticketsReady)
				{
					logger.Error("Tickets are not assign on the prefab. Please make sure that they do.");
					return;
				}
			}
			ticketViews.SetTicketState(ticketIndex, state);
		}

		protected void Awake()
		{
			if (autoRegisterWithContext && !registeredWithContext)
			{
				KampaiView.BubbleToContext(this, true, false, ref currentContext);
			}
		}

		protected void Start()
		{
			if (autoRegisterWithContext && !registeredWithContext)
			{
				KampaiView.BubbleToContext(this, true, true, ref currentContext);
			}
		}

		protected void OnDestroy()
		{
			KampaiView.BubbleToContext(this, false, false, ref currentContext);
		}
	}
}
