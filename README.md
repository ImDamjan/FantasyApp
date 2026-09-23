# FantasyApp

Veb aplikacija za fantasy fudbal po uzoru na zvanični Fantasy Premier League (FPL). Korisnik pravi tim od 15 igrača Premijer lige sa budžetom od 100 miliona, bira početnu postavu i kapitena, pravi izmene (transfere) između kola, koristi čipove i takmiči se sa drugim korisnicima u ligama. Podaci o igračima, klubovima, kolima, utakmicama i poenima preuzimaju se automatski sa javnog FPL API-ja.

Projekat se sastoji od dva dela:

| Deo | Tehnologija | Folder |
| --- | --- | --- |
| Backend (REST API) | .NET 8, ASP.NET Core, EF Core, SQL Server, ASP.NET Core Identity, JWT | `server/` |
| Frontend (SPA) | Angular 22, standalone komponente, signali, SCSS | `client/` |

## Sadržaj

1. [Funkcionalnosti](#funkcionalnosti)
2. [Pravila igre](#pravila-igre)
3. [Arhitektura](#arhitektura)
4. [Model podataka](#model-podataka)
5. [Sinhronizacija sa FPL API-jem](#sinhronizacija-sa-fpl-api-jem)
6. [REST API](#rest-api)
7. [Pokretanje projekta](#pokretanje-projekta)
8. [Testiranje](#testiranje)
9. [Struktura foldera](#struktura-foldera)

## Funkcionalnosti

### Nalog i autentifikacija

- Registracija (email, korisničko ime, lozinka) i prijava.
- Kratkotrajni JWT access token (15 minuta) i refresh token (7 dana) koji se rotira pri svakoj upotrebi.
- Automatsko osvežavanje tokena na frontendu: kada API vrati 401, interceptor osveži token i ponovi zahtev.
- Zaboravljena lozinka: na email stiže link za resetovanje (slanje preko Gmail SMTP-a).
- Odjava opoziva refresh token na serveru.

### Početna strana

- Pregled tima: poeni u tekućem kolu, prosek i najveći broj poena u kolu, ukupni poeni, plasman i broj učesnika.
- Prikaz tima na terenu sa poenima svakog igrača; klik na igrača otvara statistiku (minuti, golovi, asistencije, bonus i ostalo), formu i naredne utakmice.
- Odbrojavanje do sledećeg roka (deadline) i tabela svih narednih rokova.

### Izbor tima (Pick Team)

- Pravljenje početnog tima od 15 igrača klikom na prazna mesta na terenu, uz pretragu i filtere (pozicija, maksimalna cena).
- Opcija **Auto Pick** koja nasumično sastavlja validan tim i iskoristi skoro ceo budžet.
- Izmena postave: zamene između početnih 11 i klupe (dozvoljene su samo zamene koje ostavljaju validnu formaciju), redosled klupe, kapiten i vice-kapiten.
- Aktiviranje čipova: Triple Captain, Bench Boost i Wild Card.

### Transferi

- Označavanje jednog ili više igrača za prodaju i izbor zamene iz liste igrača iste pozicije.
- Pregled budžeta posle transfera, broja besplatnih transfera i cene u poenima pre potvrde.

### Lige

- Svaki korisnik je automatski član zvanične lige **Overall League**.
- Pravljenje privatne lige (dobija se kod od 6 znakova) i pridruživanje tuđoj ligi pomoću koda.
- Tabela lige sa ukupnim poenima, poenima u tekućem kolu i promenom plasmana u odnosu na prethodno kolo.

## Pravila igre

| Pravilo | Vrednost |
| --- | --- |
| Početni budžet | 100.0 m |
| Sastav tima | 2 golmana, 5 odbrambenih, 5 veznih, 3 napadača |
| Maksimalno igrača iz istog kluba | 3 |
| Početna postava | 11 igrača: tačno 1 golman, 3-5 odbrambenih, 3-5 veznih, 1-3 napadača |
| Kapiten | dupli poeni; ako ne odigra ni minut, dupli poeni idu vice-kapitenu |
| Novi tim | neograničeni transferi do prvog roka posle pravljenja tima |
| Besplatni transferi | 1 novi po kolu, neiskorišćeni se prenose, najviše 5 |
| Dodatni transfer | -4 poena; upisuje se kolu za koje važi i oduzima od ukupnog zbira tek kada prođe rok tog kola (poeni samog kola se ne smanjuju) |
| Poništen transfer | ako se igrač vrati u tim pre roka, transfer se ne računa |
| Prodaja kapitena | vice-kapiten postaje kapiten, a za vice-kapitena se bira najskuplji igrač iz postave |
| Triple Captain | kapiten dobija trostruke poene u kolu |
| Bench Boost | poeni igrača sa klupe se računaju u kolu |
| Wild Card | neograničen broj besplatnih transfera do roka; ako je već uzet -4 u tom kolu, poništava se |

Svaki čip može da se iskoristi jednom u sezoni, a u jednom kolu može biti aktivan najviše jedan čip. Transferi, izmene postave i čipovi uvek važe za naredno kolo.

### Snimak tima po kolu

U trenutku kada prođe rok (deadline) kola, sastav svakog tima (15 igrača, postava, klupa, kapiten, vice-kapiten i aktivni čip) se zamrzava u tabeli `GameweekPicks`. Poeni za kolo se računaju isključivo iz tog snimka, pa izmene napravljene posle roka važe tek za sledeće kolo. Tim napravljen posle roka tekućeg kola ne dobija poene za to kolo: na početnoj strani se prikazuje bez poena, uz poruku od kog kola počinje da boduje. Snimak pravi pozadinski servis, a pre svake izmene tima proverava se da li je u međuvremenu prošao neki rok, tako da izmena ne može da se provuče između roka i snimanja.

## Arhitektura

### Backend

Rešenje `server/FantasyApp.sln` ima pet projekata raspoređenih po slojevima:

```
FantasyApp.Entity        modeli (EF entiteti) i DTO klase; nema zavisnosti
FantasyApp.Common        -> Entity: JWT, slanje emaila, FPL HTTP klijent, klase podešavanja
FantasyApp.Repository    -> Entity: ApplicationDbContext, migracije, repozitorijumi
FantasyApp.BusinessLogic -> Entity, Repository, Common: servisi sa poslovnom logikom
FantasyApp.Api           -> svi projekti: kontroleri, DI konfiguracija, pozadinski servis
```

Ključni delovi:

- **Autentifikacija**: korisnike, heširanje lozinki i tokene za reset lozinke vodi ASP.NET Core Identity (`ApplicationUser : IdentityUser<long>`). Identity tabele su preimenovane u `Users`, `Roles`, `UserRoles` i slično. JWT sadrži `sub`, `email`, ime i `jti`; aplikacija nema uloge, pa je `[Authorize]` dovoljan za zaštitu ruta. Refresh tokeni se čuvaju u tabeli `RefreshTokens`.
- **Servisi**: `AuthService`, `SquadService` (tim, postava, kapiten, čipovi), `TransferService`, `LeagueService`, `PointsService`, `ScoringService` (obračun poena po kolu iz snimka), `GameweekSnapshotService` (snimak timova na roku), `PlayerService`, `GameweekService` i `FplDataSyncService`.
- **Rezultat servisa**: servisi vraćaju `ServiceResult<T>` (uspeh sa podacima ili poruka o grešci), a kontroleri ga pretvaraju u `200 OK` ili `400 Bad Request`.
- **Datumi**: `UtcDateTimeConverter` obezbeđuje da se svi datumi šalju kao UTC (sa sufiksom `Z`), pa ih browser ispravno prikazuje u lokalnoj vremenskoj zoni.

### Frontend

Struktura `client/src/app/` je podeljena po funkcionalnostima:

```
core/        servisi (Auth, Squad, Transfer, League, Points, Player, Gameweek, Toast),
             guard-ovi, HTTP interceptor, modeli, konstante, validatori
shared/      komponente koje koristi više stranica: app-shell (bočni meni), teren (pitch-view,
             squad-build-pitch), kartica igrača, pretraga igrača, popup sa statistikom, toast poruke
features/
  auth/      prijava/registracija, zaboravljena lozinka, reset lozinke
  home/      početna strana
  pick-team/ izbor tima i postave
  transfers/ transferi
  leagues/   lige i tabela lige
```

- Stanje se čuva u Angular signalima; aplikacija radi bez `zone.js`.
- `authInterceptor` dodaje `Authorization: Bearer` header i osvežava token na 401. Više istovremenih 401 odgovora čeka na isto osvežavanje.
- `authGuard` pušta na zaštićene stranice samo prijavljene korisnike, a `guestGuard` preusmerava prijavljene korisnike sa `/auth` stranica.
- Stilovi su čist SCSS bez UI biblioteke; boje i ostale vrednosti dizajna su CSS promenljive `--fa-*` u `src/styles.scss`.

## Model podataka

| Tabela | Opis |
| --- | --- |
| `Users`, `Roles`, ... | Identity tabele |
| `RefreshTokens` | refresh tokeni sa datumom isteka, opoziva i zamene |
| `Teams` | klubovi Premijer lige (iz FPL-a) |
| `Players` | igrači: pozicija, klub, cena (u desetinama miliona), ukupni poeni, forma, status |
| `Gameweeks` | kola: rok, oznaka završeno, da li je snimak timova napravljen i da li su poeni konačni |
| `Fixtures` | utakmice: domaćin, gost, rezultat, težina, vreme početka |
| `PlayerGameweekStats` | statistika igrača po kolu (poeni, minuti, golovi, asistencije, ...) |
| `FantasyTeams` | tim korisnika: budžet, besplatni transferi, iskorišćeni i aktivni čipovi |
| `SquadPlayers` | trenutni tim od 15 igrača (važi za naredni rok): postava, redosled klupe, kapiten, vice-kapiten |
| `GameweekPicks` | zamrznut tim za svako kolo, pravi se kada prođe rok; iz njega se računaju poeni |
| `Transfers` | istorija transfera po kolu |
| `Leagues`, `LeagueMemberships` | lige (naziv 3-20 znakova) i članstva |
| `UserGameweekScores` | poeni korisnika po kolu: osvojeni, cena transfera, neto, iskorišćen čip |

Cene se u bazi čuvaju kao celi brojevi u desetinama miliona (na primer, 105 znači 10.5 m), a API ih vraća u milionima.

## Sinhronizacija sa FPL API-jem

Pozadinski servis `FplSyncService` se pokreće zajedno sa API-jem:

- **Na startu i zatim na svakih 60 minuta** preuzima `bootstrap-static` i `fixtures` (klubovi, igrači, cene, kola, utakmice).
- **Na svakih 5 minuta** pravi snimke timova za kola čiji je rok prošao, a zatim za svako kolo koje je počelo (prva utakmica kola je startovala) i čiji poeni još nisu konačni preuzima statistiku uživo (`event/{id}/live`) i ponovo obračunava poene. Bodovanje traje od prve do poslednje utakmice kola; kada FPL označi kolo kao završeno (posle poslednje utakmice i bonus poena), poeni postaju konačni i kolo se više ne osvežava.
- Tekuće i sledeće kolo se određuju po vremenu roka, a ne po FPL oznakama koje se osvežavaju samo jednom na sat.

Za prvu sinhronizaciju je potreban pristup internetu; dok se ona ne završi, lista igrača je prazna.

## REST API

Sve rute osim prvih šest u tabeli zahtevaju header `Authorization: Bearer <accessToken>`. Kada API radi u Development okruženju, interaktivna dokumentacija je dostupna na `http://localhost:5080/swagger`.

| Metoda | Ruta | Opis |
| --- | --- | --- |
| POST | `/api/auth/register` | registracija |
| POST | `/api/auth/login` | prijava |
| POST | `/api/auth/refresh-token` | novi par tokena na osnovu refresh tokena |
| POST | `/api/auth/revoke-token` | opoziv refresh tokena (odjava) |
| POST | `/api/auth/forgot-password` | slanje emaila za reset lozinke |
| POST | `/api/auth/reset-password` | postavljanje nove lozinke |
| GET | `/api/auth/me` | podaci o prijavljenom korisniku |
| GET | `/api/players` | lista igrača; parametri `position`, `maxPrice`, `search`, `teamId`, `page`, `pageSize` |
| GET | `/api/players/{id}` | detalji igrača sa narednih 5 utakmica |
| GET | `/api/squad` | tim korisnika, uključujući preostale besplatne transfere i da li su transferi neograničeni |
| POST | `/api/squad` | izbor početnog tima (15 igrača, postava, kapiteni, klupa) |
| PUT | `/api/squad/lineup` | izmena postave, klupe i kapitena |
| PUT | `/api/squad/captain` | izmena samo kapitena i vice-kapitena |
| PUT | `/api/squad/chip` | aktiviranje čipa (`TripleCaptain`, `BenchBoost`, `WildCard`) |
| POST | `/api/transfers` | potvrda jednog ili više transfera |
| GET | `/api/transfers/history` | istorija transfera |
| GET | `/api/points/summary` | pregled poena i plasmana |
| GET | `/api/points/history` | poeni po kolima |
| GET | `/api/points/squad` | tim iz snimka tekućeg kola sa poenima; `isScoring` je `false` ako tim nije postojao u trenutku roka |
| GET | `/api/leagues/mine` | lige korisnika |
| POST | `/api/leagues` | pravljenje lige |
| POST | `/api/leagues/join` | pridruživanje ligi pomoću koda |
| GET | `/api/leagues/{id}/standings` | tabela lige |
| GET | `/api/gameweeks/deadlines` | rokovi za kola |

## Pokretanje projekta

### Preduslovi

| Alat | Verzija | Napomena |
| --- | --- | --- |
| .NET SDK | 8.0 | `dotnet --version` |
| Node.js | 22 ili noviji | uz npm |
| Angular CLI | 22 | `npm install -g @angular/cli` |
| Docker | bilo koja novija | za SQL Server |
| dotnet-ef | 8.x | `dotnet tool install --global dotnet-ef` |

### 1. Pokretanje SQL Servera

SQL Server se pokreće u Docker kontejneru (na Linux distribucijama poput Fedore ne postoji nativna instalacija). Lozinka mora da ima najmanje 8 znakova, veliko i malo slovo, broj i specijalni znak.

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<lozinka>" \
  -p 1433:1433 --name fantasyapp-sqlserver --restart unless-stopped \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Kontejner se posle restarta računara pokreće sam. Ako je zaustavljen, pokreće se komandom `docker start fantasyapp-sqlserver`.

### 2. Tajni podaci (user secrets)

Tajni podaci se ne nalaze u `appsettings.json` i ne ulaze u git. Podešavaju se preko `dotnet user-secrets` iz foldera API projekta:

```bash
cd server/src/FantasyApp.Api

dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=FantasyAppDb;User Id=sa;Password=<lozinka>;TrustServerCertificate=True;"

# nasumičan ključ od najmanje 32 znaka, na primer: openssl rand -base64 48
dotnet user-secrets set "Jwt:Key" "<dugacak-nasumican-kljuc>"

# potrebno samo za slanje emaila za reset lozinke (Gmail App Password)
dotnet user-secrets set "Smtp:Username" "<gmail-adresa>"
dotnet user-secrets set "Smtp:FromEmail" "<gmail-adresa>"
dotnet user-secrets set "Smtp:AppPassword" "<gmail-app-password>"
```

Gmail App Password se pravi na Google nalogu (Security, 2-Step Verification, App passwords). Bez SMTP podešavanja aplikacija radi normalno, ali slanje emaila za reset lozinke neće uspeti.

Ostala podešavanja nisu tajna i nalaze se u `server/src/FantasyApp.Api/appsettings.json`:

| Ključ | Podrazumevano | Opis |
| --- | --- | --- |
| `Jwt:Issuer`, `Jwt:Audience` | `FantasyApp`, `FantasyAppClient` | vrednosti u JWT-u |
| `Jwt:AccessTokenExpirationMinutes` | `15` | trajanje access tokena |
| `Jwt:RefreshTokenExpirationDays` | `7` | trajanje refresh tokena |
| `Smtp:Host`, `Smtp:Port` | `smtp.gmail.com`, `587` | SMTP server |
| `App:ClientUrl` | `http://localhost:4200` | adresa frontenda (koristi se u linku za reset lozinke) |
| `Fpl:BaseUrl` | `https://fantasy.premierleague.com/api/` | adresa FPL API-ja |

### 3. Kreiranje baze

Migracije se ne primenjuju automatski pri startu, pa se baza kreira ručno:

```bash
cd server/src/FantasyApp.Api
dotnet ef database update --project ../FantasyApp.Repository --startup-project .
```

### 4. Pokretanje backenda

```bash
cd server
dotnet build

cd src/FantasyApp.Api
dotnet run --urls "http://localhost:5080"
```

API radi na `http://localhost:5080`, a Swagger na `http://localhost:5080/swagger`. Pri prvom pokretanju API sam napravi ligu "Overall League" i počinje sinhronizaciju sa FPL-om, što traje oko minut.

### 5. Pokretanje frontenda

```bash
cd client
npm install
ng serve --port 4200
```

Aplikacija je dostupna na `http://localhost:4200`. Frontend očekuje API na `http://localhost:5080/api` (podešeno u `client/src/environments/environment.ts`), a CORS na backendu dozvoljava samo `http://localhost:4200`.

### Korisne komande

```bash
# nova migracija (iz server/src/FantasyApp.Api)
dotnet ef migrations add <Naziv> --project ../FantasyApp.Repository --startup-project . --output-dir Migrations

# brisanje baze
dotnet ef database drop --project ../FantasyApp.Repository --startup-project . --force

# pregled podataka u bazi
docker exec -it fantasyapp-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<lozinka>' -C -d FantasyAppDb

# produkcioni build frontenda (rezultat u client/dist/client)
cd client && ng build
```

## Testiranje

```bash
cd client
ng test --watch=false
```

Frontend testovi koriste Vitest. Za backend trenutno ne postoji test projekat. API može ručno da se testira preko Swagger-a.

## Struktura foldera

```
FantasyApp/
  client/                         Angular aplikacija
    public/                       statički fajlovi (logo, favicon)
    src/
      app/                        core, shared, features (opisano gore)
      environments/               adresa API-ja
      styles.scss                 globalni stilovi i CSS promenljive
  server/
    FantasyApp.sln
    src/
      FantasyApp.Api/             Program.cs, kontroleri, pozadinski servis, JSON konverter
      FantasyApp.BusinessLogic/   servisi i interfejsi servisa
      FantasyApp.Common/          JWT, email, FPL klijent, podešavanja
      FantasyApp.Entity/          modeli i DTO klase
      FantasyApp.Repository/      DbContext, migracije, repozitorijumi
  CLAUDE.md                       uputstvo za rad sa Claude Code alatom
  README.md
```
