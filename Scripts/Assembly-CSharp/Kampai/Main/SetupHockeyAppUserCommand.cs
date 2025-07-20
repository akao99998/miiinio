using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class SetupHockeyAppUserCommand : Command
	{
		[Inject]
		public string UserId { get; set; }

		public override void Execute()
		{
			GameObject gameObject = GameObject.Find("HockeyApp");
			if (!(gameObject == null))
			{
				HockeyAppAndroid component = gameObject.GetComponent<HockeyAppAndroid>();
				component.userId = UserId;
			}
		}
	}
}
