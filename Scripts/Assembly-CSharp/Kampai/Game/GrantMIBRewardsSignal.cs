using Kampai.Game.Transaction;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class GrantMIBRewardsSignal : Signal<MIBRewardType, TransactionDefinition, Vector3>
	{
	}
}
