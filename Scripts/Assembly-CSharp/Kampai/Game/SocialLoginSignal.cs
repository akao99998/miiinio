using System;
using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class SocialLoginSignal : Signal<ISocialService, Boxed<Action>>
	{
	}
}
