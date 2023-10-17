using trrne.Teeth;
using UnityEngine;

namespace trrne
{
    public static partial class Constant
    {
        public readonly struct Scenes
        {
            public const string
            Prefix = "Game",
            Game0 = "Game0",
            Game1 = "Game1",
            Game2 = "Game2";
        }

        public readonly struct Layers
        {
            public const int
            None = 1 << 0,
            Player = 1 << 6,
            Ground = 1 << 7,
            Object = 1 << 8,
            Creature = 1 << 9;
        }

        public readonly struct Keys
        {
            public static string
            Horizontal = "Horizontal",
            Vertical = "Vertical",
            Wheel = "Mouse ScrollWheel",
            Jump = "Jump",
            Down = "Down",
            Zoom = "Zoom",
            Button = "Button",
            Pause = "Pause";
        }

        public readonly struct Tags
        {
            public static string
            Manager = "Manager",
            Player = "Player",
            Ladder = "Ladder",
            Pad = "Pad",
            MainCamera = "MainCamera",
            Enemy = "Enemy",
            Panel = "Panel",
            Ice = "Ice",
            Hole = "Hole";
        }

        public readonly struct Animations
        {
            public static string
            Idle = "Idle",
            Jump = "Jump",
            Walk = "Walk",
            Venom = "Venom";
        }

        public readonly struct SpawnPositions
        {
            public static Vector2
            Stage1 = Vector100.zero;
        }
    }
}