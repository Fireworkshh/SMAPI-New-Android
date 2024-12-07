namespace StardewModdingAPI.Mods.VirtualKeyboard
{
	internal class ModEntry : Mod
	{
		public static float ZoomScale;

		public override void Entry(IModHelper helper)
		{
			new VirtualToggle(helper, base.Monitor);
		}
	}
}
