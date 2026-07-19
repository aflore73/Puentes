namespace Puentes.Infrastructure.Database.Scripts;

public static class MedicationTurnScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS MedicationTurns
        (
            Id TEXT PRIMARY KEY,
            Type INTEGER NOT NULL,
            Name TEXT NOT NULL,
            DisplayOrder INTEGER NOT NULL,
            ReferenceTime TEXT NULL,
            IsActive INTEGER NOT NULL
        );
        """;

    public const string Insert = """
        INSERT INTO MedicationTurns
        (
            Id,
            Type,
            Name,
            DisplayOrder,
            ReferenceTime,
            IsActive
        )
        VALUES
        (
            @Id,
            @Type,
            @Name,
            @DisplayOrder,
            @ReferenceTime,
            @IsActive
        );
        """;

    public const string Update = """
        UPDATE MedicationTurns
        SET
            Name = @Name,
            DisplayOrder = @DisplayOrder,
            Type = @Type,
            ReferenceTime = @ReferenceTime,
            IsActive = @IsActive
        WHERE Id = @Id;
        """;

    public const string Delete = """
        DELETE FROM MedicationTurns
        WHERE Id = @Id;
        """;

    public const string SelectAll = """
        SELECT
            Id,
            Type,
            Name,
            DisplayOrder,
            ReferenceTime,
            IsActive
        FROM MedicationTurns
        ORDER BY DisplayOrder;
        """;

    public const string SelectById = """
        SELECT
            Id,
            Type,
            Name,
            DisplayOrder,
            ReferenceTime,
            IsActive
        FROM MedicationTurns
        WHERE Id = @Id;
        """;
}