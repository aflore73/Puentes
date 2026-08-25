using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DementiaAssistantPOC.Models;

namespace DementiaAssistantPOC.Data
{
    public class DatabaseCRUDTests
    {
        private readonly AppDbContext _context;
        
        public DatabaseCRUDTests(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task RunCRUDTestsAsync()
        {
            Console.WriteLine("\n📝 PRUEBAS CRUD COMPLETAS\n");
            Console.WriteLine(new string('=', 60));
            
            var testId = await TestCreate();
            await TestRead(testId);
            await TestUpdate(testId);
            await TestDelete(testId);
            
            Console.WriteLine("\n✅ PRUEBAS CRUD COMPLETADAS");
        }
        
        private async Task<string> TestCreate()
        {
            Console.WriteLine("\n📝 CREATE: Crear nueva persona");
            
            var person = new Person
            {
                Name = $"CRUD_Test_{DateTime.Now.Ticks}",
                BirthDate = new DateTime(1950, 1, 1),
                City = "Test City",
                Province = "Test Province",
                Notes = "Persona de prueba CRUD"
            };
            
            _context.People.Add(person);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"   ✅ Creada persona con ID: {person.Id}");
            return person.Id;
        }
        
        private async Task TestRead(string id)
        {
            Console.WriteLine("\n📝 READ: Leer persona");
            
            var person = await _context.People.FindAsync(id);
            
            if (person != null)
            {
                Console.WriteLine($"   ✅ Encontrada: {person.Name}");
                Console.WriteLine($"   Nacimiento: {person.BirthDate:dd/MM/yyyy}");
                Console.WriteLine($"   Ciudad: {person.City}");
            }
            else
            {
                Console.WriteLine("   ❌ No se encontró la persona");
            }
        }
        
        private async Task TestUpdate(string id)
        {
            Console.WriteLine("\n📝 UPDATE: Actualizar persona");
            
            var person = await _context.People.FindAsync(id);
            
            if (person != null)
            {
                person.City = "Updated City";
                person.Notes = "Actualizada en prueba CRUD";
                
                await _context.SaveChangesAsync();
                Console.WriteLine($"   ✅ Actualizada: {person.Name}");
                Console.WriteLine($"   Nueva ciudad: {person.City}");
            }
        }
        
        private async Task TestDelete(string id)
        {
            Console.WriteLine("\n📝 DELETE: Eliminar persona");
            
            var person = await _context.People.FindAsync(id);
            
            if (person != null)
            {
                _context.People.Remove(person);
                await _context.SaveChangesAsync();
                Console.WriteLine($"   ✅ Eliminada: {person.Name}");
            }
            
            // Verificar
            var deleted = await _context.People.FindAsync(id);
            Console.WriteLine($"   Verificación - Existe: {deleted != null}");
        }
    }
}