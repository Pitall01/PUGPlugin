using System;
using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;

namespace PUGPlugin
{
    public class PUGPlugin : BasePlugin
    {
        public override string ModuleName => "PUG Plugin";
        public override string ModuleVersion => "1.0.2";
        public override string ModuleAuthor => "YourName";

        // Zmienne do zarządzania stanem gry
        private List<CCSPlayerController> _pugPlayers = new();
        private List<CCSPlayerController> _ctPlayers = new();
        private List<CCSPlayerController> _tPlayers = new();
        private bool _isPugStarted; // Flaga oznaczająca, że PUG został uruchomiony
        private bool _isKnifeRoundInProgress;
        private bool _isWarmupRoundInProgress;
        private CsTeam _knifeRoundWinner = CsTeam.None;
        private bool _isTestMode = false; // Flaga trybu testowego

        // Lista zmiennych serwera do konfiguracji rundy nożowej
        private Dictionary<string, string> _knifeRoundSettings = new Dictionary<string, string>
        {
            { "mp_weapons_allow_map_placed", "0" },
            { "mp_give_player_c4", "0" },
            { "mp_free_armor", "1" },             // 1 = tylko kamizelka bez hełmu
            { "mp_ct_default_secondary", "" },
            { "mp_ct_default_primary", "" },
            { "mp_t_default_secondary", "" },
            { "mp_t_default_primary", "" },
            { "mp_buy_anywhere", "0" },
            { "mp_buytime", "0" },
            { "mp_startmoney", "0" },
            { "mp_weapons_glow_on_ground", "0" }, // Wyłącz podświetlenie broni na ziemi
            { "mp_death_drop_gun", "0" },         // Wyłącz wypadanie broni po śmierci
            { "mp_solid_teammates", "1" },        // Włącz kolizję między graczami
            { "mp_weapons_allow_zeus", "0" },     // Wyłącz zakup Zeus'a
            { "mp_drop_knife_enable", "0" },      // Wyłącz możliwość upuszczania noża
            { "mp_roundtime", "1.92" },           // Dokładnie 1.92 minuty dla rundy nożowej
            { "mp_roundtime_defuse", "1.92" },    // Dokładnie 1.92 minuty dla map z bombą
            { "mp_freezetime", "15" }             // 15 sekund zamrożenia na początku rundy
        };

        // Lista zmiennych serwera dla rundy rozgrzewkowej
        private Dictionary<string, string> _warmupSettings = new Dictionary<string, string>
        {
            { "mp_weapons_allow_map_placed", "1" },
            { "mp_give_player_c4", "1" },
            { "mp_free_armor", "1" },             // Darmowa pełna zbroja
            { "mp_ct_default_secondary", "weapon_hkp2000" },
            { "mp_ct_default_primary", "" },
            { "mp_t_default_secondary", "weapon_glock" },
            { "mp_t_default_primary", "" },
            { "mp_buy_anywhere", "1" },           // Możliwość kupowania wszędzie
            { "mp_buytime", "999" },              // Nieskończony czas na zakupy
            { "mp_startmoney", "16000" },         // Maksymalne pieniądze
            { "mp_weapons_glow_on_ground", "1" },
            { "mp_death_drop_gun", "1" },
            { "mp_solid_teammates", "0" },
            { "mp_weapons_allow_zeus", "1" },
            { "mp_drop_knife_enable", "1" },
            { "mp_roundtime", "5" },              // 5 minut na rundę rozgrzewkową
            { "mp_roundtime_defuse", "5" },       // 5 minut na rundę rozgrzewkową
            { "mp_death_drop_grenade", "1" },     // Wypadanie granatów po śmierci
            { "mp_respawn_on_death_t", "1" },     // Respawn po śmierci dla T
            { "mp_respawn_on_death_ct", "1" },    // Respawn po śmierci dla CT
            { "mp_freezetime", "5" }              // Krótszy freezetime dla rozgrzewki
        };

