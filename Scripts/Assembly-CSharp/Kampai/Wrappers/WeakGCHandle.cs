using System;

namespace Kampai.Wrappers
{
	public class WeakGCHandle : SafeGCHandle
	{
		public WeakGCHandle(IntPtr ptr)
		{
			handle = ptr;
		}

		protected override bool ReleaseHandle()
		{
			handle = IntPtr.Zero;
			return true;
		}
	}
}
