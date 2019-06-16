using System;
using System.Collections.Generic;
using System.Text;
using Script.Events;

namespace Script
{
    public partial class Main
    {
        public static IEvent ActiveEvent { get; set; }

        public static IEvent BuildEvent(string identifier)
        {
            switch (identifier)
            {
                case "treasurehunt":
                    return new TreasureHuntEvent();
                case "shinyspectacular":
                    return new ShinySpectacular();
                case "paintball":
                    return new Paintball();
                case "werewolf":
                    return new WerewolfEvent();
                default:
                    return null;
            }
        }
    }
}
