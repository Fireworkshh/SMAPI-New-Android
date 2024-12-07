using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace StardewModdingAPI
{
    public class CommandConsole
    {
        private List<string> logMessages = new List<string>();  // �洢��־��Ϣ
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
            if (logMessages.Count > 20)  // ������ʾ����־����
                logMessages.RemoveAt(0);  // ɾ���������־

        }

        public void Draw()
        {
            // ���Ʊ���
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 800, 600), Color.Black);

            // ������־��Ϣ
            int yOffset = 20;
            foreach (var message in logMessages)
            {
                spriteBatch.DrawString(font, message, new Vector2(10, yOffset), Color.White);
                yOffset += 30;  // ÿ����Ϣ���
            }
        }
    }
}
