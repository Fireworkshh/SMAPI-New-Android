using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace StardewModdingAPI
{
    public class CommandConsole
    {
        private List<string> logMessages = new List<string>();  
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D backgroundTexture;

        public CommandConsole(SpriteBatch spriteBatch, SpriteFont font, Texture2D backgroundTexture)
        {
            this.spriteBatch = spriteBatch;
            this.font = font;
            this.backgroundTexture = backgroundTexture;
        }

        public void AddLogMessage(string message)
        {
            logMessages.Add(message);
            if (logMessages.Count > 20) 
                logMessages.RemoveAt(0); 

        }

        public void Draw()
        {
        
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 800, 600), Color.Black);

        
            int yOffset = 20;
            foreach (var message in logMessages)
            {
                spriteBatch.DrawString(font, message, new Vector2(10, yOffset), Color.White);
                yOffset += 30; 
            }
        }
    }
}
