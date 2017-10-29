﻿using System;
using System.Reflection;
using System.Text;
using Land.Classes;
using Land.Common;
using Land.Enums;
using Land.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Land.Components
{
    public class PlayingStartedEventArgs : EventArgs
    {
        public int Range { get; set; }
    }

    public class Splash : BaseDrawableGameComponent
    {
        private readonly StringBuilder _infoText = new StringBuilder();
        private readonly Vector2 _infoVector = new Vector2(1*16, 17*32);
        private readonly Vector2 _logoVector = new Vector2(4*16, 1*32);
        private DisplayModeEnum _displayMode = DisplayModeEnum.Splash;
        private KeyboardState _oldKeyState;
        private TimeSpan _splashInterval;
        private readonly Version _version;

        public Splash(TheGame game)
            : base(game)
        {
            _version = Assembly.GetEntryAssembly().GetName().Version;            
            _infoText.Append(
                "This is retrospective of the formerly popular game \"LAND\". It was originally developed by ASP corp. in 1986 and ran under PDP-11 compatible computers.\r\n");
            _infoText.Append(
                string.Format("The game was ported using MonoGame by Andriy S'omak (semack@gmail.com), 2014-2017. Source code http://www.github.com/onlinico/land"));
        }


        public event EventHandler<PlayingStartedEventArgs> OnPlayingStarted;

        protected override void OnVisibleChanged(object sender, EventArgs args)
        {
            _splashInterval = new TimeSpan();
            base.OnVisibleChanged(sender, args);
        }


        private void UpdateSplash(KeyboardState kState, GameTime gameTime)
        {
            if (kState.IsKeyPressed(_oldKeyState, Keys.Enter, Keys.Space, Keys.Escape, Keys.D0, Keys.D1, Keys.D2,
                Keys.D3,
                Keys.D4, Keys.D5, Keys.D6, Keys.D7))
            {
                _displayMode = DisplayModeEnum.GameStart;
                _splashInterval = new TimeSpan(Game.GameSpeedScaleFactor*40);
            }

            _splashInterval = _splashInterval - gameTime.ElapsedGameTime;
            if (_splashInterval.Ticks < 0)
            {
                Game.BackColor = (Game.BackColor == BackColorEnum.White ? BackColorEnum.Black : BackColorEnum.White);
                _splashInterval = new TimeSpan(Game.GameSpeedScaleFactor*20);
            }
        }

        private void UpdateGameStart(KeyboardState kState, GameTime gameTime)
        {
            if (kState.IsKeyPressed(_oldKeyState, Keys.Enter, Keys.Space, Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4,
                Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9))
            {
                _displayMode = DisplayModeEnum.Splash;
                string value = Encoding.UTF8.GetString(new[] {(byte) kState.GetPressedKeys()[0]}, 0, 1);
                int newRange;
                int range = int.TryParse(value, out newRange) ? newRange : Game.Range;

                if (OnPlayingStarted != null)
                {
                    OnPlayingStarted(this, new PlayingStartedEventArgs
                    {
                        Range = range
                    });
                }
                return;
            }
            _splashInterval = _splashInterval - gameTime.ElapsedGameTime;
            if (_splashInterval.Ticks < 0)
            {
                _displayMode = DisplayModeEnum.Splash;
            }
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState kState = Keyboard.GetState();

            if (_displayMode == DisplayModeEnum.Splash)
            {
                UpdateSplash(kState, gameTime);
            }
            else
            {
                UpdateGameStart(kState, gameTime);
            }
            base.Update(gameTime);
            _oldKeyState = kState;
        }

        public void DrawSplash(SpriteBatch spriteBatch)
        {
            Game.DrawScores(spriteBatch);
            spriteBatch.Draw(Game.Sprites[SpriteTypeEnum.Splash, Game.BackColor].Texture, _logoVector, Game.ForegroudColor);
            spriteBatch.DrawString(Game.InfoFont, _infoText, _infoVector, Game.ForegroudColor);
            spriteBatch.DrawString(Game.InfoFont, string.Format("Using bank of maps \"{0}\", press F10 to change.", Maps.Banks[Game.MapBank]),
                new Vector2(1 * 16, 16 * 32 + 10), Game.ForegroudColor);

            spriteBatch.DrawString(Game.InfoFont, string.Format("Version {0}", _version),
                new Vector2(44*16, 16*32 + 10), Game.ForegroudColor);            
        }

        public void DrawGameStart(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Maps.CapacityX; i++)
            {
                for (int j = 0; j < Maps.CapacityY + 2; j++)
                {
                    if (i == 0 || i == Maps.CapacityX - 1)
                        spriteBatch.Draw(Game.Sprites[SpriteTypeEnum.StoneWall, Game.BackColor].Texture,
                            new Vector2(i*16, j*32), Color.White);
                    if (j == 0 || j == Maps.CapacityY + 2 - 1)
                        spriteBatch.Draw(Game.Sprites[SpriteTypeEnum.StoneWall, Game.BackColor].Texture,
                            new Vector2(i*16, j*32), Color.White);
                }
            }
            spriteBatch.Draw(Game.Sprites[SpriteTypeEnum.YourRangeLabel, Game.BackColor].Texture, new Vector2(20*16, 8*32),
                Color.White);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Game.BackgroundColor);
            Game.SpriteBatch.Begin();
            if (_displayMode == DisplayModeEnum.Splash)
                DrawSplash(Game.SpriteBatch);
            else
                DrawGameStart(Game.SpriteBatch);
            Game.SpriteBatch.End();
            base.Draw(gameTime);
        }

        private enum DisplayModeEnum
        {
            Splash,
            GameStart
        }
    }
}
