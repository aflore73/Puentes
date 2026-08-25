using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DementiaAssistantPOC.Data;

namespace DementiaAssistantPOC.Services
{
    public class RelationshipTests
    {
        private readonly AppDbContext _context;
        private readonly RelationshipService _relationshipService;
        
        public RelationshipTests(AppDbContext context, RelationshipService relationshipService)
        {
            _context = context;
            _relationshipService = relationshipService;
        }
        
        public async Task RunAllRelationshipTestsAsync()
        {
            Console.WriteLine("\n🔗 PRUEBAS DE RELACIONES\n");
            Console.WriteLine(new string('=', 60));
            
            // Buscar una persona que tenga relaciones
            var personWithRelations = await _context.PersonRelationships
                .Select(r => r.PersonId)
                .Distinct()
                .FirstOrDefaultAsync();
            
            if (personWithRelations == null)
            {
                Console.WriteLine("⚠️ No hay relaciones en la base de datos.");
                Console.WriteLine("   Usa la opción 15 para agregar personas y relaciones, o carga datos de ejemplo.");
                return;
            }
            
            var person = await _context.People
                .Where(p => p.Id == personWithRelations)
                .FirstOrDefaultAsync();
            
            if (person == null)
            {
                Console.WriteLine("❌ No se encontró la persona asociada a la relación.");
                return;
            }
            
            Console.WriteLine($"👤 Usando persona: {person.Name} (ID: {person.Id.Substring(0,8)}...)\n");
            
            await Test1_GetAllRelationships(person.Id, person.Name);
            await Test2_GetCloseRelationships(person.Id);
            await Test3_GetEmergencyContacts(person.Id);
            await Test4_GetRelationshipStats(person.Id);
            await Test5_GetSharedEvents(person.Id);
            
            Console.WriteLine("\n✅ PRUEBAS DE RELACIONES COMPLETADAS");
        }
        
        private async Task Test1_GetAllRelationships(string personId, string personName)
        {
            Console.WriteLine("\n📝 Test 1: Obtener todas las relaciones");
            
            var relationships = await _relationshipService.GetRelationshipsWithDetailsAsync(personId);
            
            Console.WriteLine($"   Relaciones encontradas para {personName}: {relationships.Count}");
            foreach (var rel in relationships)
            {
                Console.WriteLine($"\n   👤 {rel.Name}");
                Console.WriteLine($"      Tipo: {rel.Type}");
                Console.WriteLine($"      Cercanía: {rel.ClosenessLevel}/10");
                Console.WriteLine($"      Frecuencia: {rel.FrequencyOfContact}");
                if (rel.IsEmergencyContact)
                    Console.WriteLine($"      🚨 Contacto de emergencia");
                if (!string.IsNullOrEmpty(rel.EmotionalSignificance))
                    Console.WriteLine($"      ❤️ Significado: {rel.EmotionalSignificance}");
            }
        }
        
        private async Task Test2_GetCloseRelationships(string personId)
        {
            Console.WriteLine("\n📝 Test 2: Obtener relaciones más cercanas");
            
            var closePeople = await _relationshipService.GetClosestRelationshipsAsync(personId, 3);
            
            Console.WriteLine($"   Personas más cercanas:");
            foreach (var person in closePeople)
            {
                Console.WriteLine($"   - {person.Name}");
            }
        }
        
        private async Task Test3_GetEmergencyContacts(string personId)
        {
            Console.WriteLine("\n📝 Test 3: Obtener contactos de emergencia");
            
            var emergencyContacts = await _relationshipService.GetEmergencyContactsAsync(personId);
            
            Console.WriteLine($"   Contactos de emergencia: {emergencyContacts.Count}");
            foreach (var contact in emergencyContacts)
            {
                Console.WriteLine($"   🚨 {contact.Name}");
            }
        }
        
        private async Task Test4_GetRelationshipStats(string personId)
        {
            Console.WriteLine("\n📝 Test 4: Estadísticas de relaciones");
            
            var stats = await _relationshipService.GetRelationshipTypeStatsAsync(personId);
            
            foreach (var stat in stats)
            {
                Console.WriteLine($"   {stat.Key}: {stat.Value}");
            }
        }
        
        private async Task Test5_GetSharedEvents(string personId)
        {
            Console.WriteLine("\n📝 Test 5: Eventos compartidos");
            
            var relationships = await _relationshipService.GetRelationshipsWithDetailsAsync(personId);
            
            foreach (var rel in relationships)
            {
                if (rel.SharedEvents.Any())
                {
                    Console.WriteLine($"\n   Eventos con {rel.Name}:");
                    foreach (var evt in rel.SharedEvents)
                    {
                        var emotion = evt.IsPositive ? "😊" : "😢";
                        Console.WriteLine($"   {emotion} {evt.Title} ({evt.Date:yyyy})");
                    }
                }
            }
        }
    }
}