namespace Puentes.Infrastructure.Database.Scripts;

public static class PersonSupportContentScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS PersonSupportContents
        (
            Id TEXT PRIMARY KEY,
            PersonId TEXT NOT NULL,
            Title TEXT NOT NULL,
            Content TEXT NOT NULL,
            Attribution TEXT NULL,
            Reference TEXT NULL,
            Tags TEXT NULL,
            IsActive INTEGER NOT NULL DEFAULT 1,
            FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE,
            CHECK (length(trim(Title)) > 0),
            CHECK (length(trim(Content)) > 0),
            CHECK (IsActive IN (0, 1))
        );

        CREATE INDEX IF NOT EXISTS IX_PersonSupportContents_PersonId
        ON PersonSupportContents (PersonId);
        """;

    public const string Insert = """
        INSERT INTO PersonSupportContents
        (
            Id, PersonId, Title, Content,
            Attribution, Reference, Tags, IsActive
        )
        VALUES
        (
            @Id, @PersonId, @Title, @Content,
            @Attribution, @Reference, @Tags, @IsActive
        );
        """;

    public const string SelectActiveByPersonId = """
        SELECT
            Id, PersonId, Title, Content,
            Attribution, Reference, Tags, IsActive
        FROM PersonSupportContents
        WHERE PersonId = @PersonId
          AND IsActive = 1
        ORDER BY Title;
        """;

    public const string Update = """
        UPDATE PersonSupportContents
        SET Title = @Title,
            Content = @Content,
            Attribution = @Attribution,
            Reference = @Reference,
            Tags = @Tags,
            IsActive = @IsActive
        WHERE Id = @Id AND PersonId = @PersonId;
        """;
}
