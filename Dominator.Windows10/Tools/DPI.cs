using System;
using System.Runtime.InteropServices;

namespace Dominator.Windows10.Tools
{
	static class DPI
	{
		public static readonly int Display = getDPI();

		static int getDPI()
		{
			IntPtr hDc = GetDC(IntPtr.Zero);
			if (hDc == null)
				throw new Exception("Failed to get DPIs for the display");
			try
			{
				// int dpiX = GetDeviceCaps(hDc, LOGPIXELSX);
				int dpiY = GetDeviceCaps(hDc, LOGPIXELSY);
				return dpiY;
			}
			finally
			{
				ReleaseDC(IntPtr.Zero, hDc);
			}
		}

		const int LOGPIXELSX = 88;
		const int LOGPIXELSY = 90;

		[DllImport("user32.dll")]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

		[DllImport("gdi32.dll")]
		static extern int GetDeviceCaps(IntPtr hDc, int nIndex);
	}
}
