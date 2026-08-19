namespace Puentes.Infrastructure.Database.Scripts;

public static class PersonScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS People
        (
            Id TEXT PRIMARY KEY,
            Name TEXT NOT NULL,
            BirthDate TEXT NULL,
            City TEXT NOT NULL,
            Province TEXT NOT NULL,
            Country TEXT NOT NULL,
            Notes TEXT NULL
        );
        """;

    public const string Insert = """
        INSERT INTO People (Id, Name, BirthDate, City, Province, Country, Notes)
        VALUES (@Id, @Name, @BirthDate, @City, @Province, @Country, @Notes);
        """;

    public const string AddBirthDateColumn = """
        ALTER TABLE People ADD COLUMN BirthDate TEXT NULL;
        """;

    public const string AddCityColumn = """
        ALTER TABLE People
        ADD COLUMN City TEXT NOT NULL DEFAULT '';
        """;

    public const string AddProvinceColumn = """
        ALTER TABLE People
        ADD COLUMN Province TEXT NOT NULL DEFAULT '';
        """;

    public const string AddCountryColumn = """
        ALTER TABLE People
        ADD COLUMN Country TEXT NOT NULL DEFAULT 'Argentina';
        """;

    public const string AddNotesColumn = """
        ALTER TABLE People ADD COLUMN Notes TEXT NULL;
        """;

    public const string SelectAll = """
        SELECT Id, Name, BirthDate, City, Province, Country, Notes
        FROM People
        ORDER BY Name;
        """;

    public const string SelectById = """
        SELECT Id, Name, BirthDate, City, Province, Country, Notes
        FROM People
        WHERE Id = @Id;
        """;

    public const string Update = """
        UPDATE People
        SET Name = @Name,
            BirthDate = @BirthDate,
            City = @City,
            Province = @Province,
            Country = @Country,
            Notes = @Notes
        WHERE Id = @Id;
        """;
}
