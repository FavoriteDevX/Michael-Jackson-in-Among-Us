using System;

namespace MichaelJacksonRole.Utilities;

/// <summary>
/// Shows short messages in the game's notification feed (bottom-left corner).
/// All on-screen text goes through here so there is a single place to adapt if the game API changes.
/// </summary>
public static class Notifications
{
    public static void Show(string message)
    {
        try
        {
            if (HudManager.Instance != null && HudManager.Instance.Notifier != null)
            {
                HudManager.Instance.Notifier.AddDisconnectMessage(message);
                return;
            }
        }
        catch (Exception e)
        {
            MichaelJacksonPlugin.Logger.LogWarning($"Could not show notification: {e.Message}");
        }

        MichaelJacksonPlugin.Logger.LogInfo($"[Notification] {message}");
    }
}
