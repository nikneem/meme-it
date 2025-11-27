using HexMaster.MemeIt.Games.Domains;

namespace HexMaster.MemeIt.Games.Tests.Domains;

public sealed class GameRoundTests
{
    [Fact]
    public void Constructor_Creates_Round_With_Valid_Number()
    {
        // Act
        var round = new GameRound(1);

        // Assert
        Assert.Equal(1, round.RoundNumber);
        Assert.False(round.HasCreativePhaseEnded);
        Assert.False(round.HasClosedRound);
        Assert.Empty(round.Submissions);
    }

    [Fact]
    public void Constructor_Throws_When_Round_Number_Is_Zero()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameRound(0));
    }

    [Fact]
    public void Constructor_Throws_When_Round_Number_Is_Negative()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new GameRound(-1));
    }

    [Fact]
    public void MarkCreativePhaseEnded_Sets_Flag()
    {
        // Arrange
        var round = new GameRound(1);

        // Act
        round.MarkCreativePhaseEnded();

        // Assert
        Assert.True(round.HasCreativePhaseEnded);
    }

    [Fact]
    public void MarkRoundClosed_Sets_Flag()
    {
        // Arrange
        var round = new GameRound(1);

        // Act
        round.MarkRoundClosed();

        // Assert
        Assert.True(round.HasClosedRound);
    }

    [Fact]
    public void AddScore_Adds_Valid_Score()
    {
        // Arrange
        var round = new GameRound(1);
        var playerId = Guid.NewGuid();
        var voterId = Guid.NewGuid();
        var submission = new MemeSubmission(playerId, Guid.NewGuid(), Array.Empty<MemeTextEntry>());
        round.UpsertSubmission(submission);
        var submissionId = round.Submissions.First().SubmissionId;

        // Act
        round.AddScore(submissionId, voterId, 5);

        // Assert
        var scores = round.GetScoresForSubmission(submissionId);
        Assert.Single(scores);
        Assert.Equal(5, scores[voterId]);
    }

    [Fact]
    public void AddScore_Throws_When_Rating_Too_High()
    {
        // Arrange
        var round = new GameRound(1);
        var playerId = Guid.NewGuid();
        var submission = new MemeSubmission(playerId, Guid.NewGuid(), Array.Empty<MemeTextEntry>());
        round.UpsertSubmission(submission);
        var submissionId = round.Submissions.First().SubmissionId;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => round.AddScore(submissionId, Guid.NewGuid(), 6));
    }

    [Fact]
    public void AddScore_Throws_When_Rating_Too_Low()
    {
        // Arrange
        var round = new GameRound(1);
        var playerId = Guid.NewGuid();
        var submission = new MemeSubmission(playerId, Guid.NewGuid(), Array.Empty<MemeTextEntry>());
        round.UpsertSubmission(submission);
        var submissionId = round.Submissions.First().SubmissionId;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => round.AddScore(submissionId, Guid.NewGuid(), -1));
    }

    [Fact]
    public void UpsertSubmission_Adds_New_Submission()
    {
        // Arrange
        var round = new GameRound(1);
        var playerId = Guid.NewGuid();
        var submission = new MemeSubmission(playerId, Guid.NewGuid(), Array.Empty<MemeTextEntry>());

        // Act
        round.UpsertSubmission(submission);

        // Assert
        Assert.Single(round.Submissions);
        Assert.Contains(round.Submissions, s => s.PlayerId == playerId);
    }

    [Fact]
    public void UpsertSubmission_Updates_Existing_Submission()
    {
        // Arrange
        var round = new GameRound(1);
        var playerId = Guid.NewGuid();
        var template1 = Guid.NewGuid();
        var template2 = Guid.NewGuid();

        var submission1 = new MemeSubmission(playerId, template1, Array.Empty<MemeTextEntry>());
        round.UpsertSubmission(submission1);

        // Act
        var submission2 = new MemeSubmission(playerId, template2, Array.Empty<MemeTextEntry>());
        round.UpsertSubmission(submission2);

        // Assert
        Assert.Single(round.Submissions);
        Assert.Equal(template2, round.Submissions.First().MemeTemplateId);
    }

    [Fact]
    public void RemoveSubmissionForPlayer_Removes_Player_Submission()
    {
        // Arrange
        var round = new GameRound(1);
        var playerId = Guid.NewGuid();
        var submission = new MemeSubmission(playerId, Guid.NewGuid(), Array.Empty<MemeTextEntry>());
        round.UpsertSubmission(submission);

        // Act
        round.RemoveSubmissionForPlayer(playerId);

        // Assert
        Assert.Empty(round.Submissions);
    }

    [Fact]
    public void MarkMemeScorePhaseEnded_Marks_Submission()
    {
        // Arrange
        var round = new GameRound(1);
        var playerId = Guid.NewGuid();
        var submission = new MemeSubmission(playerId, Guid.NewGuid(), Array.Empty<MemeTextEntry>());
        round.UpsertSubmission(submission);
        var submissionId = round.Submissions.First().SubmissionId;

        // Act
        round.MarkMemeScorePhaseEnded(submissionId);

        // Assert
        Assert.True(round.HasScoringPhaseBeenEnded(submissionId));
    }

    [Fact]
    public void HasScorePhaseEnded_Returns_True_When_All_Submissions_Ended()
    {
        // Arrange
        var round = new GameRound(1);
        var submission1 = new MemeSubmission(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<MemeTextEntry>());
        var submission2 = new MemeSubmission(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<MemeTextEntry>());
        round.UpsertSubmission(submission1);
        round.UpsertSubmission(submission2);

        var sub1Id = round.Submissions.ElementAt(0).SubmissionId;
        var sub2Id = round.Submissions.ElementAt(1).SubmissionId;

        // Act
        round.MarkMemeScorePhaseEnded(sub1Id);
        round.MarkMemeScorePhaseEnded(sub2Id);

        // Assert
        Assert.True(round.HasScorePhaseEnded);
    }
}
