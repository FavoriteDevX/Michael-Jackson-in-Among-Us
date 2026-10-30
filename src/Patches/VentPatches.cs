using HarmonyLib;
using MichaelJacksonRole.Audio;

namespace MichaelJacksonRole.Patches;

/// <summary>
/// Plays thriller_laugh.wav when Michael Jackson enters or exits a vent.
/// The vent coroutines run on every client for the venting player, so the audience filter decides
/// who actually hears it.
/// </summary>
[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
public static class EnterVentPatch
{
    public static void Prefix(PlayerPhysics __instance) => VentSound.TryPlay(__instance.myPlayer);
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoExitVent))]
public static class ExitVentPatch
{
    public static void Prefix(PlayerPhysics __instance) => VentSound.TryPlay(__instance.myPlayer);
}

internal static class VentSound
{
    public static void TryPlay(PlayerControl player)
    {
        if (!MichaelJacksonRole.Is(player)) return;

        var audience = MichaelJacksonPlugin.Instance.VentSoundAudience.Value;
        if (SoundAudienceFilter.ShouldHear(audience, player.GetTruePosition(), player))
            SoundLibrary.Play(SoundLibrary.ThrillerLaugh);
    }
}
