using AudioBuddy;
using MenuBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AudioBuddyTest
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
#if !__IOS__ && !ANDROID
	public class Game1 : MouseGame
#else
	public class Game1 : TouchGame
#endif
	{
		public Game1()
		{
			Graphics.SupportedOrientations = DisplayOrientation.Portrait | DisplayOrientation.PortraitDown;

			IsMouseVisible = true;

			VirtualResolution = new Point(720, 1280);
			ScreenResolution = new Point(720, 1280);

			Fullscreen = false;
			Letterbox = false;
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			AudioManager.Initialize(this);

			base.LoadContent();
		}

		public override IScreen[] GetMainMenuScreenStack()
		{
			return new IScreen[] { new MainMenuScreen() };
		}
	}
}

