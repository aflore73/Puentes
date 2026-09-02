namespace Puentes.Infrastructure.Database.Scripts;

public static class PersonAliasScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS PersonAliases
        (
            Id TEXT PRIMARY KEY,
            PersonId TEXT NOT NULL,
            Alias TEXT NOT NULL,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE,
            CHECK (length(trim(Alias)) > 0),
            UNIQUE (PersonId, Alias)
        );

        CREATE INDEX IF NOT EXISTS IX_PersonAliases_Alias
        ON PersonAliases (Alias);
        """;

    public const string Insert = """
        INSERT INTO PersonAliases (Id, PersonId, Alias)
        VALUES (@Id, @PersonId, @Alias);
        """;

    public const string SelectAll = """
        SELECT Id, PersonId, Alias
        FROM PersonAliases
        ORDER BY Alias;
        """;

    public const string SelectByPersonId = """
        SELECT Id, PersonId, Alias
        FROM PersonAliases
        WHERE PersonId = @PersonId
        ORDER BY Alias;
        """;
}
