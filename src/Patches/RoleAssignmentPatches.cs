using HarmonyLib;

namespace MichaelJacksonRole.Patches;

/// <summary>
/// After the host's vanilla role selection finishes, promote one plain Impostor to Michael Jackson.
/// </summary>
[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class SelectRolesPatch
{
    public static void Prefix() => MichaelJacksonRole.Reset();

    public static void Postfix() => MichaelJacksonRole.AssignAsHost();
}
