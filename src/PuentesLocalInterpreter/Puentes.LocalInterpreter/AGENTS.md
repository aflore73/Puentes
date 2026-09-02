# Puentes.LocalInterpreter

Local (offline, no-LLM) Spanish-language interpreter that parses free-text phrases into a structured
`InterpretationResult`: normalizes text, detects a known person/relation, detects an intent, and builds a
canonical Spanish interpretation. Console app; the text pipeline (`Interpreter.Interpret`) is pure
rule-based, no I/O calls. Also supports a voice round-trip (mic → Whisper.net STT → interpreter →
System.Speech TTS), see "Voice" below. Targets `net10.0-windows` because of `System.Speech`/`NAudio`.

## Build & run

```powershell
dotnet build
dotnet run
```

`Program.cs` runs a batch of hardcoded example phrases (`RunExamples`) then drops into an interactive
loop reading stdin until the user types `SALIR`, or `VOZ` to do a voice turn instead of typing.

Tests live in the sibling `../Puentes.LocalInterpreter.Tests` project (xUnit, same convention as the
repo's main `Puentes.Tests` project):

```powershell
cd ../Puentes.LocalInterpreter.Tests
dotnet test
```

## Voice (Voice/*.cs)

- `MicrophoneRecorder` — records mic input via NAudio (`WaveInEvent`) at 16kHz mono PCM (the format
  Whisper.net expects) into an in-memory WAV stream, until Enter is pressed.
- `SpeechTranscriber` — downloads `ggml-base.bin` on first use (via `WhisperGgmlDownloader`, needs
  internet once) into the working directory, then transcribes the WAV stream to Spanish text with
  Whisper.net. The model file is cached on disk between runs.
- `TextToSpeechService` — speaks text back using `System.Speech.Synthesis`, preferring an installed
  `es-ES` voice if present. Windows-only (marked `[SupportedOSPlatform("windows")]`).
- Wired together in `Program.RunVoiceTurnAsync`: record → transcribe → `Interpreter.Interpret` → print →
  speak the resulting `Interpretation`.

## Database context (Data/*.cs, Services/DatabaseContextService.cs)

Unlike the pure text pipeline above, this part *does* do I/O: it queries the real Puentes SQLite database
shared with the main API (`src/Puentes`), via a `ProjectReference` to `Puentes.Infrastructure`.

- `DatabaseConnectionFactory` opens `Data/Puentes.db` (relative to the working directory, overridable via
  `PUENTES_DB_PATH`), registers the same Dapper type handlers as `src/Puentes/Program.cs`
  (`GuidTypeHandler`, `DateOnlyTypeHandler`, `DateTimeOffsetTypeHandler` — without these, Guid/date columns
  fail to deserialize), and runs `DatabaseInitializer.InitializeAsync()` once to create any missing tables.
- `PersonAlias` (`Puentes.Core/Domain/PersonAlias.cs` + `PersonAliasRepository`) is a small addition to the
  shared schema: maps nicknames (e.g. "eze", "sequi") to a real `People.Id`, since the DB has no alias
  concept otherwise. Follow the existing `Scripts/*.cs` convention (constants with raw SQL) if you add more
  tables.
- `DatabaseContextService.EnrichAsync` takes the already-resolved local `PersonResult`/`IntentResult`,
  matches the local person against the real DB (by name, then by alias), and depending on intent queries
  `LifeEventRepository` (UBICACION/EVENTO), `PersonAgendaItemRepository` (AGENDA), or
  `PersonSupportContentRepository` (EMOCION) for a one-line summary. Returns `null`/`Found = false` when
  there's no local person or no matching data — callers must handle that gracefully.
- Wired into both the text loop and `RunVoiceTurnAsync` in `Program.cs`: printed as an extra "CONTEXTO BD"
  section by `ConsolePrinter`, and appended to the spoken reply in the voice flow.


## Pipeline (Services/Interpreter.cs)

`Interpreter.Interpret(input)` runs, in order:
1. `TextNormalizer.Normalize` — lowercases, strips `...`/`…`, and expands local slang/typos via whole-word
   regex replace (e.g. `ta`→`está`, `ijo`→`hijo`, `manana`→`mañana`). Add new colloquialisms here.
2. `PersonDetector.Detect` — matches against `Context/PeopleContext.People` (name + alias list) first;
   if no alias matches, falls back to family-relation keywords (`hijo`, `hermana`, ...) and resolves
   ambiguity via `PeopleContext.GetContextualPerson` (the "default" person for that relation).
3. `IntentDetector.Detect` — ordered cascade of keyword categories (UBICACION, PERSONA, EMOCION,
   NECESIDAD, AGENDA, EVENTO), first match wins. **Order matters**: earlier categories have implicit
   priority over later ones even if a later category's keyword also appears in the text.
4. `Interpreter.BuildInterpretation` — renders a normalized Spanish sentence per intent, substituting
   the resolved person's canonical name for aliases/relation words.

`Confidence` (`ALTA`/`MEDIA`/`BAJA`) is derived from whether both person and intent were resolved.

## Conventions specific to this codebase

- All keyword/alias matching uses whole-word regex (`ContainsWholeWord`/`ReplaceWholeWord`), duplicated
  per file — not shared, so keep the same `(?<![\p{L}\p{N}])...(?![\p{L}\p{N}])` pattern if you add matching
  elsewhere.
- Static classes + static methods throughout `Services/` and `Context/` — no DI, no instance state.
- Models (`Models/*.cs`) are `sealed` classes with `init`-only properties and hardcoded Spanish defaults
  (e.g. `Intent = "DESCONOCIDA"`, `Confidence = "BAJA"`) — treat these string literals as the canonical
  enum values (no actual enum type is used).
- People/aliases/relations are hardcoded in `Context/PeopleContext.cs`; there's no external config or
  database. Adding a person means adding to `People` and, if they should be a relation's default, updating
  `GetContextualPerson`.
- User-facing strings and all example phrases are in Spanish (Argentine colloquial); keep new
  strings/output consistent with that register.
