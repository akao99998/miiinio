using System;
using System.Runtime.InteropServices;

namespace Kampai.Wrappers
{
	public class SafeGCHandle : SafeHandle
	{
		public override bool IsInvalid
		{
			get
			{
				return handle == IntPtr.Zero;
			}
		}

		public object Target
		{
			get
			{
				return GCHandle.FromIntPtr(handle).Target;
			}
		}

		public SafeGCHandle()
			: base(IntPtr.Zero, true)
		{
			handle = IntPtr.Zero;
		}

		public SafeGCHandle(object o)
			: base(IntPtr.Zero, true)
		{
			GCHandle value = GCHandle.Alloc(o);
			handle = GCHandle.ToIntPtr(value);
		}

		protected override bool ReleaseHandle()
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(handle);
			handle = IntPtr.Zero;
			gCHandle.Free();
			return true;
		}
	}
}
