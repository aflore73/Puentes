-- Carga manual de alias/apodos en PersonAliases.
-- Pegar y ejecutar en cualquier cliente SQLite (DB Browser for SQLite, etc.) apuntando a Data/Puentes.db.
-- Es seguro re-ejecutarlo: el NOT EXISTS evita duplicar un alias ya cargado para esa persona.

INSERT INTO PersonAliases (Id, PersonId, Alias)
SELECT
    upper(
        hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-' ||
        hex(randomblob(2)) || '-' || hex(randomblob(2)) || '-' ||
        hex(randomblob(6))
    ),
    p.Id,
    v.Alias
FROM People p
JOIN (
    -- Agregar acá una fila por cada (nombre de persona, alias) que se quiera cargar.
    SELECT 'Ezequiel' AS Name, 'eze' AS Alias
    UNION ALL SELECT 'Ezequiel', 'sequi'
    UNION ALL SELECT 'Ezequiel', 'ezequi'
    UNION ALL SELECT 'Alejandro', 'ale'
    UNION ALL SELECT 'Alejandro', 'alejo'
    UNION ALL SELECT 'Laura', 'lau'
    UNION ALL SELECT 'Marta', 'marti'
) v ON v.Name = p.Name
WHERE NOT EXISTS (
    SELECT 1 FROM PersonAliases pa
    WHERE pa.PersonId = p.Id
      AND pa.Alias = v.Alias
);

-- Verificación:
SELECT p.Name, pa.Alias
FROM PersonAliases pa
JOIN People p ON p.Id = pa.PersonId
ORDER BY p.Name, pa.Alias;
