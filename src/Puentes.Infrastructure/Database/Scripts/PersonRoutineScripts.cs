namespace Puentes.Infrastructure.Database.Scripts;

public static class PersonRoutineScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS PersonRoutines
        (
            Id TEXT PRIMARY KEY,
            PersonId TEXT NOT NULL,
            Title TEXT NOT NULL,
            Notes TEXT NOT NULL,
            DaysOfWeek TEXT NULL,
            StartTime TEXT NULL,
            EndTime TEXT NULL,
            IsActive INTEGER NOT NULL DEFAULT 1,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE,
            CHECK (length(trim(Title)) > 0),
            CHECK (length(trim(Notes)) > 0),
            CHECK (IsActive IN (0, 1))
        );

        CREATE INDEX IF NOT EXISTS IX_PersonRoutines_PersonId
        ON PersonRoutines (PersonId);
        """;

    public const string AddDaysOfWeekColumn =
        "ALTER TABLE PersonRoutines ADD COLUMN DaysOfWeek TEXT NULL;";

    public const string AddStartTimeColumn =
        "ALTER TABLE PersonRoutines ADD COLUMN StartTime TEXT NULL;";

    public const string AddEndTimeColumn =
        "ALTER TABLE PersonRoutines ADD COLUMN EndTime TEXT NULL;";

    public const string Insert = """
        INSERT INTO PersonRoutines
            (Id, PersonId, Title, Notes, DaysOfWeek, StartTime, EndTime, IsActive)
        VALUES
            (@Id, @PersonId, @Title, @Notes, @DaysOfWeek, @StartTime, @EndTime, @IsActive);
        """;

    public const string SelectActiveByPersonId = """
        SELECT Id, PersonId, Title, Notes, DaysOfWeek, StartTime, EndTime, IsActive
        FROM PersonRoutines
        WHERE PersonId = @PersonId
          AND IsActive = 1
        ORDER BY Title;
        """;

    public const string Update = """
        UPDATE PersonRoutines
        SET Title = @Title,
            Notes = @Notes,
            DaysOfWeek = @DaysOfWeek,
            StartTime = @StartTime,
            EndTime = @EndTime,
            IsActive = @IsActive
        WHERE Id = @Id AND PersonId = @PersonId;
        """;
}
