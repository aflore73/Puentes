using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DementiaAssistantPOC.Models;

namespace DementiaAssistantPOC.Data
{
    public class DatabaseIntegrityTests
    {
        private readonly AppDbContext _context;
        
        public DatabaseIntegrityTests(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task RunIntegrityTestsAsync()
        {
            Console.WriteLine("\n🔍 PRUEBAS DE INTEGRIDAD DE DATOS\n");
            Console.WriteLine(new string('=', 60));
            
            await Test1_NoOrphanedRelationships();
            await Test2_NoOrphanedRoutines();
            await Test3_NoOrphanedLifeEvents();
            await Test4_NoOrphanedAgendaItems();
            await Test5_NoDuplicateNames();
            await Test6_ValidDates();
            await Test7_ValidTimeSpans();
            
            Console.WriteLine("\n✅ PRUEBAS DE INTEGRIDAD COMPLETADAS");
        }
        
        private async Task Test1_NoOrphanedRelationships()
        {
            Console.WriteLine("\n📝 Test: Relaciones huérfanas");
            
            var orphanedRelationships = await _context.PersonRelationships
                .Where(r => !_context.People.Any(p => p.Id == r.PersonId))
                .CountAsync();
            
            Console.WriteLine($"   Relaciones huérfanas: {orphanedRelationships}");
            
            if (orphanedRelationships == 0)
                Console.WriteLine("   ✅ No hay relaciones huérfanas");
            else
                Console.WriteLine("   ⚠️ Se encontraron relaciones huérfanas");
        }
        
        private async Task Test2_NoOrphanedRoutines()
        {
            Console.WriteLine("\n📝 Test: Rutinas huérfanas");
            
            var orphanedRoutines = await _context.PersonRoutines
                .Where(r => !_context.People.Any(p => p.Id == r.PersonId))
                .CountAsync();
            
            Console.WriteLine($"   Rutinas huérfanas: {orphanedRoutines}");
            
            if (orphanedRoutines == 0)
                Console.WriteLine("   ✅ No hay rutinas huérfanas");
            else
                Console.WriteLine("   ⚠️ Se encontraron rutinas huérfanas");
        }
        
        private async Task Test3_NoOrphanedLifeEvents()
        {
            Console.WriteLine("\n📝 Test: Eventos de vida huérfanos");
            
            var orphanedEvents = await _context.PersonLifeEvents
                .Where(e => !_context.People.Any(p => p.Id == e.PersonId))
                .CountAsync();
            
            Console.WriteLine($"   Eventos huérfanos: {orphanedEvents}");
            
            if (orphanedEvents == 0)
                Console.WriteLine("   ✅ No hay eventos huérfanos");
            else
                Console.WriteLine("   ⚠️ Se encontraron eventos huérfanos");
        }
        
        private async Task Test4_NoOrphanedAgendaItems()
        {
            Console.WriteLine("\n📝 Test: Agenda huérfana");
            
            var orphanedAgenda = await _context.PersonAgendaItems
                .Where(a => !_context.People.Any(p => p.Id == a.PersonId))
                .CountAsync();
            
            Console.WriteLine($"   Items de agenda huérfanos: {orphanedAgenda}");
            
            if (orphanedAgenda == 0)
                Console.WriteLine("   ✅ No hay agenda huérfana");
            else
                Console.WriteLine("   ⚠️ Se encontraron items huérfanos");
        }
        
        private async Task Test5_NoDuplicateNames()
        {
            Console.WriteLine("\n📝 Test: Nombres duplicados");
            
            var duplicates = await _context.People
                .GroupBy(p => p.Name)
                .Where(g => g.Count() > 1)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToListAsync();
            
            Console.WriteLine($"   Nombres duplicados: {duplicates.Count}");
            
            if (duplicates.Count == 0)
                Console.WriteLine("   ✅ No hay nombres duplicados");
            else
            {
                foreach (var dup in duplicates)
                {
                    Console.WriteLine($"   ⚠️ {dup.Name}: {dup.Count} veces");
                }
            }
        }
        
        private async Task Test6_ValidDates()
        {
            Console.WriteLine("\n📝 Test: Fechas válidas");
            
            var invalidDates = await _context.People
                .Where(p => p.BirthDate.HasValue && p.BirthDate > DateTime.Now)
                .CountAsync();
            
            Console.WriteLine($"   Fechas de nacimiento futuras: {invalidDates}");
            
            if (invalidDates == 0)
                Console.WriteLine("   ✅ Todas las fechas son válidas");
            else
                Console.WriteLine("   ⚠️ Hay fechas inválidas");
        }
        
        private async Task Test7_ValidTimeSpans()
        {
            Console.WriteLine("\n📝 Test: Horarios válidos");
            
            var routines = await _context.PersonRoutines
                .Where(r => r.StartTime.HasValue && r.EndTime.HasValue)
                .ToListAsync();
            
            var invalidTimeSpans = routines
                .Where(r => r.StartTime > r.EndTime)
                .Count();
            
            Console.WriteLine($"   Rutinas con horarios invertidos: {invalidTimeSpans}");
            
            if (invalidTimeSpans == 0)
                Console.WriteLine("   ✅ Todos los horarios son válidos");
            else
                Console.WriteLine("   ⚠️ Hay horarios inválidos");
        }
    }
}