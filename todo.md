# TODO: StaticMethods/JsonMethods.cs

## Błędy (najpierw)
- [X] **Modyfikatory dostępu** (klasa i metody): bez nich `DataBaseJson` nie wywoła metod. Ujednolić widoczność klasy i enuma (klasa `internal`, enum `public`).
- [X] **`templist!` w deserializacji**: plik z samym `null` zwraca `Success` i `null`. Nie traktować tego jak sukcesu.
- [ zmiana bedzie zrobiona puzniej ] **`File.Create` obcina plik przed zapisem**: błąd w trakcie serializacji = utrata starych danych. Zapis powinien być bezpieczny (odpowiedzialność tej warstwy).
- [x] **Brakujące wyjątki** (`NotSupportedException`, `ArgumentException`): ustalone, że to błędy programisty, nie są łapane i mają wylecieć (fail-fast). Do zrobienia: zapisać to w kontrakcie metod (komentarz XML) i ustalić, gdzie waliduje się `path` (jeśli pochodzi od użytkownika/konfiguracji, to `ArgumentException` jest błędem środowiska, nie bugiem).
- [ ] **`JsonException` przy zapisie** i nazwa `InvalidJson`: mylące przy zapisie. Przy założeniu "błędy programisty lecą" to też bug (cykle, głębokość), więc `catch` przy zapisie prawdopodobnie zbędny. Zdecydować.
- [ ] **Pusty plik (0 bajtów)** daje `InvalidJson`: zdecydować, czy `DataBaseJson` ma go odróżniać od uszkodzonego.

## Projekt
- [ ] **Pusta lista przy błędzie odczytu**: ryzyko, że `DataBaseJson` zignoruje wynik i nadpisze plik pustymi danymi. Wymusić sprawdzenie wyniku przed użyciem danych.
- [ ] **Rozróżnić przypadki**: `FileNotFound` (pierwsze uruchomienie, pusta baza) vs `InvalidJson`/`IoError` (nie wolno nadpisywać pliku). Decyzja po stronie `DataBaseJson`, ale `JsonMethods` musi dać dość informacji.
- [ ] **Enum traci informacje** (brak komunikatu/wyjątku, brak logowania).
- [ ] **Granica odpowiedzialności**: czy `JsonMethods` musi znać `List<T>`? Wybór kolekcji i ścieżka do pliku to raczej sprawa `DataBaseJson`.
- [ ] **Współbieżność**: równoległe zapisy do jednego pliku się zderzą. Ustalić, kto serializuje dostęp (raczej `DataBaseJson`).
- [ ] **`JsonOptions` współdzielone** przez zapis i odczyt: `WriteIndented`/`Encoder` nie mają sensu przy odczycie; zdecydować o wielkości liter właściwości.
- [ ] **`CancellationToken`** w metodach async.
- [ ] **Statyka a przyszła biblioteka**: jedno miejsce w `DataBaseJson`, z którego wołane jest `JsonMethods`, żeby ewentualna zamiana na zależność była tania.

## Styl
- [ ] Literówka `Metods` w nazwie enuma (`JsonMetodsResult`).
- [ ] Nazwy `SerializationAsync` / `DeserializationAsync` na idiomatyczne (`SerializeAsync` / `DeserializeAsync` albo nazwy opisujące cel).
- [ ] Zmienna `templist` (nazwa).
- [ ] Formatowanie: `List <T>`, `return(`, puste linie w nagłówku i przed zamykającymi nawiasami, białe znaki na końcach linii.
- [ ] Namespace `posConsole` vs nazwa projektu.
- [ ] Enum do osobnego pliku.

## Zrobione dobrze (nie ruszać)
- `await using` i `async`/`await`.
- `JsonSerializerOptions` jako `static readonly`.
- Kolejność `catch` od szczegółowych do ogólnych.
- Enum wyniku zamiast wyjątków w normalnym przepływie.
- Metody generyczne, file-scoped namespace.

## Ustalenia: DataBaseJson.Save (bezpieczny zapis)
Plan (odpowiedzialność `DataBaseJson`, nie `JsonMethods`):
1. Zapis do pliku tymczasowego (`.tmp`) przez `JsonMethods`.
2. Sprawdzenie spójności pliku tymczasowego.
3. Stary plik jako backup, nowy jako aktualny.

Zasady:
- `JsonMethods.SerializationAsync` (`File.Create` obcina plik) nigdy nie dostaje ścieżki pliku produkcyjnego, tylko `.tmp`.
- Plik `.tmp` leży w tym samym katalogu co docelowy (atomowość tylko w obrębie jednego wolumenu).

Do przemyślenia / rozstrzygnięcia:
- [ ] **Podmiana nie jest atomowa**, jeśli to dwa osobne przeniesienia (stary → backup, nowy → aktualny): crash między nimi = brak pliku aktualnego. Sprawdzić `File.Replace` / `File.Move` z nadpisaniem.
- [ ] **Odzyskiwanie po awarii przy starcie**: co robić, gdy został `.tmp`, brak aktualnego, jest tylko backup.
- [ ] **Flush na dysk** przed podmianą (dane mogą siedzieć w buforach systemu).
- [ ] **Co znaczy "spójność"**: sama deserializacja sprawdza tylko składnię. Czy porównywać np. liczbę elementów? Pamiętać o `null` (`NullList`).
- [ ] **Backup**: jedna generacja czy kilka; nie nadpisywać dobrego backupu uszkodzonym plikiem.
- [ ] **Sprzątanie `.tmp`**, gdy weryfikacja się nie powiedzie lub przeleci wyjątek (`try/finally` po stronie `DataBaseJson`).
- [ ] **Współbieżność**: unikalna nazwa `.tmp` i serializacja dostępu do `Save`.
- [ ] **Sprawdzanie wyniku odczytu** przed użyciem listy (pusta lista przy błędzie nie może prowadzić do nadpisania pliku).

## Kolejność pracy
1. Zrobione: modyfikatory, `null` po deserializacji.
2. Decyzje w `JsonMethods`: `JsonException` przy zapisie, pusty plik, kontrakt wyjątków.
3. `DataBaseJson.Save` według sekcji "Ustalenia" (bezpieczny zapis).
4. Styl na końcu.
