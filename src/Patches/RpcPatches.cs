using HarmonyLib;
using Hazel;

namespace MichaelJacksonRole.Patches;

/// <summary>Receives the host's "who is Michael Jackson" broadcast.</summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class HandleRpcPatch
{
    public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        if (callId != MichaelJacksonRole.SetPlayerRpc) return;

        MichaelJacksonRole.SetPlayer(reader.ReadByte());
    }
}
