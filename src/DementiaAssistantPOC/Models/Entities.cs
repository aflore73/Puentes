using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DementiaAssistantPOC.Models
{
    public class Person
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Country { get; set; } = "Argentina";
        public string? Notes { get; set; }
        
        public virtual ICollection<PersonRelationship> Relationships { get; set; } = new List<PersonRelationship>();
        public virtual ICollection<PersonLifeEvent> LifeEvents { get; set; } = new List<PersonLifeEvent>();
        public virtual ICollection<PersonRoutine> Routines { get; set; } = new List<PersonRoutine>();
        public virtual ICollection<PersonPreference> Preferences { get; set; } = new List<PersonPreference>();
        public virtual ICollection<PersonAgendaItem> AgendaItems { get; set; } = new List<PersonAgendaItem>();
    }
    
    public class PersonRelationship
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PersonId { get; set; } = string.Empty;
        public string RelatedPersonId { get; set; } = string.Empty;
        public int Type { get; set; }
        public string? Notes { get; set; }
        public int ClosenessLevel { get; set; } = 5; // 1-10, qué tan cercana es la relación
        public int FrequencyOfContact { get; set; } = 3; // 1=diario, 2=semanal, 3=mensual
        public bool IsEmergencyContact { get; set; } = false;
        public string? RelationshipStartDate { get; set; } // Cuándo comenzó la relación
        public string? PreferredTopics { get; set; } // Temas de conversación preferidos
        public string? EmotionalSignificance { get; set; } // Importancia emocional
        [ForeignKey("PersonId")]
        public virtual Person? Person { get; set; }
        
        [ForeignKey("RelatedPersonId")]
        public virtual Person? RelatedPerson { get; set; }
    }
    
    public class PersonLifeEvent
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PersonId { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DatePrecision { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Place { get; set; }
        public bool IsPositiveMemory { get; set; }
        
        [ForeignKey("PersonId")]
        public virtual Person? Person { get; set; }
    }
    
    public class PersonRoutine
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PersonId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public string? DaysOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        
        [ForeignKey("PersonId")]
        public virtual Person? Person { get; set; }
    }
    
    public class PersonPreference
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PersonId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? Tags { get; set; }
        public bool IsActive { get; set; } = true;
        
        [ForeignKey("PersonId")]
        public virtual Person? Person { get; set; }
    }
    
    public class PersonAgendaItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PersonId { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public DateTime? EndAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Place { get; set; }
        public int Status { get; set; } = 0;
        
        [ForeignKey("PersonId")]
        public virtual Person? Person { get; set; }
    }
    
    public class Conversation
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PersonId { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public DateTime? EndedAt { get; set; }
        public string? ContextType { get; set; }
        public string? EmotionalStateStart { get; set; }
        public string? EmotionalStateEnd { get; set; }
        public string? Summary { get; set; }
        public int? Effectiveness { get; set; }
        
        [ForeignKey("PersonId")]
        public virtual Person? Person { get; set; }
        public virtual ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
    }
    
    public class ConversationMessage
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ConversationId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Speaker { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Intent { get; set; }
        
        [ForeignKey("ConversationId")]
        public virtual Conversation? Conversation { get; set; }
    }
    
    public class EmotionalMemory
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PersonId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? TriggerContext { get; set; }
        public string? EmotionType { get; set; }
        public int Intensity { get; set; }
        public string? ResponseStrategy { get; set; }
        public bool IsResolved { get; set; } = false;
        
        [ForeignKey("PersonId")]
        public virtual Person? Person { get; set; }
    }
}