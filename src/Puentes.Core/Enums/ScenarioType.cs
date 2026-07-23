using System.Text.Json.Serialization;
namespace Puentes.Shared.Enums;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ScenarioType
{
    Unknown = 0,
    MedicationReminder = 1,
    Conversation = 2,
    AppointmentReminder = 3,
    DailySummary = 4,
    Emergency = 5,
    MemorySupport = 6
}