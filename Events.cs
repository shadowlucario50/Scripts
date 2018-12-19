using System;
using System.Collections.Generic;
using System.Text;

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
                default:
                    return null;
            }
        }
    }
}
