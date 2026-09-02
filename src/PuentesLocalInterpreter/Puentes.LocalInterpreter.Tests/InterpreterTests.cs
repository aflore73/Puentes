using Puentes.LocalInterpreter.Services;

namespace Puentes.LocalInterpreter.Tests;

public sealed class InterpreterTests
{
    [Fact]
    public void ResolvesAliasAndIntentWithHighConfidence()
    {
        var result = Interpreter.Interpret("Eze dónde está");

        Assert.Equal("Ezequiel", result.Person.Person?.Name);
        Assert.Equal("UBICACION", result.Intent.Intent);
        Assert.Equal("ALTA", result.Confidence);
        Assert.Equal("¿Dónde está Ezequiel?", result.Interpretation);
    }

    [Fact]
    public void FallsBackToMediumConfidenceWhenPersonIsUnknown()
    {
        var result = Interpreter.Interpret("Pedro dónde está");

        Assert.Null(result.Person.Person);
        Assert.Equal("UBICACION", result.Intent.Intent);
        Assert.Equal("MEDIA", result.Confidence);
    }

    [Fact]
    public void ReturnsLowConfidenceWhenNothingIsResolved()
    {
        var result = Interpreter.Interpret("blablabla");

        Assert.Null(result.Person.Person);
        Assert.Equal("DESCONOCIDA", result.Intent.Intent);
        Assert.Equal("BAJA", result.Confidence);
    }
}
