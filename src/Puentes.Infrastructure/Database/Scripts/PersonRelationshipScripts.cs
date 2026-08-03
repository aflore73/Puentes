namespace Puentes.Infrastructure.Database.Scripts;

public static class PersonRelationshipScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS PersonRelationships
        (
            Id TEXT PRIMARY KEY,
            PersonId TEXT NOT NULL,
            RelatedPersonId TEXT NOT NULL,
            Type INTEGER NOT NULL,
            Notes TEXT NULL,
            FOREIGN KEY (PersonId) REFERENCES People(Id),
            FOREIGN KEY (RelatedPersonId) REFERENCES People(Id),
            CHECK (PersonId <> RelatedPersonId),
            UNIQUE (PersonId, RelatedPersonId, Type)
        );
        """;

    public const string Insert = """
        INSERT INTO PersonRelationships
            (Id, PersonId, RelatedPersonId, Type, Notes)
        VALUES
            (@Id, @PersonId, @RelatedPersonId, @Type, @Notes);
        """;

    public const string SelectByParticipantId = """
        SELECT Id, PersonId, RelatedPersonId, Type, Notes
        FROM PersonRelationships
        WHERE PersonId = @PersonId
           OR RelatedPersonId = @PersonId
        ORDER BY Type, RelatedPersonId;
        """;
}
