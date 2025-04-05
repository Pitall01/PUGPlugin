# 🎮 PUG Plugin dla CounterStrikeSharp

<div align="center">
  
![Wersja](https://img.shields.io/badge/wersja-1.0.2-blue)
![CS2](https://img.shields.io/badge/gra-CS2-orange)
![CounterStrikeSharp](https://img.shields.io/badge/wymaga-CounterStrikeSharp-red)

**System zarządzania meczami PUG (Pick-Up Game) dla serwerów Counter-Strike 2**
</div>

## 📋 Spis treści

- [🔍 Przegląd](#-przegląd)
- [✨ Funkcje](#-funkcje)
- [⚙️ Instalacja](#️-instalacja)
- [🕹️ Komendy](#️-komendy)
- [🔄 Przebieg rozgrywki](#-przebieg-rozgrywki)
- [🧪 Tryb testowy](#-tryb-testowy)
- [📝 Konfiguracja](#-konfiguracja)
- [⚠️ Rozwiązywanie problemów](#️-rozwiązywanie-problemów)

## 🔍 Przegląd

PUG Plugin to zaawansowane rozwiązanie dla serwerów CS2, które automatyzuje organizację meczów w formacie ligowym/turniejowym. Wtyczka zarządza całym procesem - od wyboru drużyn, przez rundę nożową, aż po właściwy mecz w formacie MR12.


## ✨ Funkcje

- ✅ **Automatyczne wykrywanie graczy** - identyfikacja po Steam ID z pominięciem botów
- ✅ **Zarządzanie drużynami** - losowy podział 10 graczy na dwie 5-osobowe drużyny
- ✅ **Runda nożowa** - automatyczna konfiguracja specjalnej rundy nożowej do wyłonienia zwycięzcy
- ✅ **Wybór stron** - zwycięska drużyna z rundy nożowej wybiera stronę startową
- ✅ **Runda rozgrzewkowa** - 5-minutowa sesja rozgrzewkowa między rundą nożową a meczem
- ✅ **Format MR12** - standardowy format turniejowy (12 rund na połowę, 24 rundy łącznie)
- ✅ **Tryb testowy** - testowanie funkcjonalności wtyczki z wykorzystaniem botów

## ⚙️ Instalacja

### Wymagania wstępne
- Serwer Counter-Strike 2
- Zainstalowany i skonfigurowany CounterStrikeSharp
- Uprawnienia administratora serwera

### Krok po kroku

1. **Pobierz wtyczkę**


2. **Instalacja plików**
   - Skopiuj zawartość katalogu do:
     ```
     /game/csgo/addons/counterstrikesharp/plugins/
     ```
   - Upewnij się, że struktura katalogów wygląda następująco:
     ```
     /plugins/
       /PUGPlugin/
         PUGPlugin.dll
     ```

3. **Aktywacja wtyczki**
   - Zrestartuj serwer CS2, lub
   - Użyj komendy: `css_plugins_load PUGPlugin`

4. **Weryfikacja instalacji**
   - Wpisz w konsoli lub czacie: `!hello`
   - Prawidłowa odpowiedź: `[PUG] Test pomyślny.`

## 🕹️ Komendy

### Komendy administratora

| Komenda | Alternatywa | Opis |
|:-------:|:------------:|:-----|
| `!pugstart` | `css_pugstart` | Rozpoczyna tryb PUG (wymaga 10 graczy) |
| `!pugtest` | `css_pugtest` | Uruchamia tryb testowy z botami |
| `!endtest` | `css_endtest` | Kończy tryb testowy i usuwa boty |

### Komendy dla graczy

| Komenda | Alternatywa | Opis |
|:-------:|:------------:|:-----|
| `!hello` | `css_hello` | Komenda testowa |
| `!ct` | `css_ct` | Wybór strony CT po wygraniu rundy nożowej |
| `!t` | `css_t` | Wybór strony T po wygraniu rundy nożowej |
| `!knifewin` | `css_knifewin` | Symulacja wygranej rundy nożowej (tylko w trybie testowym) |

## 🔄 Przebieg rozgrywki


### 1️⃣ Inicjalizacja PUG

Po wywołaniu komendy `!pugstart`:
- Wtyczka weryfikuje obecność 10 graczy
- Przydziela losowo po 5 graczy do drużyn CT i T
- Konfiguruje format MR12
- Powiadamia graczy o rozpoczęciu PUG

### 2️⃣ Runda nożowa

- Automatyczne konfigurowanie serwera:
  - Tylko noże są dozwolone
  - Gracze otrzymują podstawową kamizelkę
  - Zakupy są wyłączone
- Czas rundy: 1.92 minuty
- Zwycięska drużyna zyskuje prawo wyboru strony

### 3️⃣ Runda rozgrzewkowa

Po zakończeniu rundy nożowej:
- Rozpoczyna się 5-minutowa rozgrzewka
- Gracze otrzymują maksymalne fundusze (16000$)
- Włączone zakupy w dowolnym miejscu mapy
- Włączone odradzanie się po śmierci
- Zwycięska drużyna wybiera stronę używając `!ct` lub `!t`

### 4️⃣ Właściwy mecz

Po wyborze stron:
- Serwer wykonuje 3 restarty dla zapewnienia poprawnego stanu gry
- Stosowane są standardowe ustawienia competitive (MR12)
- Wyświetlany jest wyraźny komunikat "LIVE" oznaczający rozpoczęcie meczu

## 🧪 Tryb testowy

Tryb testowy pozwala sprawdzić działanie wtyczki bez konieczności posiadania 10 graczy.

### Uruchomienie i obsługa

1. **Start trybu testowego**
   ```
   !pugtest
   ```
   - Wtyczka doda boty do obu drużyn
   - Ty (jako admin) zostaniesz przydzielony do jednej z drużyn

2. **Symulacja rundy nożowej**
   ```
   !knifewin
   ```
   - Symuluje wygraną rundy nożowej dla Twojej drużyny

3. **Wybór strony**
   ```
   !ct
   ```
   lub
   ```
   !t
   ```
   - Wybór preferowanej strony dla Twojej drużyny

4. **Zakończenie testu**
   ```
   !endtest
   ```
   - Usuwa wszystkie boty i przywraca normalne ustawienia

## 📝 Konfiguracja

### Zmienne serwerowe

Wtyczka zarządza trzema zestawami zmiennych serwerowych, stosowanych na różnych etapach:

<details>
<summary><b>🔪 Ustawienia rundy nożowej</b> (kliknij aby rozwinąć)</summary>

```
mp_weapons_allow_map_placed 0
mp_give_player_c4 0
mp_free_armor 1
mp_ct_default_secondary ""
mp_ct_default_primary ""
mp_t_default_secondary ""
mp_t_default_primary ""
mp_buy_anywhere 0
mp_buytime 0
mp_startmoney 0
mp_weapons_glow_on_ground 0
mp_death_drop_gun 0
mp_solid_teammates 1
mp_weapons_allow_zeus 0
mp_drop_knife_enable 0
mp_roundtime 1.92
mp_roundtime_defuse 1.92
mp_freezetime 15
```
</details>

<details>
<summary><b>🔥 Ustawienia rozgrzewki</b> (kliknij aby rozwinąć)</summary>

```
mp_weapons_allow_map_placed 1
mp_give_player_c4 1
mp_free_armor 1
mp_ct_default_secondary "weapon_hkp2000"
mp_ct_default_primary ""
mp_t_default_secondary "weapon_glock"
mp_t_default_primary ""
mp_buy_anywhere 1
mp_buytime 999
mp_startmoney 16000
mp_weapons_glow_on_ground 1
mp_death_drop_gun 1
mp_solid_teammates 0
mp_weapons_allow_zeus 1
mp_drop_knife_enable 1
mp_roundtime 5
mp_roundtime_defuse 5
mp_death_drop_grenade 1
mp_respawn_on_death_t 1
mp_respawn_on_death_ct 1
mp_freezetime 5
```
</details>

<details>
<summary><b>🏆 Ustawienia competitive</b> (kliknij aby rozwinąć)</summary>

```
mp_weapons_allow_map_placed 1
mp_give_player_c4 1
mp_free_armor 0
mp_ct_default_secondary "weapon_hkp2000"
mp_ct_default_primary ""
mp_t_default_secondary "weapon_glock"
mp_t_default_primary ""
mp_buy_anywhere 0
mp_buytime 20
mp_startmoney 800
mp_weapons_glow_on_ground 1
mp_death_drop_gun 1
mp_solid_teammates 0
mp_weapons_allow_zeus 1
mp_drop_knife_enable 1
mp_roundtime 1.92
mp_roundtime_defuse 1.92
mp_death_drop_grenade 1
mp_respawn_on_death_t 0
mp_respawn_on_death_ct 0
mp_freezetime 15
```
</details>


## ⚠️ Rozwiązywanie problemów

<details>
<summary><b>Komendy nie działają</b></summary>

- Sprawdź czy wtyczka jest załadowana (`css_plugins_list`)
- Upewnij się, że ścieżki instalacji są poprawne
- Przeładuj wtyczkę (`css_plugins_reload PUGPlugin`)
- Sprawdź logi serwera pod kątem błędów
</details>

<details>
<summary><b>Nie można rozpocząć PUG</b></summary>

- Upewnij się, że na serwerze jest dokładnie 10 graczy
- Sprawdź, czy żaden z graczy nie jest botem
- Spróbuj trybu testowego (`!pugtest`)
</details>

<details>
<summary><b>Problemy z rundą nożową</b></summary>

- Upewnij się, że wszystkie zmienne serwerowe są poprawnie ustawione
- Sprawdź, czy gracze mają tylko noże
- W trybie testowym użyj `!knifewin` aby przejść do następnego etapu
</details>

<details>
<summary><b>Wybór stron nie działa</b></summary>

- Upewnij się, że komendę `!ct` lub `!t` używa gracz ze zwycięskiej drużyny
- Sprawdź, czy runda nożowa została zakończona i aktywna jest rozgrzewka
- W trybie testowym najpierw użyj `!knifewin` przed wyborem strony
</details>


