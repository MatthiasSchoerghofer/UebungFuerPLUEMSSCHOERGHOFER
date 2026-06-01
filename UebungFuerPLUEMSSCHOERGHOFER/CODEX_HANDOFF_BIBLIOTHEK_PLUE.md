# Codex Handoff: Bibliothek PLUE Uebungsangabe

## Kontext

Workspace:

`C:\SPENGERGASSE\Programming\POS4TE\260521-PLFS-Vorlage`

Der User hat eine POS/PLUE/PLFS-Uebungsangabe fuer eine HTL-Spengergasse-artige ASP.NET-Core-Vorlage gewollt. Die Vorlage ist eine bestehende Solution mit:

- `src/Aufgabe1_ORMapping`
- `src/Aufgabe2_BusinessServices`
- `src/Aufgabe3_RestfulApi`
- passenden Testprojekten unter `tests/`

Die vorhandene Vorlage ist fachlich eine Song-App. Sie zeigt die gewuenschte Architektur:

- EF-Core Entities + DbContext + LINQ-Queries
- BusinessServices mit Commands, DTOs, Mapper, Service, TestFixtures
- REST API mit Controllern
- Tests fuer DbContext/LINQ, Service und REST-Endpunkte

User-Ziel:

Eine nachbaubare Uebungsangabe fuer Bibliothek/Buecherverleih, damit der User REST, LINQ, EF Core, DTOs, Mapper, Services und Tests lernen kann.

## Wichtige User-Praeferenz

Der erste Entwurf war zu gross. Der User meinte sinngemaess:

- Nicht 10 Mapper-Methoden, die alle fast gleich sind.
- Nicht zu viel redundante Klassenarbeit.
- Lieber weniger Scope, schneller umsetzbar, mehr Lernen.

Daraufhin wurde die Angabe bewusst entschlackt.

## Aktueller Stand

Diese Dateien wurden erstellt/aktualisiert:

- `BIBLIOTHEK_PLUE_ANGABE.md`
- `BIBLIOTHEK_PLUE_ANGABE.html`
- `BIBLIOTHEK_PLUE_ANGABE.pdf`
- `scripts/render-bibliothek-pdf.cjs`

PDF wurde zuletzt erfolgreich neu gerendert:

- PDF-Pfad: `C:\SPENGERGASSE\Programming\POS4TE\260521-PLFS-Vorlage\BIBLIOTHEK_PLUE_ANGABE.pdf`
- Seitenzahl: 12
- Renderer: lokales Chrome ueber Playwright

## Finaler fachlicher Scope der Angabe

Entities:

- `Author`
- `Book`
- `BookLoan`

Enums:

- `BookGenre`
- `LoanStatus`

Bewusst entfernt:

- keine `LibraryMember`-Entity
- kein `MemberResponseDto`
- kein `AuthorStatisticDto`
- kein `MembersController`
- kein `AuthorsController`
- kein Popular-Authors-Use-Case

Grund:

Der Borrower wird direkt als `BorrowedBy` in `BookLoan` gespeichert. Dadurch bleiben Beziehungen, LINQ, DTOs, Services und REST erhalten, aber der User muss nicht noch eine fast gleiche Member-Verwaltung bauen.

## Gewollte Mapper-Methoden

Die Angabe verlangt genau vier Mapper-Methoden:

```csharp
Book ToEntity(CreateBookCmd cmd, Author author, DateTime addedAt)
BookLoan ToEntity(Book book, BorrowBookCmd cmd, DateTime borrowedAt, DateTime dueAt)
BookResponseDto ToDto(Book book)
LoanResponseDto ToDto(BookLoan loan)
```

Hinweise:

- Der Service sucht oder erstellt `Author`.
- Der Mapper bekommt den fertigen `Author`.
- `ReturnBookCmd` braucht keine Mapper-Methode, weil eine bestehende `BookLoan` veraendert wird.
- `UpdateCopiesCmd` braucht keine Mapper-Methode, weil eine bestehende `Book`-Entity veraendert wird.

## Finaler REST-Scope

Nur zwei Controller:

`BooksController`

- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books`
- `PUT /api/books/{id}/copies`
- `DELETE /api/books/{id}`
- `GET /api/books/available?genre=Programming`
- `GET /api/books/by-author?authorName=Robert%20C.%20Martin`

`LoansController`

- `POST /api/loans`
- `PUT /api/loans/{loanId}/return`
- `GET /api/loans/overdue?today=2026-05-18`

## LINQ-Queries im DbContext

Die Angabe verlangt drei Queries:

```csharp
QueryAvailableBooks(BookGenre? genre = null)
QueryOverdueLoans(DateTime today)
QueryBooksByAuthor(string authorName)
```

Diese decken ab:

- `Where`
- optionale Filter
- `Include`
- `ThenInclude`
- `OrderBy`
- Navigation Properties

## Commands und DTOs

Commands:

- `CreateBookCmd`
- `UpdateCopiesCmd`
- `BorrowBookCmd`
- `ReturnBookCmd`

Response-DTOs:

- `BookResponseDto`
- `LoanResponseDto`

## PDF/HTML Render-Skript

Das Skript liegt hier:

`scripts/render-bibliothek-pdf.cjs`

Es:

- liest `BIBLIOTHEK_PLUE_ANGABE.md`
- rendert daraus HTML
- fuegt Visualisierungen ein:
  - Architektur
  - Datenmodell
  - wichtigste Endpunkte
  - Teststrategie
- druckt per Playwright/Chrome nach PDF

Render-Befehl:

```powershell
& 'C:\Users\Mahi0\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe' 'scripts\render-bibliothek-pdf.cjs'
```

Falls noetig: Playwright kommt aus dem gebuendelten Runtime-Node-Modules-Pfad:

`C:\Users\Mahi0\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules`

## Letzte relevante User-Fragen

User fragte, wie man beim Mapper weiss, was man braucht.

Kurzantwort:

- Command wird zu Entity, wenn neu gespeichert wird.
- Entity wird zu DTO, wenn etwas ueber API zurueckgegeben wird.
- Keine Mapper-Methode fuer reine Updates an bestehenden Entities.
- Mapper darf keine Businesslogik, keine Datenbankzugriffe und keine HTTP-Logik enthalten.

## Falls weitergearbeitet wird

Wahrscheinliche naechste Aufgaben:

- PDF nochmal anpassen.
- Mapper/Service beispielhaft skizzieren, aber evtl. nicht komplett loesen, weil der User selbst lernen will.
- Aus der Angabe eine noch kuerzere Checkliste machen.
- Oder mit dem User Schritt fuer Schritt die Aufgabe implementieren.

Wichtig: Der User moechte lernen und nicht alles fertig hingestellt bekommen, ausser er fragt explizit danach.

