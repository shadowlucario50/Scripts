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
using Server;
using Server.Scripting;
using System;
using System.Drawing;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using PMDCP.DatabaseConnector;
using PMDCP.DatabaseConnector.SQLite;
using System.IO;

namespace Script {
    public static class ChatMessageWatcher {
        // Finish this class when the server update that has the OnChatMessageRecieved script method is added
        static SQLite watcherDB;

        public static SQLite WatcherDB {
            get {
                return watcherDB;
            }
        }

        public static void Initialize() {
            //watcherDB = new SQLite(Server.IO.Paths.ScriptsFolder + "ScriptIO/ChatMessageWatcherDB.sqlite", false);
        }

        private static void VerifyDatabase() {
            if (Server.IO.IO.FileExists(Path.Combine(Server.IO.Paths.ScriptsFolder, "ScriptIO", "ChatMessageWatcherDB.sqlite")) == false) {
                System.IO.File.Create(Path.Combine(Server.IO.Paths.ScriptsFolder, "ScriptIO", "ChatMessageWatcherDB.sqlite")).Close();

            }
        }
    }
}