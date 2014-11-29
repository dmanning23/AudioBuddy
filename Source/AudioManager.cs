using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
			audioManager = new AudioManager(game);
			if (game != null)
			{
				game.Components.Add(audioManager);
				audioManager.MusicManager = new ContentManager(game.Services, "Content");
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
			Debug.Assert(null != soundFxName);

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

		/// <summary>
		/// Flag for whether or not the music needs to be restarted
		/// </summary>
		private bool _startSong;

		/// <summary>
		/// The background music
		/// </summary>
#if WINDOWS
		private WinmmWrapper CurrentMusic { get; set; }
#else
		private Song CurrentMusic { get; set; }
#endif

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
		public static void PlayMusic(Filename musicFile, bool loop = true)
		{
			// start the new music cue
			if (audioManager != null)
			{
				audioManager._musicFileStack.Clear();
				PushMusic(musicFile, loop);
			}
		}

		/// <summary>
		/// Plays the music for this game, adding it to the music stack.
		/// </summary>
		/// <param name="musicFile">The name of the music cue to play.</param>
		public static void PushMusic(Filename musicFile, bool loop = true)
		{
			// start the new music cue
			if (audioManager != null)
			{
				//add to the queue
				audioManager._musicFileStack.Push(musicFile);
				audioManager.StartMusic(musicFile, loop);
			}
		}

		/// <summary>
		/// Start playing a music file.
		/// </summary>
		/// <param name="musicFile"></param>
		private void StartMusic(Filename musicFile, bool loop)
		{
			//load the music
#if WINDOWS
			CurrentMusic = new WinmmWrapper();
#else
			CurrentMusic = MusicManager.Load<Song>(musicFile.GetRelPathFileNoExt());
#endif
			_startSong = true;
			MediaPlayer.IsRepeating = loop;
			MediaPlayer.Volume = 0.7f;
			CurrentSongFile = musicFile;
		}

		/// <summary>
		/// Stops the current music and plays the previous music on the stack.
		/// </summary>
		public static void PopMusic()
		{
			//get the previous music file from the stack
			if ((audioManager != null) && (audioManager._musicFileStack.Count > 0))
			{
				audioManager._musicFileStack.Pop();
				if (audioManager._musicFileStack.Count > 0)
				{
					//play the music (if popping off, want to loop the prev track)
					audioManager.StartMusic(audioManager._musicFileStack.Peek(), true);
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
#if WINDOWS
				if (null != audioManager.CurrentMusic)
				{
					audioManager.CurrentMusic.Close();
				}
#else
				MediaPlayer.Stop();
#endif
				audioManager._startSong = false;
				audioManager.CurrentMusic = null;
				audioManager.MusicManager.Unload();
				audioManager.CurrentSongFile = null;
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
			//restart the music?
			if (_startSong)
			{
#if WINDOWS
				CurrentMusic.Close();
				CurrentMusic.Open(CurrentSongFile.File);
				CurrentMusic.Play(true);
#else
				MediaPlayer.Play(CurrentMusic);
#endif
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
