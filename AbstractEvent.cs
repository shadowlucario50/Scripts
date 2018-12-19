using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Server.Combat;
using Server.Events;
using Server.Maps;
using Server.Network;
using Server.Players;
using Server.Stories;

namespace Script
{
    public abstract class AbstractEvent<TData> : IEvent where TData : AbstractEventData, new()
    {
        public abstract string Identifier { get; }

        public abstract string Name { get; }

        public abstract string IntroductionMessage { get; }

        public TData Data { get; private set; }

        public AbstractEvent()
        {
            this.Data = new TData();
        }

        protected abstract List<EventRanking> DetermineRankings();

        public void AnnounceWinner()
        {
            var rankings = DetermineRankings();

            var sortedRankings = rankings.OrderByDescending(x => x.Score).ToList();

            var rewards = new string[3];

            if (sortedRankings.Count >= 3)
            {
                rewards[2] = HandoutReward(sortedRankings[2], 3);
            }
            if (sortedRankings.Count >= 2)
            {
                rewards[1] = HandoutReward(sortedRankings[1], 2);
            }
            if (sortedRankings.Count >= 1)
            {
                rewards[0] = HandoutReward(sortedRankings[0], 1);
            }

            foreach (var client in EventManager.GetRegisteredClients())
            {
                CleanPlayer(client);

                var story = new Story();
                var segment = StoryBuilder.BuildStory();
                StoryBuilder.AppendSaySegment(segment, $"And the winners are...!", -1, 0, 0);

                if (sortedRankings.Count >= 3)
                {
                    StoryBuilder.AppendSaySegment(segment, "In third place...", -1, 0, 0);
                    StoryBuilder.AppendSaySegment(segment, $"{sortedRankings[2].Client.Player.DisplayName}, with a score of {sortedRankings[2].Score}!", -1, 0, 0);
                    if (!string.IsNullOrEmpty(rewards[2]))
                    {
                        StoryBuilder.AppendSaySegment(segment, $"They recieved {rewards[2]}!", -1, 0, 0);
                    }
                }

                if (sortedRankings.Count >= 2)
                {
                    StoryBuilder.AppendSaySegment(segment, "In second place...", -1, 0, 0);
                    StoryBuilder.AppendSaySegment(segment, $"{sortedRankings[1].Client.Player.DisplayName}, with a score of {sortedRankings[1].Score}!", -1, 0, 0);
                    if (!string.IsNullOrEmpty(rewards[1]))
                    {
                        StoryBuilder.AppendSaySegment(segment, $"They recieved {rewards[1]}!", -1, 0, 0);

                    }
                }

                if (sortedRankings.Count >= 1)
                {
                    StoryBuilder.AppendSaySegment(segment, "In first place...", -1, 0, 0);
                    StoryBuilder.AppendSaySegment(segment, $"{sortedRankings[0].Client.Player.DisplayName}, with a score of {sortedRankings[0].Score}!", -1, 0, 0);
                    if (!string.IsNullOrEmpty(rewards[0]))
                    {
                        StoryBuilder.AppendSaySegment(segment, $"They recieved {rewards[0]}!", -1, 0, 0);
                    }
                }

                if (sortedRankings.Count == 0)
                {
                    StoryBuilder.AppendSaySegment(segment, "...no one. Strange?", -1, 0, 0);
                }

                segment.AppendToStory(story);
                StoryManager.PlayStory(client, story);
            }
        }

        public virtual string HandoutReward(EventRanking eventRanking, int position)
        {
            return "";
        }

        public virtual void ConfigurePlayer(Client client)
        {
        }

        public virtual void DeconfigurePlayer(Client client)
        {
        }

        public virtual void CleanPlayer(Client client)
        {
        }

        public virtual void End()
        {
            Data.Started = false;

            foreach (var client in EventManager.GetRegisteredClients())
            {
                Messenger.PlayerWarp(client, 152, 15, 16);
            }
        }

        public virtual void Start()
        {
            Data.Started = true;
        }

        public void Load(string data)
        {
            this.Data = JsonConvert.DeserializeObject<TData>(data);
        }

        public string Save()
        {
            return JsonConvert.SerializeObject(Data);
        }

        public virtual void OnActivateMap(IMap map)
        {
        }

        public virtual void OnDeath(Client client)
        {
        }

        public virtual void OnPickupItem(ICharacter character, int itemSlot, InventoryItem invItem)
        {
        }

        public virtual void OnNpcSpawn(IMap map, MapNpcPreset npc, MapNpc spawnedNpc, PacketHitList hitlist)
        {
        }

        public virtual void OnNpcDeath(PacketHitList hitlist, ICharacter attacker, MapNpc npc)
        {
        }
    }
}
