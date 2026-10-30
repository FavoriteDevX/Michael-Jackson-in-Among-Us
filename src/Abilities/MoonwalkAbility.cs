using System.Collections.Generic;
using MichaelJacksonRole.Utilities;
using UnityEngine;

namespace MichaelJacksonRole.Abilities;

/// <summary>
/// Moonwalk Speed: temporarily multiplies the local player's movement speed.
///
/// Triggered either by the configured key or by quickly alternating horizontal movement
/// (left-right-left...). Movement is client-authoritative in Among Us, so changing the local
/// <see cref="PlayerPhysics.Speed"/> is enough for other players to see the faster slide.
/// </summary>
public static class MoonwalkAbility
{
    private const float DirectionThreshold = 0.1f;

    private static float? _originalSpeed;
    private static float _activeUntil;
    private static float _cooldownUntil;
    private static bool _readyAnnounced = true;

    private static int _lastDirection;
    private static readonly Queue<float> DirectionFlips = new();

    public static bool IsActive => _originalSpeed.HasValue;

    public static float CooldownRemaining => Mathf.Max(0f, _cooldownUntil - Time.time);

    /// <summary>Called once per frame for the local player.</summary>
    public static void Update(PlayerControl player)
    {
        if (!CanUse(player))
        {
            if (IsActive) Stop(player);
            return;
        }

        var now = Time.time;

        if (IsActive && now >= _activeUntil) Stop(player);

        if (!IsActive && !_readyAnnounced && CooldownRemaining <= 0f)
        {
            _readyAnnounced = true;
            Notifications.Show("<color=#FFD700>Moonwalk ready!</color>");
        }

        if (Input.GetKeyDown(MichaelJacksonPlugin.Instance.MoonwalkKey.Value))
            TryActivate(player);

        if (MichaelJacksonPlugin.Instance.GestureEnabled.Value)
            TrackGesture(player, now);
    }

    public static bool TryActivate(PlayerControl player)
    {
        if (IsActive || CooldownRemaining > 0f || !CanUse(player)) return false;

        var config = MichaelJacksonPlugin.Instance;
        var physics = player.MyPhysics;

        _originalSpeed = physics.Speed;
        physics.Speed = _originalSpeed.Value * config.SpeedMultiplier.Value;
        _activeUntil = Time.time + config.MoonwalkDuration.Value;

        Notifications.Show("<color=#FFD700>Moonwalk!</color>");
        return true;
    }

    /// <summary>Restores the original speed (if boosted) and clears timers. Safe to call any time.</summary>
    public static void Reset()
    {
        if (IsActive && PlayerControl.LocalPlayer != null)
            PlayerControl.LocalPlayer.MyPhysics.Speed = _originalSpeed.Value;

        _originalSpeed = null;
        _activeUntil = 0f;
        _cooldownUntil = 0f;
        _readyAnnounced = true;
        _lastDirection = 0;
        DirectionFlips.Clear();
    }

    private static void Stop(PlayerControl player)
    {
        if (player != null && _originalSpeed.HasValue)
            player.MyPhysics.Speed = _originalSpeed.Value;

        _originalSpeed = null;
        _cooldownUntil = Time.time + MichaelJacksonPlugin.Instance.MoonwalkCooldown.Value;
        _readyAnnounced = false;
    }

    private static bool CanUse(PlayerControl player) =>
        player != null
        && player.AmOwner
        && MichaelJacksonRole.Is(player)
        && player.Data != null
        && !player.Data.IsDead
        && !player.inVent
        && MeetingHud.Instance == null
        && ExileController.Instance == null;

    private static void TrackGesture(PlayerControl player, float now)
    {
        var velocityX = player.MyPhysics.body.velocity.x;
        var direction = Mathf.Abs(velocityX) > DirectionThreshold ? (int)Mathf.Sign(velocityX) : 0;

        if (direction != 0)
        {
            if (_lastDirection != 0 && direction != _lastDirection)
                DirectionFlips.Enqueue(now);
            _lastDirection = direction;
        }

        var window = MichaelJacksonPlugin.Instance.GestureWindow.Value;
        while (DirectionFlips.Count > 0 && now - DirectionFlips.Peek() > window)
            DirectionFlips.Dequeue();

        if (DirectionFlips.Count >= MichaelJacksonPlugin.Instance.GestureFlips.Value)
        {
            DirectionFlips.Clear();
            TryActivate(player);
        }
    }
}
