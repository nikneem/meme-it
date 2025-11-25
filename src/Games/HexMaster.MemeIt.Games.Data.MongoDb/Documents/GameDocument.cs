using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HexMaster.MemeIt.Games.Data.MongoDb.Documents;

internal sealed class GameDocument
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("gameCode")]
    public string GameCode { get; set; } = string.Empty;

    [BsonElement("adminPlayerId")]
    [BsonRepresentation(BsonType.String)]
    public Guid AdminPlayerId { get; set; }

    [BsonElement("password")]
    public string? Password { get; set; }

    [BsonElement("state")]
    public string State { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [BsonElement("players")]
    public List<GamePlayerDocument> Players { get; set; } = new();

    [BsonElement("rounds")]
    public List<GameRoundDocument> Rounds { get; set; } = new();
}

internal sealed class GamePlayerDocument
{
    [BsonElement("playerId")]
    [BsonRepresentation(BsonType.String)]
    public Guid PlayerId { get; set; }

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("isReady")]
    public bool IsReady { get; set; }
}

internal sealed class GameRoundDocument
{
    [BsonElement("roundNumber")]
    public int RoundNumber { get; set; }

    [BsonElement("submissions")]
    public List<MemeSubmissionDocument> Submissions { get; set; } = new();

    [BsonElement("creativePhaseEnded")]
    public bool CreativePhaseEnded { get; set; }

    [BsonElement("scorePhaseEnded")]
    public bool ScorePhaseEnded { get; set; }

    [BsonElement("memesWithEndedScorePhase")]
    [BsonRepresentation(BsonType.String)]
    public List<Guid> MemesWithEndedScorePhase { get; set; } = new();

    [BsonElement("scores")]
    public Dictionary<string, Dictionary<string, int>> Scores { get; set; } = new();
}

internal sealed class MemeSubmissionDocument
{
    [BsonElement("playerId")]
    [BsonRepresentation(BsonType.String)]
    public Guid PlayerId { get; set; }

    [BsonElement("memeTemplateId")]
    [BsonRepresentation(BsonType.String)]
    public Guid MemeTemplateId { get; set; }

    [BsonElement("textEntries")]
    public List<MemeTextEntryDocument> TextEntries { get; set; } = new();
}

internal sealed class MemeTextEntryDocument
{
    [BsonElement("textFieldId")]
    [BsonRepresentation(BsonType.String)]
    public Guid TextFieldId { get; set; }

    [BsonElement("value")]
    public string Value { get; set; } = string.Empty;
}
