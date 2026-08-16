namespace Puentes.Infrastructure.Database.Scripts;

public static class PersonTrustedContactScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS PersonTrustedContacts
        (
            Id TEXT PRIMARY KEY,
            PersonId TEXT NOT NULL,
            ContactPersonId TEXT NOT NULL,
            Priority INTEGER NOT NULL DEFAULT 1,
            Notes TEXT NULL,
            IsActive INTEGER NOT NULL DEFAULT 1,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE,
            FOREIGN KEY (ContactPersonId) REFERENCES People(Id),
            CHECK (PersonId <> ContactPersonId),
            CHECK (Priority > 0),
            CHECK (IsActive IN (0, 1)),
            UNIQUE (PersonId, ContactPersonId)
        );
        """;
    public const string Insert = """
        INSERT INTO PersonTrustedContacts
            (Id, PersonId, ContactPersonId, Priority, Notes, IsActive)
        VALUES (@Id, @PersonId, @ContactPersonId, @Priority, @Notes, @IsActive);
        """;
    public const string SelectActive = """
        SELECT Id, PersonId, ContactPersonId, Priority, Notes, IsActive
        FROM PersonTrustedContacts
        WHERE PersonId = @PersonId AND IsActive = 1
        ORDER BY Priority, Id;
        """;
    public const string Update = """
        UPDATE PersonTrustedContacts
        SET ContactPersonId = @ContactPersonId,
            Priority = @Priority, Notes = @Notes, IsActive = @IsActive
        WHERE Id = @Id AND PersonId = @PersonId;
        """;
}
