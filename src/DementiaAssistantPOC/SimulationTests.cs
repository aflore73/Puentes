using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DementiaAssistantPOC.Services;

namespace DementiaAssistantPOC
{
    public class SimulationTests
    {
        private readonly AIService _aiService;
        
        public SimulationTests(AIService aiService)
        {
            _aiService = aiService;
        }
        
        public void RunAllSimulationTests()
        {
            Console.WriteLine("\n🤖 PRUEBAS DE SIMULACIÓN DE IA\n");
            Console.WriteLine(new string('=', 60));
            
            TestEmotionalResponses();
            TestConfusionResponses();
            TestRoutineResponses();
            TestFamilyResponses();
            TestMedicationResponses();
            
            Console.WriteLine("\n✅ PRUEBAS DE SIMULACIÓN COMPLETADAS");
        }
        
        private void TestEmotionalResponses()
        {
            Console.WriteLine("\n📝 Test: Respuestas emocionales");
            
            var testCases = new Dictionary<string, string>
            {
                { "Me siento muy triste hoy", "tristeza" },
                { "Estoy contento de verte", "alegría" },
                { "Tengo miedo", "miedo" },
                { "Me siento solo", "soledad" }
            };
            
            foreach (var test in testCases)
            {
                var response = _aiService.GetSimulatedResponse("", test.Key);
                Console.WriteLine($"\n   Input: \"{test.Key}\"");
                Console.WriteLine($"   Esperado: Detectar {test.Value}");
                Console.WriteLine($"   Respuesta: \"{response}\"");
                
                // Verificar si la respuesta es apropiada
                bool isAppropriate = !string.IsNullOrEmpty(response) && response.Length > 10;
                Console.WriteLine($"   ✅ Apropiada: {isAppropriate}");
            }
        }
        
        private void TestConfusionResponses()
        {
            Console.WriteLine("\n📝 Test: Respuestas a confusión");
            
            var testCases = new[]
            {
                "¿Dónde estoy?",
                "No reconozco este lugar",
                "¿Quién eres tú?",
                "Estoy perdido"
            };
            
            foreach (var input in testCases)
            {
                var response = _aiService.GetSimulatedResponse("", input);
                Console.WriteLine($"\n   Input: \"{input}\"");
                Console.WriteLine($"   Respuesta: \"{response}\"");
                
                // Verificar que la respuesta sea tranquilizadora
                bool isReassuring = response.Contains("seguro") || 
                                   response.Contains("tranquil") || 
                                   response.Contains("aquí");
                Console.WriteLine($"   ✅ Tranquilizadora: {isReassuring}");
            }
        }
        
        private void TestRoutineResponses()
        {
            Console.WriteLine("\n📝 Test: Respuestas sobre rutinas");
            
            var testCases = new[]
            {
                "¿Qué tengo que hacer hoy?",
                "¿Cuál es mi rutina?",
                "¿Ya comí?",
                "¿Tengo algo planeado?"
            };
            
            foreach (var input in testCases)
            {
                var response = _aiService.GetSimulatedResponse("", input);
                Console.WriteLine($"\n   Input: \"{input}\"");
                Console.WriteLine($"   Respuesta: \"{response}\"");
            }
        }
        
        private void TestFamilyResponses()
        {
            Console.WriteLine("\n📝 Test: Respuestas sobre familia");
            
            var testCases = new[]
            {
                "Extraño a mi familia",
                "¿Dónde está mi hijo?",
                "Quiero ver a mis nietos",
                "¿Cuándo viene Carlos?"
            };
            
            foreach (var input in testCases)
            {
                var response = _aiService.GetSimulatedResponse("", input);
                Console.WriteLine($"\n   Input: \"{input}\"");
                Console.WriteLine($"   Respuesta: \"{response}\"");
            }
        }
        
        private void TestMedicationResponses()
        {
            Console.WriteLine("\n📝 Test: Respuestas sobre medicación");
            
            var testCases = new[]
            {
                "¿Tomé mis pastillas?",
                "¿Qué medicamentos tengo que tomar?",
                "No quiero tomar la medicación"
            };
            
            foreach (var input in testCases)
            {
                var response = _aiService.GetSimulatedResponse("", input);
                Console.WriteLine($"\n   Input: \"{input}\"");
                Console.WriteLine($"   Respuesta: \"{response}\"");
            }
        }
    }
}