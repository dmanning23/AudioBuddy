using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AudioBuddy
{
	class WinmmWrapper
	{
		private string _command;
		private bool isOpen;

		[DllImport("winmm.dll")]
		private static extern long mciSendString(string strCommand, StringBuilder lpszReturnString, int iReturnLength, IntPtr hwndCallback);

		[DllImport("winmm.dll")]
		private static extern bool mciGetErrorString(long fdwError, StringBuilder lpszErrorText, int cchErrorText);

		public void Close()
		{
			_command = "close MediaFile";

			var result = mciSendString(_command, null, 0, IntPtr.Zero);

			if (0 == result)
			{
				isOpen = false;
			}
		}

		public void Open(string sFileName)
		{
			_command = "open \"" + sFileName + "\" type mpegvideo alias MediaFile";
			var result = mciSendString(_command, null, 0, IntPtr.Zero);

			if (0 == result)
			{
				isOpen = true;
			}
			else
			{
				var buffer = new StringBuilder(512);
				bool returnValue = mciGetErrorString(result, buffer, buffer.Capacity);
				throw new Exception(buffer.ToString());
			}
		}

		public void Play(bool loop)
		{
			if (isOpen)
			{
				_command = "play MediaFile";
				if (loop)
				{
					_command += " REPEAT";
				}
				var result = mciSendString(_command, null, 0, IntPtr.Zero);
			}
		}
	}
}
