using Puentes.Orchestrator.Services;

namespace Puentes.Tests;

public sealed class KnownPersonNameMatcherTests
{
    [Theory]
    [InlineData("maria", "maria")]
    [InlineData("marta", "marta")]
    [InlineData("siquiel", "ezequiel")]
    [InlineData("esequiel", "ezequiel")]
    public void AcceptsNamesAndPlausibleTranscriptionErrors(
        string heard,
        string known)
    {
        Assert.True(KnownPersonNameMatcher.IsSimilarWord(heard, known));
    }

    [Theory]
    [InlineData("seria", "maria")]
    [InlineData("lindo", "lina")]
    [InlineData("bueno", "giano")]
    public void RejectsOrdinaryWordsThatAreNotNames(
        string heard,
        string known)
    {
        Assert.False(KnownPersonNameMatcher.IsSimilarWord(heard, known));
    }
}
