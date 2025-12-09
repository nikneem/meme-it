namespace HexMaster.MemeIt.IntegrationEvents.Events;

/// <summary>
/// Event published when a player starts a new game from a concluded game, inviting previous players to join.
/// </summary>
/// <param name="PreviousGameCode">The game code of the concluded game.</param>
/// <param name="NewGameCode">The game code of the newly created game.</param>
/// <param name="InitiatedByPlayerName">Display name of the player who started the new game.</param>
public sealed record NewGameStartedEvent(string PreviousGameCode, string NewGameCode, string InitiatedByPlayerName);
