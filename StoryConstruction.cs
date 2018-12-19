// This file is part of Mystery Dungeon eXtended.

// Copyright (C) 2015 Pikablu, MDX Contributors, PMU Staff

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.

// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Stories;
using Server.Network;
using Server.Maps;
using Server.Pokedex;

namespace Script {
    public class StoryConstruction {
        public static Story CreateIntroStory(Client client) {
            Story story = new Story();

            StoryBuilderSegment segment = new StoryBuilderSegment();

            Pokemon eevee = Pokedex.FindByName("Eevee");
            string map1ID = Main.Crossroads;

            StoryBuilder.AppendMapVisibilityAction(segment, false);
            StoryBuilder.AppendPlayerPadlockAction(segment, "Lock");
            StoryBuilder.AppendWarpAction(segment, map1ID, 25, 25);
            StoryBuilder.AppendPlayMusicAction(segment, "PMD2) Strange Happenings.mp3", true, true);
            StoryBuilder.AppendSaySegment(segment, "You step into the light of the crystal...", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "You feel its energy going through your body...", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "Dizzy... are you moving?", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "You feel a cool breeze... maybe, just maybe...", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "You slowly open your eyes...", -1, 0, 0);
            StoryBuilder.AppendPauseAction(segment, 1000);
            StoryBuilder.AppendPlayMusicAction(segment, "%mapmusic%", true, true);
            StoryBuilder.AppendSaySegment(segment, "Hey! You!", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "Are you okay?", -1, 0, 0);
            StoryBuilder.AppendCreateFNPCAction(segment, "0", map1ID, 25, 23, eevee.ID);
            StoryBuilder.AppendChangeFNPCDirAction(segment, "0", Server.Enums.Direction.Down);
            StoryBuilder.AppendMapVisibilityAction(segment, true);
            StoryBuilder.AppendSaySegment(segment, "Oh, good! You're awake!", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "You've been laying there for a while now. Are you okay? Are you hurt?", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "(An eevee is talking to me...)", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "Oh, well. The important thing is that you are awake now!", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "So, where are you from? I haven't seen you in these parts before.", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "(Hmm... Where am I from? I don't remember a thing)", -1, 0, 0);
            StoryBuilder.AppendPauseAction(segment, 1000);
            StoryBuilder.AppendSaySegment(segment, "Oh, you don't remember? Strange. Anyway, you're probably wondering who I am...", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "...I am the great Eevocious! Master of evolutions and battle!", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "(Eevocious, huh... what an odd name. But, alright. Eevocious it is.)", -1, 0, 0);
            StoryBuilder.AppendPauseAction(segment, 2000);
            StoryBuilder.AppendSaySegment(segment, "...I bet I fooled you, didn't I.", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "And you actually thought I was Eevocious!", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "Yeah, I'm sorry. It's just hard to resist. A new Pokemon just appearing out of no where gives me a chance to be whomever I want to be!", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "(Very funny)", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "Anyway, my real name is Eevee. I'm an explorer. Or, I want to become an explorer. See...", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "I can't become a true explorer until I prove that I am capable of handling the toughest situations. That's why I'm training to be able to get through Tiny Grotto!", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "(Tiny Grotto?)", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "You're probably wondering what Tiny Grotto even is! I'll explain.", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "It's a dungeon. It's not the hardest dungeon, but it's good practice. Yup, so I'm going to Tiny Grotto!", eevee.ID, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "(Oh look, Eevee is leaving me.)", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "Bye~!", eevee.ID, 0, 0);
            StoryBuilder.AppendMoveFNPCAction(segment, "0", 1, 24, Server.Enums.Speed.Walking, true);
            StoryBuilder.AppendDeleteFNPCAction(segment, "0");
            StoryBuilder.AppendSaySegment(segment, "(Now what do I do next? I know I can't go to Tiny Grotto... I don't even know how to battle!)", -1, 0, 0);
            StoryBuilder.AppendSaySegment(segment, "(Maybe I'll find out more if I continue going North (^))", -1, 0, 0);
            StoryBuilder.AppendPlayerPadlockAction(segment, "Unlock");

            if (client.Player.Veteran) {
                StoryBuilder.AppendPlayerPadlockAction(segment, "Lock");
                StoryBuilder.AppendPauseAction(segment, 2000);
                StoryBuilder.AppendSaySegment(segment, "Pikablu: Cool! Eevee is back!", -1, 0, 0);
                StoryBuilder.AppendSaySegment(segment, "Pikablu: Have fun exploring!", -1, 0, 0);
                StoryBuilder.AppendPlayerPadlockAction(segment, "Unlock");
            }
            #region old
            //StoryBuilder.AppendMapVisibilityAction(segment, false);
            //StoryBuilder.AppendSaySegment(segment, "Hey! You!", -1, 0, 0);
            //StoryBuilder.AppendSaySegment(segment, "Are you okay?", -1, 0, 0);
            //StoryBuilder.AppendCreateFNPCAction(segment, "0", map1ID, 10, 6, eevee.Sprite);
            //StoryBuilder.AppendMapVisibilityAction(segment, true);
            //StoryBuilder.AppendSaySegment(segment, "Oh, good! You're awake!", eevee.Mugshot, 0, 0);
            //StoryBuilder.AppendSaySegment(segment, "You've been laying there for a while now. Are you okay? Are you hurt?", eevee.Mugshot, 0, 0);
            //StoryBuilder.AppendSaySegment(segment, "(An eevee is talking to me...)", -1, 0, 0);
            //StoryBuilder.AppendSaySegment(segment, "You're probably wondering what Tiny Woods even is! I'll explain.", eevee.Mugshot , 0, 0);
            //StoryBuilder.AppendSaySegment(segment, "It's a dungeon. It's not the hardest dungeon, but it's good practice. Yup, so I'm going to Tiny Woods!", eevee.Mugshot, 0, 0);
            //StoryBuilder.AppendSaySegment(segment, "(Oh look, Eevee is leaving me. Fun.", -1, 0, 0);
            //StoryBuilder.AppendSaySegment(segment, "Bye~!", eevee.Mugshot , 0, 0);
            //StoryBuilder.AppendMoveFNPCAction(segment, "0", 0, 7, Server.Enums.Speed.Walking, true);
            //StoryBuilder.AppendDeleteFNPCAction(segment, "0");
            //StoryBuilder.AppendSaySegment(segment, "(Now what do I do next? I know I can't go to Tiny Woods... I don't even know how to battle!", -1, 0, 0);
            //StoryBuilder.AppendSaySegment(segment, "(Maybe I'll find out more if I continue going North (^))", -1, 0, 0);
            //StoryBuilder.AppendPlayerPadlockAction(segment, "Unlock");
            #endregion

            segment.AppendToStory(story);
            return story;
        }
    }
}
