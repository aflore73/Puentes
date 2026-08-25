using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DementiaAssistantPOC.Data;
using DementiaAssistantPOC.Models;
using DementiaAssistantPOC.Services;

namespace DementiaAssistantPOC
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("🎯 POC - Asistente IA para Demencia\n");
            Console.WriteLine("Selecciona una opción:");
            Console.WriteLine("1. Ejecutar demo básica");
            Console.WriteLine("2. Ejecutar pruebas de base de datos");
            Console.WriteLine("3. Ejecutar pruebas de simulación");
            Console.WriteLine("4. Ejecutar todas las pruebas");
            Console.WriteLine("5. Ejecutar escenarios reales");
            Console.WriteLine("6. Pruebas de conexión a BD");
            Console.WriteLine("7. Pruebas de integridad");
            Console.WriteLine("8. Pruebas CRUD");
            Console.WriteLine("9. Todas las pruebas de BD");
            Console.WriteLine("10. Ver contexto completo");
            Console.WriteLine("11. Probar reconocimiento de nombres");
            Console.WriteLine("12. Buscar persona");
            Console.WriteLine("13. Probar relaciones");
            Console.WriteLine("14. Ver relaciones detalladas");
            Console.WriteLine("15. Gestión de base de datos");
            Console.WriteLine("0. Salir");
            Console.Write("\nOpción: ");
            
            var option = Console.ReadLine();
            
            // Configuración
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            // Servicios
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            // Agregar DbContext con SQLite
            // En Program.cs, dentro del bloque de servicios
            var dbPath = configuration["Database:SQLite:DatabasePath"] ?? "dementia_assistant.db";

            // Asegurar que el directorio exista
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var connectionString = $"Data Source={dbPath}";
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));
            // Fin Configurar DbContext con SQLite
          
            services.AddScoped<ContextBuilderService>();
            services.AddScoped<AIService>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var configurationService = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                
                // Inicializar base de datos
                await context.Database.EnsureCreatedAsync();
                
                // Crear datos de ejemplo si no existen
                await SeedDemoDataAsync(context);
                
                var contextBuilder = scope.ServiceProvider.GetRequiredService<ContextBuilderService>();
                var aiService = scope.ServiceProvider.GetRequiredService<AIService>();
                var relationshipService = new RelationshipService(context);
                
                switch (option)
                {
                    case "1":
                        await RunBasicDemoAsync(context, contextBuilder, aiService);
                        break;
                        
                    case "2":
                        var dbTests = new DatabaseTests(context, contextBuilder);
                        await dbTests.RunAllTestsAsync();
                        break;
                        
                    case "3":
                        var simTests = new SimulationTests(aiService);
                        simTests.RunAllSimulationTests();
                        break;
                        
                    case "4":
                        var dbTestsAll = new DatabaseTests(context, contextBuilder);
                        await dbTestsAll.RunAllTestsAsync();
                        Console.WriteLine("\n" + new string('=', 60));
                        var simTestsAll = new SimulationTests(aiService);
                        simTestsAll.RunAllSimulationTests();
                        break;
                        
                    case "5":
                        var scenarioTests = new ScenarioTests(context, contextBuilder, aiService);
                        await scenarioTests.RunScenarioTestsAsync();
                        break;
                        
                    case "6":
                        var connectionTests = new DatabaseConnectionTests(context, configurationService);
                        await connectionTests.RunAllConnectionTestsAsync();
                        break;
                        
                    case "7":
                        var integrityTests = new DatabaseIntegrityTests(context);
                        await integrityTests.RunIntegrityTestsAsync();
                        break;
                        
                    case "8":
                        var crudTests = new DatabaseCRUDTests(context);
                        await crudTests.RunCRUDTestsAsync();
                        break;
                        
                    case "9":
                        var connTests = new DatabaseConnectionTests(context, configurationService);
                        await connTests.RunAllConnectionTestsAsync();
                        Console.WriteLine("\n" + new string('=', 60));
                        var integTests = new DatabaseIntegrityTests(context);
                        await integTests.RunIntegrityTestsAsync();
                        Console.WriteLine("\n" + new string('=', 60));
                        var crudTestsAll = new DatabaseCRUDTests(context);
                        await crudTestsAll.RunCRUDTestsAsync();
                        break;
                        
                    case "10":
                        var contextViewer = new ContextViewerService(context, contextBuilder);
                        await contextViewer.ShowFullContextAsync();
                        break;
                        
                    case "11":
                        var nameRecognition = new ContextViewerService(context, contextBuilder);
                        await nameRecognition.TestNameRecognitionAsync();
                        break;
                        
                    case "12":
                        var personFinder = new ContextViewerService(context, contextBuilder);
                        await personFinder.FindPersonInteractivelyAsync();
                        break;
                        
                    case "13":
                        var relationshipTests = new RelationshipTests(context, relationshipService);
                        await relationshipTests.RunAllRelationshipTestsAsync();
                        break;
                        
                    case "14":
                        await ShowDetailedRelationshipsAsync(context, relationshipService);
                        break;
                        
                    case "15":
                        await DatabaseManagementMenuAsync(context, configurationService);
                        break;
                        
                    case "0":
                        Console.WriteLine("👋 ¡Hasta pronto!");
                        return;
                        
                    default:
                        Console.WriteLine("❌ Opción no válida");
                        break;
                }
            }
            
            Console.WriteLine("\n✨ Programa finalizado");
        }
        
        static async Task RunBasicDemoAsync(AppDbContext context, ContextBuilderService contextBuilder, AIService aiService)
        {
            Console.WriteLine("\n📋 DEMOSTRACIÓN BÁSICA\n");
            Console.WriteLine(new string('=', 60));
            
            var patient = await context.People
                .Where(p => p.Name == "María González")
                .FirstOrDefaultAsync();
            
            if (patient == null)
            {
                Console.WriteLine("❌ No se encontró a María González");
                return;
            }
            
            await SimulateConversationAsync(context, contextBuilder, aiService, patient.Id, 
                "¿Dónde estoy? No reconozco este lugar...");
            
            Console.WriteLine("\n" + new string('=', 60));
            
            await SimulateConversationAsync(context, contextBuilder, aiService, patient.Id, 
                "Me siento triste, extraño a mi esposo Juan...");
            
            Console.WriteLine("\n" + new string('=', 60));
            
            await SimulateConversationAsync(context, contextBuilder, aiService, patient.Id, 
                "¿Qué tengo que hacer hoy?");
            
            Console.WriteLine("\n✅ Demo completada exitosamente");
        }
        
        static async Task SimulateConversationAsync(
            AppDbContext context, 
            ContextBuilderService contextBuilder, 
            AIService aiService, 
            string personId, 
            string userInput)
        {
            Console.WriteLine($"\n👤 PACIENTE: {userInput}");
            
            // Construir contexto
            var prompt = await contextBuilder.GetTherapeuticPromptAsync(personId, userInput);
            
            // Obtener respuesta
            string response;
            try
            {
                response = await aiService.GetResponseAsync(prompt);
                if (response.StartsWith("⚠️"))
                {
                    response = aiService.GetSimulatedResponse(prompt, userInput);
                    Console.WriteLine("💡 (Usando respuesta simulada - configura API key para IA real)");
                }
            }
            catch
            {
                response = aiService.GetSimulatedResponse(prompt, userInput);
            }
            
            Console.WriteLine($"\n🤖 ASISTENTE: {response}");
            
            // Guardar conversación
            var conversation = new Conversation
            {
                PersonId = personId,
                ContextType = "demo",
                EmotionalStateStart = "neutral",
                Summary = "Conversación de demo"
            };
            
            context.Conversations.Add(conversation);
            await context.SaveChangesAsync();
            
            context.ConversationMessages.AddRange(
                new ConversationMessage
                {
                    ConversationId = conversation.Id,
                    Speaker = "user",
                    Content = userInput
                },
                new ConversationMessage
                {
                    ConversationId = conversation.Id,
                    Speaker = "assistant",
                    Content = response
                }
            );
            
            await context.SaveChangesAsync();
            
            // Mostrar resumen del contexto
            Console.WriteLine("\n📊 CONTEXTO UTILIZADO:");
            Console.WriteLine(new string('─', 40));
            
            var contextLines = prompt.Split('\n')
                .Where(l => l.StartsWith("===") || l.StartsWith("-"))
                .Take(15);
            
            foreach (var line in contextLines)
            {
                Console.WriteLine(line);
            }
        }
        
        static async Task ShowDetailedRelationshipsAsync(AppDbContext context, RelationshipService relationshipService)
        {
            Console.WriteLine("\n💕 RELACIONES DETALLADAS\n");
            Console.WriteLine(new string('=', 60));
            
            // Buscar a María
            var maria = await context.People
                .Where(p => p.Name == "María González")
                .FirstOrDefaultAsync();
            
            if (maria == null)
            {
                Console.WriteLine("❌ No se encontró a María González");
                return;
            }
            
            var relationships = await relationshipService.GetRelationshipsWithDetailsAsync(maria.Id);
            
            if (!relationships.Any())
            {
                Console.WriteLine("❌ No se encontraron relaciones");
                return;
            }
            
            foreach (var rel in relationships)
            {
                Console.WriteLine($"\n👤 {rel.Name}");
                Console.WriteLine($"   Tipo: {rel.Type}");
                Console.WriteLine($"   Cercanía: {rel.ClosenessLevel}/10");
                Console.WriteLine($"   Frecuencia de contacto: {rel.FrequencyOfContact}");
                
                if (rel.IsEmergencyContact)
                    Console.WriteLine("   🚨 Contacto de emergencia");
                
                if (!string.IsNullOrEmpty(rel.EmotionalSignificance))
                    Console.WriteLine($"   ❤️ Significado emocional: {rel.EmotionalSignificance}");
                
                if (!string.IsNullOrEmpty(rel.Notes))
                    Console.WriteLine($"   📝 Notas: {rel.Notes}");
                
                if (rel.SharedEvents.Any())
                {
                    Console.WriteLine("   📅 Eventos compartidos:");
                    foreach (var evt in rel.SharedEvents)
                    {
                        var emotion = evt.IsPositive ? "😊" : "😢";
                        Console.WriteLine($"      {emotion} {evt.Title} ({evt.Date:yyyy})");
                    }
                }
                
                Console.WriteLine(new string('─', 40));
            }
        }
        
        static async Task DatabaseManagementMenuAsync(AppDbContext context, IConfiguration configuration)
        {
            var dbManager = new DatabaseManager(context);
            var dbInitializer = new DatabaseInitializer(context, configuration);
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine("🗄️ GESTIÓN DE BASE DE DATOS\n");
                Console.WriteLine("1. Ver resumen");
                Console.WriteLine("2. Listar todas las personas");
                Console.WriteLine("3. Agregar persona");
                Console.WriteLine("4. Agregar rutina");
                Console.WriteLine("5. Agregar evento de vida");
                Console.WriteLine("6. Hacer backup");
                Console.WriteLine("7. Listar backups");
                Console.WriteLine("8. Reiniciar base de datos");
                Console.WriteLine("0. Volver al menú principal");
                Console.Write("\nOpción: ");
                
                var option = Console.ReadLine();
                
                switch (option)
                {
                    case "1":
                        await dbInitializer.InitializeAsync();
                        break;
                        
                    case "2":
                        await dbManager.ListAllPeopleAsync();
                        break;
                        
                    case "3":
                        Console.Write("Nombre: ");
                        var name = Console.ReadLine();
                        Console.Write("Fecha de nacimiento (dd/mm/yyyy): ");
                        var birthDateStr = Console.ReadLine();
                        DateTime? birthDate = null;
                        if (DateTime.TryParse(birthDateStr, out var parsedDate))
                            birthDate = parsedDate;
                        
                        await dbManager.AddPersonAsync(name?? "", birthDate);
                        break;
                        
                    case "4":
                        await dbManager.ListAllPeopleAsync();
                        Console.Write("\nID de la persona: ");
                        var personId = Console.ReadLine();
                        Console.Write("Título de la rutina: ");
                        var routineTitle = Console.ReadLine();
                        Console.Write("Hora de inicio (HH:mm): ");
                        var startTimeStr = Console.ReadLine();
                        Console.Write("Hora de fin (HH:mm): ");
                        var endTimeStr = Console.ReadLine();
                        
                        if (TimeSpan.TryParse(startTimeStr, out var startTime) && 
                            TimeSpan.TryParse(endTimeStr, out var endTime))
                        {
                            await dbManager.AddRoutineAsync(personId?? "", routineTitle?? "", startTime, endTime);
                        }
                        else
                        {
                            Console.WriteLine("❌ Formato de hora inválido");
                        }
                        break;
                        
                    case "5":
                        await dbManager.ListAllPeopleAsync();
                        Console.Write("\nID de la persona: ");
                        var eventPersonId = Console.ReadLine();
                        Console.Write("Título del evento: ");
                        var eventTitle = Console.ReadLine();
                        Console.Write("Fecha (dd/mm/yyyy): ");
                        var eventDateStr = Console.ReadLine();
                        Console.Write("¿Es un recuerdo positivo? (s/n): ");
                        var isPositive = Console.ReadLine()?.ToLower() == "s";
                        
                        if (DateTime.TryParse(eventDateStr, out var eventDate))
                        {
                            await dbManager.AddLifeEventAsync(eventPersonId?? "", eventTitle?? "", eventDate, isPositive);
                        }
                        else
                        {
                            Console.WriteLine("❌ Formato de fecha inválido");
                        }
                        break;
                        
                    case "8":
                        await dbManager.ClearDatabaseAsync();
                        break;
                        
                    case "0":
                        return;
                        
                    default:
                        Console.WriteLine("❌ Opción no válida");
                        break;
                }
                
                if (option != "0")
                {
                    Console.WriteLine("\nPresiona cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }
        
        static async Task SeedDemoDataAsync(AppDbContext context)
        {
            if (await context.People.AnyAsync())
                return;
            
            Console.WriteLine("🌱 Creando datos de ejemplo...\n");
            
            // Personas
            var maria = new Person
            {
                Name = "María González",
                BirthDate = new DateTime(1945, 3, 15),
                City = "Buenos Aires",
                Province = "CABA",
                Country = "Argentina",
                Notes = "Paciente principal. Demencia leve."
            };
            
            var juan = new Person
            {
                Name = "Juan Pérez",
                BirthDate = new DateTime(1942, 7, 20),
                City = "Buenos Aires",
                Province = "CABA",
                Notes = "Esposo de María (fallecido)"
            };
            
            var carlos = new Person
            {
                Name = "Carlos González",
                BirthDate = new DateTime(1970, 1, 10),
                City = "Buenos Aires",
                Province = "CABA",
                Notes = "Hijo de María. Contacto principal."
            };
            
            var ana = new Person
            {
                Name = "Ana Martínez",
                BirthDate = new DateTime(1975, 5, 22),
                City = "Buenos Aires",
                Province = "CABA",
                Notes = "Cuidadora profesional"
            };
            
            context.People.AddRange(maria, juan, carlos, ana);
            await context.SaveChangesAsync();
            
            // Relaciones con nuevos campos
            context.PersonRelationships.AddRange(
                new PersonRelationship
                {
                    PersonId = maria.Id,
                    RelatedPersonId = juan.Id,
                    Type = 1,
                    Notes = "Esposo",
                    ClosenessLevel = 10,
                    FrequencyOfContact = 1,
                    IsEmergencyContact = false,
                    EmotionalSignificance = "Amor de su vida",
                    PreferredTopics = "Bailar, viajar, la familia"
                },
                new PersonRelationship
                {
                    PersonId = maria.Id,
                    RelatedPersonId = carlos.Id,
                    Type = 1,
                    Notes = "Hijo",
                    ClosenessLevel = 9,
                    FrequencyOfContact = 1,
                    IsEmergencyContact = true,
                    EmotionalSignificance = "Su orgullo y alegría",
                    PreferredTopics = "La familia, los nietos, el trabajo"
                },
                new PersonRelationship
                {
                    PersonId = maria.Id,
                    RelatedPersonId = ana.Id,
                    Type = 3,
                    Notes = "Cuidadora principal",
                    ClosenessLevel = 7,
                    FrequencyOfContact = 1,
                    IsEmergencyContact = true,
                    EmotionalSignificance = "Apoyo diario",
                    PreferredTopics = "Actividades diarias, salud"
                }
            );
            
            // Rutinas
            context.PersonRoutines.AddRange(
                new PersonRoutine
                {
                    PersonId = maria.Id,
                    Title = "Despertar",
                    Notes = "Levantarse de la cama con calma",
                    DaysOfWeek = "1,2,3,4,5,6,7",
                    StartTime = new TimeSpan(7, 30, 0),
                    EndTime = new TimeSpan(8, 0, 0)
                },
                new PersonRoutine
                {
                    PersonId = maria.Id,
                    Title = "Desayuno",
                    Notes = "Tostadas con mate",
                    DaysOfWeek = "1,2,3,4,5,6,7",
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(8, 30, 0)
                },
                new PersonRoutine
                {
                    PersonId = maria.Id,
                    Title = "Almuerzo",
                    Notes = "Comida liviana",
                    DaysOfWeek = "1,2,3,4,5,6,7",
                    StartTime = new TimeSpan(12, 30, 0),
                    EndTime = new TimeSpan(13, 15, 0)
                },
                new PersonRoutine
                {
                    PersonId = maria.Id,
                    Title = "Cena",
                    Notes = "Comida liviana",
                    DaysOfWeek = "1,2,3,4,5,6,7",
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(20, 30, 0)
                }
            );
            
            // Eventos de vida
            context.PersonLifeEvents.AddRange(
                new PersonLifeEvent
                {
                    PersonId = maria.Id,
                    StartDate = new DateTime(1965, 6, 12),
                    Title = "Casamiento con Juan",
                    Description = "Boda en la iglesia del barrio con Juan",
                    Place = "Buenos Aires",
                    IsPositiveMemory = true
                },
                new PersonLifeEvent
                {
                    PersonId = maria.Id,
                    StartDate = new DateTime(1970, 4, 5),
                    Title = "Nacimiento de Carlos",
                    Description = "Nacimiento de su hijo Carlos",
                    Place = "Buenos Aires",
                    IsPositiveMemory = true
                },
                new PersonLifeEvent
                {
                    PersonId = maria.Id,
                    StartDate = new DateTime(1980, 9, 15),
                    Title = "Vacaciones familiares",
                    Description = "Verano en la playa con Juan y Carlos",
                    Place = "Mar del Plata",
                    IsPositiveMemory = true
                }
            );
            
            // Preferencias
            context.PersonPreferences.AddRange(
                new PersonPreference
                {
                    PersonId = maria.Id,
                    Title = "Música",
                    Notes = "Le gusta el tango y la música clásica",
                    Tags = "música, tango, clásica"
                },
                new PersonPreference
                {
                    PersonId = maria.Id,
                    Title = "Comida",
                    Notes = "Prefiere comidas caseras, especialmente pastas",
                    Tags = "comida, pastas, casera"
                },
                new PersonPreference
                {
                    PersonId = maria.Id,
                    Title = "Actividades",
                    Notes = "Disfruta tejer y mirar fotos familiares",
                    Tags = "tejer, fotos, familia"
                }
            );
            
            // Agenda
            var today = DateTime.Now.Date;
            context.PersonAgendaItems.AddRange(
                new PersonAgendaItem
                {
                    PersonId = maria.Id,
                    ScheduledAt = today.AddHours(9),
                    Title = "Visita de Carlos",
                    Description = "Carlos viene a visitar",
                    Place = "Casa"
                },
                new PersonAgendaItem
                {
                    PersonId = maria.Id,
                    ScheduledAt = today.AddHours(16),
                    Title = "Té de la tarde",
                    Description = "Tomar té con galletitas",
                    Place = "Cocina"
                }
            );
            
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Datos de ejemplo creados\n");
        }
    }
}