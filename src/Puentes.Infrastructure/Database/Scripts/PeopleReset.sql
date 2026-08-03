PRAGMA foreign_keys = OFF;

BEGIN TRANSACTION;

DROP TABLE IF EXISTS PersonRelationships;
DROP TABLE IF EXISTS People;

CREATE TABLE People
(
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    BirthDate TEXT NULL,
    City TEXT NOT NULL DEFAULT '',
    Province TEXT NOT NULL DEFAULT '',
    Country TEXT NOT NULL DEFAULT 'Argentina'
);

CREATE INDEX IX_People_Name
ON People (Name);

CREATE TABLE PersonRelationships
(
    Id TEXT PRIMARY KEY,
    PersonId TEXT NOT NULL,
    RelatedPersonId TEXT NOT NULL,
    Type INTEGER NOT NULL,
    Notes TEXT NULL,

    FOREIGN KEY (PersonId)
        REFERENCES People(Id)
        ON DELETE CASCADE,

    FOREIGN KEY (RelatedPersonId)
        REFERENCES People(Id)
        ON DELETE CASCADE,

    CHECK (PersonId <> RelatedPersonId),
    UNIQUE (PersonId, RelatedPersonId, Type)
);

CREATE INDEX IX_PersonRelationships_PersonId
ON PersonRelationships (PersonId);

CREATE INDEX IX_PersonRelationships_RelatedPersonId
ON PersonRelationships (RelatedPersonId);

COMMIT;

PRAGMA foreign_keys = ON;
