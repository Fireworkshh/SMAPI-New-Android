using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using Toolbar = StardewValley.Menus.Toolbar;

namespace StardewModdingAPI.Mods.VirtualKeyboard
{
	internal class KeyButton
	{
		private readonly Rectangle ButtonRectangle;

		private readonly SButton ButtonKey;

		private readonly float Transparency;

		private readonly string Alias;

		private readonly string Command;

		public bool Hidden;

		private bool RaisingPressed;

		private bool RaisingReleased;

		public KeyButton(IModHelper helper, ModConfig.VirtualButton buttonDefine, IMonitor monitor)
		{
			Hidden = true;
			ButtonRectangle = new Rectangle(buttonDefine.rectangle.X, buttonDefine.rectangle.Y, buttonDefine.rectangle.Width, buttonDefine.rectangle.Height);
			ButtonKey = buttonDefine.key;
			if (buttonDefine.alias == null)
			{
				Alias = ButtonKey.ToString();
			}
			else
			{
				Alias = buttonDefine.alias;
			}
			Command = buttonDefine.command;
			if (buttonDefine.transparency <= 0.01f || buttonDefine.transparency > 1f)
			{
				buttonDefine.transparency = 0.5f;
			}
			Transparency = buttonDefine.transparency;
			helper.Events.Display.Rendered += OnRendered;
			helper.Events.Input.ButtonReleased += EventInputButtonReleased;
			helper.Events.Input.ButtonPressed += EventInputButtonPressed;
		}

		private bool ShouldTrigger(Vector2 screenPixels, SButton button)
		{
			if (ButtonRectangle.Contains(screenPixels.X, screenPixels.Y) && !Hidden && button == SButton.MouseLeft)
			{
				if (!Hidden)
				{
					Toolbar.toolbarPressed = true;
				}
				return true;
			}
			return false;
		}

		private void EventInputButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (RaisingPressed)
			{
				return;
			}
			Vector2 screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
			if (ButtonKey != 0 && ShouldTrigger(screenPixels, e.Button))
			{
				RaisingPressed = true;
				if (Game1.input is StardewModdingAPI.Framework.Input.SInputState sInputState)
				{
					sInputState.OverrideButton(ButtonKey, setDown: true);
				}
				RaisingPressed = false;
			}
		}

		private void EventInputButtonReleased(object sender, ButtonReleasedEventArgs e)
		{
			if (RaisingReleased)
			{
				return;
			}
			Vector2 screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
			if (!ShouldTrigger(screenPixels, e.Button))
			{
				return;
			}
			if (ButtonKey == SButton.RightWindows)
			{
				KeyboardInput.Show("Command", "").ContinueWith(delegate(Task<string> s)
				{
					string result = s.Result;
					if (result.Length > 0)
					{
						SendCommand(result);
					}
					return result;
				});
			}
			else if (ButtonKey == SButton.RightControl)
			{
			//666	SGameConsole.Instance.Show();
			}
			else if (!string.IsNullOrEmpty(Command))
			{
				SendCommand(Command);
			}
			else
			{
				RaisingReleased = true;
				if (Game1.input is StardewModdingAPI.Framework.Input.SInputState sInputState)
				{
					sInputState.OverrideButton(ButtonKey, setDown: false);
				}
				RaisingReleased = false;
			}
		}

		private void SendCommand(string command)
		{
		//	StardewModdingAPI.Framework.SCore core = SMainActivity.Instance.core;
		//	core.RawCommandQueue?.Add(command);
		}

		private void OnRendered(object sender, EventArgs e)
		{
			if (!Hidden)
			{
				float num = Transparency;
				if (!Game1.eventUp && !(Game1.activeClickableMenu is GameMenu) && !(Game1.activeClickableMenu is ShopMenu) && Game1.activeClickableMenu == null)
				{
					num *= 0.5f;
				}
				Matrix? matrix = ((Game1.spriteBatch.GetType().GetField("_spriteEffect", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(Game1.spriteBatch) is SpriteEffect spriteEffect) ? spriteEffect.TransformMatrix : null);
				Game1.spriteBatch.End();
				Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(1f));
				IClickableMenu.drawTextureBoxWithIconAndText(Game1.spriteBatch, Game1.smallFont, Game1.mouseCursors, new Rectangle(256, 256, 10, 10), null, new Rectangle(0, 0, 1, 1), Alias, ButtonRectangle.X, ButtonRectangle.Y, ButtonRectangle.Width, ButtonRectangle.Height, Color.BurlyWood * num, 4f, drawShadow: true, iconLeft: false, isClickable: true, heldDown: false, drawIcon: false, reverseColors: false, bold: false);
				Game1.spriteBatch.End();
				if (matrix.HasValue)
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, matrix.Value);
				}
				else
				{
					Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				}
			}
		}
	}
}
