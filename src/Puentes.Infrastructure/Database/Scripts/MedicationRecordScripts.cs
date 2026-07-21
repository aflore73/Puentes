public static class MedicationRecordScripts
{
    public const string CreateTable = """
        CREATE TABLE IF NOT EXISTS MedicationRecords
        (
            Id TEXT PRIMARY KEY,
            PatientId TEXT NOT NULL,
            Turn INTEGER NOT NULL,
            RecordedAt TEXT NOT NULL,
            Confirmed INTEGER NOT NULL,
            Notes TEXT NULL
        );
        """;
    public const string CreateIndex = """
        CREATE UNIQUE INDEX IF NOT EXISTS IX_MedicationRecords_Patient_Turn_Day
        ON MedicationRecords
        (
            PatientId,
            Turn,
            date(RecordedAt)
        );
        """;
    public const string Insert = """
        INSERT INTO MedicationRecords
        (
            Id,
            PatientId,
            Turn,
            RecordedAt,
            Confirmed,
            Notes
        )
        VALUES
        (
            @Id,
            @PatientId,
            @Turn,
            @RecordedAt,
            @Confirmed,
            @Notes
        );
        """;

    public const string SelectToday = """
        SELECT *
        FROM MedicationRecords
        WHERE PatientId = @PatientId
          AND date(RecordedAt) = date('now','localtime')
        ORDER BY RecordedAt;
        """;

    public const string SelectTodayByTurn = """
        SELECT *
        FROM MedicationRecords
        WHERE PatientId = @PatientId
          AND Turn = @Turn
          AND date(RecordedAt) = date('now','localtime')
        LIMIT 1;
        """;
}