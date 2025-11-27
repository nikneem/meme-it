using HexMaster.MemeIt.Games.Domains;

namespace HexMaster.MemeIt.Games.Tests.Domains;

public sealed class MemeSubmissionTests
{
    [Fact]
    public void Constructor_Creates_Submission_With_Empty_Text_Entries()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var templateId = Guid.NewGuid();

        // Act
        var submission = new MemeSubmission(playerId, templateId, Array.Empty<MemeTextEntry>());

        // Assert
        Assert.Equal(playerId, submission.PlayerId);
        Assert.Equal(templateId, submission.MemeTemplateId);
        Assert.Empty(submission.TextEntries);
        Assert.False(submission.HasScorePhaseEnded);
    }

    [Fact]
    public void Constructor_Creates_Submission_With_Text_Entries()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var textFieldId = Guid.NewGuid();
        var textEntries = new[] { new MemeTextEntry(textFieldId, "Test Text") };

        // Act
        var submission = new MemeSubmission(playerId, templateId, textEntries);

        // Assert
        Assert.Equal(playerId, submission.PlayerId);
        Assert.Equal(templateId, submission.MemeTemplateId);
        Assert.Single(submission.TextEntries);
        Assert.Equal("Test Text", submission.TextEntries.First().Value);
    }

    [Fact]
    public void EndScorePhase_Sets_Flag()
    {
        // Arrange
        var submission = new MemeSubmission(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<MemeTextEntry>());

        // Act
        submission.EndScorePhase();

        // Assert
        Assert.True(submission.HasScorePhaseEnded);
    }

    [Fact]
    public void SubmissionId_Is_Generated()
    {
        // Arrange & Act
        var submission = new MemeSubmission(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<MemeTextEntry>());

        // Assert
        Assert.NotEqual(Guid.Empty, submission.SubmissionId);
    }
}
