using HarmonyLib;
using MichaelJacksonRole.Utilities;

namespace MichaelJacksonRole.Patches;

/// <summary>Clears all role state between matches so nothing leaks into the next game.</summary>
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public static class GameEndPatch
{
    public static void Postfix() => ResetAll();

    internal static void ResetAll()
    {
        MichaelJacksonRole.Reset();
        Scheduler.Clear();
    }
}

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbyStartPatch
{
    public static void Postfix() => GameEndPatch.ResetAll();
}
