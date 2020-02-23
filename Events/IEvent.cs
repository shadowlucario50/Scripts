using System;
using System.Collections.Generic;
using System.Text;
using Server.Network;
using Server.Maps;
using Server.Players;
using Server.Combat;
using Server;

namespace Script.Events
{
    public interface IEvent
    {
        string Identifier { get; }
        string Name { get; }

        string IntroductionMessage { get; }
        string RewardMessage { get; }
        TimeSpan? Duration { get; }

        void ConfigurePlayer(Client client);
        void DeconfigurePlayer(Client client);

        void OnServerTick(TickCount tickCount);
        void OnMapTick(IMap map);
        void OnActivateMap(IMap map);
        void OnPickupItem(ICharacter character, int itemSlot, InventoryItem invItem);
        void OnDeath(Client client);
        void OnNpcSpawn(IMap map, MapNpcPreset npc, MapNpc spawnedNpc, PacketHitList hitlist);
        void OnNpcDeath(PacketHitList hitlist, ICharacter attacker, MapNpc npc);
        void OnMoveHitCharacter(Client attacker, Client defender);
        bool ProcessCommand(Client client, Command command, string joinedArgs);

        void Load(string data);
        string Save();

        void Start();
        void End();
        void AnnounceWinner();
    }
}
