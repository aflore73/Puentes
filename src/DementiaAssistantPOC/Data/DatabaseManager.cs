using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DementiaAssistantPOC.Models;

namespace DementiaAssistantPOC.Data
{
    public class DatabaseManager
    {
        private readonly AppDbContext _context;
        
        public DatabaseManager(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task AddPersonAsync(string name, DateTime? birthDate = null, string notes = null)
        {
            var person = new Person
            {
                Name = name,
                BirthDate = birthDate,
                Notes = notes
            };
            
            _context.People.Add(person);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Persona agregada: {name}");
        }
        
        public async Task AddRelationshipAsync(string personId, string relatedPersonId, int type, string notes = null)
        {
            var relationship = new PersonRelationship
            {
                PersonId = personId,
                RelatedPersonId = relatedPersonId,
                Type = type,
                Notes = notes
            };
            
            _context.PersonRelationships.Add(relationship);
            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Relación agregada");
        }
        
        public async Task AddRoutineAsync(string personId, string title, TimeSpan startTime, TimeSpan endTime, string daysOfWeek = "1,2,3,4,5,6,7")
        {
            var routine = new PersonRoutine
            {
                PersonId = personId,
                Title = title,
                StartTime = startTime,
                EndTime = endTime,
                DaysOfWeek = daysOfWeek
            };
            
            _context.PersonRoutines.Add(routine);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Rutina agregada: {title}");
        }
        
        public async Task AddLifeEventAsync(string personId, string title, DateTime date, bool isPositive = true, string description = null)
        {
            var lifeEvent = new PersonLifeEvent
            {
                PersonId = personId,
                Title = title,
                StartDate = date,
                IsPositiveMemory = isPositive,
                Description = description
            };
            
            _context.PersonLifeEvents.Add(lifeEvent);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Evento agregado: {title}");
        }
        
        public async Task ListAllPeopleAsync()
        {
            var people = await _context.People.ToListAsync();
            
            Console.WriteLine("\n👥 LISTA DE PERSONAS");
            Console.WriteLine(new string('=', 40));
            
            foreach (var person in people)
            {
                Console.WriteLine($"ID: {person.Id}");
                Console.WriteLine($"Nombre: {person.Name}");
                if (person.BirthDate.HasValue)
                    Console.WriteLine($"Nacimiento: {person.BirthDate:dd/MM/yyyy}");
                if (!string.IsNullOrEmpty(person.Notes))
                    Console.WriteLine($"Notas: {person.Notes}");
                Console.WriteLine(new string('-', 30));
            }
        }
        
        public async Task ClearDatabaseAsync()
        {
            Console.WriteLine("⚠️ ¿Estás seguro de que quieres borrar TODOS los datos? (s/n)");
            var response = Console.ReadLine();
            
            if (response?.ToLower() == "s")
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
                Console.WriteLine("✅ Base de datos reiniciada");
            }
            else
            {
                Console.WriteLine("❌ Operación cancelada");
            }
        }
    }
}