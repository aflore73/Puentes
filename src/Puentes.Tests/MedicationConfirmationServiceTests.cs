using Puentes.Orchestrator.Services;

namespace Puentes.Tests;

public class MedicationConfirmationServiceTests
{
    private readonly MedicationConfirmationService _service = new();

    [Theory]
    [InlineData("Sí")]
    [InlineData("Sí, ya la tomé")]
    [InlineData("Ya tomé las pastillas")]
    [InlineData("Listo, ya está")]
    [InlineData("Me la tomé")]
    [InlineData("Sí.")]
    [InlineData("Ya está.")]
    [InlineData("La tomé")]
    [InlineData("Tomé todo")]
    [InlineData("Tomé todas las pastillas")]
    [InlineData("Me tomé todos los remedios")]
    [InlineData("Ya he tomado la medicación")]
    [InlineData("Ya terminé de tomar todo")]
    [InlineData("Están tomadas")]
    [InlineData("Hecho, gracias")]
    public void ExplicitConfirmationIsRecognized(string input)
    {
        Assert.True(_service.IsExplicitConfirmation(input));
    }

    [Theory]
    [InlineData("No")]
    [InlineData("Todavía no la tomé")]
    [InlineData("No sé cuál tomar")]
    [InlineData("Después la tomo")]
    [InlineData("Creo que la tomé")]
    [InlineData("Me parece que ya la tomé")]
    [InlineData("Tal vez la tomé")]
    [InlineData("¿La tomé?")]
    [InlineData("Me falta una pastilla")]
    [InlineData("Ahora la tomo")]
    [InlineData("Voy a tomarla")]
    [InlineData("")]
    public void AmbiguousOrNegativeInputIsNotConfirmation(string input)
    {
        Assert.False(_service.IsExplicitConfirmation(input));
    }
}
