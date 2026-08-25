using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DementiaAssistantPOC.Data;
using DementiaAssistantPOC.Models;

namespace DementiaAssistantPOC.Services
{
    public class ScenarioTests
    {
        private readonly AppDbContext _context;
        private readonly ContextBuilderService _contextBuilder;
        private readonly AIService _aiService;
        
        public ScenarioTests(AppDbContext context, ContextBuilderService contextBuilder, AIService aiService)
        {
            _context = context;
            _contextBuilder = contextBuilder;
            _aiService = aiService;
        }
        
        public async Task RunScenarioTestsAsync()
        {
            Console.WriteLine("\n🎭 PRUEBAS DE ESCENARIOS REALES\n");
            Console.WriteLine(new string('=', 60));
            
            await Scenario1_ConfusionEspacial();
            await Scenario2_RecuerdoFamiliar();
            await Scenario3_RutinaDiaria();
            await Scenario4_AnsiedadNocturna();
            await Scenario5_Medicacion();
            
            Console.WriteLine("\n✅ ESCENARIOS COMPLETADOS");
        }
        
        private async Task Scenario1_ConfusionEspacial()
        {
            Console.WriteLine("\n🎭 Escenario 1: Confusión espacial");
            Console.WriteLine("─".PadRight(40, '─'));
            
            var patient = await _context.People
                .Where(p => p.Name == "María González")
                .FirstOrDefaultAsync();
            
            if (patient == null) return;
            
            var userInput = "¿Dónde estoy? No reconozco esta casa";
            
            var prompt = await _contextBuilder.GetTherapeuticPromptAsync(patient.Id, userInput);
            var response = _aiService.GetSimulatedResponse(prompt, userInput);
            
            Console.WriteLine($"👤 Paciente: {userInput}");
            Console.WriteLine($"🤖 Asistente: {response}");
            
            Console.WriteLine("\n📊 Elementos de contexto disponibles:");
            if (prompt.Contains("Buenos Aires")) Console.WriteLine("   ✅ Ubicación conocida");
            if (prompt.Contains("Carlos")) Console.WriteLine("   ✅ Familiar cercano");
            if (prompt.Contains("Rutina")) Console.WriteLine("   ✅ Rutinas diarias");
        }
        
        private async Task Scenario2_RecuerdoFamiliar()
        {
            Console.WriteLine("\n🎭 Escenario 2: Recuerdo de familiar fallecido");
            Console.WriteLine("─".PadRight(40, '─'));
            
            var patient = await _context.People
                .Where(p => p.Name == "María González")
                .FirstOrDefaultAsync();
            
            if (patient == null) return;
            
            var userInput = "Extraño mucho a Juan, ¿dónde está?";
            
            var prompt = await _contextBuilder.GetTherapeuticPromptAsync(patient.Id, userInput);
            var response = _aiService.GetSimulatedResponse(prompt, userInput);
            
            Console.WriteLine($"👤 Paciente: {userInput}");
            Console.WriteLine($"🤖 Asistente: {response}");
            
            Console.WriteLine("\n📊 Manejo de memoria:");
            if (prompt.Contains("Juan")) Console.WriteLine("   ✅ Reconoce a Juan en el contexto");
            if (prompt.Contains("Casamiento")) Console.WriteLine("   ✅ Tiene recuerdos positivos con Juan");
            
            var emotionalMemory = new EmotionalMemory
            {
                PersonId = patient.Id,
                TriggerContext = "Recuerdo de esposo fallecido",
                EmotionType = "tristeza",
                Intensity = 8,
                ResponseStrategy = "Redirigir a recuerdos positivos"
            };
            
            _context.EmotionalMemories.Add(emotionalMemory);
            await _context.SaveChangesAsync();
            Console.WriteLine("   ✅ Registro emocional guardado");
        }
        
        private async Task Scenario3_RutinaDiaria()
        {
            Console.WriteLine("\n🎭 Escenario 3: Orientación en rutina diaria");
            Console.WriteLine("─".PadRight(40, '─'));
            
            var patient = await _context.People
                .Where(p => p.Name == "María González")
                .FirstOrDefaultAsync();
            
            if (patient == null) return;
            
            var userInput = "¿Ya almorcé? No me acuerdo";
            
            var prompt = await _contextBuilder.GetTherapeuticPromptAsync(patient.Id, userInput);
            var response = _aiService.GetSimulatedResponse(prompt, userInput);
            
            Console.WriteLine($"👤 Paciente: {userInput}");
            Console.WriteLine($"🤖 Asistente: {response}");
            
            Console.WriteLine("\n📊 Información de rutina:");
            var currentTime = DateTime.Now.TimeOfDay;
            var lunchTime = new TimeSpan(12, 30, 0);
            
            if (currentTime > lunchTime.Add(new TimeSpan(1, 0, 0)))
            {
                Console.WriteLine("   ✅ Ya pasó la hora del almuerzo (12:30)");
                Console.WriteLine($"   Hora actual: {DateTime.Now:HH:mm}");
            }
        }
        
        private async Task Scenario4_AnsiedadNocturna()
        {
            Console.WriteLine("\n🎭 Escenario 4: Ansiedad y desorientación");
            Console.WriteLine("─".PadRight(40, '─'));
            
            var patient = await _context.People
                .Where(p => p.Name == "María González")
                .FirstOrDefaultAsync();
            
            if (patient == null) return;
            
            var userInput = "Tengo miedo, no sé qué va a pasar mañana";
            
            var prompt = await _contextBuilder.GetTherapeuticPromptAsync(patient.Id, userInput);
            var response = _aiService.GetSimulatedResponse(prompt, userInput);
            
            Console.WriteLine($"👤 Paciente: {userInput}");
            Console.WriteLine($"🤖 Asistente: {response}");
            
            Console.WriteLine("\n📊 Sugerencias de contexto:");
            if (prompt.Contains("Música")) Console.WriteLine("   ✅ Puede usar música para calmarse");
            if (prompt.Contains("Té")) Console.WriteLine("   ✅ Tiene actividad relajante programada");
            if (prompt.Contains("Carlos")) Console.WriteLine("   ✅ Puede contactar a su hijo");
        }
        
        private async Task Scenario5_Medicacion()
        {
            Console.WriteLine("\n🎭 Escenario 5: Preocupación por medicación");
            Console.WriteLine("─".PadRight(40, '─'));
            
            var patient = await _context.People
                .Where(p => p.Name == "María González")
                .FirstOrDefaultAsync();
            
            if (patient == null) return;
            
            var userInput = "¿Tomé mis pastillas hoy?";
            
            var prompt = await _contextBuilder.GetTherapeuticPromptAsync(patient.Id, userInput);
            var response = _aiService.GetSimulatedResponse(prompt, userInput);
            
            Console.WriteLine($"👤 Paciente: {userInput}");
            Console.WriteLine($"🤖 Asistente: {response}");
            
            Console.WriteLine("\n📊 Información de medicación:");
            var hasMedicationInfo = prompt.Contains("medicación") || prompt.Contains("Medication");
            Console.WriteLine($"   {(hasMedicationInfo ? "✅" : "⚠️")} Hay información de medicación en el contexto");
        }
    }
}