using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Keyboard_Usurper.Handles
{
	public class MySafeHandle : SafeHandle
	{
		public MySafeHandle(IntPtr invalidHandleValue, bool ownsHandle) : base(invalidHandleValue, ownsHandle) { }

		public override bool IsInvalid => handle == IntPtr.Zero;

		protected override bool ReleaseHandle() {
			return true;
		}
	}
}
