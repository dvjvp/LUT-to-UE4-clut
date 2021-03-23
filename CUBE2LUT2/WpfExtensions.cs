using System;
using WinFormWindow = System.Windows.Forms.IWin32Window;

public static class WpfExtensions
{
	public static WinFormWindow AsWinformWindow(this System.Windows.Media.Visual visual)
	{
		var source = System.Windows.PresentationSource.FromVisual( visual ) as System.Windows.Interop.HwndSource;
		return new WindowHandle( source.Handle );
	}

	private class WindowHandle : WinFormWindow
	{
		private readonly IntPtr handle;
		public WindowHandle(IntPtr rawptrHandle)
		{
			handle = rawptrHandle;
		}

		public IntPtr Handle => handle;
	}
}
