using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DementiaAssistantPOC.Data;
using DementiaAssistantPOC.Models;

namespace DementiaAssistantPOC.Services
{
    public class RelationshipService
    {
        private readonly AppDbContext _context;
        
        public RelationshipService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<RelationshipInfo>> GetRelationshipsWithDetailsAsync(string personId)
        {
            var relationships = await _context.PersonRelationships
                .Where(r => r.PersonId == personId)
                .Include(r => r.RelatedPerson)
                .ToListAsync();
            
            var detailedRelationships = new List<RelationshipInfo>();
            
            foreach (var rel in relationships)
            {
                if (rel.RelatedPerson == null) continue;
                
                var info = new RelationshipInfo
                {
                    Id = rel.Id,
                    PersonId = rel.RelatedPerson.Id,
                    Name = rel.RelatedPerson.Name,
                    Type = GetRelationshipTypeName(rel.Type),
                    TypeId = rel.Type,
                    Notes = rel.Notes,
                    ClosenessLevel = rel.ClosenessLevel,
                    FrequencyOfContact = GetFrequencyName(rel.FrequencyOfContact),
                    IsEmergencyContact = rel.IsEmergencyContact,
                    EmotionalSignificance = rel.EmotionalSignificance
                };
                
                info.SharedEvents = await GetSharedEventsAsync(personId, rel.RelatedPerson.Id);
                info.LastInteraction = await GetLastInteractionAsync(personId);
                
                detailedRelationships.Add(info);
            }
            
            return detailedRelationships
                .OrderByDescending(r => r.ClosenessLevel)
                .ThenBy(r => r.TypeId)
                .ToList();
        }
        
        private async Task<List<SharedEvent>> GetSharedEventsAsync(string personId, string relatedPersonId)
        {
            var events = new List<SharedEvent>();
            
            var relatedPerson = await _context.People
                .Where(p => p.Id == relatedPersonId)
                .FirstOrDefaultAsync();
            
            if (relatedPerson == null) return events;
            
            var firstName = relatedPerson.Name.Split(' ')[0];
            
            var lifeEvents = await _context.PersonLifeEvents
                .Where(e => e.PersonId == personId && 
                           e.Description != null && 
                           e.Description.Contains(firstName))
                .ToListAsync();
            
            foreach (var lifeEvent in lifeEvents)
            {
                events.Add(new SharedEvent
                {
                    Title = lifeEvent.Title,
                    Date = lifeEvent.StartDate,
                    IsPositive = lifeEvent.IsPositiveMemory
                });
            }
            
            return events;
        }
        
        private async Task<DateTime?> GetLastInteractionAsync(string personId)
        {
            var lastConversation = await _context.Conversations
                .Where(c => c.PersonId == personId)
                .OrderByDescending(c => c.StartedAt)
                .FirstOrDefaultAsync();
            
            return lastConversation?.StartedAt;
        }
        
        // ✅ CORREGIDO - Opción 2 (filtrar en la consulta)
        public async Task<List<Person>> GetEmergencyContactsAsync(string personId)
        {
            var emergencyContacts = await _context.PersonRelationships
                .Where(r => r.PersonId == personId && r.IsEmergencyContact)
                .Include(r => r.RelatedPerson)
                .Where(r => r.RelatedPerson != null)
                .Select(r => r.RelatedPerson!)
                .ToListAsync();
            
            return emergencyContacts;
        }
        
        // ✅ CORREGIDO - Opción 2 (filtrar en la consulta)
        public async Task<List<Person>> GetClosestRelationshipsAsync(string personId, int top = 3)
        {
            var closeRelationships = await _context.PersonRelationships
                .Where(r => r.PersonId == personId)
                .OrderByDescending(r => r.ClosenessLevel)
                .Take(top)
                .Include(r => r.RelatedPerson)
                .Where(r => r.RelatedPerson != null)
                .Select(r => r.RelatedPerson!)
                .ToListAsync();
            
            return closeRelationships;
        }
        
        public async Task<Dictionary<string, int>> GetRelationshipTypeStatsAsync(string personId)
        {
            var stats = await _context.PersonRelationships
                .Where(r => r.PersonId == personId)
                .GroupBy(r => r.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => GetRelationshipTypeName(x.Type), x => x.Count);
            
            return stats;
        }
        
        private string GetRelationshipTypeName(int type)
        {
            return type switch
            {
                1 => "Familiar",
                2 => "Amigo/a",
                3 => "Cuidador/a",
                4 => "Vecino/a",
                5 => "Médico/a",
                _ => "Otro"
            };
        }
        
        private string GetFrequencyName(int frequency)
        {
            return frequency switch
            {
                1 => "Diario",
                2 => "Semanal",
                3 => "Mensual",
                4 => "Ocasional",
                _ => "No especificado"
            };
        }
    }
    
    public class RelationshipInfo
    {
        public string Id { get; set; } = string.Empty;
        public string PersonId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int TypeId { get; set; }
        public string? Notes { get; set; }
        public int ClosenessLevel { get; set; }
        public string FrequencyOfContact { get; set; } = string.Empty;
        public bool IsEmergencyContact { get; set; }
        public string? EmotionalSignificance { get; set; }
        public List<SharedEvent> SharedEvents { get; set; } = new();
        public DateTime? LastInteraction { get; set; }
    }
    
    public class SharedEvent
    {
        public string Title { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
        public bool IsPositive { get; set; }
    }
}