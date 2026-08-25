using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DementiaAssistantPOC.Data;
using DementiaAssistantPOC.Models;
using DementiaAssistantPOC.Services;

namespace DementiaAssistantPOC
{
    public class DatabaseTests
    {
        private readonly AppDbContext _context;
        private readonly ContextBuilderService _contextBuilder;
        
        public DatabaseTests(AppDbContext context, ContextBuilderService contextBuilder)
        {
            _context = context;
            _contextBuilder = contextBuilder;
        }
        
        public async Task RunAllTestsAsync()
        {
            Console.WriteLine("\n🧪 INICIANDO PRUEBAS DE BASE DE DATOS\n");
            Console.WriteLine(new string('=', 60));
            
            await Test1_PersonExists();
            await Test2_RelationshipsExist();
            await Test3_RoutinesExist();
            await Test4_LifeEventsExist();
            await Test5_ContextBuilderWorks();
            await Test6_AgendaItemsExist();
            await Test7_PreferencesExist();
            await Test8_ConversationPersistence();
            await Test9_EmotionalMemory();
            
            Console.WriteLine("\n✅ TODAS LAS PRUEBAS COMPLETADAS");
        }
        
        private async Task Test1_PersonExists()
        {
            Console.WriteLine("\n📝 Test 1: Verificar pacientes");
            var people = await _context.People.ToListAsync();
            Console.WriteLine($"   Personas encontradas: {people.Count}");
            foreach (var person in people)
            {
                Console.WriteLine($"   - {person.Name} (ID: {person.Id.Substring(0, 8)}...)");
            }
        }
        
        private async Task Test2_RelationshipsExist()
        {
            Console.WriteLine("\n📝 Test 2: Verificar relaciones");
            var relationships = await _context.PersonRelationships
                .Include(r => r.Person)
                .Include(r => r.RelatedPerson)
                .ToListAsync();
            
            Console.WriteLine($"   Relaciones encontradas: {relationships.Count}");
            foreach (var rel in relationships)
            {
                Console.WriteLine($"   - {rel.Person?.Name} → {rel.RelatedPerson?.Name} ({rel.Notes})");
            }
        }
        
        private async Task Test3_RoutinesExist()
        {
            Console.WriteLine("\n📝 Test 3: Verificar rutinas");
            var routines = await _context.PersonRoutines
                .Include(r => r.Person)
                .Where(r => r.IsActive)
                .ToListAsync();
            
            Console.WriteLine($"   Rutinas activas: {routines.Count}");
            foreach (var routine in routines)
            {
                Console.WriteLine($"   - {routine.Person?.Name}: {routine.Title} a las {routine.StartTime}");
            }
        }
        
        private async Task Test4_LifeEventsExist()
        {
            Console.WriteLine("\n📝 Test 4: Verificar eventos de vida");
            var events = await _context.PersonLifeEvents
                .Include(e => e.Person)
                .ToListAsync();
            
            Console.WriteLine($"   Eventos encontrados: {events.Count}");
            foreach (var lifeEvent in events)
            {
                var emotion = lifeEvent.IsPositiveMemory ? "😊" : "😢";
                Console.WriteLine($"   {emotion} {lifeEvent.Title} - {lifeEvent.StartDate:yyyy}");
            }
        }
        
        private async Task Test5_ContextBuilderWorks()
        {
            Console.WriteLine("\n📝 Test 5: Verificar ContextBuilderService");
            var patient = await _context.People.FirstAsync(p => p.Name == "María González");
            
            var context = await _contextBuilder.BuildContextAsync(patient.Id);
            
            Console.WriteLine("   Contexto generado:");
            Console.WriteLine("   " + new string('─', 40));
            
            // Mostrar primeras 10 líneas del contexto
            var lines = context.Split('\n').Take(15);
            foreach (var line in lines)
            {
                Console.WriteLine($"   {line}");
            }
        }
        
        private async Task Test6_AgendaItemsExist()
        {
            Console.WriteLine("\n📝 Test 6: Verificar agenda");
            var today = DateTime.Now.Date;
            var agendaItems = await _context.PersonAgendaItems
                .Include(a => a.Person)
                .Where(a => a.ScheduledAt >= today)
                .OrderBy(a => a.ScheduledAt)
                .ToListAsync();
            
            Console.WriteLine($"   Actividades próximas: {agendaItems.Count}");
            foreach (var item in agendaItems)
            {
                Console.WriteLine($"   - {item.ScheduledAt:HH:mm} - {item.Title}");
            }
        }
        
        private async Task Test7_PreferencesExist()
        {
            Console.WriteLine("\n📝 Test 7: Verificar preferencias");
            var preferences = await _context.PersonPreferences
                .Include(p => p.Person)
                .Where(p => p.IsActive)
                .ToListAsync();
            
            Console.WriteLine($"   Preferencias activas: {preferences.Count}");
            foreach (var pref in preferences)
            {
                Console.WriteLine($"   - {pref.Person?.Name} prefiere: {pref.Title}");
            }
        }
        
        private async Task Test8_ConversationPersistence()
        {
            Console.WriteLine("\n📝 Test 8: Verificar persistencia de conversaciones");
            
            var patient = await _context.People.FirstAsync(p => p.Name == "María González");
            
            // Crear una conversación de prueba
            var conversation = new Conversation
            {
                PersonId = patient.Id,
                ContextType = "test",
                EmotionalStateStart = "neutral",
                Summary = "Conversación de prueba"
            };
            
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            
            // Agregar mensajes
            _context.ConversationMessages.AddRange(
                new ConversationMessage
                {
                    ConversationId = conversation.Id,
                    Speaker = "user",
                    Content = "Hola, ¿cómo estás?"
                },
                new ConversationMessage
                {
                    ConversationId = conversation.Id,
                    Speaker = "assistant",
                    Content = "Bien, ¿y tú?"
                }
            );
            
            await _context.SaveChangesAsync();
            
            // Verificar que se guardó
            var savedConversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversation.Id);
            
            Console.WriteLine($"   Conversación guardada: {savedConversation?.Id.Substring(0, 8)}...");
            Console.WriteLine($"   Mensajes guardados: {savedConversation?.Messages.Count}");
        }
        
        private async Task Test9_EmotionalMemory()
        {
            Console.WriteLine("\n📝 Test 9: Verificar memoria emocional");
            
            var patient = await _context.People.FirstAsync(p => p.Name == "María González");
            
            // Crear registro emocional
            var emotionalMemory = new EmotionalMemory
            {
                PersonId = patient.Id,
                TriggerContext = "Recordó a su esposo",
                EmotionType = "tristeza",
                Intensity = 7,
                ResponseStrategy = "Hablar de recuerdos felices"
            };
            
            _context.EmotionalMemories.Add(emotionalMemory);
            await _context.SaveChangesAsync();
            
            // Verificar
            var memories = await _context.EmotionalMemories
                .Where(e => e.PersonId == patient.Id)
                .ToListAsync();
            
            Console.WriteLine($"   Memorias emocionales: {memories.Count}");
            foreach (var memory in memories)
            {
                Console.WriteLine($"   - {memory.EmotionType} (intensidad: {memory.Intensity}/10)");
            }
        }
    }
}