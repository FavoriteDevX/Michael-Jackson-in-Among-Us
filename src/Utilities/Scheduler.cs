using System;
using System.Collections.Generic;
using UnityEngine;

namespace MichaelJacksonRole.Utilities;

/// <summary>
/// Tiny frame-driven timer, ticked from <c>HudManager.Update</c>.
/// Avoids IL2CPP coroutine interop for simple delayed actions.
/// </summary>
public static class Scheduler
{
    private static readonly List<(float At, Action Action)> Pending = new();

    public static void After(float seconds, Action action) => Pending.Add((Time.time + seconds, action));

    public static void Tick()
    {
        if (Pending.Count == 0) return;

        var now = Time.time;
        for (var i = Pending.Count - 1; i >= 0; i--)
        {
            if (Pending[i].At > now) continue;

            var action = Pending[i].Action;
            Pending.RemoveAt(i);
            try
            {
                action();
            }
            catch (Exception e)
            {
                MichaelJacksonPlugin.Logger.LogError($"Scheduled action failed: {e}");
            }
        }
    }

    public static void Clear() => Pending.Clear();
}
