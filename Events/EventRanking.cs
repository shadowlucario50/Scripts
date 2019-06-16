using System;
using System.Collections.Generic;
using System.Text;
using Server.Network;

namespace Script.Events
{
    public class EventRanking
    {
        public Client Client { get; }
        public int Score { get; }

        public EventRanking(Client client, int score)
        {
            this.Client = client;
            this.Score = score;
        }
    }
}
