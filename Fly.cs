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
using Server.Fly;

namespace Script
{
    public partial class Main
    {
        public static void InitializeFlyPoints() {
            FlightManager.ClearFlightPoints();

            FlightManager.AddFlightPoint(new FlightPoint() { ID = 0, Name = "Florea Town", MapNumber = 5, Description = "Where everything blooms beautifully." });
            FlightManager.AddFlightPoint(new FlightPoint() { ID = 1, Name = "Newpond Village", MapNumber = 54, Description = "Fresh air and fresh water." });
            FlightManager.AddFlightPoint(new FlightPoint() { ID = 2, Name = "Dream Village", MapNumber = 35, Description = "Hidden within the cherry blossoms." });
            FlightManager.AddFlightPoint(new FlightPoint() { ID = 3, Name = "Borealis City", MapNumber = 223, Description = "A city that rose from a glorious history." });
            FlightManager.AddFlightPoint(new FlightPoint() { ID = 4, Name = "Mysveil Town", MapNumber = 358, Description = "Beyond the veil of time." });
        }
    }
}

