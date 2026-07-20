namespace Puentes.Infrastructure.Database.Scripts;

public static class MedicationScheduleScripts
{
    public const string CreateTable = """
    CREATE TABLE IF NOT EXISTS MedicationSchedules
    (
        Id TEXT PRIMARY KEY,
        MedicationId TEXT NOT NULL,
        Turn INTEGER NOT NULL,
        Quantity REAL NOT NULL,
        IsActive INTEGER NOT NULL,
        FOREIGN KEY (MedicationId)
        REFERENCES Medications(Id)
    );
    """;
    public const string SelectByMedicationId = """
    SELECT
    Id,
    MedicationId,
    Turn,
    Quantity,
    IsActive
    FROM MedicationSchedules
    WHERE MedicationId = @MedicationId
    ORDER BY Turn;
    """;
    public const string SelectById = """
    SELECT
        Id,
        MedicationId,
        Turn,
        Quantity,
        IsActive
    FROM MedicationSchedules
    WHERE Id = @Id;
    """;

    public const string DeleteByMedicationId = """
    DELETE FROM MedicationSchedules
    WHERE MedicationId = @MedicationId;
    """;

    public const string Insert = """
    INSERT INTO MedicationSchedules
    (
        Id,
        MedicationId,
        Turn,
        Quantity,
        IsActive
    )
    VALUES
    (
        @Id,
        @MedicationId,
        @Turn,
        @Quantity,
        @IsActive
    );
    """;

}