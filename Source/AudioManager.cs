using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using FilenameBuddy;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace AudioBuddy
{
	/// <summary>
	/// Component that manages audio playback for all cues.
	/// </summary>
	public class AudioManager : GameComponent
	{
		#region Singleton

		/// <summary>
		/// The singleton for this type.
		/// </summary>
		private static AudioManager audioManager = null;

		#endregion //Singleton

		#region Audio Data

		/// <summary>
		/// The audio engine used to play all cues.
		/// </summary>
		private AudioEngine AudioEngine { get; set; }

		/// <summary>
		/// The SoundBank that contains all cues.
		/// </summary>
		private SoundBank SoundBank { get; set; }

		/// <summary>
		/// The WaveBank with all wave files for this game.
		/// </summary>
		private WaveBank WaveBank { get; set; }

		#endregion //Audio Data

		#region Initialization Methods

		/// <summary>
		/// Constructs the manager for audio playback of all cues.
		/// </summary>
		/// <param name="game">The game that this component will be attached to.</param>
		/// <param name="settingsFile">The filename of the XACT settings file.</param>
		/// <param name="waveBankFile">The filename of the XACT WaveBank file.</param>
		/// <param name="soundBankFile">The filename of the XACT SoundBank file.</param>
		private AudioManager(Game game, 
			string settingsFile,
			string waveBankFile,
			string soundBankFile)
			: base(game)
		{
			try
			{
				AudioEngine = new AudioEngine("Content\\" + settingsFile);
				SoundBank = new SoundBank(AudioEngine, "Content\\" + soundBankFile);
				WaveBank = new WaveBank(AudioEngine, "Content\\" + waveBankFile);

				_startSong = false;
				CurrentMusic = null;
			}
			catch (NoAudioHardwareException)
			{
				// silently fall back to silence
				AudioEngine = null;
				WaveBank = null;
				SoundBank = null;
			}
		}

		/// <summary>
		/// Initialize the static AudioManager functionality.
		/// </summary>
		/// <param name="game">The game that this component will be attached to.</param>
		/// <param name="settingsFile">The filename of the XACT settings file.</param>
		/// <param name="waveBankFile">The filename of the XACT WaveBank file.</param>
		/// <param name="soundBankFile">The filename of the XACT SoundBank file.</param>
		public static void Initialize(Game game,
			string settingsFile,
			string waveBankFile,
			string soundBankFile)
		{
			audioManager = new AudioManager(game, 
				settingsFile, 
				waveBankFile, 
				soundBankFile);
			if (game != null)
			{
				game.Components.Add(audioManager);
				audioManager.MusicManager = new ContentManager(game.Services, "Content");
			}
		}

		#endregion //Initialization Methods

		#region Cue Methods

		/// <summary>
		/// Retrieve a cue by name.
		/// </summary>
		/// <param name="cueName">The name of the cue requested.</param>
		/// <returns>The cue corresponding to the name provided.</returns>
		public static Cue GetCue(string cueName)
		{
			if (String.IsNullOrEmpty(cueName) ||
				(audioManager == null) ||
				(audioManager.AudioEngine == null) ||
				(audioManager.SoundBank == null) || 
				(audioManager.WaveBank == null))
			{
				return null;
			}
			return audioManager.SoundBank.GetCue(cueName);
		}

		/// <summary>
		/// Plays a cue by name.
		/// </summary>
		/// <param name="cueName">The name of the cue to play.</param>
		public static void PlayCue(string cueName)
		{
			if ((audioManager != null) &&
				(audioManager.AudioEngine != null) &&
				(audioManager.SoundBank != null) && 
				(audioManager.WaveBank != null))
			{
				audioManager.SoundBank.PlayCue(cueName);
			}
		}

		#endregion //Cue Methods

		#region Music

		/// <summary>
		/// Flag for whether or not the music needs to be restarted
		/// </summary>
		private bool _startSong;

		/// <summary>
		/// The background music
		/// </summary>
		private Song CurrentMusic { get; set; }

		/// <summary>
		/// Separate content manager for loading music content.
		/// </summary>
		private ContentManager MusicManager { get; set; }

		/// <summary>
		/// Stack of music cue names, for layered music playback.
		/// </summary>
		private Stack<Filename> _musicFileStack = new Stack<Filename>();

		/// <summary>
		/// Plays the desired music, clearing the stack of music cues.
		/// </summary>
		/// <param name="cueName">The name of the music cue to play.</param>
		public static void PlayMusic(Filename musicFile)
		{
			// start the new music cue
			if (audioManager != null)
			{
				audioManager._musicFileStack.Clear();
				PushMusic(musicFile);
			}
		}

		/// <summary>
		/// Plays the music for this game, adding it to the music stack.
		/// </summary>
		/// <param name="musicFile">The name of the music cue to play.</param>
		public static void PushMusic(Filename musicFile)
		{
			// start the new music cue
			if (audioManager != null)
			{
				//add to the queue
				audioManager._musicFileStack.Push(musicFile);

				audioManager.StartMusic(musicFile);
			}
		}

		/// <summary>
		/// Start playing a music file.
		/// </summary>
		/// <param name="musicFile"></param>
		private void StartMusic(Filename musicFile)
		{
			//TODO: stop the old music?
			//MeidaPlayer.Stop();
			//CurrentMusic.Dispose();

			//start the music
			CurrentMusic = MusicManager.Load<Song>(musicFile.GetRelPathFileNoExt());
			_startSong = true;
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = 1.0f;
		}

		/// <summary>
		/// Stops the current music and plays the previous music on the stack.
		/// </summary>
		public static void PopMusic()
		{
			// start the new music cue
			if (audioManager != null)
			{
				//get the previous music file from the stack
				if (audioManager._musicFileStack.Count > 0)
				{
					audioManager._musicFileStack.Pop();
					if (audioManager._musicFileStack.Count > 0)
					{
						//play the music?
						audioManager.StartMusic(audioManager._musicFileStack.Peek());
					}
				}
			}
		}

		/// <summary>
		/// Stop music playback, clearing the cue.
		/// </summary>
		public static void StopMusic()
		{
			if (audioManager != null)
			{
				audioManager._musicFileStack.Clear();
				MediaPlayer.Stop();
				audioManager._startSong = false;
				audioManager.CurrentMusic = null;
				audioManager.MusicManager.Unload();
			}
		}

		#endregion //Music

		#region Updating Methods

		/// <summary>
		/// Update the audio manager, particularly the engine.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		public override void Update(GameTime gameTime)
		{
			// update the audio engine
			if (AudioEngine != null)
			{
				AudioEngine.Update();
			}

			//restart the music?
			if (_startSong)
			{
				MediaPlayer.Play(CurrentMusic);
				_startSong = false;
			}

			base.Update(gameTime);
		}

		#endregion //Updating Methods

		#region Instance Disposal Methods

		/// <summary>
		/// Clean up the component when it is disposing.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					StopMusic();
					if (SoundBank != null)
					{
						SoundBank = null;
					}
					if (WaveBank != null)
					{
						WaveBank = null;
					}
					if (AudioEngine != null)
					{
						AudioEngine = null;
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion //Instance Disposal Methods
	}
}
