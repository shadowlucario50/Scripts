using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Server;
using Server.Events;
using Server.Network;
using Server.Players;

namespace Script
{
    public class Paintball : AbstractEvent<Paintball.PaintballData, Paintball.PlayerData>
    {
        public static readonly int ArenaMap = 381;

        public override string Identifier => "paintball";
        public override string Name => "Paintball";
        public override string IntroductionMessage => "Splash your enemies!";

        public enum Team
        {
            Red,
            Blue,
            Green,
            Yellow
        }

        public class PaintballData : AbstractEventData<PlayerData>
        {
        }

        public class PlayerData
        {
            public Team Team { get; set; }
            public double Score { get; set; }
        }

        public override void Start()
        {
            base.Start();

            var registeredClients = EventManager.GetRegisteredClients().ToList();

            int availableTeams = 2;

            if (registeredClients.Count >= 12)
            {
                availableTeams++;
            }
            if (registeredClients.Count >= 16)
            {
                availableTeams++;
            }

            var currentTeam = 0;
            foreach (var client in registeredClients)
            {
                var playerData = Data.ExtendPlayer(client);
                playerData.Team = (Team)currentTeam;

                WarpPlayerToTeamSpawn(client);

                Messenger.PlayerMsg(client, $"You are on the {playerData.Team.ToString().ToLower()} team!", Text.BrightGreen);

                currentTeam++;
                if (currentTeam >= availableTeams)
                {
                    currentTeam = 0;
                }
            }
        }

        public override void ConfigurePlayer(Client client)
        {
            base.ConfigurePlayer(client);

            if (Data.Started)
            {
                var playerData = Data.ExtendPlayer(client);

                client.Player.Status = playerData.Team.ToString();
                Messenger.SendPlayerData(client);

                if (client.Player.HasItem(446) == 0)
                {
                    client.Player.GiveItem(446, 999);
                    Messenger.PlayerMsg(client, "You were given some snowballs!", Text.BrightGreen);
                }
            }
        }

        public override void DeconfigurePlayer(Client client)
        {
            base.DeconfigurePlayer(client);

            var playerData = Data.ExtendPlayer(client);

            client.Player.Status = "";
            Messenger.SendPlayerData(client);

            if (client.Player.HasItem(446) > 0)
            {
                client.Player.TakeItem(446, client.Player.HasItem(446));
            }
        }

        protected override List<EventRanking> DetermineRankings()
        {
            var rankings = new List<EventRanking>();

            foreach (var client in EventManager.GetRegisteredClients())
            {
                var playerData = Data.ExtendPlayer(client);

                rankings.Add(new EventRanking(client, (int)System.Math.Ceiling(playerData.Score)));
            }

            return rankings;
        }

        private void WarpPlayerToTeamSpawn(Client client)
        {
            var playerData = Data.ExtendPlayer(client);

            int x = 0;
            int y = 0;

            switch (playerData.Team)
            {
                case Team.Red:
                    x = 5;
                    y = 4;
                    break;
                case Team.Blue:
                    x = 45;
                    y = 46;
                    break;
                case Team.Green:
                    x = 45;
                    y = 4;
                    break;
                case Team.Yellow:
                    x = 5;
                    y = 46;
                    break;
            }

            Messenger.PlayerWarp(client, ArenaMap, x, y);
        }

        public override void OnMoveHitCharacter(Client attacker, Client defender)
        {
            base.OnMoveHitCharacter(attacker, defender);

            var attackerPlayerData = Data.ExtendPlayer(attacker);
            var defenderPlayerData = Data.ExtendPlayer(defender);

            if (attackerPlayerData.Team != defenderPlayerData.Team && !IsInSafeZone(defender))
            {
                attackerPlayerData.Score += 1;
                defenderPlayerData.Score -= 0.25;

                Messenger.PlayerMsg(attacker, $"You hit {defender.Player.DisplayName}!", Text.BrightGreen);
                Messenger.PlayerMsg(defender, $"You were hit by {attacker.Player.DisplayName}!", Text.BrightRed);

                WarpPlayerToTeamSpawn(defender);
            }
        }

        private bool IsInSafeZone(Client client)
        {
            var playerData = Data.ExtendPlayer(client);

            var bounds = new Rectangle();

            switch (playerData.Team)
            {
                case Team.Red:
                    bounds = new Rectangle(1, 1, 8, 6);
                    break;
                case Team.Blue:
                    bounds = new Rectangle(41, 43, 9, 7);
                    break;
                case Team.Green:
                    bounds = new Rectangle(41, 1, 9, 6);
                    break;
                case Team.Yellow:
                    bounds = new Rectangle(1, 43, 8, 7);
                    break;
            }

            return bounds.Contains(new Point(client.Player.X, client.Player.Y));
        }

        public override string HandoutReward(EventRanking eventRanking, int position)
        {
            base.HandoutReward(eventRanking, position);

            if (Ranks.IsAllowed(eventRanking.Client, Enums.Rank.Scripter))
            {
                switch (position)
                {
                    case 1:
                        {
                            eventRanking.Client.Player.GiveItem(133, 10);
                            return "10 event tokens";
                        }
                    case 2:
                        {
                            eventRanking.Client.Player.GiveItem(133, 5);
                            return "5 event tokens";
                        }
                    case 3:
                        {
                            eventRanking.Client.Player.GiveItem(133, 3);
                            return "3 event tokens";
                        }
                }
            }

            return "";
        }
    }
}
