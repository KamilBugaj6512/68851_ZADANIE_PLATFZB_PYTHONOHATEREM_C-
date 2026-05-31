   using System;
using System.Collections.Generic;
using System.Threading;

namespace PlatformowkazBohaterem
{
    class Hero
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsJumping { get; private set; }
        private int jumpTicksLeft;

        public Hero(int startX, int startY) { X = startX; Y = startY; }

        public void Move(int dir, int max) => X = Math.Clamp(X + dir, 0, max - 1);

        public void StartJump(List<Platform> plats, int groundY)
        {
            if (!IsJumping && (Y == groundY || IsOnPlatform(plats)))
            { IsJumping = true; jumpTicksLeft = 5; }
        }

        public void UpdatePhysics(List<Platform> plats, int groundY)
        {
            if (IsJumping && jumpTicksLeft-- > 0) Y--;
            else { IsJumping = false; if (Y < groundY && !IsOnPlatform(plats)) Y++; }
        }

        private bool IsOnPlatform(List<Platform> plats) =>
            plats.Exists(p => Y == p.Y - 1 && X >= p.X && X < p.X + p.Width);
    }

    class Enemy
    {
        public int X { get; set; }
        public int Y { get; set; }
        private int dir = 1;
        public int MinX, MaxX;

        public Enemy(int x, int y, int min, int max) { X = x; Y = y; MinX = min; MaxX = max; }

        public void MovePatrol() { X += dir; if (X <= MinX || X >= MaxX) dir *= -1; }
    }

    class Platform
    {
        public int X, Y, Width;
        public Platform(int x, int y, int w) { X = x; Y = y; Width = w; }
    }

    class Item
    {
        public int X, Y;
        public bool IsCollected;
        public Item(int x, int y) { X = x; Y = y; }
    }

    class Level
    {
        public int Width => 40; public int Height => 15; public int GroundY => Height - 2;
        public Hero Player; public Enemy BadGuy; public Item Coin;
        public List<Platform> Platforms = new();
        public int CurrentLevelNumber { get; private set; } = 1;
        public bool LevelCompleted;

        public Level() => LoadLevel(1);

        public void LoadLevel(int lvl)
        {
            CurrentLevelNumber = lvl; LevelCompleted = false; Platforms.Clear();
            Player = new Hero(lvl == 1 ? 3 : 2, GroundY);
            BadGuy = lvl == 1 ? new Enemy(25, GroundY, 20, 35) : new Enemy(15, 9, 12, 22);
            Coin = new Item(lvl == 1 ? 18 : 32, lvl == 1 ? 5 : 7);

            if (lvl == 1) Platforms.AddRange(new[] { new Platform(5, 11, 8), new Platform(15, 8, 8), new Platform(26, 7, 10) });
            else Platforms.AddRange(new[] { new Platform(2, 12, 6), new Platform(11, 10, 12), new Platform(27, 9, 8) });
        }

        public void CheckCollisions()
        {
            if (Player.X == BadGuy.X && Player.Y == BadGuy.Y) EndGame("PRZEGRAŁEŚ! Wróg Cię dopadł.");
            if (Player.X == Coin.X && Player.Y == Coin.Y) { Coin.IsCollected = true; LevelCompleted = true; }
        }

        public void EndGame(string msg)
        {
            Console.Clear();
            Console.WriteLine($"=================================\n  {msg}\n=================================");
            Environment.Exit(0);
        }

        public void Render()
        {
            Console.SetCursorPosition(0, 0);
            for (int y = 0; y < Height; y++, Console.WriteLine())
                for (int x = 0; x < Width; x++)
                    if (x == Player.X && y == Player.Y) Console.Write("H");
                    else if (x == BadGuy.X && y == BadGuy.Y) Console.Write("E");
                    else if (!Coin.IsCollected && x == Coin.X && y == Coin.Y) Console.Write("$");
                    else if (Platforms.Exists(p => y == p.Y && x >= p.X && x < p.X + p.Width)) Console.Write("=");
                    else Console.Write(y == GroundY + 1 ? "_" : " ");

            Console.WriteLine($"{new string('-', Width)}\nPOZIOM: {CurrentLevelNumber} / 2\nSterowanie: A/D (Ruch), Spacja (Skok)");
        }
    }

    class Program
    {
        static void Main()
        {
            try { Console.WindowWidth = 50; Console.WindowHeight = 22; } catch { }
            Console.CursorVisible = false;
            Level lvl = new();

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.A) lvl.Player.Move(-1, lvl.Width);
                    if (k == ConsoleKey.D) lvl.Player.Move(1, lvl.Width);
                    if (k == ConsoleKey.Spacebar) lvl.Player.StartJump(lvl.Platforms, lvl.GroundY);
                }

                lvl.Player.UpdatePhysics(lvl.Platforms, lvl.GroundY);
                lvl.BadGuy.MovePatrol();
                lvl.CheckCollisions();

                if (lvl.LevelCompleted)
                {
                    if (lvl.CurrentLevelNumber == 1)
                    {
                        Console.Clear();
                        Console.WriteLine("=================================\n  POZIOM 1 UKOŃCZONY!\n  Kliknij klawisz, by wejść do Lvl 2...\n=================================");
                        Console.ReadKey(true);
                        lvl.LoadLevel(2);
                        Console.Clear();
                    }
                    else lvl.EndGame("GRATULACJE! Zebrałeś monetę!\n    UKOŃCZYŁEŚ CAŁĄ GRĘ (100%)");
                }

                lvl.Render();
                Thread.Sleep(90);
            }
        }
    }
}
