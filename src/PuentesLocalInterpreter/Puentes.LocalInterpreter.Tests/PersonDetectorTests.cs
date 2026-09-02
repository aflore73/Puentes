using Puentes.LocalInterpreter.Services;

namespace Puentes.LocalInterpreter.Tests;

public sealed class PersonDetectorTests
{
    [Theory]
    [InlineData("eze dónde está", "Ezequiel")]
    [InlineData("sequi vino", "Ezequiel")]
    [InlineData("ale vino ayer", "Alejandro")]
    [InlineData("laura viene hoy", "Laura")]
    [InlineData("marta está acá", "Marta")]
    public void ResolvesKnownAliasesToCanonicalPerson(string text, string expectedName)
    {
        var result = PersonDetector.Detect(text);

        Assert.Equal(expectedName, result.Person?.Name);
        Assert.Equal("Confirmado", result.State);
    }

    [Theory]
    [InlineData("mi hijo vino hoy", "Ezequiel")]
    [InlineData("mi hija viene mañana", "Laura")]
    [InlineData("mi hermana está acá", "Marta")]
    public void ResolvesFamilyRelationToContextualDefault(string text, string expectedName)
    {
        var result = PersonDetector.Detect(text);

        Assert.Equal(expectedName, result.Person?.Name);
        Assert.Equal("Contexto", result.State);
    }

    [Fact]
    public void ReturnsUnknownWhenNoPersonOrRelationMatches()
    {
        var result = PersonDetector.Detect("estoy triste");

        Assert.Null(result.Person);
        Assert.Equal("Desconocido", result.State);
    }
}