        // Lista zmiennych serwera do przywrócenia po wyborze stron (tryb competitive)
        private Dictionary<string, string> _competitiveSettings = new Dictionary<string, string>
        {
            { "mp_weapons_allow_map_placed", "1" },
            { "mp_give_player_c4", "1" },
            { "mp_free_armor", "0" },
            { "mp_ct_default_secondary", "weapon_hkp2000" },
            { "mp_ct_default_primary", "" },
            { "mp_t_default_secondary", "weapon_glock" },
            { "mp_t_default_primary", "" },
            { "mp_buy_anywhere", "0" },
            { "mp_buytime", "20" },
            { "mp_startmoney", "800" },
            { "mp_weapons_glow_on_ground", "1" },
            { "mp_death_drop_gun", "1" },
            { "mp_solid_teammates", "0" },
            { "mp_weapons_allow_zeus", "1" },
            { "mp_drop_knife_enable", "1" },
            { "mp_roundtime", "1.92" },           // Standardowy czas rundy competitive
            { "mp_roundtime_defuse", "1.92" },    // Standardowy czas rundy competitive
            { "mp_death_drop_grenade", "1" },
            { "mp_respawn_on_death_t", "0" },     // Wyłącz respawn po śmierci
            { "mp_respawn_on_death_ct", "0" },    // Wyłącz respawn po śmierci
            { "mp_freezetime", "15" }             // 15 sekund zamrożenia na początku rundy dla meczu
        };

        public override void Load(bool hotReload)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("PUG Plugin is loading!");
            Console.WriteLine("========================================");
        }

        // Testowa komenda Hello
        [ConsoleCommand("css_hello", "Test command")]
        [ConsoleCommand("!hello", "Test command")]
        public void OnCommandHello(CCSPlayerController? player, CommandInfo command)
        {
            Console.WriteLine("Hello command executed!");
            player?.PrintToChat("[PUG] Test pomyślny.");
        }

        // Komenda do rozpoczęcia PUG
        [ConsoleCommand("css_pugstart", "Start PUG mode")]
        [ConsoleCommand("!pugstart", "Start PUG mode")]
        public void OnCommandPugStart(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;

            var players = Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot).ToList();
            if (players.Count != 10)
            {
                player.PrintToChat($"[PUG] Wymaganych jest 10 graczy. Aktualnie jest {players.Count}.");
                return;
            }

