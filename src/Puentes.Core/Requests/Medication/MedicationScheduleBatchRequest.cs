using Puentes.Api.Requests.Medications;

public class MedicationScheduleBatchRequest
{
    public Guid MedicationId { get; set; }

    public List<MedicationScheduleItemRequest> Schedules { get; set; } = [];
}