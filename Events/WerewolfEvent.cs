using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Server;
using Server.Network;
using Server.Events;
using Server.Maps;
using System.Linq;
using Server.Combat;
using Server.Players;
using Server.Stories;

namespace Script.Events
{
    public class WerewolfEvent : AbstractEvent<WerewolfEvent.WerewolfData>
    {
        public static readonly int WerewolfCount = 2;

        public class UserInfo
        {
            public UserRole Role { get; set; }
            public bool IsDead { get; set; }

            public UserInfo()
            {
                this.Role = UserRole.Villager;
            }
        }

        public enum UserRole
        {
            Villager,
            Seer,
            Doctor,
            Werewolf
        }

        public enum GameState
        {
            WerewolfSelecting,
            DoctorSelecting,
            SeerSelecting,
            VillagersSelecting
        }

        public class WerewolfData : AbstractEventData
        {
            public Dictionary<string, UserInfo> Users { get; set; }

            public WerewolfData()
            {
                this.Users = new Dictionary<string, UserInfo>();
            }
        }

        public override string Identifier => "werewolf";

        public override string Name => "Werewolf";

        public override string IntroductionMessage => "...";

        public override void ConfigurePlayer(Client client)
        {
            base.ConfigurePlayer(client);

            if (Data.Started)
            {

            }
        }

        public override void DeconfigurePlayer(Client client)
        {
            base.DeconfigurePlayer(client);
        }

        public override void Start()
        {
            base.Start();

            SetupPlayers();
        }

        private void SetupPlayers()
        {
            foreach (var client in EventManager.GetRegisteredClients())
            {
                Data.Users.Add(client.Player.CharID, new UserInfo());
            }

            var candidates = Data.Users.Keys.ToList();


            // Select a seer
            var seerId = SelectUser(candidates);
            Data.Users[seerId].Role = UserRole.Seer;

            var doctorId = SelectUser(candidates);
            Data.Users[doctorId].Role = UserRole.Doctor;

            for (var i = 0; i < WerewolfCount; i++)
            {
                var werewolfId = SelectUser(candidates);
                Data.Users[werewolfId].Role = UserRole.Werewolf;               
            }
        }

        private string SelectUser(List<string> users)
        {
            var slot = Server.Math.Rand(0, users.Count);

            var user = users[slot];

            users.RemoveAt(slot);

            return user;
        }

        public override void End()
        {
            base.End();
        }

        protected override List<EventRanking> DetermineRankings()
        {
            var rankings = new List<EventRanking>();
            foreach (var client in EventManager.GetRegisteredClients())
            {

            }

            return rankings;
        }

        public override bool ProcessCommand(Client client, Command command, string joinedArgs) {
            switch (command[0]) {
                case "/werewolfrole": 
                    Messenger.PlayerMsg(client, $"You are a {Data.Users[client.Player.CharID].Role}!", Text.BrightGreen);
                    return true;
                case "/werewolfchoose":
                    return true;
                default:
                    return false;
            }
        }
    }
}

