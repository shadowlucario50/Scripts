using System;
using System.Collections.Generic;
using System.Text;
using Server.Network;
using Server.Maps;
using Server.Players;
using Server.Combat;

namespace Script
{
    public interface IEvent
    {
        string Identifier { get; }
        string Name { get; }

        string IntroductionMessage { get; }

        void ConfigurePlayer(Client client);
        void DeconfigurePlayer(Client client);

        void OnActivateMap(IMap map);
        void OnPickupItem(ICharacter character, int itemSlot, InventoryItem invItem);
        void OnDeath(Client client);
        void OnNpcSpawn(IMap map, MapNpcPreset npc, MapNpc spawnedNpc, PacketHitList hitlist);
        void OnNpcDeath(PacketHitList hitlist, ICharacter attacker, MapNpc npc);
        void OnMoveHitCharacter(Client attacker, Client defender);

        void Load(string data);
        string Save();

        void Start();
        void End();
        void AnnounceWinner();
    }
}
