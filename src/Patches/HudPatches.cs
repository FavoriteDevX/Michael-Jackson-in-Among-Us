using HarmonyLib;
using MichaelJacksonRole.Abilities;
using MichaelJacksonRole.Utilities;

namespace MichaelJacksonRole.Patches;

/// <summary>Per-frame hook: drives the Moonwalk ability and the scheduler.</summary>
[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
public static class HudUpdatePatch
{
    public static void Postfix()
    {
        Scheduler.Tick();

        var local = PlayerControl.LocalPlayer;
        if (local != null && MichaelJacksonRole.IsAssigned)
            MoonwalkAbility.Update(local);
    }
}
