namespace Puentes.Infrastructure.Database.Scripts;

public static class PersonAgendaItemScripts
{
    public const string CreateTables = """
        CREATE TABLE IF NOT EXISTS PersonAgendaItems
        (
            Id TEXT PRIMARY KEY,
            PersonId TEXT NOT NULL,
            ScheduledAt TEXT NOT NULL,
            EndAt TEXT NULL,
            Title TEXT NOT NULL,
            Description TEXT NULL,
            Place TEXT NULL,
            Status INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE,
            CHECK (length(trim(Title)) > 0),
            CHECK (Status IN (0, 1, 2)),
            CHECK (EndAt IS NULL OR EndAt >= ScheduledAt)
        );

        CREATE INDEX IF NOT EXISTS IX_PersonAgendaItems_PersonId_ScheduledAt
        ON PersonAgendaItems (PersonId, ScheduledAt);

        CREATE TABLE IF NOT EXISTS AgendaItemParticipants
        (
            AgendaItemId TEXT NOT NULL,
            PersonId TEXT NOT NULL,
            Role TEXT NULL,
            PRIMARY KEY (AgendaItemId, PersonId),
            FOREIGN KEY (AgendaItemId)
                REFERENCES PersonAgendaItems(Id) ON DELETE CASCADE,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE
        );
        """;

    public const string Insert = """
        INSERT INTO PersonAgendaItems
            (Id, PersonId, ScheduledAt, EndAt, Title, Description, Place, Status)
        VALUES
            (@Id, @PersonId, @ScheduledAt, @EndAt, @Title, @Description,
             @Place, @Status);
        """;

    public const string SelectByPerson = """
        SELECT Id, PersonId, ScheduledAt, EndAt, Title, Description, Place, Status
        FROM PersonAgendaItems
        WHERE PersonId = @PersonId
          AND (@IncludePast = 1 OR ScheduledAt >= @Now)
        ORDER BY ScheduledAt, Title;
        """;
}
