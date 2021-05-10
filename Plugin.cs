using Rocket.Core.Plugins;
using System;
using System.Collections.Generic;
using Logger = Rocket.Core.Logging.Logger;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Rocket.Unturned.Player;
using Rocket.API.Collections;
using Rocket.Unturned.Chat;
using System.Collections;
using Rocket.Core;
using Rocket.API;
using Rocket.Unturned.Events;
using SDG.Unturned;
using Steamworks;

namespace SDEventKoH
{
    public class Plugin : RocketPlugin<Config>
    {
        public static Plugin Instance;
        public static int timer;
        public static Loc Location;
        public static bool isGame = false;
        public static Coroutine Rutina = null;
        public static List<GamePlayer> Players;

        public class GamePlayer
        {
            public UnturnedPlayer player;
            public int timer;
            public Coroutine personalroutine;
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList
                {
                    {"waitersec", "[KoH] King of Hill will start in {1} in {0} seconds"},
                    {"waitermin", "[KoH] King of Hill will start in {1} in {0} minutes"},
                    {"startgame", "[KoH] King of Hill started in {0}!"},
                    {"gamesec", "[KoH] King of Hill finishing in {0} seconds!"},
                    {"noonewin", "[KoH] No one winned in KoH event!"},
                    {"win", "[KoH] {0} win in KoH and get '{1}' reward!"},
                    {"noplayers", "[KoH] Event not started. Need more players!"},
                    {"noone", "[KoH] No one in zone!"},
                    {"inradius", "{0} ({1}s)"},
                    {"timeleft", "{0}s left"},
                    {"meters", "{0}m"},
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            Logger.Log("------------------------------------------------------------", System.ConsoleColor.Blue);
            Logger.Log("|                                                          |", System.ConsoleColor.Blue);
            Logger.Log("|                       Order Plugin                       |", System.ConsoleColor.Blue);
            Logger.Log("|                   SodaDevs: Event KoH                    |", System.ConsoleColor.Blue);
            Logger.Log("|                        RocketMod                         |", System.ConsoleColor.Blue);
            Logger.Log("|                                                          |", System.ConsoleColor.Blue);
            Logger.Log("------------------------------------------------------------", System.ConsoleColor.Blue);
            Logger.Log("Version: " + Assembly.GetName().Version, System.ConsoleColor.Blue);
            UnturnedPlayerEvents.OnPlayerUpdatePosition += OnPos;
            UnturnedPlayerEvents.OnPlayerDeath += OnDeath;
            StartWaiter();
            
        }

        private void OnDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            if (Players.Exists(x => x.player == player) && (Vector3.Distance(player.Position, Location.pos) > Location.radius || player.Player.life.isDead))
            {
                if (Players.Find(x => x.player == player).personalroutine != null) StopCoroutine(Players.Find(x => x.player == player).personalroutine);
                Players.RemoveAll(x => x.player == player);
            }
        }

        private void OnPos(UnturnedPlayer player, Vector3 position)
        {
            if (isGame)
            {
                if (!player.Player.life.isDead && !Players.Exists(x => x.player == player) && Vector3.Distance(player.Position, Location.pos) <= Location.radius)
                {
                    Players.Add(new GamePlayer
                    {
                        player = player,
                        timer = Instance.Configuration.Instance.CaptureTime,
                        personalroutine = null
                    });
                    Players.Find(x => x.player == player).personalroutine = StartCoroutine(Personal(player));
                }
                if (Players.Exists(x => x.player == player) && (Vector3.Distance(player.Position, Location.pos) > Location.radius || player.Player.life.isDead))
                {
                    if (Players.Find(x => x.player == player).personalroutine != null) StopCoroutine(Players.Find(x => x.player == player).personalroutine);
                    Players.RemoveAll(x => x.player == player);
                    UpdateUI();
                }
                if (Vector3.Distance(player.Position, Location.pos) <= Location.radius)
                    EffectManager.sendUIEffectText(43, Provider.findTransportConnection(player.CSteamID), true, "koh_pos","<Color=Green>" + Translate("meters", Math.Round(Vector3.Distance(player.Position, Location.pos),2)) + "</Color>");
                else
                    EffectManager.sendUIEffectText(43, Provider.findTransportConnection(player.CSteamID), true, "koh_pos", "<Color=Red>" + Translate("meters", Math.Round(Vector3.Distance(player.Position, Location.pos), 2)) + "</Color>");

            }
        }

        public void StartWaiter()
        {
            if (Instance.Configuration.Instance.Locations.Count <= 0) return;
            if (Rutina != null) StopCoroutine(Rutina);
            isGame = false;
            Players = new List<GamePlayer>();
            System.Random random = new System.Random();
            if (Provider.clients.Count > 0)
                foreach (SteamPlayer player in Provider.clients)
                    EffectManager.askEffectClearByID(Instance.Configuration.Instance.UI, Provider.findTransportConnection(player.playerID.steamID));
            timer = random.Next(Instance.Configuration.Instance.MinTime, Instance.Configuration.Instance.MaxTime);
            Location = Instance.Configuration.Instance.Locations[random.Next(0, Instance.Configuration.Instance.Locations.Count)];
            isGame = false;
            UnturnedChat.Say(Translate("waitersec", timer, Location.Name), Color.cyan);
            Rutina = StartCoroutine(Waiter());
        }

