using Kampai.Game;
using Kampai.Game.Transaction;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class StoreButtonClickSignal : Signal<Definition, TransactionDefinition, Vector3>
	{
	}
}
