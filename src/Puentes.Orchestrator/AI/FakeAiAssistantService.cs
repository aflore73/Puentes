//namespace Puentes.Orchestrator.AI;

//public class FakeAiAssistantService : IAssistantService
//{
//    //public Task<string> GenerateMedicationReminderAsync(
//    //    MedicationReminderContext context)
//    //{
//    //    var message =
//    //        $"Marta, turno {context.Turn}. " +
//    //        $"Cantidad de medicamentos: {context.Medications.Count}.";

//    //    return Task.FromResult(message);
//    //}
//    public Task<string> GenerateMedicationReminderAsync(
//    MedicationReminderContext context)
//    {
//        foreach (var med in context.Medications)
//        {
//            Console.WriteLine(
//                $"{med.Name} | " +
//                $"SpeakName: {med.SpeakName} | " +
//                $"Quantity: {med.Quantity} | " +
//                $"Form: {med.Form} | " +
//                $"Shape: {med.Shape} | " +
//                $"Color: {med.Color}");
//        }

//        return Task.FromResult(
//            $"Marta, turno {context.Turn}");
//    }
//}