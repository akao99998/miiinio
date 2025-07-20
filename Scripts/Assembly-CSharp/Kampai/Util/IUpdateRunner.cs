using System;

namespace Kampai.Util
{
	public interface IUpdateRunner
	{
		void Subscribe(Action action);

		void Unsubscribe(Action action);
	}
}
