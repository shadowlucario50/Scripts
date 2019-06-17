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
    public class WerewolfEvent : AbstractEvent<WerewolfEvent.WerewolfData, WerewolfEvent.UserInfo>
    {
        public static readonly int WerewolfCount = 2;

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

        public class WerewolfData : AbstractEventData<UserInfo>
        {
            public GameState GameState { get; set; }
            public string WerewolfKilledUser { get; set; }

            public WerewolfData()
            {
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
            var candidates = GetRegisteredClients().ToList();

            var seer = SelectUser(candidates);
            Data.ExtendPlayer(seer).Role = UserRole.Seer;

            var doctor = SelectUser(candidates);
            Data.ExtendPlayer(doctor).Role = UserRole.Doctor;

            for (var i = 0; i < WerewolfCount; i++)
            {
                var werewolf = SelectUser(candidates);
                Data.ExtendPlayer(werewolf).Role = UserRole.Werewolf;
            }
        }

        private Client SelectUser(List<Client> users)
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
            foreach (var client in GetRegisteredClients())
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

            var userData = Data.ExtendPlayer(client);

            switch (command[0])
            {
                case "/wrole":
                    Messenger.PlayerMsg(client, $"You are a {userData.Role}!", Text.BrightGreen);
                    return true;
                case "/walive":
                    Messenger.PlayerMsg(client, "The following players are alive:", Text.BrightGreen);
                    foreach (var eventClient in GetRegisteredClients().Where(x => !Data.ExtendPlayer(x).IsDead)) {
                        Messenger.PlayerMsg(client, eventClient.Player.DisplayName, Text.BrightGreen);
                    }
                    return true;
                case "/wchoose":
                    var chosenClient = ClientManager.FindClient(joinedArgs);
                    if (chosenClient == null)
                    {
                        Messenger.PlayerMsg(client, "Player is offline.", Text.BrightRed);
                        return true;
                    }

                    if (Data.ExtendPlayer(chosenClient).IsDead)
                    {
                        Messenger.PlayerMsg(client, "Player is already dead!", Text.BrightRed);
                        return true;
                    }
                    if (userData.IsDead)
                    {
                        Messenger.PlayerMsg(client, "You can't make any choices while dead!", Text.BrightRed);
                        return true;
                    }

                    switch (Data.GameState)
                    {
                        case GameState.WerewolfSelecting:
                            {
                                if (userData.Role == UserRole.Werewolf)
                                {
                                    if (Data.ExtendPlayer(chosenClient).Role == UserRole.Werewolf)
                                    {
                                        Messenger.PlayerMsg(client, "You can't choose another werewolf!", Text.BrightRed);
                                        return true;
                                    }

                                    userData.SelectedCharId = chosenClient.Player.CharID;
                                    RoleAlertMessage(UserRole.Werewolf, $"{client.Player.DisplayName} chose {chosenClient.Player.DisplayName}!");
                                }

                                if (CanTransition())
                                {
                                    TransitionNext(GameState.DoctorSelecting);
                                }
                            }
                            break;
                        case GameState.DoctorSelecting:
                            {
                                if (userData.Role == UserRole.Doctor)
                                {
                                    userData.SelectedCharId = chosenClient.Player.CharID;
                                }

                                if (CanTransition())
                                {
                                    TransitionNext(GameState.SeerSelecting);
                                }
                            }
                            break;
                        case GameState.SeerSelecting:
                            {
                                if (userData.Role == UserRole.Seer)
                                {
                                    userData.SelectedCharId = chosenClient.Player.CharID;

                                    if (Data.ExtendPlayer(chosenClient).Role == UserRole.Werewolf)
                                    {
                                        Messenger.PlayerMsg(client, $"{chosenClient.Player.DisplayName} is a werewolf!", Text.BrightGreen);
                                    }
                                    else
                                    {
                                        Messenger.PlayerMsg(client, $"{chosenClient.Player.DisplayName} is not a werewolf!", Text.BrightRed);
                                    }
                                }

                                if (CanTransition())
                                {
                                    Transition(GameState.VillagersSelecting);
                                }
                            }
                            break;
                        case GameState.VillagersSelecting:
                            {
                                userData.SelectedCharId = chosenClient.Player.CharID;

                                foreach (var eventClient in GetRegisteredClients())
                                {
                                    Messenger.PlayerMsg(eventClient, $"[Werewolf] {client.Player.DisplayName} chose {chosenClient.Player.DisplayName}!", Text.White);
                                }

                                if (CanTransition())
                                {
                                    var majoritySelection = GetMajorityPlayerSelection();

                                    ExecuteTurn(majoritySelection);
                                }
                            }
                            break;
                    }
                    return true;
                case "/w":
                    {
                        if (userData.Role == UserRole.Werewolf)
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

        private void TransitionNext(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.DoctorSelecting:
                    {
                        Transition(gameState);

                        if (IsRoleDead(UserRole.Doctor))
                        {
                            TransitionNext(GameState.SeerSelecting);
                        }
                    }
                    break;
                case GameState.SeerSelecting:
                    {
                        Transition(gameState);

                        if (IsRoleDead(UserRole.Seer))
                        {
                            Transition(GameState.VillagersSelecting);
                        }
                    }
                    break;
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
            foreach (var eventClient in GetRegisteredClients().Where(x => Data.ExtendPlayer(x).Role == UserRole.Werewolf))
            {
                Messenger.PlayerMsg(eventClient, $"[Werewolf Chat] {client.Player.DisplayName}: {message}", Text.White);
            }
        }

        public void RoleAlertMessage(UserRole role, string message)
        {
            foreach (var eventClient in GetRegisteredClients().Where(x => Data.ExtendPlayer(x).Role == role))
            {
                Messenger.PlayerMsg(eventClient, $"[Werewolf] {message}", Text.White);
            }
        }

        private IEnumerable<Client> GetRoleClients(UserRole role)
        {
            foreach (var eventClient in GetRegisteredClients().Where(x => Data.ExtendPlayer(x).Role == role).Where(x => !Data.ExtendPlayer(x).IsDead))
            {
                yield return eventClient;
            }
        }

        private bool CanTransition()
        {
            switch (Data.GameState)
            {
                case GameState.WerewolfSelecting:
                    {
                        if (GetRoleClients(UserRole.Werewolf).Where(x => string.IsNullOrEmpty(Data.ExtendPlayer(x).SelectedCharId)).Any()) {
                            return false;
                        }

                        return GetRoleClients(UserRole.Werewolf).Where(x => !Data.ExtendPlayer(x).IsDead)
                                                                .Select(x => Data.ExtendPlayer(x).SelectedCharId)
                                                                .Distinct().Count() == 1;
                    }
                case GameState.DoctorSelecting:
                    {
                        var doctor = GetRoleClients(UserRole.Doctor).FirstOrDefault();
                        if (doctor == null)
                        {
                            return true;
                        }

                        return !string.IsNullOrEmpty(Data.ExtendPlayer(doctor).SelectedCharId);
                    }
                case GameState.SeerSelecting:
                    {
                        var seer = GetRoleClients(UserRole.Seer).FirstOrDefault();
                        if (seer == null)
                        {
                            return true;
                        }

                        return !string.IsNullOrEmpty(Data.ExtendPlayer(seer).SelectedCharId);
                    }
                case GameState.VillagersSelecting:
                    {
                        var majoritySelection = GetMajorityPlayerSelection();

                        return !string.IsNullOrEmpty(majoritySelection);
                    }
            }

            return false;
        }

        private string GetMajorityPlayerSelection()
        {
            var alivePlayers = GetRegisteredClients().Where(x => !Data.ExtendPlayer(x).IsDead).ToArray();

            var groupings = alivePlayers.Select(x => Data.ExtendPlayer(x).SelectedCharId).GroupBy(x => x);
            foreach (var grouping in groupings)
            {
                if (grouping.Count() >= (alivePlayers.Length / 2) + 1)
                {
                    return grouping.Key;
                }
            }

            return null;
        }

        private void Transition(GameState newState)
        {
            Data.GameState = newState;

            if (newState == GameState.VillagersSelecting)
            {
                TransitionToDaytime();
            }

            foreach (var eventClient in GetRegisteredClients())
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
                case GameState.SeerSelecting:
                    {
                        ApplySeerSelectingState(client);
                    }
                    break;
                case GameState.VillagersSelecting:
                    {
                        ApplyVillagerSelectingState(client);
                    }
                    break;
            }
        }

        private void TransitionToDaytime()
        {
            var doctor = GetRoleClients(UserRole.Doctor).FirstOrDefault();
            var werewolf = GetRoleClients(UserRole.Werewolf).First();

            var chosenCharId = Data.ExtendPlayer(werewolf).SelectedCharId;

            var saved = false;
            if (doctor != null && Data.ExtendPlayer(doctor).SelectedCharId == chosenCharId)
            {
                saved = true;
            }

            if (!saved)
            {
                Data.ExtendPlayer(chosenCharId).IsDead = true;
            }

            Data.WerewolfKilledUser = chosenCharId;

            foreach (var eventClient in GetRegisteredClients())
            {
                Data.ExtendPlayer(eventClient).SelectedCharId = null;
            }
        }

        private void ApplyVillagerSelectingState(Client client)
        {
            var chosenUser = ClientManager.FindClientFromCharID(Data.WerewolfKilledUser);

            var story = new Story(Guid.NewGuid().ToString());
            var segment = StoryBuilder.BuildStory();
            StoryBuilder.AppendSaySegment(segment, "Daytime has arrived!", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "While you slept, werewolves attacked.", -1, 0, 0);

            if (!Data.ExtendPlayer(Data.WerewolfKilledUser).IsDead)
            {
                StoryBuilder.AppendSaySegment(segment, "...however, the doctor protected the village, and no one was hurt!", -1, 0, 0);
            }
            else
            {
                if (chosenUser == null)
                {
                    StoryBuilder.AppendSaySegment(segment, $"Character {Data.WerewolfKilledUser} was killed. They logged off in fear.", -1, 0, 0);
                }
                else
                {
                    StoryBuilder.AppendSaySegment(segment, $"{chosenUser.Player.DisplayName} was killed in the attack.", -1, 0, 0);
                }
            }

            StoryBuilder.AppendSaySegment(segment, "Now, you must stop the werewolves!", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "Who will be killed? Make your decision with /wchoose.", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "None shall rest until a majority decision is made.", -1, 0, 0);

            segment.AppendToStory(story);
            StoryManager.PlayStory(client, story);
        }

        private void ApplySeerSelectingState(Client client)
        {
            client.Player.Muted = true;

            var story = new Story(Guid.NewGuid().ToString());
            var segment = StoryBuilder.BuildStory();
            StoryBuilder.AppendSaySegment(segment, "Night has fallen!", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "The seer must now decide.", -1, 0, 0);

            if (Data.ExtendPlayer(client).Role == UserRole.Seer)
            {
                StoryBuilder.AppendSaySegment(segment, "You are the seer. Make your decision with /wchoose.", -1, 0, 0);
            }

            StoryBuilder.AppendSaySegment(segment, "Everyone is muted while the seer decides.", -1, 0, 0);
            if (IsRoleDead(UserRole.Seer))
            {
                StoryBuilder.AppendPauseAction(segment, 5000);
            }

            segment.AppendToStory(story);
            StoryManager.PlayStory(client, story);
        }

        private void ApplyDoctorSelectingState(Client client)
        {
            client.Player.Muted = true;


            var story = new Story(Guid.NewGuid().ToString());
            var segment = StoryBuilder.BuildStory();
            StoryBuilder.AppendSaySegment(segment, "Night has fallen!", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "The doctor must now decide who to save.", -1, 0, 0);

            if (Data.ExtendPlayer(client).Role == UserRole.Doctor)
            {
                StoryBuilder.AppendSaySegment(segment, "You are the doctor. Make your decision with /wchoose.", -1, 0, 0);
            }

            StoryBuilder.AppendSaySegment(segment, "Everyone is muted while the doctor decides.", -1, 0, 0);
            if (IsRoleDead(UserRole.Doctor))
            {
                StoryBuilder.AppendPauseAction(segment, 5000);
            }

            segment.AppendToStory(story);
            StoryManager.PlayStory(client, story);
        }

        private void ApplyWerewolfSelectingState(Client client)
        {
            client.Player.Muted = true;

            var story = new Story(Guid.NewGuid().ToString());
            var segment = StoryBuilder.BuildStory();
            StoryBuilder.AppendSaySegment(segment, "Night has fallen!", -1, 0, 0);

            switch (Data.ExtendPlayer(client).Role)
            {
                case UserRole.Werewolf:
                    {
                        foreach (var eventClient in GetRoleClients(UserRole.Werewolf))
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

        private void ExecuteTurn(string selectionCharId)
        {
            Data.ExtendPlayer(selectionCharId).IsDead = true;
            foreach (var eventClient in GetRegisteredClients())
            {
                Data.ExtendPlayer(eventClient).SelectedCharId = null;
            }

            var chosenUser = ClientManager.FindClientFromCharID(selectionCharId);
            var werewolfCount = GetRoleClients(UserRole.Werewolf).Count();
            var aliveVillagersCount = GetRegisteredClients().Where(x => Data.ExtendPlayer(x).Role != UserRole.Werewolf).Where(x => !Data.ExtendPlayer(x).IsDead).Count();
            var gameOver = false;

            foreach (var eventClient in GetRegisteredClients())
            {
                var story = new Story(Guid.NewGuid().ToString());
                var segment = StoryBuilder.BuildStory();
                StoryBuilder.AppendSaySegment(segment, "A decision has been made!", -1, 0, 0);
                if (chosenUser == null)
                {
                    StoryBuilder.AppendSaySegment(segment, $"Character {selectionCharId} was killed. They logged off in fear.", -1, 0, 0);
                }
                else
                {
                    StoryBuilder.AppendSaySegment(segment, $"{chosenUser.Player.DisplayName} was hanged! Hopefully they were a werewolf...", -1, 0, 0);
                }

                if (werewolfCount == 0)
                {
                    StoryBuilder.AppendSaySegment(segment, $"The villagers win! All the werewolves have been killed!", -1, 0, 0);
                    gameOver = true;
                }
                else if (werewolfCount == aliveVillagersCount)
                {
                    StoryBuilder.AppendSaySegment(segment, $"The werewolves win!", -1, 0, 0);
                    gameOver = true;
                }

                segment.AppendToStory(story);
                StoryManager.PlayStory(eventClient, story);
            }

            if (!gameOver)
            {
                Transition(GameState.WerewolfSelecting);
            }
            else
            {
                Main.EndEvent();
            }
        }

        private bool IsRoleDead(UserRole role)
        {
            foreach (var roleClient in GetRoleClients(role))
            {
                if (!Data.ExtendPlayer(roleClient).IsDead)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

