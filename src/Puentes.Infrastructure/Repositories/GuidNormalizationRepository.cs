using Dapper;
using Puentes.Infrastructure.Database;

namespace Puentes.Infrastructure.Repositories;

public sealed class GuidNormalizationRepository
{
    private static readonly IReadOnlyDictionary<string, string[]> Columns =
        new Dictionary<string, string[]>
        {
            ["People"] = ["Id"],
            ["PersonRelationships"] = ["Id", "PersonId", "RelatedPersonId"],
            ["PersonLifeEvents"] = ["Id", "PersonId"],
            ["LifeEventParticipants"] = ["LifeEventId", "PersonId"],
            ["PersonRoutines"] = ["Id", "PersonId"],
            ["PersonPreferences"] = ["Id", "PersonId"],
            ["PersonSupportContents"] = ["Id", "PersonId"],
            ["PersonBelongings"] = ["Id", "PersonId"],
            ["PersonTrustedContacts"] = ["Id", "PersonId", "ContactPersonId"],
            ["ContentTopics"] = ["Id"],
            ["PersonLifeEventTopics"] = ["LifeEventId", "TopicId"],
            ["PersonSupportContentTopics"] = ["SupportContentId", "TopicId"],
            ["PersonPreferenceTopics"] = ["PreferenceId", "TopicId"],
            ["Medications"] = ["Id"],
            ["MedicationSchedules"] = ["Id", "MedicationId"],
            ["MedicationTurns"] = ["Id"],
            ["MedicationRecords"] = ["Id", "PatientId"],
            ["Events"] = ["Id", "PersonId"]
        };

    private readonly AccessDb _accessDb;

    public GuidNormalizationRepository(AccessDb accessDb) =>
        _accessDb = accessDb;

    public async Task NormalizeAsync()
    {
        using var connection = _accessDb.OpenConnection();
        var existingTables = (await connection.QueryAsync<string>(
            "SELECT name FROM sqlite_master WHERE type = 'table';"))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var columnsByTable = new Dictionary<string, HashSet<string>>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var table in existingTables.Where(table =>
            !table.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase)))
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var tableColumns = await connection.QueryAsync<TableColumn>(
                $"SELECT name Name, pk IsPrimaryKey FROM pragma_table_info('{table}');");
            columns.UnionWith(tableColumns
                .Where(column => column.IsPrimaryKey > 0)
                .Select(column => column.Name));
            var foreignKeys = await connection.QueryAsync<ForeignKeyColumn>(
                $"SELECT \"from\" ColumnName FROM pragma_foreign_key_list('{table}');");
            columns.UnionWith(foreignKeys.Select(key => key.ColumnName));
            if (Columns.TryGetValue(table, out var knownColumns))
            {
                columns.UnionWith(knownColumns);
            }
            columnsByTable[table] = columns;
        }

        await connection.ExecuteAsync("PRAGMA foreign_keys = OFF;");
        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var (table, columns) in columnsByTable)
            {
                var existingColumns = (await connection.QueryAsync<string>(
                    $"SELECT name FROM pragma_table_info('{table}');",
                    transaction: transaction)).ToHashSet(
                        StringComparer.OrdinalIgnoreCase);
                foreach (var column in columns.Where(existingColumns.Contains))
                {
                    var expression = CanonicalExpression(column);
                    if (column == "Id")
                    {
                        var duplicate = await connection.ExecuteScalarAsync<int>(
                            $"SELECT count(*) FROM (SELECT {expression} value " +
                            $"FROM {table} GROUP BY value HAVING count(*) > 1);",
                            transaction: transaction);
                        if (duplicate > 0)
                        {
                            throw new InvalidOperationException(
                                $"No se pueden normalizar GUID duplicados en {table}.{column}.");
                        }
                    }

                    await connection.ExecuteAsync(
                        $"UPDATE {table} SET {column} = {expression} " +
                        $"WHERE {column} IS NOT NULL AND length(replace({column}, '-', '')) = 32;",
                        transaction: transaction);
                }
            }

            var violations = (await connection.QueryAsync<ForeignKeyViolation>(
                "SELECT \"table\" TableName, rowid RowId, parent ParentTable, " +
                "fkid ForeignKeyId FROM pragma_foreign_key_check;",
                transaction: transaction)).ToArray();
            if (violations.Length > 0)
            {
                throw new InvalidOperationException(
                    "La normalizacion de GUID detecto relaciones invalidas: " +
                    string.Join("; ", violations.Select(item =>
                        $"{item.TableName}[rowid={item.RowId}] -> " +
                        $"{item.ParentTable} (FK {item.ForeignKeyId})")));
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            await connection.ExecuteAsync("PRAGMA foreign_keys = ON;");
        }
    }

    private static string CanonicalExpression(string column)
    {
        var compact = $"upper(replace({column}, '-', ''))";
        return $"substr({compact}, 1, 8) || '-' || " +
            $"substr({compact}, 9, 4) || '-' || " +
            $"substr({compact}, 13, 4) || '-' || " +
            $"substr({compact}, 17, 4) || '-' || " +
            $"substr({compact}, 21, 12)";
    }

    private sealed class ForeignKeyViolation
    {
        public string TableName { get; set; } = string.Empty;
        public long RowId { get; set; }
        public string ParentTable { get; set; } = string.Empty;
        public int ForeignKeyId { get; set; }
    }

    private sealed class TableColumn
    {
        public string Name { get; set; } = string.Empty;
        public int IsPrimaryKey { get; set; }
    }

    private sealed class ForeignKeyColumn
    {
        public string ColumnName { get; set; } = string.Empty;
    }
}
