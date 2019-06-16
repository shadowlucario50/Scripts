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
        public static readonly int WerewolfCount = 1;

        public class UserInfo
        {
            public UserRole Role { get; set; }
            public bool IsDead { get; set; }
            public string SelectedCharId { get; set; }

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
            public GameState GameState { get; set; }

            public WerewolfData()
            {
                this.Users = new Dictionary<string, UserInfo>();
                this.GameState = GameState.WerewolfSelecting;
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
                ApplyState(client);
            }
        }

        public override void DeconfigurePlayer(Client client)
        {
            base.DeconfigurePlayer(client);

            client.Player.Muted = false;
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

        public override bool ProcessCommand(Client client, Command command, string joinedArgs)
        {
            if (!Data.Started)
            {
                return false;
            }

            var userData = Data.Users[client.Player.CharID];

            switch (command[0])
            {
                case "/wrole":
                    Messenger.PlayerMsg(client, $"You are a {userData.Role}!", Text.BrightGreen);
                    return true;
                case "/wchoose":
                    var chosenClient = ClientManager.FindClient(joinedArgs);
                    if (chosenClient == null) {
                        Messenger.PlayerMsg(client, "Player is offline.", Text.BrightRed);
                        return true;
                    }

                    switch (Data.GameState)
                    {
                        case GameState.WerewolfSelecting:
                            {
                                if (userData.Role == UserRole.Werewolf)
                                {
                                    userData.SelectedCharId = chosenClient.Player.CharID;
                                    RoleAlertMessage(UserRole.Werewolf, $"{client.Player.DisplayName} chose {chosenClient.Player.DisplayName}!");
                                }

                                if (CanTransition())
                                {
                                    Transition(GameState.DoctorSelecting);
                                }
                            }
                            break;
                        case GameState.DoctorSelecting:
                            {
                                if (userData.Role == UserRole.Doctor)
                                {
                                    Messenger.PlayerMsg(client, "Decision made.", Text.BrightGreen);
                                }
                            }
                            break;
                    }
                    return true;
                case "/wstate":
                    Messenger.PlayerMsg(client, $"{Data.GameState}", Text.BrightGreen);
                    ApplyState(client);
                    return true;
                case "/w":
                    {
                        if (Data.Users[client.Player.CharID].Role == UserRole.Werewolf)
                        {
                            WerewolfMessage(client, joinedArgs);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                default:
                    return false;
            }
        }

        public void StoryMessage(Client client, string message)
        {
            var story = new Story(Guid.NewGuid().ToString());
            var segment = StoryBuilder.BuildStory();
            StoryBuilder.AppendSaySegment(segment, message, -1, 0, 0);
            segment.AppendToStory(story);
            StoryManager.PlayStory(client, story);
        }

        public void WerewolfMessage(Client client, string message)
        {
            foreach (var eventClient in EventManager.GetRegisteredClients().Where(x => Data.Users[x.Player.CharID].Role == UserRole.Werewolf))
            {
                Messenger.PlayerMsg(eventClient, $"[Werewolf] {client.Player.DisplayName}: {message}", Text.White);
            }
        }

        public void RoleAlertMessage(UserRole role, string message)
        {
            foreach (var eventClient in EventManager.GetRegisteredClients().Where(x => Data.Users[x.Player.CharID].Role == role))
            {
                Messenger.PlayerMsg(eventClient, $"[Werewolf] {message}", Text.White);
            }
        }

        private IEnumerable<Client> GetRoleClients(UserRole role)
        {
            foreach (var eventClient in EventManager.GetRegisteredClients().Where(x => Data.Users[x.Player.CharID].Role == role))
            {
                yield return eventClient;
            }
        }

        private bool CanTransition()
        {
            switch (Data.GameState)
            {
                case GameState.WerewolfSelecting:
                    return GetRoleClients(UserRole.Werewolf).Select(x => Data.Users[x.Player.CharID].SelectedCharId)
                                                            .Where(x => !string.IsNullOrEmpty(x))
                                                            .Distinct().Count() == 1;
            }

            return false;
        }

        private void Transition(GameState newState)
        {
            Data.GameState = newState;

            foreach (var eventClient in EventManager.GetRegisteredClients())
            {
                ApplyState(eventClient);
            }
        }

        private void ApplyState(Client client)
        {
            client.Player.Muted = false;

            switch (Data.GameState)
            {
                case GameState.WerewolfSelecting:
                    {
                        ApplyWerewolfSelectingState(client);
                    }
                    break;
                case GameState.DoctorSelecting:
                    {
                        ApplyDoctorSelectingState(client);
                    }
                    break;
            }
        }

        private void ApplyDoctorSelectingState(Client client)
        {
            client.Player.Muted = true;

            var story = new Story(Guid.NewGuid().ToString());
            var segment = StoryBuilder.BuildStory();
            StoryBuilder.AppendSaySegment(segment, "Night has fallen!", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "The doctor must now decide who to save.", -1, 0, 0);

            if (Data.Users[client.Player.CharID].Role == UserRole.Doctor)
            {
                StoryBuilder.AppendSaySegment(segment, "Make your decision with /wchoose.", -1, 0, 0);
            }

            StoryBuilder.AppendSaySegment(segment, "Everyone is muted while the doctor decides.", -1, 0, 0);

            segment.AppendToStory(story);
            StoryManager.PlayStory(client, story);
        }

        private void ApplyWerewolfSelectingState(Client client)
        {
            client.Player.Muted = true;

            var story = new Story(Guid.NewGuid().ToString());
            var segment = StoryBuilder.BuildStory();
            StoryBuilder.AppendSaySegment(segment, "Night has fallen!", -1, 0, 0);

            switch (Data.Users[client.Player.CharID].Role)
            {
                case UserRole.Werewolf:
                    {
                        foreach (var eventClient in EventManager.GetRegisteredClients().Where(x => Data.Users[x.Player.CharID].Role == UserRole.Werewolf))
                        {
                            StoryBuilder.AppendSaySegment(segment, $"{eventClient.Player.DisplayName} is a werewolf!", -1, 0, 0);
                        }
                        StoryBuilder.AppendSaySegment(segment, $"Choose a player to eat with /wchoose", -1, 0, 0);
                    }
                    break;
                case UserRole.Seer:
                    {
                        StoryBuilder.AppendSaySegment(segment, "You are a seer.", -1, 0, 0);
                        StoryBuilder.AppendSaySegment(segment, "Every night you will choose one player and learn about what they are.", -1, 0, 0);
                    }
                    break;
                case UserRole.Doctor:
                    {
                        StoryBuilder.AppendSaySegment(segment, "You are a doctor.", -1, 0, 0);
                        StoryBuilder.AppendSaySegment(segment, "Every night you will choose one player to protect from the werewolves.", -1, 0, 0);
                    }
                    break;
                case UserRole.Villager:
                    {
                        StoryBuilder.AppendSaySegment(segment, "You are a villager.", -1, 0, 0);
                        StoryBuilder.AppendSaySegment(segment, "Try not to get eaten!", -1, 0, 0);
                    }
                    break;
            }

            StoryBuilder.AppendSaySegment(segment, "Everyone is muted while werewolves decide.", -1, 0, 0);

            segment.AppendToStory(story);
            StoryManager.PlayStory(client, story);
        }
    }
}