            StartPUG(players);
        }

        // Komenda do testowania funkcjonalności PUG
        [ConsoleCommand("css_pugtest", "Test PUG mode with bots")]
        [ConsoleCommand("!pugtest", "Test PUG mode with bots")]
        public void OnCommandPugTest(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            
            player.PrintToChat("[PUG] Rozpoczynanie trybu testowego PUG...");
            
            // Ustaw flagę trybu testowego
            _isTestMode = true;
            
            // Ustawienia botów dla trybu testowego
            Server.ExecuteCommand("bot_difficulty 1"); // Łatwy poziom botów
            Server.ExecuteCommand("bot_dont_shoot 1"); // Boty nie strzelają
            
            // Dodaj boty do CT
            for (int i = 0; i < 5; i++)
            {
                Server.ExecuteCommand("bot_add_ct");
            }
            
            // Dodaj boty do T
            for (int i = 0; i < 4; i++)
            {
                Server.ExecuteCommand("bot_add_t");
            }
            
            // Daj czas na dodanie botów
            AddTimer(2.0f, () =>
            {
                // Pobierz wszystkich graczy, włącznie z botami
                var allPlayers = Utilities.GetPlayers().Where(p => p.IsValid).ToList();
                
                // Upewnij się, że mamy 10 graczy (włącznie z botami)
                if (allPlayers.Count < 10)
                {
                    player.PrintToChat($"[PUG] Nie można dodać wystarczającej liczby botów. Obecna liczba: {allPlayers.Count}");
                    return;
                }
                
                player.PrintToChat("[PUG] Dodano boty. Rozpoczynam tryb testowy PUG...");
                player.PrintToChat("[PUG] Instrukcja trybu testowego:");
                player.PrintToChat("[PUG] 1. Najpierw rozpocznie się runda nożowa");
                player.PrintToChat("[PUG] 2. Użyj !knifewin aby określić zwycięzcę rundy nożowej");
                player.PrintToChat("[PUG] 3. Nastąpi runda rozgrzewkowa 5 minut");
                player.PrintToChat("[PUG] 4. Użyj !ct lub !t aby wybrać stronę");
                player.PrintToChat("[PUG] 5. Użyj !endtest aby zakończyć tryb testowy");
                
                // Upewnij się, że prawdziwy gracz jest kapitanem jednej z drużyn
                List<CCSPlayerController> testPlayers = new List<CCSPlayerController>();
                testPlayers.Add(player); // Dodaj prawdziwego gracza jako pierwszego
                
                // Dodaj resztę botów
                testPlayers.AddRange(allPlayers.Where(p => p.IsBot && !testPlayers.Contains(p)).Take(9));
                
                StartPUG(testPlayers);
            });
        }

        // Komenda do wyboru CT po rundzie nożowej
        [ConsoleCommand("css_ct", "Select CT side after knife round")]
        [ConsoleCommand("!ct", "Select CT side after knife round")]
        public void OnCommandCt(CCSPlayerController? player, CommandInfo command)
        {
            HandleSideSelection(player, CsTeam.CounterTerrorist);
        }

        // Komenda do wyboru T po rundzie nożowej
        [ConsoleCommand("css_t", "Select T side after knife round")]
        [ConsoleCommand("!t", "Select T side after knife round")]
        public void OnCommandT(CCSPlayerController? player, CommandInfo command)
        {
            HandleSideSelection(player, CsTeam.Terrorist);
        }

        // Event - początek rundy
        [GameEventHandler]
        public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            if (_isKnifeRoundInProgress)
            {
                // Specjalne ustawienia dla rundy nożowej
                Server.PrintToChatAll("[PUG] Runda nożowa: tylko noże są dozwolone!");
                
                // Dodatkowe komendy, aby upewnić się, że każdy ma tylko nóż i kamizelkę bez hełmu
                AddTimer(0.1f, () => {
                    Server.ExecuteCommand("mp_remove_all_weapons 1");
                    Server.ExecuteCommand("mp_give_weapon_to_all weapon_knife");
                    Server.ExecuteCommand("mp_free_armor 1");
                    
                    // Wyświetlenie komunikatu "[PUG] Nożówka" 4 razy na zielono
                    for (int i = 0; i < 4; i++)
                    {
                        int index = i; // Zapisujemy indeks w zmiennej lokalnej dla lambda
                        AddTimer(index * 0.5f, () => {
                            Server.PrintToChatAll(" \u0004[PUG] Nożówka");
                        });
                    }
                });
                
                // W trybie testowym, wyświetl instrukcje dla testowania rundy nożowej
                if (_isTestMode)
                {
                    var realPlayer = _pugPlayers.FirstOrDefault(p => !p.IsBot);
                    if (realPlayer != null)
                    {
                        realPlayer.PrintToChat("[PUG] Tryb testowy: Runda nożowa rozpoczęta.");
                        realPlayer.PrintToChat("[PUG] Użyj komendy !knifewin aby symulować wygraną rundy nożowej.");
                    }
                }
            }
            return HookResult.Continue;
        }

        // Event - koniec rundy
        [GameEventHandler]
        public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            if (_isKnifeRoundInProgress)
            {
                // W trybie testowym, pozwól na ręczne wybieranie zwycięzcy rundy nożowej
                if (!_isTestMode)
                {
                    // @event.Winner zawiera indeks drużyny (3 = CT, 2 = T)
                    _knifeRoundWinner = @event.Winner == 3 ? CsTeam.CounterTerrorist : CsTeam.Terrorist;
                    Server.PrintToChatAll($"[PUG] Runda nożowa zakończona. Zwycięska drużyna: {(_knifeRoundWinner == CsTeam.Terrorist ? "T" : "CT")}");
                    Server.PrintToChatAll("[PUG] Rozpoczyna się runda rozgrzewkowa (5 minut).");
                    Server.PrintToChatAll("[PUG] Zwycięska drużyna może użyć !ct lub !t, aby wybrać stronę.");
                    
                    // Rozpocznij rundę rozgrzewkową
                    StartWarmupRound();
                }
                else
                {
                    // W trybie testowym przypominamy o możliwości ręcznego wyboru zwycięzcy
                    var realPlayer = _pugPlayers.FirstOrDefault(p => !p.IsBot);
                    if (realPlayer != null)
                    {
                        realPlayer.PrintToChat("[PUG] Tryb testowy: Runda zakończona, ale musisz użyć !knifewin aby określić zwycięzcę.");
                    }
                }
            }
            return HookResult.Continue;
        }

        // Metoda rozpoczynająca PUG
        private void StartPUG(List<CCSPlayerController> players)
        {
            _pugPlayers.Clear();
            _pugPlayers.AddRange(players);
            _ctPlayers.Clear();
            _tPlayers.Clear();
            _isPugStarted = true;
            _isKnifeRoundInProgress = true;
            _isWarmupRoundInProgress = false;
            _knifeRoundWinner = CsTeam.None;

            // W trybie testowym, upewnij się, że prawdziwy gracz jest w jednej z drużyn
            if (_isTestMode)
            {
                var realPlayer = players.FirstOrDefault(p => !p.IsBot);
                if (realPlayer != null)
                {
                    // Losowo przydzielaj gracza do CT lub T
                    bool assignToT = new Random().Next(2) == 0;
                    
                    if (assignToT)
                    {
                        // Przydziel prawdziwego gracza do T
                        _tPlayers.Add(realPlayer);
                        // Przydziel 4 boty do T
                        _tPlayers.AddRange(players.Where(p => p.IsBot).Take(4));
                        // Przydziel pozostałe 5 botów do CT
                        _ctPlayers.AddRange(players.Where(p => p.IsBot && !_tPlayers.Contains(p)).Take(5));
                    }
                    else
                    {
                        // Przydziel prawdziwego gracza do CT
                        _ctPlayers.Add(realPlayer);
                        // Przydziel 4 boty do CT
                        _ctPlayers.AddRange(players.Where(p => p.IsBot).Take(4));
                        // Przydziel pozostałe 5 botów do T
                        _tPlayers.AddRange(players.Where(p => p.IsBot && !_ctPlayers.Contains(p)).Take(5));
                    }
                }
            }
            else
            {
                // Standardowy tryb - losowe przydzielanie graczy
                var shuffledPlayers = players.OrderBy(x => Guid.NewGuid()).ToList();
                _ctPlayers = shuffledPlayers.Take(5).ToList();
                _tPlayers = shuffledPlayers.Skip(5).Take(5).ToList();
            }

            foreach (var player in _ctPlayers)
            {
                player.SwitchTeam(CsTeam.CounterTerrorist);
            }

            foreach (var player in _tPlayers)
            {
                player.SwitchTeam(CsTeam.Terrorist);
            }

            Server.PrintToChatAll("[PUG] Rozpoczęto tryb PUG!");
            if (_isTestMode)
            {
                Server.PrintToChatAll("[PUG] Tryb testowy aktywny.");
            }
            else
            {
                Server.PrintToChatAll("[PUG] Gracze zostali rozlosowani.");
            }

            SetMR12Mode();
            StartKnifeRound();
        }

        // Ustawienie trybu MR12
        private void SetMR12Mode()
        {
            Server.ExecuteCommand("mp_maxrounds 24");
            Server.ExecuteCommand("mp_overtime_enable 1");
        }

        // Rozpoczęcie rundy nożowej
        private void StartKnifeRound()
        {
            _isKnifeRoundInProgress = true;
            _isWarmupRoundInProgress = false;
            Server.PrintToChatAll("[PUG] Rozpoczęto rundę nożową!");
            
            // Zastosuj ustawienia rundy nożowej
            foreach (var setting in _knifeRoundSettings)
            {
                Server.ExecuteCommand($"{setting.Key} {setting.Value}");
            }
            
            // Dodatkowe komendy dla zapewnienia, że nikt nie ma broni
            Server.ExecuteCommand("mp_warmup_end");       // Zakończ rozgrzewkę jeśli jest aktywna
            Server.ExecuteCommand("mp_remove_all_weapons 1"); // Usuń wszystkie bronie od graczy
            Server.ExecuteCommand("mp_give_weapon_to_all weapon_knife"); // Daj wszystkim nóż
            
            // Zastosuj ponownie, aby upewnić się, że ustawienia są prawidłowe
            Server.ExecuteCommand("mp_free_armor 1");     // Tylko kamizelka bez hełmu
            Server.ExecuteCommand("mp_ct_default_secondary \"\"");
            Server.ExecuteCommand("mp_ct_default_primary \"\"");
            Server.ExecuteCommand("mp_t_default_secondary \"\"");
            Server.ExecuteCommand("mp_t_default_primary \"\"");
            
            // Restart rundy
            Server.ExecuteCommand("mp_restartgame 1");
        }

        // Rozpoczęcie rundy rozgrzewkowej
        private void StartWarmupRound()
        {
            _isKnifeRoundInProgress = false;
            _isWarmupRoundInProgress = true;
            Server.PrintToChatAll("[PUG] Rozpoczęto rundę rozgrzewkową (5 minut)!");
            
            // Zastosuj ustawienia rundy rozgrzewkowej
            foreach (var setting in _warmupSettings)
            {
                Server.ExecuteCommand($"{setting.Key} {setting.Value}");
            }
            
            // Restart rundy z nowym ustawieniami
            Server.ExecuteCommand("mp_restartgame 1");
        }

        // Komenda do symulacji wygranej rundy nożowej w trybie testowym
        [ConsoleCommand("css_knifewin", "Simulate a knife round win in test mode")]
        [ConsoleCommand("!knifewin", "Simulate a knife round win in test mode")]
        public void OnCommandKnifeWin(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            
            if (!_isTestMode || !_isKnifeRoundInProgress)
            {
                player.PrintToChat("[PUG] Ta komenda działa tylko w trybie testowym podczas rundy nożowej.");
                return;
            }
            
            // Sprawdź, w której drużynie jest prawdziwy gracz
            CsTeam realPlayerTeam = (CsTeam)player.TeamNum;
            _knifeRoundWinner = realPlayerTeam;
            
            // Powiadom o wygranej rundy nożowej
            Server.PrintToChatAll($"[PUG] Symulowana wygrana rundy nożowej. Zwycięska drużyna: {(_knifeRoundWinner == CsTeam.Terrorist ? "T" : "CT")}");
            Server.PrintToChatAll("[PUG] Rozpoczyna się runda rozgrzewkowa (5 minut).");
            Server.PrintToChatAll("[PUG] Zwycięska drużyna może użyć !ct lub !t, aby wybrać stronę.");
            
            // Rozpocznij rundę rozgrzewkową
            StartWarmupRound();
        }
        
        // Komenda do zakończenia trybu testowego
        [ConsoleCommand("css_endtest", "End test mode")]
        [ConsoleCommand("!endtest", "End test mode")]
        public void OnCommandEndTest(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;
            
            if (!_isTestMode)
            {
                player.PrintToChat("[PUG] Nie jesteś w trybie testowym.");
                return;
            }
            
            player.PrintToChat("[PUG] Kończenie trybu testowego...");
            _isTestMode = false;
            
            // Usuń wszystkie boty
            Server.ExecuteCommand("bot_kick all");
            
            // Przywróć ustawienia botów
            Server.ExecuteCommand("bot_difficulty 2");
            Server.ExecuteCommand("bot_dont_shoot 0");
            
            player.PrintToChat("[PUG] Tryb testowy zakończony. Wszystkie boty zostały usunięte.");
        }
        
        // Obsługa wyboru strony po rundzie nożowej
        private void HandleSideSelection(CCSPlayerController? player, CsTeam selectedSide)
        {
            if (_isTestMode)
            {
                if (_knifeRoundWinner == CsTeam.None)
                {
                    player?.PrintToChat("[PUG] Musisz najpierw użyć !knifewin, aby określić zwycięzcę rundy nożowej.");
                    return;
                }
            }
            else if (_knifeRoundWinner == CsTeam.None)
            {
                player?.PrintToChat("[PUG] Nie można wybrać strony w tym momencie.");
                return;
            }

            if (!_isWarmupRoundInProgress)
            {
                player?.PrintToChat("[PUG] Wybór stron jest możliwy tylko podczas rundy rozgrzewkowej.");
                return;
            }

            // W trybie testowym, pozwól wybierać stronę nawet jeśli nie jesteś kapitanem
            if (!_isTestMode)
            {
                if (player == null || (!_ctPlayers.Contains(player) && !_tPlayers.Contains(player))) 
                {
                    player?.PrintToChat("[PUG] Tylko gracze uczestniczący w PUG mogą wybierać strony.");
                    return;
                }

                // Sprawdź czy gracz należy do zwycięskiej drużyny
                bool isPlayerInWinningTeam = _knifeRoundWinner == CsTeam.CounterTerrorist
                    ? _ctPlayers.Contains(player)
                    : _tPlayers.Contains(player);

                if (!isPlayerInWinningTeam)
                {
                    player.PrintToChat("[PUG] Tylko zwycięska drużyna może wybrać stronę.");
                    return;
                }
            }
            
            if (selectedSide == CsTeam.CounterTerrorist)
            {
                // Jeśli wybrano CT, a zwycięzcy są już CT, nic nie zmieniamy
                // Jeśli wybrano CT, a zwycięzcy są T, zamieniamy drużyny
                if (_knifeRoundWinner == CsTeam.Terrorist)
                {
                    (_ctPlayers, _tPlayers) = (_tPlayers, _ctPlayers);
                }
            }
            else // Wybrano T
            {
                // Jeśli wybrano T, a zwycięzcy są już T, nic nie zmieniamy
                // Jeśli wybrano T, a zwycięzcy są CT, zamieniamy drużyny
                if (_knifeRoundWinner == CsTeam.CounterTerrorist)
                {
                    (_ctPlayers, _tPlayers) = (_tPlayers, _ctPlayers);
                }
            }

            // Przełącz graczy do odpowiednich drużyn
            foreach (var p in _ctPlayers)
            {
                p.SwitchTeam(CsTeam.CounterTerrorist);
            }

            foreach (var p in _tPlayers)
            {
                p.SwitchTeam(CsTeam.Terrorist);
            }

            Server.PrintToChatAll($"[PUG] Drużyny wybrane. Zwycięska drużyna wybrała stronę {(selectedSide == CsTeam.CounterTerrorist ? "CT" : "T")}.");
            _isWarmupRoundInProgress = false;
            
            // Przywróć ustawienia competitive
            foreach (var setting in _competitiveSettings)
            {
                Server.ExecuteCommand($"{setting.Key} {setting.Value}");
            }
            
            // Powiadom o przygotowaniu do LIVE
            Server.PrintToChatAll("[PUG] Przygotowanie do rozpoczęcia meczu...");
            
            // Wykonanie 3 restartów gry przed rozpoczęciem właściwego meczu
            AddTimer(0.5f, () => {
                Server.PrintToChatAll("[PUG] Restart 1 z 3...");
                Server.ExecuteCommand("mp_restartgame 1");
                
                AddTimer(1.5f, () => {
                    Server.PrintToChatAll("[PUG] Restart 2 z 3...");
                    Server.ExecuteCommand("mp_restartgame 1");
                    
                    AddTimer(1.5f, () => {
                        Server.PrintToChatAll("[PUG] Restart 3 z 3...");
                        Server.ExecuteCommand("mp_restartgame 3"); // Ostatni restart z dłuższym opóźnieniem
                        
                        // Wyświetlenie komunikatu "[PUG] LIVE" 5 razy na czerwono
                        // Dodajemy opóźnienie, aby komunikaty pojawiły się po ostatnim restarcie
                        AddTimer(3.5f, () => {
                            for (int i = 0; i < 5; i++)
                            {
                                int index = i; // Zapisujemy indeks w zmiennej lokalnej dla lambda
                                AddTimer(index * 0.5f, () => {
                                    // Wyślij czerwony tekst bezpośrednio
                                    Server.PrintToChatAll(" \u0007[PUG] LIVE");
                                });
                            }
                        });
                    });
                });
            });
            
            // W trybie testowym, informujemy o zakończeniu test-case'a
            if (_isTestMode)
            {
                Server.PrintToChatAll("[PUG] Test zakończony pomyślnie. Rozpoczęcie właściwego meczu.");
                // Nie resetujemy flagi trybu testowego, aby można było kontynuować testowanie
            }
        }
    }
}