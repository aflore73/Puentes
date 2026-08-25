using Microsoft.EntityFrameworkCore;
using DementiaAssistantPOC.Models;

namespace DementiaAssistantPOC.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) { }
        
        public DbSet<Person> People { get; set; }
        public DbSet<PersonRelationship> PersonRelationships { get; set; }
        public DbSet<PersonLifeEvent> PersonLifeEvents { get; set; }
        public DbSet<PersonRoutine> PersonRoutines { get; set; }
        public DbSet<PersonPreference> PersonPreferences { get; set; }
        public DbSet<PersonAgendaItem> PersonAgendaItems { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationMessage> ConversationMessages { get; set; }
        public DbSet<EmotionalMemory> EmotionalMemories { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<PersonRelationship>()
                .HasOne(pr => pr.Person)
                .WithMany(p => p.Relationships)
                .HasForeignKey(pr => pr.PersonId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<PersonRelationship>()
                .HasOne(pr => pr.RelatedPerson)
                .WithMany()
                .HasForeignKey(pr => pr.RelatedPersonId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Person>()
                .HasIndex(p => p.Name);
            
            modelBuilder.Entity<PersonAgendaItem>()
                .HasIndex(p => new { p.PersonId, p.ScheduledAt });
            
            modelBuilder.Entity<Conversation>()
                .HasIndex(c => new { c.PersonId, c.StartedAt });
        }
    }
}