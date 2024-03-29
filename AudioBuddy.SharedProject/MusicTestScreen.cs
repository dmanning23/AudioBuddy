﻿using FilenameBuddy;
using InputHelper;
using MenuBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using ResolutionBuddy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer = Microsoft.Xna.Framework.Media.MediaPlayer; //need this because of namespace clash on iOS

namespace AudioBuddy
{
	/// <summary>
	/// This is a screen where you can do a music test.
	/// </summary>
	public class MusicTestScreen : MenuStackScreen
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

		Label volumeLabel;

		#endregion //Properties

		#region Methods

		public MusicTestScreen()
			: base("Music Test")
		{
			//quiet please
			AudioManager.StopMusic();

			//set up the lists
			MusicNames = new List<Filename>();
			_playMusicIndex = 0;
			_pushMusicIndex = 0;
		}

		public override async Task LoadContent()
		{
			await base.LoadContent();

			SetTitleText();

			//setup the music option
			PlayMenuEntry = new MenuEntry(PlayText(), Content)
			{
				Scale = 0.5f,
				IsQuiet = true
			};
			//PlayMenuEntry.Left += PrevPlayMusic;
			//PlayMenuEntry.Right += NextPlayMusic;

			PlayMenuEntry.OnClick += NextPlayMusic;
			PlayMenuEntry.OnClick += PlayMusic;

			AddMenuEntry(PlayMenuEntry);

			PushMenuEntry = new MenuEntry(PushText(), Content)
			{
				Scale = 0.5f,
				IsQuiet = true
			};
			//PushMenuEntry.Left += PrevPushMusic;
			//PushMenuEntry.Right += NextPushMusic;

			PushMenuEntry.OnClick += NextPushMusic;
			PushMenuEntry.OnClick += PushMusic;

			AddMenuEntry(PushMenuEntry);

			var popMusic = new MenuEntry("Pop Music", Content)
			{
				Scale = 0.5f,
				IsQuiet = true
			};
			popMusic.OnClick += PopMusic;
			AddMenuEntry(popMusic);

			volumeLabel = new Label(MusicPlayer.Volume.ToString(), Content, FontSize.Small)
			{
				Position = new Point (Resolution.TitleSafeArea.Left, Resolution.TitleSafeArea.Top),
				Horizontal = HorizontalAlignment.Left,
				Vertical = VerticalAlignment.Top
			};
			AddItem(volumeLabel);

			var volumeUp = new MenuEntry("Volume Up", Content)
			{
				Scale = 0.5f,
				IsQuiet = true
			};
			volumeUp.OnClick += VolumeUp_OnClick;
			AddMenuEntry(volumeUp);
			var volumeDown = new MenuEntry("Volume Down", Content)
			{
				Scale = 0.5f,
				IsQuiet = true
			};
			volumeDown.OnClick += VolumeDown_OnClick;
			AddMenuEntry(volumeDown);

			var backMenuEntry = new MenuEntry("Back", Content)
			{
				Scale = 0.5f,
				IsQuiet = true
			};
			backMenuEntry.OnClick += Cancelled;
			AddMenuEntry(backMenuEntry);
		}

		private void VolumeUp_OnClick(object sender, ClickEventArgs e)
		{
			AudioManager.ChangeMusicVolume(1f, 2f);
		}

		private void VolumeDown_OnClick(object sender, ClickEventArgs e)
		{
			AudioManager.ChangeMusicVolume(0.25f, 2f);
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

		private void PopMusic(object sender, ClickEventArgs e)
		{
			AudioManager.PopMusic();
			SetTitleText();
		}

		public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
		{
			base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

			volumeLabel.Text = MusicPlayer.Volume.ToString();
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
		private void PlayMusic(object sender, ClickEventArgs e)
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
		private void PushMusic(object sender, ClickEventArgs e)
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