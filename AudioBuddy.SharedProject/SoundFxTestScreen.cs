using FilenameBuddy;
using InputHelper;
using MenuBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AudioBuddy
{
	/// <summary>
	/// This is a screen where you can do a sound fx test.
	/// </summary>
	public class SoundFxTestScreen : MenuScreen
	{
		#region Properties

		/// <summary>
		/// the index of the sound to play
		/// </summary>
		private int _soundIndex;

		/// <summary>
		/// list of all the sounds fx cues
		/// Add all your sound effect cues to this list
		/// </summary>
		private List<string> SoundEffectNames { get; set; }

		private List<SoundEffect> SoundEffects { get; set; }

		private MenuEntry SoundFxMenuEntry { get; set; }

		#endregion //Properties

		#region Methods

		public SoundFxTestScreen()
			: base("Sound Fx Test")
		{
			//quiet please
			AudioManager.StopMusic();

			//set up the lists
			SoundEffectNames = new List<string>() { "None" };
			SoundEffects = new List<SoundEffect>();
			_soundIndex = 0;
		}

		public override async Task LoadContent()
		{
			await base.LoadContent();

			//Setup the sound fx option
			SoundFxMenuEntry = new MenuEntry(SoundText(), Content)
			{
				IsQuiet = true
			};
#if ANDROID || __IOS__
			SoundFxMenuEntry.OnClick += NextSound;
#else
			SoundFxMenuEntry.Left += PrevSound;
			SoundFxMenuEntry.Right += NextSound;
			SoundFxMenuEntry.OnClick += PlaySound;
#endif
			AddMenuEntry(SoundFxMenuEntry);

			var backMenuEntry = new MenuEntry("Back", Content)
			{
				IsQuiet = true
			};
			backMenuEntry.OnClick += Cancelled;
			AddMenuEntry(backMenuEntry);
		}

		/// <summary>
		/// Add all the sound effect to this screen
		/// </summary>
		/// <param name="soundfx"></param>
		public void AddSoundFx(List<Filename> soundfx)
		{
			foreach (var sound in soundfx)
			{
				SoundEffectNames.Add(sound.GetFileNoExt());
				SoundEffects.Add(AudioManager.GetSoundEffect(sound));
			}
		}

		#region Sound Fx

		/// <summary>
		/// get the text for the sound effect menu entry
		/// </summary>
		/// <returns></returns>
		private string SoundText()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Sound: ");
			sb.Append(SoundEffectNames[_soundIndex]);
			return sb.ToString();
		}

		/// <summary>
		/// Increment the Sound that is playing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NextSound(object sender, EventArgs e)
		{
			//increment the Sound to play
			_soundIndex++;
			if (_soundIndex >= SoundEffectNames.Count)
			{
				_soundIndex = 0;
			}

			//set the text of the menu option
			SoundFxMenuEntry.Text = SoundText();

			//Play the sound fx
			PlaySound(sender, new ClickEventArgs(SoundFxMenuEntry.Position.ToVector2(), MouseButton.Left, null));
		}

		/// <summary>
		/// decrement the Sound that is playing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PrevSound(object sender, EventArgs e)
		{
			//decrement the Sound to play
			_soundIndex--;
			if (_soundIndex < 0)
			{
				_soundIndex = SoundEffectNames.Count - 1;
			}

			//set the text of the menu option
			SoundFxMenuEntry.Text = SoundText();

			//Play the sound fx
			PlaySound(sender, new ClickEventArgs(SoundFxMenuEntry.Position.ToVector2(), MouseButton.Left, null));
		}

		/// <summary>
		/// Play (or stop) the Sound.
		/// </summary>
		private void PlaySound(object sender, ClickEventArgs e)
		{
			if (0 != _soundIndex)
			{
				//play the selected song
				SoundEffects[_soundIndex - 1].Play();
			}
		}

		#endregion //Sound Fx

		#endregion //Methods
	}
}