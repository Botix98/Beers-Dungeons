public static class SessionManager
{
    public static string PlayerId { get; private set; }
    public static string PlayerName { get; private set; }

    public static bool IsLoggedIn => !string.IsNullOrEmpty(PlayerId);

    public static void SetSession(string playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }

    public static void Clear()
    {
        PlayerId = null;
        PlayerName = null;
    }
}
