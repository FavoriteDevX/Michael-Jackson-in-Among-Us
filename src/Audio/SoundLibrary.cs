using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MichaelJacksonRole.Audio;

/// <summary>
/// Lazily loads and caches the role's sound effects from the plugin's <c>Sounds</c> folder,
/// and plays them through the game's <see cref="SoundManager"/> so the in-game SFX volume applies.
/// </summary>
public static class SoundLibrary
{
    public const string HeeHee = "hee_hee.wav";
    public const string Ow = "ow.wav";
    public const string ThrillerLaugh = "thriller_laugh.wav";

    private static readonly Dictionary<string, AudioClip> Cache = new();
    private static readonly HashSet<string> Missing = new();
    private static string _directory;

    public static void Initialize(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(directory);
        MichaelJacksonPlugin.Logger.LogInfo($"Looking for sound files in: {directory}");
    }

    /// <summary>Plays a sound and returns its length in seconds, or 0 if it could not be played.</summary>
    public static float Play(string fileName)
    {
        var clip = Get(fileName);
        if (clip == null || SoundManager.Instance == null) return 0f;

        SoundManager.Instance.PlaySound(clip, false, MichaelJacksonPlugin.Instance.SoundVolume.Value);
        return clip.length;
    }

    private static AudioClip Get(string fileName)
    {
        if (Cache.TryGetValue(fileName, out var cached) && cached != null) return cached;
        if (Missing.Contains(fileName)) return null;

        var path = Path.Combine(_directory, fileName);
        if (!File.Exists(path))
        {
            Missing.Add(fileName);
            MichaelJacksonPlugin.Logger.LogWarning($"Sound file not found, skipping: {path}");
            return null;
        }

        try
        {
            var clip = WavLoader.Load(path);
            Cache[fileName] = clip;
            return clip;
        }
        catch (Exception e)
        {
            Missing.Add(fileName);
            MichaelJacksonPlugin.Logger.LogError($"Failed to load {path}: {e.Message}");
            return null;
        }
    }
}
