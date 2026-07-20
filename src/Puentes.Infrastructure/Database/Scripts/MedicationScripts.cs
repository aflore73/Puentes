namespace Puentes.Infrastructure.Database.Scripts;

public static class MedicationScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS Medications
        (
            Id TEXT PRIMARY KEY,
            Name TEXT NOT NULL,
            Dose TEXT NOT NULL,
            Form INTEGER NOT NULL,
            Shape INTEGER NOT NULL,
            Color TEXT NULL,
            Instructions TEXT NULL,
            IsActive INTEGER NOT NULL
        );
        """;

    public const string Insert = """
        INSERT INTO Medications
        (
            Id,
            Name,
            Dose,
            Form,
            Shape,
            Color,
            Instructions,
            IsActive
        )
        VALUES
        (
            @Id,
            @Name,
            @Dose,
            @Form,
            @Shape,
            @Color,
            @Instructions,
            @IsActive
        );
        """;

    public const string Update = """
        UPDATE Medications
        SET
            Name = @Name,
            Dose = @Dose,
            Form = @Form,
            Shape = @Shape,
            Color = @Color,
            Instructions = @Instructions,
            IsActive = @IsActive
        WHERE Id = @Id;
        """;

    public const string Delete = """
        DELETE
        FROM Medications
        WHERE Id = @Id;
        """;

    public const string SelectAll = """
        SELECT
            Id,
            Name,
            Dose,
            Instructions,
            Form,
            Shape,
            Color,
            IsActive
        FROM Medications
        ORDER BY Name;
        """;

    public const string SelectById = """
        SELECT
            Id,
            Name,
            Dose,
            Instructions,
            Form,
            Shape,
            Color,
            IsActive
        FROM Medications
        WHERE Id = @Id;
        """;
}