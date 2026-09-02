using Puentes.LocalInterpreter.Services;

namespace Puentes.LocalInterpreter.Tests;

public sealed class IntentDetectorTests
{
    [Theory]
    [InlineData("dónde está mi hijo", "UBICACION")]
    [InlineData("quién es Marta", "PERSONA")]
    [InlineData("estoy triste", "EMOCION")]
    [InlineData("quiero hablar con Eze", "NECESIDAD")]
    [InlineData("mi hija viene mañana", "AGENDA")]
    [InlineData("Eze vino hoy", "AGENDA")]
    [InlineData("Eze vino", "EVENTO")]
    public void DetectsExpectedIntent(string text, string expectedIntent)
    {
        var result = IntentDetector.Detect(text);

        Assert.Equal(expectedIntent, result.Intent);
        Assert.True(result.Score > 0);
    }

    [Fact]
    public void ReturnsUnknownWhenNoKeywordMatches()
    {
        var result = IntentDetector.Detect("blablabla");

        Assert.Equal("DESCONOCIDA", result.Intent);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void UbicacionTakesPriorityOverOtherCategories()
    {
        // "dónde" and "hoy" both present: UBICACION must win due to cascade order.
        var result = IntentDetector.Detect("dónde está hoy");

        Assert.Equal("UBICACION", result.Intent);
    }
}
