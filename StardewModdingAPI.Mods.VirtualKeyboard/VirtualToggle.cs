using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Toolbar = StardewValley.Menus.Toolbar;

namespace StardewModdingAPI.Mods.VirtualKeyboard
{
	internal class VirtualToggle
	{
		private readonly IModHelper Helper;

		private readonly IMonitor Monitor;

		private int EnabledStage = 0;

		private bool AutoHidden = true;

		private bool IsDefault = true;

		private ClickableTextureComponent VirtualToggleButton;

		private List<KeyButton> Keyboard = new List<KeyButton>();

		private List<KeyButton> KeyboardExtend = new List<KeyButton>();

		private ModConfig ModConfig;

		private Texture2D Texture;

		private int LastPressTick = 0;

		public VirtualToggle(IModHelper helper, IMonitor monitor)
		{
			Monitor = monitor;
			Helper = helper;
			Texture = Helper.ModContent.Load<Texture2D>("assets/togglebutton.png");
			ModConfig = helper.ReadConfig<ModConfig>();
			for (int i = 0; i < ModConfig.buttons.Length; i++)
			{
				Keyboard.Add(new KeyButton(helper, ModConfig.buttons[i], Monitor));
			}
			for (int j = 0; j < ModConfig.buttonsExtend.Length; j++)
			{
				KeyboardExtend.Add(new KeyButton(helper, ModConfig.buttonsExtend[j], Monitor));
			}
			if (ModConfig.vToggle.rectangle.X != 36 || ModConfig.vToggle.rectangle.Y != 12)
			{
				IsDefault = false;
			}
			AutoHidden = ModConfig.vToggle.autoHidden;
			VirtualToggleButton = new ClickableTextureComponent(new Rectangle(Game1.toolbarPaddingX + 64, 12, 128, 128), Texture, new Rectangle(0, 0, 16, 16), 4f);
			helper.WriteConfig(ModConfig);
			Helper.Events.Display.Rendered += OnRendered;
			Helper.Events.Display.MenuChanged += OnMenuChanged;
			Helper.Events.Input.ButtonPressed += VirtualToggleButtonPressed;
		}

		private void OnMenuChanged(object sender, MenuChangedEventArgs e)
		{
			if (!AutoHidden || e.NewMenu == null)
			{
				return;
			}
			foreach (KeyButton item in Keyboard)
			{
				item.Hidden = true;
			}
			foreach (KeyButton item2 in KeyboardExtend)
			{
				item2.Hidden = true;
			}
			EnabledStage = 0;
		}

		private void VirtualToggleButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			Vector2 screenPixels = Utility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
			if (ModConfig.vToggle.key != 0 && e.Button == ModConfig.vToggle.key)
			{
				ToggleLogic();
			}
			else if (e.Button == SButton.MouseLeft && ShouldTrigger(screenPixels))
			{
				ToggleLogic();
			}
		}

		private void ToggleLogic()
		{
			switch (EnabledStage)
			{
			case 0:
				foreach (KeyButton item in Keyboard)
				{
					item.Hidden = false;
				}
				foreach (KeyButton item2 in KeyboardExtend)
				{
					item2.Hidden = true;
				}
				EnabledStage = 1;
				return;
			case 1:
				if (KeyboardExtend.Count <= 0)
				{
					break;
				}
				foreach (KeyButton item3 in KeyboardExtend)
				{
					item3.Hidden = false;
				}
				EnabledStage = 2;
				return;
			}
			foreach (KeyButton item4 in Keyboard)
			{
				item4.Hidden = true;
			}
			foreach (KeyButton item5 in KeyboardExtend)
			{
				item5.Hidden = true;
			}
			EnabledStage = 0;
			IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
			if (activeClickableMenu != null && !(Game1.activeClickableMenu is DialogueBox))
			{
				activeClickableMenu.exitThisMenu();
				StardewValley.Menus.Toolbar.toolbarPressed = true;
			}
		}

		private bool ShouldTrigger(Vector2 screenPixels)
		{
			int ticks = Game1.ticks;
			if (ticks - LastPressTick <= 6)
			{
				return false;
			}
			if (VirtualToggleButton.containsPoint((int)screenPixels.X, (int)screenPixels.Y))
			{
				LastPressTick = ticks;
				Toolbar.toolbarPressed = true;
				return true;
			}
			return false;
		}

		private void OnRendered(object sender, EventArgs e)
		{
			if (IsDefault)
			{
				if (Game1.options.verticalToolbar)
				{
					VirtualToggleButton.bounds.X = Game1.toolbarPaddingX + Toolbar.Instance.itemSlotSize + 200;
				}
				else
				{
					VirtualToggleButton.bounds.X = Game1.toolbarPaddingX + Toolbar.Instance.itemSlotSize + 50;
				}
				if (Toolbar.Instance.alignTop && !Game1.options.verticalToolbar)
				{
					object obj = Helper.Reflection.GetField<int>(Toolbar.Instance, "toolbarHeight").GetValue();
					VirtualToggleButton.bounds.Y = (int)obj + 50;
				}
				else
				{
					VirtualToggleButton.bounds.Y = 12;
				}
			}
			else
			{
				VirtualToggleButton.bounds.X = ModConfig.vToggle.rectangle.X;
				VirtualToggleButton.bounds.Y = ModConfig.vToggle.rectangle.Y;
			}
			float num = 1f;
			if (EnabledStage == 0)
			{
				num = 0.5f;
			}
			if (!Game1.eventUp && !(Game1.activeClickableMenu is GameMenu) && !(Game1.activeClickableMenu is ShopMenu))
			{
				num = 0.25f;
			}
			Matrix? matrix = ((Game1.spriteBatch.GetType().GetField("_spriteEffect", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(Game1.spriteBatch) is SpriteEffect spriteEffect) ? spriteEffect.TransformMatrix : null);
			Game1.spriteBatch.End();
			Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(1f));
			VirtualToggleButton.draw(Game1.spriteBatch, Color.White * num, 1E-06f, 0);
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
