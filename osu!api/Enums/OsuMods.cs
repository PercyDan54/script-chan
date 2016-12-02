using System;

namespace Osu.Api
{
    /// <summary>
    /// Represents the osu! mods
    /// </summary>
    [FlagsAttribute]
    public enum OsuMods
    {
        /// <summary>
        /// Nomod
        /// </summary>
        None = 0,

        /// <summary>
        /// Nofail
        /// </summary>
        NoFail = 1,

        /// <summary>
        /// Easy
        /// </summary>
        Easy = 2,

        /// <summary>
        /// NoVideo. No longer used ?
        /// </summary>
        NoVideo = 4,

        /// <summary>
        /// Hidden
        /// </summary>
        Hidden = 8,

        /// <summary>
        /// Hardrock
        /// </summary>
        HardRock = 16,

        /// <summary>
        /// SuddenDeath
        /// </summary>
        SuddenDeath = 32,

        /// <summary>
        /// DoubleTime
        /// </summary>
        DoubleTime = 64,

        /// <summary>
        /// Relax
        /// </summary>
        Relax = 128,

        /// <summary>
        /// HalfTime
        /// </summary>
        HalfTime = 256,

        /// <summary>
        /// Nightcore
        /// </summary>
        Nightcore = 512,

        /// <summary>
        /// Flashlight
        /// </summary>
        Flashlight = 1024,

        /// <summary>
        /// Autoplay
        /// </summary>
        Autoplay = 2048,

        /// <summary>
        /// SpunOut
        /// </summary>
        SpunOut = 4096,

        /// <summary>
        /// Relax2. Autopilot ?
        /// </summary>
        Relax2 = 8192,

        /// <summary>
        /// Perfect
        /// </summary>
        Perfect = 16384,

        /// <summary>
        /// Key4
        /// </summary>
        Key4 = 32768,

        /// <summary>
        /// Key5
        /// </summary>
        Key5 = 65536,

        /// <summary>
        /// Key6
        /// </summary>
        Key6 = 131072,

        /// <summary>
        /// Key7
        /// </summary>
        Key7 = 262144,

        /// <summary>
        /// Key8
        /// </summary>
        Key8 = 524288,

        /// <summary>
        /// KeyMod (All the keys combined)
        /// </summary>
        KeyMod = Key4 | Key5 | Key6 | Key7 | Key8,

        /// <summary>
        /// FadeIn
        /// </summary>
        FadeIn = 1048576,

        /// <summary>
        /// Random
        /// </summary>
        Random = 2097152,

        /// <summary>
        /// LastMod
        /// </summary>
        LastMod = 4194304,

        /// <summary>
        /// FreeMod
        /// </summary>
        FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | KeyMod
    }
}
