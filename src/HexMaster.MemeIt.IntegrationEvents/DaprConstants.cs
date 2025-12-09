namespace HexMaster.MemeIt.IntegrationEvents;

/// <summary>
/// Constants for Dapr configuration.
/// </summary>
public static class DaprConstants
{
    /// <summary>
    /// The name of the Dapr pubsub component used for integration events.
    /// </summary>
    public const string PubSubName = "chatservice-pubsub";

    /// <summary>
    /// Topic names for Dapr pubsub events.
    /// </summary>
    public static class Topics
    {
        public const string PlayerStateChanged = "playerstatechanged";
        public const string PlayerRemoved = "playerremoved";
        public const string PlayerJoined = "playerjoined";
        public const string GameStarted = "gamestarted";
        public const string RoundStarted = "roundstarted";
        public const string CreativePhaseEnded = "creativephaseended";
        public const string ScorePhaseStarted = "scorephasestarted";
        public const string RoundEnded = "roundended";
        public const string NewGameStarted = "newgamestarted";
    }
}
