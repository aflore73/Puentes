using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DementiaAssistantPOC.Data;
using DementiaAssistantPOC.Models;

namespace DementiaAssistantPOC.Services
{
    public class ContextBuilderService
    {
        private readonly AppDbContext _context;
        
        public ContextBuilderService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<string> BuildContextAsync(string personId, string? userInput = null)
        {
            var sb = new StringBuilder();
            
            // 1. Información básica del paciente
            var person = await _context.People
                .FirstOrDefaultAsync(p => p.Id == personId);
            
            if (person == null) return "Paciente no encontrado";
            
            sb.AppendLine("=== CONTEXTO DEL PACIENTE ===");
            sb.AppendLine($"Nombre: {person.Name}");
            if (person.BirthDate.HasValue)
            {
                var age = DateTime.Now.Year - person.BirthDate.Value.Year;
                sb.AppendLine($"Edad: {age} años");
            }
            sb.AppendLine($"Ciudad: {person.City}, {person.Province}");
            
            // 2. Estado emocional reciente
            var recentEmotions = await _context.EmotionalMemories
                .Where(e => e.PersonId == personId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(3)
                .ToListAsync();
            
            if (recentEmotions.Any())
            {
                sb.AppendLine("\n=== ESTADO EMOCIONAL RECIENTE ===");
                foreach (var emotion in recentEmotions)
                {
                    sb.AppendLine($"- {emotion.EmotionType} (intensidad: {emotion.Intensity}/10)");
                }
            }
            
            // 3. Actividades próximas (próximas 4 horas)
            var upcomingActivities = await _context.PersonAgendaItems
                .Where(a => a.PersonId == personId && 
                           a.ScheduledAt >= DateTime.Now && 
                           a.ScheduledAt <= DateTime.Now.AddHours(4))
                .OrderBy(a => a.ScheduledAt)
                .Take(3)
                .ToListAsync();
            
            if (upcomingActivities.Any())
            {
                sb.AppendLine("\n=== ACTIVIDADES PRÓXIMAS ===");
                foreach (var activity in upcomingActivities)
                {
                    sb.AppendLine($"- {activity.ScheduledAt:HH:mm} - {activity.Title}");
                    if (!string.IsNullOrEmpty(activity.Place))
                        sb.AppendLine($"  Lugar: {activity.Place}");
                }
            }
            
            // 4. Rutinas activas
            var activeRoutines = await _context.PersonRoutines
                .Where(r => r.PersonId == personId && r.IsActive)
                .Take(3)
                .ToListAsync();
            
            if (activeRoutines.Any())
            {
                sb.AppendLine("\n=== RUTINAS HABITUALES ===");
                foreach (var routine in activeRoutines)
                {
                    sb.AppendLine($"- {routine.Title}: {routine.StartTime} - {routine.EndTime}");
                }
            }
            
            // 5. Relaciones importantes - MEJORADO
            var relationships = await _context.PersonRelationships
                .Where(r => r.PersonId == personId)
                .Include(r => r.RelatedPerson)
                .ToListAsync();
            
            if (relationships.Any())
            {
                sb.AppendLine("\n=== PERSONAS IMPORTANTES ===");
                
                // Si el usuario menciona un nombre, priorizar esa persona
                if (!string.IsNullOrEmpty(userInput))
                {
                    var mentionedRelationships = relationships
                        .Where(r => r.RelatedPerson != null && 
                                   userInput.Contains(r.RelatedPerson.Name.Split(' ')[0], StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    
                    if (mentionedRelationships.Any())
                    {
                        sb.AppendLine("--- Personas mencionadas por el paciente ---");
                        foreach (var rel in mentionedRelationships)
                        {
                            var relType = GetRelationshipType(rel.Type);
                            sb.AppendLine($"- {rel.RelatedPerson?.Name} ({relType})");
                            if (!string.IsNullOrEmpty(rel.Notes))
                                sb.AppendLine($"  Relación: {rel.Notes}");
                        }
                        
                        // Agregar información detallada de la persona mencionada
                        foreach (var rel in mentionedRelationships.Take(2))
                        {
                            if (rel.RelatedPerson != null)
                            {
                                sb.AppendLine($"\n  Detalles de {rel.RelatedPerson.Name}:");
                                if (rel.RelatedPerson.BirthDate.HasValue)
                                    sb.AppendLine($"  - Nacimiento: {rel.RelatedPerson.BirthDate:dd/MM/yyyy}");
                                if (!string.IsNullOrEmpty(rel.RelatedPerson.Notes))
                                    sb.AppendLine($"  - Notas: {rel.RelatedPerson.Notes}");
                                
                                // Buscar eventos compartidos
                                var sharedEvents = await _context.PersonLifeEvents
                                    .Where(e => e.PersonId == personId && 
                                               e.Description != null && 
                                               e.Description.Contains(rel.RelatedPerson.Name.Split(' ')[0]))
                                    .ToListAsync();
                                
                                if (sharedEvents.Any())
                                {
                                    sb.AppendLine($"  - Eventos compartidos:");
                                    foreach (var evt in sharedEvents.Take(3))
                                    {
                                        sb.AppendLine($"    * {evt.Title} ({evt.StartDate:yyyy})");
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Agregar otras relaciones importantes
                var otherRelationships = relationships
                    .Where(r => r.RelatedPerson != null && 
                               (string.IsNullOrEmpty(userInput) || 
                                !userInput.Contains(r.RelatedPerson.Name.Split(' ')[0], StringComparison.OrdinalIgnoreCase)))
                    .Take(5)
                    .ToList();
                
                if (otherRelationships.Any())
                {
                    if (relationships.Any(r => r.RelatedPerson != null && 
                                             !string.IsNullOrEmpty(userInput) && 
                                             userInput.Contains(r.RelatedPerson.Name.Split(' ')[0], StringComparison.OrdinalIgnoreCase)))
                    {
                        sb.AppendLine("\n--- Otras personas importantes ---");
                    }
                    
                    foreach (var rel in otherRelationships)
                    {
                        var relType = GetRelationshipType(rel.Type);
                        sb.AppendLine($"- {rel.RelatedPerson?.Name} ({relType})");
                    }
                }
            }
            
            // 6. Memorias positivas - MEJORADO
            var positiveMemories = await _context.PersonLifeEvents
                .Where(e => e.PersonId == personId && e.IsPositiveMemory)
                .OrderByDescending(e => e.StartDate)
                .Take(3)
                .ToListAsync();
            
            if (positiveMemories.Any())
            {
                sb.AppendLine("\n=== RECUERDOS POSITIVOS ===");
                
                // Si menciona a alguien, priorizar recuerdos con esa persona
                if (!string.IsNullOrEmpty(userInput))
                {
                    var mentionedName = ExtractMentionedName(userInput, relationships);
                    if (!string.IsNullOrEmpty(mentionedName))
                    {
                        var relevantMemories = positiveMemories
                            .Where(m => m.Description != null && 
                                       m.Description.Contains(mentionedName, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        
                        if (relevantMemories.Any())
                        {
                            sb.AppendLine($"--- Recuerdos con {mentionedName} ---");
                            foreach (var memory in relevantMemories.Take(2))
                            {
                                sb.AppendLine($"- {memory.Title} ({memory.StartDate:yyyy})");
                                if (!string.IsNullOrEmpty(memory.Description))
                                    sb.AppendLine($"  {memory.Description}");
                            }
                        }
                    }
                }
                
                // Agregar otros recuerdos
                foreach (var memory in positiveMemories.Take(2))
                {
                    sb.AppendLine($"- {memory.Title} ({memory.StartDate:yyyy})");
                }
            }
            
            // 7. Preferencias
            var preferences = await _context.PersonPreferences
                .Where(p => p.PersonId == personId && p.IsActive)
                .Take(3)
                .ToListAsync();
            
            if (preferences.Any())
            {
                sb.AppendLine("\n=== PREFERENCIAS ===");
                foreach (var pref in preferences)
                {
                    sb.AppendLine($"- {pref.Title}: {pref.Notes}");
                }
            }
            
            // 8. Conversación reciente
            var recentConversations = await _context.Conversations
                .Where(c => c.PersonId == personId)
                .OrderByDescending(c => c.StartedAt)
                .Take(1)
                .Include(c => c.Messages)
                .ToListAsync();
            
            if (recentConversations.Any())
            {
                var lastConv = recentConversations.First();
                sb.AppendLine("\n=== ÚLTIMA CONVERSACIÓN ===");
                var lastMessages = lastConv.Messages
                    .OrderByDescending(m => m.Timestamp)
                    .Take(3)
                    .Reverse();
                
                foreach (var msg in lastMessages)
                {
                    sb.AppendLine($"{msg.Speaker}: {msg.Content}");
                }
            }
            
            // 9. Input del usuario
            if (!string.IsNullOrEmpty(userInput))
            {
                sb.AppendLine($"\n=== INPUT ACTUAL DEL USUARIO ===\n{userInput}");
            }
            
            return sb.ToString();
        }
        
        private string GetRelationshipType(int type)
        {
            return type switch
            {
                1 => "Familiar",
                2 => "Amigo/a",
                3 => "Cuidador/a",
                _ => "Conocido/a"
            };
        }
        
        private string? ExtractMentionedName(string userInput, System.Collections.Generic.List<PersonRelationship> relationships)
        {
            foreach (var rel in relationships)
            {
                if (rel.RelatedPerson != null)
                {
                    var firstName = rel.RelatedPerson.Name.Split(' ')[0];
                    if (userInput.Contains(firstName, StringComparison.OrdinalIgnoreCase))
                    {
                        return firstName;
                    }
                }
            }
            return null;
        }
        
        public async Task<string> GetTherapeuticPromptAsync(string personId, string userInput)
        {
            var context = await BuildContextAsync(personId, userInput);
            
            return $@"
[SISTEMA]
Eres un asistente terapéutico especializado en demencia, ansiedad y pérdida de memoria.
Tu objetivo es proporcionar apoyo emocional, orientación y compañía.

[DIRECTRICES IMPORTANTES]
1. Mantén respuestas cortas y simples (máximo 2-3 oraciones)
2. Usa un tono calmado, cálido y tranquilizador
3. NO corrijas memorias erróneas - redirige suavemente
4. Valida las emociones siempre
5. Usa los nombres de personas importantes cuando sea apropiado
6. Si hay confusión, orienta gentilmente con información del contexto
7. Prioriza la seguridad y el bienestar emocional
8. Si detectas angustia severa, sugiere contactar a un familiar o cuidador
9. Si el paciente menciona a alguien, usa esa información para conectar emocionalmente

[CONTEXTO DEL PACIENTE]
{context}

[RESPUESTA TERAPÉUTICA]";
        }
    }
}