using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using MichaelJacksonRole.Abilities;
using MichaelJacksonRole.Utilities;
using UnityEngine;

namespace MichaelJacksonRole;

/// <summary>
/// The "Michael Jackson" Impostor role.
///
/// The role is layered on top of the vanilla <see cref="RoleTypes.Impostor"/> role, so the player keeps
/// every standard Impostor ability (kill, sabotage, vents). This class only tracks *which* player holds
/// the role and keeps that information in sync between clients through a custom RPC.
/// </summary>
public static class MichaelJacksonRole
{
    public const string RoleName = "Michael Jackson";
    public const string VictimMessage = "You've been struck by a Smooth Criminal!";

    /// <summary>Custom RPC id. Must not collide with vanilla <c>RpcCalls</c> or other mods.</summary>
    public const byte SetPlayerRpc = 221;

    public static readonly Color RoleColor = new Color32(255, 215, 0, 255);

    private static byte? _playerId;

    public static bool IsAssigned => _playerId.HasValue;

    public static bool Is(PlayerControl player) =>
        player != null && _playerId.HasValue && player.PlayerId == _playerId.Value;

    public static bool IsLocalPlayer => Is(PlayerControl.LocalPlayer);

    /// <summary>
    /// Host only: picks one plain Impostor to become Michael Jackson and broadcasts the choice.
    /// </summary>
    public static void AssignAsHost()
    {
        if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

        var chance = MichaelJacksonPlugin.Instance.RoleChance.Value;
        if (Random.Range(0, 100) >= chance) return;

        var candidates = new List<PlayerControl>();
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.Disconnected) continue;
            if (player.Data.Role == null || player.Data.Role.Role != RoleTypes.Impostor) continue;
            candidates.Add(player);
        }

        if (candidates.Count == 0)
        {
            MichaelJacksonPlugin.Logger.LogInfo("No plain Impostor available; Michael Jackson was not assigned.");
            return;
        }

        var chosen = candidates[Random.Range(0, candidates.Count)];
        SetPlayer(chosen.PlayerId);

        var writer = AmongUsClient.Instance.StartRpcImmediately(
            PlayerControl.LocalPlayer.NetId, SetPlayerRpc, SendOption.Reliable, -1);
        writer.Write(chosen.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    /// <summary>Called locally on every client (host directly, others through the RPC).</summary>
    public static void SetPlayer(byte playerId)
    {
        _playerId = playerId;
        MichaelJacksonPlugin.Logger.LogInfo($"Player {playerId} is {RoleName}.");

        if (!IsLocalPlayer) return;

        var key = MichaelJacksonPlugin.Instance.MoonwalkKey.Value;
        // Delay the announcement so it shows after the intro cutscene.
        Scheduler.After(6f, () => Notifications.Show(
            $"<color=#FFD700>You are {RoleName}!</color> Press {key} or moonwalk (left-right-left) to slide."));
    }

    public static void Reset()
    {
        _playerId = null;
        MoonwalkAbility.Reset();
    }
}
