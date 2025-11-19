using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.ValueObjects;
using HexMaster.MemeIt.Games.Data.MongoDb.Documents;
using HexMaster.MemeIt.Games.Domains;

namespace HexMaster.MemeIt.Games.Data.MongoDb.Mappings;

internal static class GameDocumentMapper
{
    public static GameDocument ToDocument(IGame game)
    {
        return new GameDocument
        {
            GameCode = game.GameCode,
            AdminPlayerId = game.AdminPlayerId,
            Password = game.Password,
            CreatedAt = game.CreatedAt,
            State = game.State.Name,
            Players = game.Players.Select(MapPlayer).ToList(),
            Rounds = game.Rounds.Select(MapRound).ToList()
        };
    }

    public static Game FromDocument(GameDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var players = document.Players.Select(p => new GamePlayer(p.PlayerId, p.DisplayName));
        var game = new Game(
            document.GameCode,
            document.AdminPlayerId,
            document.Password,
            players,
            document.CreatedAt);

        // Restore state
        var targetState = GameState.FromName(document.State);
        if (!game.State.Equals(targetState))
        {
            game.ChangeState(targetState);
        }

        // Restore rounds and submissions
        foreach (var roundDoc in document.Rounds.OrderBy(r => r.RoundNumber))
        {
            var round = game.NextRound();
            foreach (var submissionDoc in roundDoc.Submissions)
            {
                var textEntries = submissionDoc.TextEntries
                    .Select(te => new MemeTextEntry(te.TextFieldId, te.Value))
                    .ToList();

                var submission = new MemeSubmission(
                    submissionDoc.PlayerId,
                    submissionDoc.MemeTemplateId,
                    textEntries);

                game.AddMemeSubmission(round.RoundNumber, submission);
            }
        }

        return game;
    }

    private static GamePlayerDocument MapPlayer(IGamePlayer player)
        => new()
        {
            PlayerId = player.PlayerId,
            DisplayName = player.DisplayName
        };

    private static GameRoundDocument MapRound(IGameRound round)
        => new()
        {
            RoundNumber = round.RoundNumber,
            Submissions = round.Submissions.Select(MapSubmission).ToList()
        };

    private static MemeSubmissionDocument MapSubmission(IMemeSubmission submission)
        => new()
        {
            PlayerId = submission.PlayerId,
            MemeTemplateId = submission.MemeTemplateId,
            TextEntries = submission.TextEntries.Select(MapTextEntry).ToList()
        };

    private static MemeTextEntryDocument MapTextEntry(IMemeTextEntry entry)
        => new()
        {
            TextFieldId = entry.TextFieldId,
            Value = entry.Value
        };
}
