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
            IsActive INTEGER NOT NULL DEFAULT 1,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE,
            CHECK (length(trim(Title)) > 0),
            CHECK (length(trim(Notes)) > 0),
            CHECK (IsActive IN (0, 1))
        );

        CREATE INDEX IF NOT EXISTS IX_PersonRoutines_PersonId
        ON PersonRoutines (PersonId);
        """;

    public const string Insert = """
        INSERT INTO PersonRoutines (Id, PersonId, Title, Notes, IsActive)
        VALUES (@Id, @PersonId, @Title, @Notes, @IsActive);
        """;

    public const string SelectActiveByPersonId = """
        SELECT Id, PersonId, Title, Notes, IsActive
        FROM PersonRoutines
        WHERE PersonId = @PersonId
          AND IsActive = 1
        ORDER BY Title;
        """;
}
