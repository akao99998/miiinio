using System;
using Kampai.Game;
using Kampai.Game.Trigger;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class TSMItemPanelMediator : KampaiMediator
	{
		private TriggerRewardDefinition m_reward;

		private Transform parent;

		[Inject]
		public TSMItemPanelView view { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void Initialize(GUIArguments args)
		{
			m_reward = args.Get<TriggerRewardDefinition>();
			parent = args.Get<Transform>();
			Action<TriggerRewardDefinition> onPurchaseCallback = args.Get<Action<TriggerRewardDefinition>>();
			bool flag = args.Get<bool>();
			view.Init(m_reward, parent, currencyService, gameContext, onPurchaseCallback);
			if (flag)
			{
				view.Disable();
			}
		}

		public override void OnRegister()
		{
		}

		public override void OnRemove()
		{
		}
	}
}
