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
                    submissionDoc.MemeId != Guid.Empty ? submissionDoc.MemeId : Guid.NewGuid(),
                    submissionDoc.PlayerId,
                    submissionDoc.MemeTemplateId,
                    textEntries);

                game.AddMemeSubmission(round.RoundNumber, submission);
            }

            // Restore phase tracking
            if (roundDoc.CreativePhaseEnded && round is GameRound concreteRound)
            {
                concreteRound.MarkCreativePhaseEnded();
            }

            if (roundDoc.ScorePhaseEnded && round is GameRound concreteRound2)
            {
                concreteRound2.MarkScorePhaseEnded();
            }

            // Restore memes with ended score phase
            if (round is GameRound concreteRound3)
            {
                foreach (var memeId in roundDoc.MemesWithEndedScorePhase)
                {
                    concreteRound3.MarkMemeScorePhaseEnded(memeId);
                }
            }

            // Restore scores
            if (round is GameRound concreteRound4)
            {
                foreach (var memeScores in roundDoc.Scores)
                {
                    var memeId = Guid.Parse(memeScores.Key);
                    foreach (var voterScore in memeScores.Value)
                    {
                        var voterId = Guid.Parse(voterScore.Key);
                        concreteRound4.AddScore(memeId, voterId, voterScore.Value);
                    }
                }
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
            Submissions = round.Submissions.Select(MapSubmission).ToList(),
            CreativePhaseEnded = round.CreativePhaseEnded,
            ScorePhaseEnded = round.ScorePhaseEnded
        };

        // Map scores dictionary
        if (round is GameRound concreteRound)
        {
            // Map the internal _memesWithEndedScorePhase HashSet
            doc.MemesWithEndedScorePhase = round.Submissions
                .Where(s => round.IsMemeScorePhaseEnded(s.MemeId))
                .Select(s => s.MemeId)
                .ToList();

            // Map scores [memeId][voterId] = score
            foreach (var submission in round.Submissions)
            {
                var scores = round.GetScoresForMeme(submission.MemeId);
                if (scores.Any())
                {
                    doc.Scores[submission.MemeId.ToString()] = scores
                        .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
                }
            }
        }

        return doc;
    }

    private static MemeSubmissionDocument MapSubmission(IMemeSubmission submission)
        => new()
        {
            MemeId = submission.MemeId,
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
