using System;
using System.Threading.Tasks;
using DementiaAssistantPOC.Data;
using Microsoft.EntityFrameworkCore;

namespace DementiaAssistantPOC.Services
{
    public class ContextViewerService
    {
        private readonly AppDbContext _context;
        private readonly ContextBuilderService _contextBuilder;
        
        public ContextViewerService(AppDbContext context, ContextBuilderService contextBuilder)
        {
            _context = context;
            _contextBuilder = contextBuilder;
        }
        
        public async Task FindPersonInteractivelyAsync()
        {
            Console.WriteLine("\n🔍 BUSCAR PERSONA\n");
            Console.WriteLine(new string('=', 60));
            
            Console.Write("Ingresa el nombre (o parte del nombre): ");
            var searchTerm = Console.ReadLine();
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                Console.WriteLine("❌ Debes ingresar un nombre");
                return;
            }
            
            var people = await _context.People
                .Where(p => p.Name.Contains(searchTerm))
                .ToListAsync();
            
            if (people.Any())
            {
                Console.WriteLine($"\n✅ Se encontraron {people.Count} personas:");
                foreach (var person in people)
                {
                    Console.WriteLine($"   - {person.Name}");
                }
            }
            else
            {
                Console.WriteLine($"❌ No se encontraron personas con: {searchTerm}");
            }
        }
        public async Task ShowFullContextAsync(string personName = "María González")
        {
            Console.WriteLine("\n🔍 VISUALIZADOR DE CONTEXTO COMPLETO\n");
            Console.WriteLine(new string('=', 60));
            
            var person = await _context.People
                .FirstOrDefaultAsync(p => p.Name == personName);
            
            if (person == null)
            {
                Console.WriteLine($"❌ No se encontró a {personName}");
                return;
            }
            
            // Mostrar contexto sin input del usuario
            var baseContext = await _contextBuilder.BuildContextAsync(person.Id);
            
            Console.WriteLine("📋 CONTEXTO BASE (sin input del usuario):\n");
            Console.WriteLine(baseContext);
            
            Console.WriteLine("\n" + new string('=', 60));
            
            // Mostrar contexto con diferentes inputs
            var testInputs = new[]
            {
                "Extraño a Juan",
                "¿Dónde está Carlos?",
                "Tengo miedo",
                "¿Qué tengo que hacer hoy?"
            };
            
            foreach (var input in testInputs)
            {
                Console.WriteLine($"\n📋 CONTEXTO CON INPUT: \"{input}\"\n");
                var contextWithInput = await _contextBuilder.BuildContextAsync(person.Id, input);
                
                // Mostrar solo las partes relevantes (Personas importantes y Recuerdos)
                var lines = contextWithInput.Split('\n');
                bool inRelevantSection = false;
                
                foreach (var line in lines)
                {
                    if (line.Contains("PERSONAS IMPORTANTES") || line.Contains("RECUERDOS POSITIVOS"))
                    {
                        inRelevantSection = true;
                        Console.WriteLine(line);
                        continue;
                    }
                    
                    if (inRelevantSection)
                    {
                        if (line.StartsWith("===") && !line.Contains("PERSONAS") && !line.Contains("RECUERDOS"))
                        {
                            inRelevantSection = false;
                        }
                        else
                        {
                            Console.WriteLine(line);
                        }
                    }
                }
                
                Console.WriteLine(new string('─', 40));
            }
        }
        
        public async Task TestNameRecognitionAsync(string personName = "María González")
        {
            Console.WriteLine("\n🧪 PRUEBA DE RECONOCIMIENTO DE NOMBRES\n");
            Console.WriteLine(new string('=', 60));
            
            var person = await _context.People
                .Where(p => p.Name == personName).FirstOrDefaultAsync();
            
            if (person == null) return;
            
            var testInputs = new[]
            {
                "Extraño mucho a Juan",
                "¿Cuándo viene Carlos?",
                "Me acuerdo de Ana",
                "Quiero ver a mi familia"
            };
            
            foreach (var input in testInputs)
            {
                var context = await _contextBuilder.BuildContextAsync(person.Id, input);
                
                Console.WriteLine($"\nInput: \"{input}\"");
                
                // Verificar si reconoció nombres
                if (input.Contains("Juan") && context.Contains("Juan Pérez"))
                    Console.WriteLine("✅ Reconoció a Juan");
                else if (input.Contains("Juan"))
                    Console.WriteLine("❌ No reconoció a Juan");
                
                if (input.Contains("Carlos") && context.Contains("Carlos González"))
                    Console.WriteLine("✅ Reconoció a Carlos");
                else if (input.Contains("Carlos"))
                    Console.WriteLine("❌ No reconoció a Carlos");
                
                if (input.Contains("Ana") && context.Contains("Ana Martínez"))
                    Console.WriteLine("✅ Reconoció a Ana");
                else if (input.Contains("Ana"))
                    Console.WriteLine("❌ No reconoció a Ana");
            }
        }
    }
}