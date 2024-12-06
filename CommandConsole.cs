using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace StardewModdingAPI
{
    public class CommandConsole
    {
        private List<string> logMessages = new List<string>();  // 存储日志消息
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
            if (logMessages.Count > 20)  // 限制显示的日志条数
                logMessages.RemoveAt(0);  // 删除最早的日志

        }

        public void Draw()
        {
            // 绘制背景
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 800, 600), Color.Black);

            // 绘制日志消息
            int yOffset = 20;
            foreach (var message in logMessages)
            {
                spriteBatch.DrawString(font, message, new Vector2(10, yOffset), Color.White);
                yOffset += 30;  // 每行消息间隔
            }
        }
    }
}
