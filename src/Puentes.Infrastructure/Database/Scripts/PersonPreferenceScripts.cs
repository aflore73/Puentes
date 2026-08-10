namespace Puentes.Infrastructure.Database.Scripts;

public static class PersonPreferenceScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS PersonPreferences
        (
            Id TEXT PRIMARY KEY,
            PersonId TEXT NOT NULL,
            Title TEXT NOT NULL,
            Notes TEXT NOT NULL,
            Tags TEXT NULL,
            IsActive INTEGER NOT NULL DEFAULT 1,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE,
            CHECK (length(trim(Title)) > 0),
            CHECK (length(trim(Notes)) > 0),
            CHECK (IsActive IN (0, 1))
        );

        CREATE INDEX IF NOT EXISTS IX_PersonPreferences_PersonId
        ON PersonPreferences (PersonId);
        """;

    public const string Insert = """
        INSERT INTO PersonPreferences
            (Id, PersonId, Title, Notes, Tags, IsActive)
        VALUES
            (@Id, @PersonId, @Title, @Notes, @Tags, @IsActive);
        """;

    public const string SelectActiveByPersonId = """
        SELECT Id, PersonId, Title, Notes, Tags, IsActive
        FROM PersonPreferences
        WHERE PersonId = @PersonId
          AND IsActive = 1
        ORDER BY Title;
        """;
}
