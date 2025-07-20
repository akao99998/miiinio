using System;

namespace Kampai.Wrappers
{
	public class WeakLuaState : LuaState
	{
		public WeakLuaState(IntPtr handle)
			: base(false)
		{
			base.handle = handle;
		}

		protected override bool ReleaseHandle()
		{
			handle = IntPtr.Zero;
			return true;
		}
	}
}
