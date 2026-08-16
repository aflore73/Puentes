namespace Puentes.Infrastructure.Database.Scripts;

public static class PersonBelongingScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS PersonBelongings
        (
            Id TEXT PRIMARY KEY,
            PersonId TEXT NOT NULL,
            Name TEXT NOT NULL,
            Notes TEXT NOT NULL,
            Tags TEXT NULL,
            IsActive INTEGER NOT NULL DEFAULT 1,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE,
            CHECK (length(trim(Name)) > 0),
            CHECK (length(trim(Notes)) > 0),
            CHECK (IsActive IN (0, 1))
        );

        CREATE INDEX IF NOT EXISTS IX_PersonBelongings_PersonId
        ON PersonBelongings (PersonId);
        """;

    public const string SelectActiveByPersonId = """
        SELECT Id, PersonId, Name, Notes, Tags, IsActive
        FROM PersonBelongings
        WHERE PersonId = @PersonId
          AND IsActive = 1
        ORDER BY Name;
        """;

    public const string Insert = """
        INSERT INTO PersonBelongings
            (Id, PersonId, Name, Notes, Tags, IsActive)
        VALUES
            (@Id, @PersonId, @Name, @Notes, @Tags, @IsActive);
        """;

    public const string Update = """
        UPDATE PersonBelongings
        SET Name = @Name, Notes = @Notes, Tags = @Tags, IsActive = @IsActive
        WHERE Id = @Id AND PersonId = @PersonId;
        """;
}
