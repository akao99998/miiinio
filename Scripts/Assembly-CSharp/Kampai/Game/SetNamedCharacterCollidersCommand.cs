using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetNamedCharacterCollidersCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SetNamedCharacterCollidersCommand") as IKampaiLogger;

		private NamedCharacterManagerView namedCharacterManagerView;

		[Inject]
		public bool Enabled { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject NamedCharacterManager { get; set; }

		public override void Execute()
		{
			namedCharacterManagerView = NamedCharacterManager.GetComponent<NamedCharacterManagerView>();
			List<NamedCharacter> instancesByType = PlayerService.GetInstancesByType<NamedCharacter>();
			if (instancesByType == null)
			{
				logger.Warning("Unable to find any named characters!");
				return;
			}
			List<NamedCharacter>.Enumerator enumerator = instancesByType.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					NamedCharacter current = enumerator.Current;
					if (!(current is TSMCharacter))
					{
						NamedCharacterObject namedCharacterObject = namedCharacterManagerView.Get(current.ID);
						if (namedCharacterObject != null)
						{
							namedCharacterObject.GetComponent<Collider>().enabled = Enabled;
						}
					}
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}
	}
}
