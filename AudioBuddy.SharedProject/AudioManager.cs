using FilenameBuddy;
using GameTimer;
using MenuBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using MusicPlayer = Microsoft.Xna.Framework.Media.MediaPlayer; //need this because of namespace clash on iOS

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
		/// content manage rused to load soud effects
		/// </summary>
		private ContentManager _content;

		#endregion //Audio Data

		#region Initialization Methods

		/// <summary>
		/// Constructs the manager for audio playback of all cues.
		/// </summary>
		/// <param name="game">The game that this component will be attached to.</param>
		/// <param name="settingsFile">The filename of the XACT settings file.</param>
		/// <param name="waveBankFile">The filename of the XACT WaveBank file.</param>
		/// <param name="soundBankFile">The filename of the XACT SoundBank file.</param>
		private AudioManager(Game game)
			: base(game)
		{
			try
			{
				_content = game.Content;
				_startSong = false;
				CurrentMusic = null;
				FadeTimer = new CountdownTimer();
				FadeTimer.Stop();
			}
			catch (NoAudioHardwareException)
			{
				_content = null;
			}
		}

		/// <summary>
		/// Initialize the static AudioManager functionality.
		/// </summary>
		/// <param name="game">The game that this component will be attached to.</param>
		/// <param name="settingsFile">The filename of the XACT settings file.</param>
		/// <param name="waveBankFile">The filename of the XACT WaveBank file.</param>
		/// <param name="soundBankFile">The filename of the XACT SoundBank file.</param>
		public static void Initialize(Game game)
		{
#if !DESKTOP
			audioManager = new AudioManager(game);
#endif
			if (game != null)
			{
				game.Components.Add(audioManager);
				if (null != audioManager)
				{
					audioManager.MusicManager = new ContentManager(game.Services, "Content");
				}
			}
		}

		#endregion //Initialization Methods

		#region Sound Effect Methods

		/// <summary>
		/// Retrieve a sound effect by name.
		/// </summary>
		/// <param name="soundFxName">The name of the cue requested.</param>
		/// <returns>The cue corresponding to the name provided.</returns>
		public static SoundEffect GetSoundEffect(Filename soundFxName)
		{
			if (String.IsNullOrEmpty(soundFxName.ToString()) ||
				(audioManager == null) ||
				(audioManager._content == null))
			{
				return null;
			}
			return audioManager._content.Load<SoundEffect>(soundFxName.GetRelPathFileNoExt());
		}

		#endregion //Sound Effect Methods

		#region Music

		const float MaxVolume = 0.7f;

		/// <summary>
		/// Flag for whether or not the music needs to be restarted
		/// </summary>
		private bool _startSong;

		/// <summary>
		/// The background music
		/// </summary>
		private Song CurrentMusic { get; set; }

		private Filename CurrentSongFile { get; set; }

		/// <summary>
		/// Separate content manager for loading music content.
		/// </summary>
		private ContentManager MusicManager { get; set; }

		/// <summary>
		/// Stack of music cue names, for layered music playback.
		/// </summary>
		private Stack<Filename> _musicFileStack = new Stack<Filename>();

		/// <summary>
		/// Timer used to fade the music out
		/// </summary>
		private CountdownTimer FadeTimer { get; set; }

		private float FadeTime { get; set; }

		/// <summary>
		/// Get the name of the file currently being played
		/// </summary>
		/// <returns></returns>
		public static string CurrentMusicFile()
		{
			if ((audioManager != null) && (null != audioManager.CurrentSongFile))
			{
				return audioManager.CurrentSongFile.GetFileNoExt();
			}
			else
			{
				return "None";
			}
		}

		/// <summary>
		/// Plays the desired music, clearing the stack of music cues.
		/// </summary>
		/// <param name="cueName">The name of the music cue to play.</param>
		public static void PlayMusic(Filename musicFile, bool loop = true, float volume = 1f)
		{
			// start the new music cue
			if (audioManager != null)
			{
				audioManager._musicFileStack.Clear();
				PushMusic(musicFile, loop, volume);
			}
		}

		/// <summary>
		/// Plays the music for this game, adding it to the music stack.
		/// </summary>
		/// <param name="musicFile">The name of the music cue to play.</param>
		public static void PushMusic(Filename musicFile, bool loop = true, float volume = 1f)
		{
			// start the new music cue
			if (audioManager != null)
			{
				//add to the queue
				audioManager._musicFileStack.Push(musicFile);
				audioManager.StartMusic(musicFile, loop, volume);
			}
		}

		/// <summary>
		/// Start playing a music file.
		/// </summary>
		/// <param name="musicFile"></param>
		private void StartMusic(Filename musicFile, bool loop, float volume)
		{
			FadeTimer.Stop();

			//load the music
			CurrentMusic = MusicManager.Load<Song>(musicFile.GetRelPathFileNoExt());

			_startSong = true;
			MusicPlayer.IsRepeating = loop;
			MusicPlayer.Volume = volume * MaxVolume;
			CurrentSongFile = musicFile;
		}

		/// <summary>
		/// Stops the current music and plays the previous music on the stack.
		/// </summary>
		public static void PopMusic(float volume = 1f)
		{
			//get the previous music file from the stack
			if ((audioManager != null) && (audioManager._musicFileStack.Count > 0))
			{
				audioManager._musicFileStack.Pop();
				if (audioManager._musicFileStack.Count > 0)
				{
					//play the music (if popping off, want to loop the prev track)
					audioManager.StartMusic(audioManager._musicFileStack.Peek(), true, volume);
				}
				else
				{
					StopMusic();
					audioManager.CurrentSongFile = null;
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
				var state = MusicPlayer.State;
				MusicPlayer.Stop();
				audioManager._startSong = false;
				audioManager.CurrentMusic = null;
				audioManager.MusicManager.Unload();
				audioManager.CurrentSongFile = null;
			}
		}

		public static void FadeMusic(float fadeTime)
		{
			if (audioManager != null)
			{
				if (0f < fadeTime)
				{
					audioManager.FadeTime = fadeTime;
					audioManager.FadeTimer.Start(fadeTime);
				}
			}
		}

		public static void Pause(bool pauseUnpause)
		{
			if (audioManager != null)
			{
				if (pauseUnpause)
				{
					MusicPlayer.Stop();
				}
				else
				{
					audioManager._startSong = true;
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
			try
			{
				FadeTimer.Update(gameTime);

				//restart the music?
				if (_startSong)
				{
					//set the flag first, in case the mediaplayer bombs
					_startSong = false;
					MusicPlayer.Play(CurrentMusic);
				}

				if (!FadeTimer.Paused && FadeTimer.HasTimeRemaining)
				{
					MusicPlayer.Volume = MusicPlayer.Volume - (FadeTimer.TimeDelta * (1f / FadeTime));
				}
				else if (!FadeTimer.Paused && !FadeTimer.HasTimeRemaining)
				{
					FadeTimer.Stop();
					StopMusic();
				}

				base.Update(gameTime);
			}
			catch (Exception ex)
			{
				var screenManager = Game.Services.GetService(typeof(IScreenManager)) as IScreenManager;
				screenManager.ErrorScreen(ex);
			}
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
					CurrentMusic = null;
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
