﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TowerDefense
{
    class MessageLog
    {
        String string1 = "";
        public MessageLog()
        {
        }
        public void Draw(SpriteBatch batch, SpriteFont font, Viewport viewport)
        {
            int stringlength1 = (int)font.MeasureString(string1).X + 10;
            int stringlength2 = (int)font.MeasureString(string1).Y + 10;
            batch.DrawString(font, string1, new Vector2(viewport.Width - stringlength1, viewport.Height - stringlength2), Color.Black,
                0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0.5f);
        }

        public void Level( int level)
        {
            string1 = "Level - " + level;
        }
        public void GameOver()
        {
            string1 = "GAME OVER!";
        }
        public void NotEnoughGold()
        {
            string1 = "Insufficient Gold";
        }
        public void LevelComplete(int gold, int level)
        {
            string1 = "level - " + level + " complete!  + " + gold + "$";
        }


    }
}