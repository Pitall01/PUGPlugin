Wprowadzenie
PUG Plugin (Pick-Up Game) to kompleksowe rozwiązanie dla serwerów Counter-Strike 2 wykorzystujących framework CounterStrikeSharp. Wtyczka została zaprojektowana, aby ułatwić organizację i przeprowadzanie meczów w trybie rywalizacji, automatyzując wiele zadań administracyjnych, takich jak wybór drużyn, przeprowadzanie rundy nożowej oraz konfiguracja serwera na różnych etapach rozgrywki.
Główne cechy

Automatyczne wykrywanie graczy na podstawie Steam ID z pominięciem botów
Losowe przydzielanie graczy do drużyn (5 vs 5)
Runda nożowa do wyłonienia drużyny, która wybiera stronę startową
Okres rozgrzewki między rundą nożową a właściwym meczem
Format MR12 (12 rund na połowę, 24 rundy łącznie)
Wsparcie dla dogrywek w przypadku remisu
Tryb testowy umożliwiający sprawdzenie funkcjonalności wtyczki z botami


Instalacja
Wymagania

Serwer Counter-Strike 2
CounterStrikeSharp (w najnowszej wersji)
Uprawnienia administratora na serwerze

Proces instalacji

Pobierz najnowszą wersję wtyczki z repozytorium
Rozpakuj zawartość archiwum do katalogu /game/csgo/addons/counterstrikesharp/plugins/
Upewnij się, że struktura plików wygląda następująco:

/plugins/
  /PUGPlugin/
    PUGPlugin.cs
    PUGPlugin.dll
    plugin.json

Zrestartuj serwer lub załaduj wtyczkę używając komendy administratora css_plugins_load PUGPlugin

Weryfikacja instalacji
Po zainstalowaniu wtyczki, możesz sprawdzić jej działanie używając testowej komendy:

Wpisz !hello w czacie gry
Jeśli instalacja przebiegła pomyślnie, zobaczysz wiadomość: [PUG] Test pomyślny.


Funkcje
1. Wykrywanie graczy
Wtyczka automatycznie wykrywa graczy na serwerze, filtrując boty i uwzględniając tylko rzeczywistych graczy na podstawie Steam ID.
2. System drużynowy

Automatyczne przydzielanie 5 graczy do drużyny CT i 5 graczy do drużyny T
Losowy wybór uczestników dla zrównoważonej rozgrywki

3. Runda nożowa

Specjalna runda, w której gracze mają dostęp wyłącznie do noży
Automatyczna konfiguracja ustawień serwera odpowiednich dla rundy nożowej
Wyłączenie wszystkich broni poza nożami
Zapewnienie kamizelki kuloodpornej bez hełmu dla wszystkich graczy

4. Wybór stron

Zwycięska drużyna z rundy nożowej otrzymuje prawo wyboru strony startowej
Komendy !ct i !t do wyboru preferowanej strony
Automatyczne przełączanie graczy między drużynami w zależności od wybranej strony

5. Runda rozgrzewkowa

5-minutowa sesja rozgrzewkowa między rundą nożową a właściwym meczem
Pełne wyposażenie i fundusze dla graczy
Możliwość odradzania się po śmierci
Możliwość kupowania broni w dowolnym miejscu mapy

6. System meczowy

Format MR12 (12 rund na połowę, łącznie 24 rundy)
Wsparcie dla dogrywek w przypadku remisu
Automatyczne restarty serwera przed rozpoczęciem właściwego meczu
Wyraźne oznaczenie rozpoczęcia meczu (komunikat "LIVE")

7. Tryb testowy

Możliwość testowania wtyczki bez konieczności posiadania 10 graczy
Automatyczne dodawanie botów do obu drużyn
Symulacja wygranej rundy nożowej
Łatwe zarządzanie procesem testowym


Komendy administratora
Komenda / Alternatywna forma
Opis
!pugstart / css_pugstart - Rozpoczyna tryb PUG z udziałem 10 graczy
!pugtest / css_pugtest - Uruchamia tryb testowy wtyczki z wykorzystaniem botów
!endtest / css_endtest - Kończy tryb testowy i usuwa wszystkie boty z serwera
Komendy dla graczy
Komenda / Alternatywna forma
Opis
!hello / css_hello - Testowa komenda sprawdzająca działanie wtyczki
!ct / css_ct - Wybór strony CT po wygraniu rundy nożowej
!t / css_t - Wybór strony T po wygraniu rundy nożowej
!knifewin / css_knifewin = Symulacja wygranej rundy nożowej (tylko w trybie testowym)

Przebieg rozgrywki
1. Inicjalizacja PUG
Po uruchomieniu komendy !pugstart, wtyczka:

Sprawdza, czy na serwerze znajduje się dokładnie 10 graczy
Jeśli warunek jest spełniony, losowo przydziela graczy do drużyn CT i T (po 5 w każdej)
Ustawia format MR12 (24 rundy łącznie)
Konfiguruje wsparcie dla dogrywek
Powiadamia graczy o rozpoczęciu trybu PUG

2. Runda nożowa
Po inicjalizacji PUG, automatycznie rozpoczyna się runda nożowa:

Wszystkie bronie są usuwane, a gracze otrzymują wyłącznie noże
Gracze otrzymują kamizelkę kuloodporną bez hełmu
Czas rundy jest ustawiony na 1.92 minuty
Zakupy są wyłączone
Na czacie pojawiają się komunikaty informujące o rundzie nożowej
Po zakończeniu rundy, zwycięska drużyna zyskuje prawo wyboru strony startowej

3. Runda rozgrzewkowa
Po zakończeniu rundy nożowej, rozpoczyna się 5-minutowa runda rozgrzewkowa:

Gracze otrzymują pełne fundusze (16000$)
Mogą kupować bronie w dowolnym miejscu mapy
Odradzanie się po śmierci jest włączone
Zwycięska drużyna z rundy nożowej może wybrać stronę startową używając komendy !ct lub !t

4. Rozpoczęcie meczu
Po wyborze stron przez zwycięską drużynę z rundy nożowej:

Serwer wykonuje 3 restarty dla zapewnienia poprawnego stanu gry
Stosowane są standardowe ustawienia turniejowe:

800$ na start
Standardowe strefy zakupów
Standardowy czas na zakupy (20 sekund)
Brak odradzania się po śmierci


Wyświetlany jest wyraźny komunikat "LIVE" oznaczający rozpoczęcie właściwego meczu
