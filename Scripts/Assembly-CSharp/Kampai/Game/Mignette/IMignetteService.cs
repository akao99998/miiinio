using System;
using Kampai.Common;
using UnityEngine;

namespace Kampai.Game.Mignette
{
	public interface IMignetteService : IPickService
	{
		void RegisterListener(Action<Vector3, int, bool> obj);

		void UnregisterListener(Action<Vector3, int, bool> obj);
	}
}
