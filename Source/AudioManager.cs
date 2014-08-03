using Microsoft.Xna.Framework;
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
		private AudioManager(Game game, string settingsFile, string waveBankFile, string soundBankFile)
			: base(game)
		{
			try
			{
				AudioEngine = new AudioEngine(settingsFile);
				SoundBank = new SoundBank(AudioEngine, soundBankFile);
				WaveBank = new WaveBank(AudioEngine, waveBankFile);
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
		public static void Initialize(Game game, string settingsFile, string waveBankFile, string soundBankFile)
		{
			audioManager = new AudioManager(game, settingsFile, waveBankFile, soundBankFile);
			if (game != null)
			{
				game.Components.Add(audioManager);
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
		/// The cue for the music currently playing, if any.
		/// </summary>
		private Cue musicCue;

		/// <summary>
		/// Stack of music cue names, for layered music playback.
		/// </summary>
		private Stack<string> musicCueNameStack = new Stack<string>();

		/// <summary>
		/// Plays the desired music, clearing the stack of music cues.
		/// </summary>
		/// <param name="cueName">The name of the music cue to play.</param>
		public static void PlayMusic(string cueName)
		{
			// start the new music cue
			if (audioManager != null)
			{
				audioManager.musicCueNameStack.Clear();
				PushMusic(cueName);
			}
		}

		/// <summary>
		/// Plays the music for this game, adding it to the music stack.
		/// </summary>
		/// <param name="cueName">The name of the music cue to play.</param>
		public static void PushMusic(string cueName)
		{
			// start the new music cue
			if ((audioManager != null) && 
				(audioManager.AudioEngine != null) &&
				(audioManager.SoundBank != null) && 
				(audioManager.WaveBank != null))
			{
				audioManager.musicCueNameStack.Push(cueName);
				if ((audioManager.musicCue == null) ||
					(audioManager.musicCue.Name != cueName))
				{
					if (audioManager.musicCue != null)
					{
						audioManager.musicCue.Stop(AudioStopOptions.AsAuthored);
						audioManager.musicCue.Dispose();
						audioManager.musicCue = null;
					}
					audioManager.musicCue = GetCue(cueName);
					if (audioManager.musicCue != null)
					{
						audioManager.musicCue.Play();
					}
				}
			}
		}

		/// <summary>
		/// Stops the current music and plays the previous music on the stack.
		/// </summary>
		public static void PopMusic()
		{
			// start the new music cue
			if ((audioManager != null) && 
				(audioManager.AudioEngine != null) &&
				(audioManager.SoundBank != null) && 
				(audioManager.WaveBank != null))
			{
				string cueName = null;
				if (audioManager.musicCueNameStack.Count > 0)
				{
					audioManager.musicCueNameStack.Pop();
					if (audioManager.musicCueNameStack.Count > 0)
					{
						cueName = audioManager.musicCueNameStack.Peek();
					}
				}
				if ((audioManager.musicCue == null) ||
					(audioManager.musicCue.Name != cueName))
				{
					if (audioManager.musicCue != null)
					{
						audioManager.musicCue.Stop(AudioStopOptions.AsAuthored);
						audioManager.musicCue.Dispose();
						audioManager.musicCue = null;
					}
					if (!String.IsNullOrEmpty(cueName))
					{
						audioManager.musicCue = GetCue(cueName);
						if (audioManager.musicCue != null)
						{
							audioManager.musicCue.Play();
						}
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
				audioManager.musicCueNameStack.Clear();
				if (audioManager.musicCue != null)
				{
					audioManager.musicCue.Stop(AudioStopOptions.AsAuthored);
					audioManager.musicCue.Dispose();
					audioManager.musicCue = null;
				}
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

			//if ((musicCue != null) && musicCue.IsStopped)
			//{
			//    AudioManager.PopMusic();
			//}

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
						SoundBank.Dispose();
						SoundBank = null;
					}
					if (WaveBank != null)
					{
						WaveBank.Dispose();
						WaveBank = null;
					}
					if (AudioEngine != null)
					{
						AudioEngine.Dispose();
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
