﻿using System;
using AudioBuddy;
using System.Text;
using ResolutionBuddy;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using MenuBuddy;
using FontBuddyLib;
using AudioBuddy;
using FilenameBuddy;
using Microsoft.Xna.Framework.Audio;

namespace AudioBuddy
{
	/// <summary>
	/// This is a screen where you can do a music test.
	/// </summary>
	public class MusicTestScreen : MenuScreen
	{
		#region Properties

		/// <summary>
		/// The index of the music to play
		/// </summary>
		private int _playMusicIndex;

		/// <summary>
		/// The index of the music to push
		/// </summary>
		private int _pushMusicIndex;

		/// <summary>
		/// list of all the music cue names
		/// Add all your music cue name to this list
		/// </summary>
		private List<Filename> MusicNames { get; set; }

		private MenuEntry PlayMenuEntry { get; set; }
		private MenuEntry PushMenuEntry { get; set; }

		#endregion //Properties

		#region Methods

		public MusicTestScreen()
			: base("")
		{
			//quiet please
			AudioManager.StopMusic();

			//make sure this is a quiet menu.
			QuietMenu = true;

			//set up the lists
			MusicNames = new List<Filename>();
			_playMusicIndex = 0;
			_pushMusicIndex = 0;

			//setup the music option
			PlayMenuEntry = new MenuEntry(PlayText());
			PlayMenuEntry.Left += PrevPlayMusic;
			PlayMenuEntry.Right += NextPlayMusic;
			PlayMenuEntry.Selected += PlayMusic;
			MenuEntries.Add(PlayMenuEntry);

			PushMenuEntry = new MenuEntry(PushText());
			PushMenuEntry.Left += PrevPushMusic;
			PushMenuEntry.Right += NextPushMusic;
			PushMenuEntry.Selected += PushMusic;
			MenuEntries.Add(PushMenuEntry);

			var popMusic = new MenuEntry("Pop Music");
			popMusic.Selected += PopMusic;
			MenuEntries.Add(popMusic);

			var backMenuEntry = new MenuEntry("Back");
			backMenuEntry.Selected += OnCancel;
			MenuEntries.Add(backMenuEntry);

			SetTitleText();
		}

		/// <summary>
		/// Add all the music to this screen after it has been initialized.
		/// </summary>
		/// <param name="music"></param>
		public void AddMusic(List<Filename> music)
		{
			MusicNames.AddRange(music);
		}

		/// <summary>
		/// get the text for the music menu option
		/// </summary>
		/// <returns></returns>
		private string TitleText()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Currently Playing: ");
			sb.Append(AudioManager.CurrentMusicFile());
			return sb.ToString();
		}

		private void SetTitleText()
		{
			ScreenName = TitleText();
		}

		private string MusicIndex(int index)
		{
			if (index > 0)
			{
				return MusicNames[index - 1].GetFileNoExt();
			}
			else
			{
				return "None";
			}
		}

		private void PopMusic(object sender, PlayerIndexEventArgs e)
		{
			AudioManager.PopMusic();
			SetTitleText();
		}

		#region Play Music

		/// <summary>
		/// get the text for the music menu option
		/// </summary>
		/// <returns></returns>
		private string PlayText()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Play: ");
			sb.Append(MusicIndex(_playMusicIndex));
			return sb.ToString();
		}

		/// <summary>
		/// Increment the music that is playing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NextPlayMusic(object sender, EventArgs e)
		{
			//increment the music to play
			_playMusicIndex++;
			if (_playMusicIndex > MusicNames.Count) //add one for the "None" item
			{
				_playMusicIndex = 0;
			}

			//set the text of the menu option
			PlayMenuEntry.Text = PlayText();
		}

		/// <summary>
		/// decrement the music that is playing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PrevPlayMusic(object sender, EventArgs e)
		{
			//decrement the music to play
			_playMusicIndex--;
			if (_playMusicIndex < 0)
			{
				_playMusicIndex = MusicNames.Count; //add one for the "None" item
			}

			//set the text of the menu option
			PlayMenuEntry.Text = PlayText();
		}

		/// <summary>
		/// Play (or stop) the music.
		/// </summary>
		private void PlayMusic(object sender, PlayerIndexEventArgs e)
		{
			if (0 == _playMusicIndex)
			{
				//check if trying to stop the music
				AudioManager.StopMusic();
			}
			else
			{
				//play the selected song
				AudioManager.PlayMusic(MusicNames[_playMusicIndex - 1]);
			}
			SetTitleText();
		}

		#endregion //Play Music

		#region Push Music

		/// <summary>
		/// get the text for the music menu option
		/// </summary>
		/// <returns></returns>
		private string PushText()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Push: ");
			sb.Append(MusicIndex(_pushMusicIndex));
			return sb.ToString();
		}

		/// <summary>
		/// Increment the music that is playing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NextPushMusic(object sender, EventArgs e)
		{
			//increment the music to play
			_pushMusicIndex++;
			if (_pushMusicIndex > MusicNames.Count)
			{
				_pushMusicIndex = 0;
			}

			//set the text of the menu option
			PushMenuEntry.Text = PushText();
		}

		/// <summary>
		/// decrement the music that is playing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PrevPushMusic(object sender, EventArgs e)
		{
			//decrement the music to play
			_pushMusicIndex--;
			if (_pushMusicIndex < 0)
			{
				_pushMusicIndex = MusicNames.Count;
			}

			//set the text of the menu option
			PushMenuEntry.Text = PushText();
		}

		/// <summary>
		/// Play (or stop) the music.
		/// </summary>
		private void PushMusic(object sender, PlayerIndexEventArgs e)
		{
			if (0 == _pushMusicIndex)
			{
				//check if trying to stop the music
				AudioManager.StopMusic();
			}
			else
			{
				//play the selected song
				AudioManager.PushMusic(MusicNames[_pushMusicIndex - 1]);
			}
			SetTitleText();
		}

		#endregion //Push Music

		#endregion //Methods
	}
}