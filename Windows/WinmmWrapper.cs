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
		private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);

		public void Close()
		{
			_command = "close MediaFile";
			mciSendString(_command, null, 0, IntPtr.Zero);
			isOpen = false;
		}

		public void Open(string sFileName)
		{
			_command = "open \"" + sFileName + "\" type mpegvideo alias MediaFile";
			mciSendString(_command, null, 0, IntPtr.Zero);
			isOpen = true;
		}

		public void Play(bool loop)
		{
			if (isOpen)
			{
				_command = "play MediaFile";
				if (loop)
					_command += " REPEAT";
				mciSendString(_command, null, 0, IntPtr.Zero);
			}
		}
	}
}
