using HarmonyLib;
using MichaelJacksonRole.Audio;
using MichaelJacksonRole.Utilities;

namespace MichaelJacksonRole.Patches;

/// <summary>
/// Custom kill effects. <see cref="PlayerControl.MurderPlayer"/> runs on every client once the
/// server confirms a kill (the final step of the PerformKill flow), so each client decides locally
/// whether to play the sound and whether it is the victim that should see the message.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class MurderPlayerPatch
{
    // Shown after the vanilla kill animation so it isn't hidden behind the overlay.
    private const float VictimMessageDelay = 2.5f;
    private const float OwFallbackDelay = 0.6f;

    public static void Postfix(
        PlayerControl __instance,
        [HarmonyArgument(0)] PlayerControl target,
        [HarmonyArgument(1)] MurderResultFlags resultFlags)
    {
        if ((resultFlags & MurderResultFlags.Succeeded) == 0) return;
        if (!MichaelJacksonRole.Is(__instance) || target == null) return;

        var config = MichaelJacksonPlugin.Instance;

        if (SoundAudienceFilter.ShouldHear(config.KillSoundAudience.Value, target.GetTruePosition(), __instance, target))
        {
            var heeHeeLength = SoundLibrary.Play(SoundLibrary.HeeHee);
            if (config.PlayOwAfterHeeHee.Value)
                Scheduler.After(heeHeeLength > 0f ? heeHeeLength : OwFallbackDelay, () => SoundLibrary.Play(SoundLibrary.Ow));
        }

        if (target.AmOwner)
            Scheduler.After(VictimMessageDelay, () =>
                Notifications.Show($"<color=#FFD700>{MichaelJacksonRole.VictimMessage}</color>"));
    }
}
