using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server;
using Server.Events;
using Server.Network;

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

            if (Data.Started)
            {
                if (client.Player.HasItem(446) > 0)
                {
                    client.Player.TakeItem(446, client.Player.HasItem(446));
                }
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

            if (attackerPlayerData.Team != defenderPlayerData.Team)
            {
                attackerPlayerData.Score += 1;
                defenderPlayerData.Score -= 0.25;

                Messenger.PlayerMsg(attacker, $"You hit {defender.Player.DisplayName}!", Text.BrightGreen);
                Messenger.PlayerMsg(defender, $"You were hit by {attacker.Player.DisplayName}!", Text.BrightRed);

                WarpPlayerToTeamSpawn(defender);
            }
        }
    }
}
