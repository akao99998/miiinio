using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PartyCommand : Command
	{
		private MinionPartyDefinition partyDefinition;

		private bool delay = true;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SetPartyStatesSignal setPartyStatesSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		public PartyCommand()
		{
		}

		public PartyCommand(bool delay)
		{
			this.delay = delay;
		}

		public override void Execute()
		{
			if (delay)
			{
				routineRunner.StartCoroutine(WaitSomeTime());
			}
			else
			{
				ResolveParty();
			}
		}

		private IEnumerator WaitSomeTime()
		{
			yield return new WaitForSeconds(0.2f);
			ResolveParty();
		}

		private void ResolveParty()
		{
			partyDefinition = definitionService.Get<MinionPartyDefinition>(80000);
			int num = timeService.AppTime();
			if (num > partyDefinition.GetPartyDuration(true))
			{
				ManagePartySize();
			}
		}

		private void ManagePartySize()
		{
			List<Minion> idleMinions = playerService.GetIdleMinions();
			float num = partyDefinition.Percent - PartyPercent(idleMinions);
			int num2 = (int)((float)idleMinions.Count * num / 100f);
			if (num2 != 0)
			{
				ChangePartyGoers(idleMinions, num > 0f, Math.Abs(num2));
				setPartyStatesSignal.Dispatch(false);
			}
		}

		private void ChangePartyGoers(List<Minion> minions, bool newState, int count)
		{
			for (int i = 0; i < minions.Count; i++)
			{
				if (minions[i].Partying != newState)
				{
					minions[i].Partying = newState;
					if (--count < 1)
					{
						break;
					}
				}
			}
		}

		private float PartyPercent(List<Minion> minions)
		{
			if (minions.Count == 0)
			{
				return 0f;
			}
			int num = 0;
			for (int i = 0; i < minions.Count; i++)
			{
				if (minions[i].Partying)
				{
					num++;
				}
			}
			if (num == 0)
			{
				return 0f;
			}
			return (float)num / (float)minions.Count * 100f;
		}
	}
}