        IEnumerator Personal(UnturnedPlayer player)
        {
            while (Players.Exists(x => x.player == player) && Players.Find(x => x.player == player).timer > 0)
            {
                Players.Find(x => x.player == player).timer--;
                UpdateUI();
                yield return new WaitForSeconds(1f);
            }
            if (Players.Exists(x => x.player == player) && Players.Find(x => x.player == player).timer <= 0)
            {
                GameWin(Players.Find(x => x.player == player));
            }
        }

        private void UpdateUI()
        {
            string text = "";
            if (Players.Count <= 0) { text = Translate("noone"); }
            foreach (GamePlayer player in Players)
            {
                text = text + Translate("inradius", player.player.DisplayName, player.timer) + "\n";
            }
            if (Provider.clients.Count > 0)
                foreach (SteamPlayer player in Provider.clients)
                    EffectManager.sendUIEffectText(43, Provider.findTransportConnection(player.playerID.steamID), true, "koh_players", text);
        }

        IEnumerator Waiter()
        {
            while (timer > 0)
            {
                timer--;
                if (timer == 600)
                    UnturnedChat.Say(Translate("waitermin", 10, Location.Name), Color.cyan);
                if (timer == 300)
                    UnturnedChat.Say(Translate("waitermin", 5, Location.Name), Color.cyan);
                if (timer == 240)
                    UnturnedChat.Say(Translate("waitermin", 4, Location.Name), Color.cyan);
                if (timer == 180)
                    UnturnedChat.Say(Translate("waitermin", 3, Location.Name), Color.cyan);
                if (timer == 120)
                    UnturnedChat.Say(Translate("waitermin", 2, Location.Name), Color.cyan);
                if (timer == 60)
                    UnturnedChat.Say(Translate("waitermin", 1, Location.Name), Color.cyan);
                if (timer == 30)
                    UnturnedChat.Say(Translate("waitersec", 30, Location.Name), Color.cyan);
                if (timer == 10)
                    UnturnedChat.Say(Translate("waitersec", 10, Location.Name), Color.cyan);
                if (timer <= 5)
                    UnturnedChat.Say(Translate("waitersec", timer, Location.Name), Color.cyan);
                yield return new WaitForSeconds(1f); 
            }
            StartGame();
        }

        IEnumerator Gaming()
        {
            isGame = true;
            while (timer > 0)
            {
                timer--;
                if (timer <= 5)
                    UnturnedChat.Say(Translate("gamesec", timer), Color.cyan);
                if (Provider.clients.Count > 0)
                    foreach (SteamPlayer player in Provider.clients)
                        EffectManager.sendUIEffectText(43, Provider.findTransportConnection(player.playerID.steamID), true, "koh_timer", Translate("timeleft",timer));
                yield return new WaitForSeconds(1f);
            }
            if (Players.Count > 0)
            {
                int lowest = Instance.Configuration.Instance.CaptureTime;
                int index = -1;
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].timer >= lowest) continue;
                    lowest = Players[i].timer;
                    index = i;
                }
                GameWin(Players[index]);
            }
            else {
                UnturnedChat.Say(Translate("noonewin"), Color.cyan);
                StartWaiter();
            }
        }

        private void GameWin(GamePlayer gamePlayer)
        {
            
            if (Rutina != null) StopCoroutine(Rutina);
            System.Random random = new System.Random();
            int rew = random.Next(0, Instance.Configuration.Instance.Rewards.Count);
            Reward reward = Instance.Configuration.Instance.Rewards[rew];
            UnturnedChat.Say(Translate("win", gamePlayer.player.DisplayName, reward.Name), Color.cyan);
            foreach (string rs in reward.Rewards)
            {
                string nrs = rs.Replace("%playerid%", gamePlayer.player.CSteamID.m_SteamID.ToString());
                nrs = nrs.Replace("%PlayerId%", gamePlayer.player.CSteamID.m_SteamID.ToString());
                nrs = nrs.Replace("%PlayerID%", gamePlayer.player.CSteamID.m_SteamID.ToString());
                nrs = nrs.Replace("%playername%", gamePlayer.player.DisplayName);
                R.Commands.Execute(new ConsolePlayer(), nrs);
            }
            foreach (GamePlayer player in Players)
                StopCoroutine(player.personalroutine);
            StartWaiter();
        }

        public void StartGame()
        {
            try
            {
                if (Provider.clients.Count >= Instance.Configuration.Instance.MinPlayers)
                {
                    if (Rutina != null) StopCoroutine(Rutina);
                    UnturnedChat.Say(Translate("startgame", Location.Name), Color.cyan);
                    Players = new List<GamePlayer>();
                    timer = Instance.Configuration.Instance.EventTime;
                    EffectManager.sendUIEffect(Instance.Configuration.Instance.UI, 43, true);
                    Rutina = StartCoroutine(Gaming());
                }
                else
                {
                    UnturnedChat.Say(Translate("noplayer"), Color.cyan);
                    StartWaiter();
                }
            }
            catch
            {
                StartWaiter();
            }
        }
    }


    public class KoHComponent : UnturnedPlayerComponent
    {
        protected override void Load()
        {
            this.isAllowHelperEdit = false;
        }
        public bool isAllowHelperEdit;
    }
}
