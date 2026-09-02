using Puentes.LocalInterpreter.Services;

namespace Puentes.LocalInterpreter.Tests;

public sealed class TextNormalizerTests
{
    [Theory]
    [InlineData("Eze... dónde ta mi ijo", "eze dónde está mi hijo")]
    [InlineData("mi hija viene manana", "mi hija viene mañana")]
    [InlineData("Hola   Mundo", "hola mundo")]
    public void NormalizesSlangCasingAndWhitespace(string input, string expected)
    {
        Assert.Equal(expected, TextNormalizer.Normalize(input));
    }

    [Fact]
    public void ReturnsEmptyForNullOrWhitespace()
    {
        Assert.Equal("", TextNormalizer.Normalize("   "));
    }

    [Fact]
    public void DoesNotReplaceSlangInsideLongerWords()
    {
        // "ta" must not match inside "estación"
        Assert.Equal("estación", TextNormalizer.Normalize("estación"));
    }
}
