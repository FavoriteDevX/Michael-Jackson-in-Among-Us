using UnityEngine;

namespace MichaelJacksonRole.Audio;

/// <summary>Decides whether the local client should hear a sound, based on a <see cref="SoundAudience"/>.</summary>
public static class SoundAudienceFilter
{
    /// <param name="audience">The configured audience.</param>
    /// <param name="source">Where the sound happens in the world.</param>
    /// <param name="involved">Players that always hear the sound (e.g. killer and victim).</param>
    public static bool ShouldHear(SoundAudience audience, Vector2 source, params PlayerControl[] involved)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null) return false;

        if (audience == SoundAudience.Everyone) return true;

        foreach (var player in involved)
            if (player != null && player.AmOwner) return true;

        if (audience == SoundAudience.Nearby)
            return Vector2.Distance(local.GetTruePosition(), source) <= MichaelJacksonPlugin.Instance.NearbyRadius.Value;

        return false;
    }
}
