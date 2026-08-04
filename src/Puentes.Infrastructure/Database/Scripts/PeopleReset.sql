PRAGMA foreign_keys = OFF;

BEGIN TRANSACTION;

DROP TABLE IF EXISTS PersonRelationships;
DROP TABLE IF EXISTS LifeEventParticipants;
DROP TABLE IF EXISTS PersonLifeEvents;
DROP TABLE IF EXISTS PersonRoutines;
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

CREATE TABLE PersonLifeEvents
(
    Id TEXT PRIMARY KEY,
    PersonId TEXT NOT NULL,
    StartDate TEXT NULL,
    EndDate TEXT NULL,
    DatePrecision INTEGER NOT NULL,
    Title TEXT NOT NULL,
    Description TEXT NULL,
    Place TEXT NULL,
    IsPositiveMemory INTEGER NOT NULL DEFAULT 0,

    FOREIGN KEY (PersonId)
        REFERENCES People(Id)
        ON DELETE CASCADE,

    CHECK (length(trim(Title)) > 0),
    CHECK (IsPositiveMemory IN (0, 1)),
    CHECK (EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate)
);

CREATE INDEX IX_PersonLifeEvents_PersonId_StartDate
ON PersonLifeEvents (PersonId, StartDate);

CREATE TABLE LifeEventParticipants
(
    LifeEventId TEXT NOT NULL,
    PersonId TEXT NOT NULL,
    Role TEXT NULL,

    PRIMARY KEY (LifeEventId, PersonId),

    FOREIGN KEY (LifeEventId)
        REFERENCES PersonLifeEvents(Id)
        ON DELETE CASCADE,

    FOREIGN KEY (PersonId)
        REFERENCES People(Id)
        ON DELETE CASCADE
);

CREATE INDEX IX_LifeEventParticipants_PersonId
ON LifeEventParticipants (PersonId);

CREATE TABLE PersonRoutines
(
    Id TEXT PRIMARY KEY,
    PersonId TEXT NOT NULL,
    Title TEXT NOT NULL,
    Notes TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,

    FOREIGN KEY (PersonId)
        REFERENCES People(Id)
        ON DELETE CASCADE,

    CHECK (length(trim(Title)) > 0),
    CHECK (length(trim(Notes)) > 0),
    CHECK (IsActive IN (0, 1))
);

CREATE INDEX IX_PersonRoutines_PersonId
ON PersonRoutines (PersonId);

COMMIT;

PRAGMA foreign_keys = ON;
