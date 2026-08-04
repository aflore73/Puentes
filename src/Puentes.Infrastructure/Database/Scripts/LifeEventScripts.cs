namespace Puentes.Infrastructure.Database.Scripts;

public static class LifeEventScripts
{
    public const string RemoveTypeColumn = """
        PRAGMA foreign_keys = OFF;

        BEGIN TRANSACTION;

        CREATE TABLE PersonLifeEvents_New
        (
            Id TEXT PRIMARY KEY,
            PersonId TEXT NOT NULL,
            StartDate TEXT NULL,
            EndDate TEXT NULL,
            DatePrecision INTEGER NOT NULL,
            Title TEXT NOT NULL,
            Description TEXT NULL,
            Place TEXT NULL,
            IsPositiveMemory INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE,
            CHECK (length(trim(Title)) > 0),
            CHECK (IsPositiveMemory IN (0, 1)),
            CHECK (EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate)
        );

        INSERT INTO PersonLifeEvents_New
        (
            Id, PersonId, StartDate, EndDate, DatePrecision,
            Title, Description, Place, IsPositiveMemory
        )
        SELECT
            Id, PersonId, StartDate, EndDate, DatePrecision,
            Title, Description, Place, IsPositiveMemory
        FROM PersonLifeEvents;

        DROP TABLE PersonLifeEvents;
        ALTER TABLE PersonLifeEvents_New RENAME TO PersonLifeEvents;

        CREATE INDEX IX_PersonLifeEvents_PersonId_StartDate
        ON PersonLifeEvents (PersonId, StartDate);

        COMMIT;

        PRAGMA foreign_keys = ON;
        """;

    public const string CreateTables = """
        CREATE TABLE IF NOT EXISTS PersonLifeEvents
        (
            Id TEXT PRIMARY KEY,
            PersonId TEXT NOT NULL,
            StartDate TEXT NULL,
            EndDate TEXT NULL,
            DatePrecision INTEGER NOT NULL,
            Title TEXT NOT NULL,
            Description TEXT NULL,
            Place TEXT NULL,
            IsPositiveMemory INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE,
            CHECK (length(trim(Title)) > 0),
            CHECK (IsPositiveMemory IN (0, 1)),
            CHECK (EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate)
        );

        CREATE INDEX IF NOT EXISTS IX_PersonLifeEvents_PersonId_StartDate
        ON PersonLifeEvents (PersonId, StartDate);

        CREATE TABLE IF NOT EXISTS LifeEventParticipants
        (
            LifeEventId TEXT NOT NULL,
            PersonId TEXT NOT NULL,
            Role TEXT NULL,
            PRIMARY KEY (LifeEventId, PersonId),
            FOREIGN KEY (LifeEventId)
                REFERENCES PersonLifeEvents(Id) ON DELETE CASCADE,
            FOREIGN KEY (PersonId)
                REFERENCES People(Id) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS IX_LifeEventParticipants_PersonId
        ON LifeEventParticipants (PersonId);
        """;

    public const string Insert = """
        INSERT INTO PersonLifeEvents
        (
            Id, PersonId, StartDate, EndDate, DatePrecision,
            Title, Description, Place, IsPositiveMemory
        )
        VALUES
        (
            @Id, @PersonId, @StartDate, @EndDate, @DatePrecision,
            @Title, @Description, @Place, @IsPositiveMemory
        );
        """;

    public const string InsertParticipant = """
        INSERT INTO LifeEventParticipants (LifeEventId, PersonId, Role)
        VALUES (@LifeEventId, @PersonId, @Role);
        """;

    public const string SelectByPersonId = """
        SELECT
            Id, PersonId, StartDate, EndDate, DatePrecision,
            Title, Description, Place, IsPositiveMemory
        FROM PersonLifeEvents
        WHERE PersonId = @PersonId
        ORDER BY StartDate, Title;
        """;

    public const string SelectParticipantsByLifeEventId = """
        SELECT LifeEventId, PersonId, Role
        FROM LifeEventParticipants
        WHERE LifeEventId = @LifeEventId
        ORDER BY PersonId;
        """;
}
