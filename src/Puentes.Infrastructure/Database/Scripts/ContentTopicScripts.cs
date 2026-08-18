namespace Puentes.Infrastructure.Database.Scripts;

public static class ContentTopicScripts
{
    public const string CreateTables = """
        CREATE TABLE IF NOT EXISTS ContentTopics
        (
            Id TEXT PRIMARY KEY,
            Code TEXT NOT NULL UNIQUE COLLATE NOCASE,
            Name TEXT NOT NULL,
            GroupName TEXT NOT NULL,
            CHECK (length(trim(Code)) > 0),
            CHECK (length(trim(Name)) > 0),
            CHECK (GroupName IN ('Memory', 'Reading', 'Interest', 'Agenda'))
        );

        CREATE TABLE IF NOT EXISTS PersonLifeEventTopics
        (
            LifeEventId TEXT NOT NULL,
            TopicId TEXT NOT NULL,
            PRIMARY KEY (LifeEventId, TopicId),
            FOREIGN KEY (LifeEventId) REFERENCES PersonLifeEvents(Id) ON DELETE CASCADE,
            FOREIGN KEY (TopicId) REFERENCES ContentTopics(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS PersonSupportContentTopics
        (
            SupportContentId TEXT NOT NULL,
            TopicId TEXT NOT NULL,
            PRIMARY KEY (SupportContentId, TopicId),
            FOREIGN KEY (SupportContentId) REFERENCES PersonSupportContents(Id) ON DELETE CASCADE,
            FOREIGN KEY (TopicId) REFERENCES ContentTopics(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS PersonPreferenceTopics
        (
            PreferenceId TEXT NOT NULL,
            TopicId TEXT NOT NULL,
            PRIMARY KEY (PreferenceId, TopicId),
            FOREIGN KEY (PreferenceId) REFERENCES PersonPreferences(Id) ON DELETE CASCADE,
            FOREIGN KEY (TopicId) REFERENCES ContentTopics(Id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS PersonAgendaItemTopics
        (
            AgendaItemId TEXT NOT NULL,
            TopicId TEXT NOT NULL,
            PRIMARY KEY (AgendaItemId, TopicId),
            FOREIGN KEY (AgendaItemId) REFERENCES PersonAgendaItems(Id) ON DELETE CASCADE,
            FOREIGN KEY (TopicId) REFERENCES ContentTopics(Id) ON DELETE CASCADE
        );
        """;

    public const string SeedTopics = """
        INSERT OR IGNORE INTO ContentTopics (Id, Code, Name, GroupName) VALUES
          ('10000000-0000-0000-0000-000000000001', 'memory.travel', 'Viajes', 'Memory'),
          ('10000000-0000-0000-0000-000000000002', 'memory.childhood', 'Infancia', 'Memory'),
          ('10000000-0000-0000-0000-000000000003', 'memory.family', 'Familia', 'Memory'),
          ('10000000-0000-0000-0000-000000000004', 'memory.life-story', 'Historias de vida', 'Memory'),
          ('20000000-0000-0000-0000-000000000001', 'reading.religious', 'Lectura religiosa', 'Reading'),
          ('20000000-0000-0000-0000-000000000002', 'reading.poetry', 'Poesia', 'Reading'),
          ('20000000-0000-0000-0000-000000000003', 'reading.story', 'Historia o relato', 'Reading'),
          ('30000000-0000-0000-0000-000000000001', 'interest.music', 'Musica', 'Interest'),
          ('30000000-0000-0000-0000-000000000002', 'interest.plants', 'Plantas', 'Interest'),
          ('40000000-0000-0000-0000-000000000001', 'agenda.medical-appointment', 'Consulta medica', 'Agenda'),
          ('40000000-0000-0000-0000-000000000002', 'agenda.personal', 'Actividad personal', 'Agenda'),
          ('40000000-0000-0000-0000-000000000003', 'agenda.family', 'Actividad familiar', 'Agenda'),
          ('40000000-0000-0000-0000-000000000004', 'agenda.errand', 'Tramite o mandado', 'Agenda');
        """;

    public const string SelectCodes = """
        SELECT t.Code
        FROM ContentTopics t
        INNER JOIN {0} r ON r.TopicId = t.Id
        WHERE r.{1} = @ItemId
        ORDER BY t.Code;
        """;
}
