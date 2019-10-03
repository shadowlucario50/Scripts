// This file is part of PMD: Shift!.

// Copyright (C) 2019 BurningBlaze

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
using System.Text;
using Server.Achievements;

namespace Script
{
    public partial class Main
    {
        public static void InitializeAchievements() {
            AchievementManager.ClearAchievements();

            AchievementManager.AddAchievement(new Achievement() { ID = 0, Name = "Be the Crow!", Description = "Use the Crow client for the first time." });
            AchievementManager.AddAchievement(new Achievement() { ID = 1, Name = "Poopy", Description = "Use a poop emoji for the first time." });
            AchievementManager.AddAchievement(new Achievement() { ID = 2, Name = "Eye Spy", Description = "Use an eyes emoji for the first time." });
            AchievementManager.AddAchievement(new Achievement() { ID = 3, Name = "Level Up!", Description = "Gain your first level." });
            AchievementManager.AddAchievement(new Achievement() { ID = 4, Name = "Farewell!", Description = "Release your first recruit." });
        }
    }
}

