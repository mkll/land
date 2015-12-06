﻿using System;
using Land.Classes;
using Land.Common;
using Land.Components.Actors;
using Land.Enums;
using Land.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Land.Components
{
    public class Room : BaseDrawableGameComponent
    {
        private readonly Biomass _biomass;
        private readonly Bullet _bullet;
        private readonly Devil _devil1;
        private readonly Devil _devil2;
        private readonly Hero _hero;
        private readonly Wall _wall;
        private int _attempts;
        private SpriteTypeEnum[,] _map;
        private GamePadState _oldButtonState;
        private KeyboardState _oldKeyState;
        public int Score;
        private int _stage;

        public Room(TheGame game)
            : base(game)
        {
            _bullet = new Bullet(Game, this);
            _hero = new Hero(Game, this, _bullet);
            _hero.OnChestHappened += OnHeroChestHappened;
            _hero.OnLifeFired += OnHeroLifeFired;
            _hero.OnRoomFinished += OnHeroRoomFinished;
            _biomass = new Biomass(Game, this);
            _wall = new Wall(Game, this);
            _devil1 = new Devil(Game, this, _hero, DevilNumberEnum.First);
            _devil2 = new Devil(Game, this, _hero, DevilNumberEnum.Second);
            _devil1.OnLifeFired += OnHeroLifeFired;
            _devil2.OnLifeFired += OnHeroLifeFired;
            _hero.OnReportPostion += OnCheckCollision;
            Reset();
        }


        public SpriteTypeEnum this[int x, int y]
        {
            get { return _map[x, y]; }
            set { _map[x, y] = value; }
        }

        private void OnCheckCollision(object sender, ReportPostionEventArgs e)
        {
            if (!_devil1.HasCaught && !_devil2.HasCaught)
            {
                _devil1.HasCaught = ((e.X == _devil1.X || e.X + 1 == _devil1.X || e.X == _devil1.X + 1 ||
                                      e.X + 1 == _devil1.X + 1) && (e.Y == _devil1.Y));
                _devil2.HasCaught = ((e.X == _devil2.X || e.X + 1 == _devil2.X || e.X == _devil2.X + 1 ||
                                      e.X + 1 == _devil2.X + 1) && (e.Y == _devil2.Y));
                if (_devil1.HasCaught || _devil2.HasCaught)
                    _hero.Visible = false;
            }
        }

        public event EventHandler OnPlayingFinished;

        private void OnHeroRoomFinished(object sender, EventArgs e)
        {
            SetNextStage();
        }

        private void OnHeroLifeFired(object sender, EventArgs e)
        {
            _attempts--;
            if (_attempts <= 0)
            {
                if (OnPlayingFinished != null)
                    OnPlayingFinished(this, new EventArgs());
            }
            else
                SetStage(_stage);
        }

        private void OnHeroChestHappened(object sender, EventArgs e)
        {
            Score += 13;
            if (Score > 99999)
                Score = 0;
        }

        public void Reset()
        {
            Score = 0;
            _stage = 1;
            _attempts = 20;
            SetStage(_stage);
        }


        public override void Initialize()
        {
            Game.Components.Add(_hero);
            Game.Components.Add(_biomass);
            Game.Components.Add(_bullet);
            Game.Components.Add(_wall);
            Game.Components.Add(_devil1);
            Game.Components.Add(_devil2);
            SetStage(_stage);
            base.Initialize();
        }

        protected override void OnEnabledChanged(object sender, EventArgs args)
        {
            _hero.Show(Enabled);
            _biomass.Enabled = Enabled;
            _bullet.Show(Enabled);
            _wall.Enabled = Enabled;
            _devil1.Show(Enabled);
            _devil2.Show(Enabled);
            base.OnEnabledChanged(sender, args);
        }

        private void SetStage(int stage)
        {
            _stage = stage;
            _map = Maps.Get(Game.MapBank, _stage);
            _hero.Reset();
            _bullet.Reset(1, 1, DirectionEnum.None);
            _devil1.Reset();
            _devil2.Reset();
        }

        private void SetNextStage()
        {
            if (_stage >= Maps.GetMapsCount(Game.MapBank))
                _stage = 1;
            else
                _stage++;
            SetStage(_stage);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            KeyboardState kState = Keyboard.GetState();
            GamePadState bState = GamePad.GetState(PlayerIndex.One);

            if (kState.IsKeyPressed(_oldKeyState, Keys.Q) || bState.IsButtonPressed(_oldButtonState, Buttons.Back))
            {
                if (OnPlayingFinished != null)
                    OnPlayingFinished(this, new EventArgs());
            }
            else if (kState.IsKeyPressed(_oldKeyState, Keys.OemSemicolon) ||
                     bState.IsButtonPressed(_oldButtonState, Buttons.Start))
            {
                Score = Score - 100;
                if (Score < 0)
                    Score = 0;
                SetNextStage();
            }
            else if (kState.IsKeyPressed(_oldKeyState, Keys.R))
                OnHeroLifeFired(this, new EventArgs());
            _oldKeyState = kState;
            _oldButtonState = bState;
        }



        private void DrawInfoPanel(SpriteBatch spriteBatch)
        {
            Game.DrawScores(spriteBatch);
            spriteBatch.Draw(Game.Sprites[SpriteTypeEnum.RangeLabel, Game.BackColor].Texture, new Vector2(16*16, 0),
                Color.White);
            spriteBatch.Draw(Game.Sprites[SpriteTypeEnum.AttemptsLabel, Game.BackColor].Texture, new Vector2(27*16, 0),
                Color.White);
            spriteBatch.Draw(Game.Sprites[SpriteTypeEnum.StageLabel, Game.BackColor].Texture, new Vector2(42*16, 0),
                Color.White);
            for (int i = 0; i < Maps.CapacityX; i++)
            {
                spriteBatch.Draw(Game.Sprites[SpriteTypeEnum.Delimiter, Game.BackColor].Texture, new Vector2(i*16, 1*32),
                    Color.White);
            }

            spriteBatch.DrawString(Game.GameFont, string.Format("{0:D2}", Game.Range), new Vector2(22*16, 0), Game.ForegroudColor);
            spriteBatch.DrawString(Game.GameFont, string.Format("{0:D2}", _attempts), new Vector2(36 * 16, 0), Game.ForegroudColor);
            spriteBatch.DrawString(Game.GameFont, string.Format("{0:D2}", _stage), new Vector2(46 * 16, 0), Game.ForegroudColor);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Game.BackgroundColor);

            Game.SpriteBatch.Begin();
            DrawInfoPanel(Game.SpriteBatch);
            for (int x = 0; x < Maps.CapacityX; x++)
            {
                for (int y = 0; y < Maps.CapacityY; y++)
                {
                    SpriteTypeEnum item = this[x, y];
                    Game.SpriteBatch.Draw(Game.Sprites[item, Game.BackColor].Texture, new Vector2(x*16, (y + 2)*32),
                        Color.White);
                }
            }
            Game.SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
