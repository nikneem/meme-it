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

        var players = document.Players.Select(p => new GamePlayer(p.PlayerId, p.DisplayName, p.IsReady));
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
                    submissionDoc.SubmissionId,
                    submissionDoc.PlayerId,
                    submissionDoc.MemeTemplateId,
                    textEntries);

                game.AddMemeSubmission(round.RoundNumber, submission);

                // Restore scores for this submission
                foreach (var scoreDoc in submissionDoc.Scores)
                {
                    submission.AddScore(scoreDoc.PlayerId, scoreDoc.Rating);
                }

                // Restore phase tracking for submission
                if (submissionDoc.HasScorePhaseStarted)
                {
                    submission.StartScorePhase();
                }
                if (submissionDoc.HasScorePhaseEnded)
                {
                    submission.EndScorePhase();
                }
            }

            // Restore phase tracking for round
            if (roundDoc.HasCreativePhaseEnded && round is GameRound concreteRound)
            {
                concreteRound.MarkCreativePhaseEnded();
            }
        }

        return game;
    }

    private static GamePlayerDocument MapPlayer(IGamePlayer player)
        => new()
        {
            PlayerId = player.PlayerId,
            DisplayName = player.DisplayName,
            IsReady = player.IsReady
        };

    private static GameRoundDocument MapRound(IGameRound round)
    {
        var doc = new GameRoundDocument
        {
            RoundNumber = round.RoundNumber,
            StartedAt = round.StartedAt,
            Submissions = round.Submissions.Select(MapSubmission).ToList(),
            HasCreativePhaseEnded = round.HasCreativePhaseEnded
        };

        return doc;
    }

    private static MemeSubmissionDocument MapSubmission(IMemeSubmission submission)
        => new()
        {
            SubmissionId = submission.SubmissionId,
            PlayerId = submission.PlayerId,
            MemeTemplateId = submission.MemeTemplateId,
            TextEntries = submission.TextEntries.Select(MapTextEntry).ToList(),
            Scores = submission.Scores.Select(MapScore).ToList(),
            HasScorePhaseStarted = submission.HasScorePhaseStarted,
            HasScorePhaseEnded = submission.HasScorePhaseEnded
        };

    private static MemeTextEntryDocument MapTextEntry(IMemeTextEntry entry)
        => new()
        {
            TextFieldId = entry.TextFieldId,
            Value = entry.Value
        };

    private static MemeSubmissionScoreDocument MapScore(IMemeSubmissionScore score)
        => new()
        {
            PlayerId = score.PlayerId,
            Rating = score.Rating
        };
}
