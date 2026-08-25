using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DementiaAssistantPOC.Models;

namespace DementiaAssistantPOC.Data
{
    public class DatabaseInitializer
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        
        public DatabaseInitializer(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        
        public async Task InitializeAsync()
        {
            // Asegurar que la base de datos existe
            await _context.Database.EnsureCreatedAsync();
            
            // Verificar si hay datos
            if (!await _context.People.AnyAsync())
            {
                Console.WriteLine("📦 Base de datos vacía. Creando estructura...");
                await SeedInitialDataAsync();
            }
            else
            {
                Console.WriteLine("✅ Base de datos existente encontrada");
                await ShowDatabaseSummaryAsync();
            }
        }
        
        private async Task SeedInitialDataAsync()
        {
            Console.WriteLine("🌱 Sembrando datos iniciales...");
            
            // Crear personas principales
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
            
            _context.People.AddRange(maria, juan, carlos, ana);
            await _context.SaveChangesAsync();
            
            // Relaciones
            _context.PersonRelationships.AddRange(
                new PersonRelationship
                {
                    PersonId = maria.Id,
                    RelatedPersonId = juan.Id,
                    Type = 1, // Familiar
                    Notes = "Esposo"
                },
                new PersonRelationship
                {
                    PersonId = maria.Id,
                    RelatedPersonId = carlos.Id,
                    Type = 1, // Familiar
                    Notes = "Hijo"
                },
                new PersonRelationship
                {
                    PersonId = maria.Id,
                    RelatedPersonId = ana.Id,
                    Type = 3, // Cuidador
                    Notes = "Cuidadora principal"
                }
            );
            
            // Rutinas completas
            _context.PersonRoutines.AddRange(
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
                    Title = "Medicación matutina",
                    Notes = "Tomar medicación con el desayuno",
                    DaysOfWeek = "1,2,3,4,5,6,7",
                    StartTime = new TimeSpan(8, 30, 0),
                    EndTime = new TimeSpan(8, 45, 0)
                },
                new PersonRoutine
                {
                    PersonId = maria.Id,
                    Title = "Caminata",
                    Notes = "Vuelta a la manzana si el clima lo permite",
                    DaysOfWeek = "1,3,5",
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(10, 30, 0)
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
                    Title = "Siesta",
                    Notes = "Descanso después del almuerzo",
                    DaysOfWeek = "1,2,3,4,5,6,7",
                    StartTime = new TimeSpan(14, 0, 0),
                    EndTime = new TimeSpan(15, 30, 0)
                },
                new PersonRoutine
                {
                    PersonId = maria.Id,
                    Title = "Té de la tarde",
                    Notes = "Con galletitas",
                    DaysOfWeek = "1,2,3,4,5,6,7",
                    StartTime = new TimeSpan(17, 0, 0),
                    EndTime = new TimeSpan(17, 30, 0)
                },
                new PersonRoutine
                {
                    PersonId = maria.Id,
                    Title = "Cena",
                    Notes = "Comida liviana",
                    DaysOfWeek = "1,2,3,4,5,6,7",
                    StartTime = new TimeSpan(20, 0, 0),
                    EndTime = new TimeSpan(20, 30, 0)
                },
                new PersonRoutine
                {
                    PersonId = maria.Id,
                    Title = "Preparación para dormir",
                    Notes = "Lavarse los dientes y cambiarse",
                    DaysOfWeek = "1,2,3,4,5,6,7",
                    StartTime = new TimeSpan(22, 0, 0),
                    EndTime = new TimeSpan(22, 30, 0)
                }
            );
            
            // Eventos de vida importantes
            _context.PersonLifeEvents.AddRange(
                new PersonLifeEvent
                {
                    PersonId = maria.Id,
                    StartDate = new DateTime(1965, 6, 12),
                    Title = "Casamiento con Juan",
                    Description = "Boda en la iglesia del barrio",
                    Place = "Buenos Aires",
                    IsPositiveMemory = true
                },
                new PersonLifeEvent
                {
                    PersonId = maria.Id,
                    StartDate = new DateTime(1970, 4, 5),
                    Title = "Nacimiento de Carlos",
                    Description = "Su único hijo",
                    Place = "Buenos Aires",
                    IsPositiveMemory = true
                },
                new PersonLifeEvent
                {
                    PersonId = maria.Id,
                    StartDate = new DateTime(1980, 9, 15),
                    Title = "Vacaciones familiares",
                    Description = "Verano en la playa",
                    Place = "Mar del Plata",
                    IsPositiveMemory = true
                }
            );
            
            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Datos iniciales creados correctamente");
        }
        
        private async Task ShowDatabaseSummaryAsync()
        {
            Console.WriteLine("\n📊 RESUMEN DE BASE DE DATOS");
            Console.WriteLine(new string('=', 40));
            
            var peopleCount = await _context.People.CountAsync();
            var relationshipsCount = await _context.PersonRelationships.CountAsync();
            var routinesCount = await _context.PersonRoutines.CountAsync();
            var lifeEventsCount = await _context.PersonLifeEvents.CountAsync();
            var preferencesCount = await _context.PersonPreferences.CountAsync();
            var conversationsCount = await _context.Conversations.CountAsync();
            
            Console.WriteLine($"👥 Personas: {peopleCount}");
            Console.WriteLine($"🔗 Relaciones: {relationshipsCount}");
            Console.WriteLine($"📅 Rutinas: {routinesCount}");
            Console.WriteLine($"📖 Eventos de vida: {lifeEventsCount}");
            Console.WriteLine($"⭐ Preferencias: {preferencesCount}");
            Console.WriteLine($"💬 Conversaciones: {conversationsCount}");
            
            // Mostrar pacientes principales
            var patients = await _context.People
                .Where(p => p.Notes != null && p.Notes.Contains("Paciente"))
                .ToListAsync();
            
            if (patients.Any())
            {
                Console.WriteLine("\n🏥 Pacientes registrados:");
                foreach (var patient in patients)
                {
                    Console.WriteLine($"   - {patient.Name}");
                }
            }
        }
        
    }
}