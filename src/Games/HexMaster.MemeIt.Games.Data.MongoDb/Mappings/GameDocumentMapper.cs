using System;
using System.Collections.Generic;
using System.Linq;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Data.MongoDb.Documents;

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
