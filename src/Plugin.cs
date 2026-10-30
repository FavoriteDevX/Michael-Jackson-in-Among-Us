using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MichaelJacksonRole.Audio;
using UnityEngine;

namespace MichaelJacksonRole;

/// <summary>
/// Plugin entry point. Binds the configuration, initializes the sound library
/// and applies every Harmony patch in this assembly.
/// </summary>
[BepInPlugin(Id, Name, Version)]
[BepInProcess("Among Us.exe")]
public sealed class MichaelJacksonPlugin : BasePlugin
{
    public const string Id = "com.favoritedevx.michaeljacksonrole";
    public const string Name = "Michael Jackson Role";
    public const string Version = "0.1.0";

    internal static MichaelJacksonPlugin Instance { get; private set; }
    internal static ManualLogSource Logger => Instance.Log;

    public Harmony Harmony { get; } = new(Id);

    // --- Role ---------------------------------------------------------------
    public ConfigEntry<int> RoleChance { get; private set; }

    // --- Moonwalk -----------------------------------------------------------
    public ConfigEntry<KeyCode> MoonwalkKey { get; private set; }
    public ConfigEntry<float> SpeedMultiplier { get; private set; }
    public ConfigEntry<float> MoonwalkDuration { get; private set; }
    public ConfigEntry<float> MoonwalkCooldown { get; private set; }
    public ConfigEntry<bool> GestureEnabled { get; private set; }
    public ConfigEntry<int> GestureFlips { get; private set; }
    public ConfigEntry<float> GestureWindow { get; private set; }

    // --- Audio --------------------------------------------------------------
    public ConfigEntry<float> SoundVolume { get; private set; }
    public ConfigEntry<bool> PlayOwAfterHeeHee { get; private set; }
    public ConfigEntry<SoundAudience> KillSoundAudience { get; private set; }
    public ConfigEntry<SoundAudience> VentSoundAudience { get; private set; }
    public ConfigEntry<float> NearbyRadius { get; private set; }

    public override void Load()
    {
        Instance = this;
        BindConfig();

        var pluginDir = Path.GetDirectoryName(typeof(MichaelJacksonPlugin).Assembly.Location) ?? Paths.PluginPath;
        SoundLibrary.Initialize(Path.Combine(pluginDir, "Sounds"));

        Harmony.PatchAll();
        Log.LogInfo($"{Name} v{Version} loaded.");
    }

    private void BindConfig()
    {
        RoleChance = Config.Bind("Role", "Chance", 100,
            new ConfigDescription("Chance (0-100) that one Impostor becomes Michael Jackson each game. Only the host's value is used.",
                new AcceptableValueRange<int>(0, 100)));

        MoonwalkKey = Config.Bind("Moonwalk", "Key", KeyCode.F,
            "Key that activates the Moonwalk ability.");
        SpeedMultiplier = Config.Bind("Moonwalk", "SpeedMultiplier", 1.5f,
            new ConfigDescription("Speed multiplier while moonwalking.", new AcceptableValueRange<float>(1f, 3f)));
        MoonwalkDuration = Config.Bind("Moonwalk", "Duration", 3f,
            new ConfigDescription("Seconds the speed boost lasts.", new AcceptableValueRange<float>(0.5f, 15f)));
        MoonwalkCooldown = Config.Bind("Moonwalk", "Cooldown", 15f,
            new ConfigDescription("Seconds before Moonwalk can be used again.", new AcceptableValueRange<float>(0f, 120f)));
        GestureEnabled = Config.Bind("Moonwalk", "GestureEnabled", true,
            "Allow triggering Moonwalk by quickly alternating left/right movement.");
        GestureFlips = Config.Bind("Moonwalk", "GestureFlips", 3,
            new ConfigDescription("Number of left/right direction changes needed to trigger the gesture.", new AcceptableValueRange<int>(2, 8)));
        GestureWindow = Config.Bind("Moonwalk", "GestureWindow", 0.75f,
            new ConfigDescription("Time window (seconds) in which the direction changes must happen.", new AcceptableValueRange<float>(0.2f, 3f)));

        SoundVolume = Config.Bind("Audio", "Volume", 1f,
            new ConfigDescription("Volume of the custom sounds (scaled by the in-game SFX volume).", new AcceptableValueRange<float>(0f, 1f)));
        PlayOwAfterHeeHee = Config.Bind("Audio", "PlayOwAfterHeeHee", true,
            "Play ow.wav right after hee_hee.wav on a kill.");
        KillSoundAudience = Config.Bind("Audio", "KillSoundAudience", SoundAudience.Self,
            "Who hears the kill sound. Self = killer and victim only.");
        VentSoundAudience = Config.Bind("Audio", "VentSoundAudience", SoundAudience.Self,
            "Who hears the vent sound. Self = the Michael Jackson player only.");
        NearbyRadius = Config.Bind("Audio", "NearbyRadius", 6f,
            "Radius (world units) used when an audience is set to Nearby.");
    }
}

/// <summary>Controls which clients hear a custom sound effect.</summary>
public enum SoundAudience
{
    /// <summary>Only the players directly involved.</summary>
    Self,
    /// <summary>The players involved plus anyone within <c>NearbyRadius</c>.</summary>
    Nearby,
    /// <summary>Every player in the match.</summary>
    Everyone,
}
