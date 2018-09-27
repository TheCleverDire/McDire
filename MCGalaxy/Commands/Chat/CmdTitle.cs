﻿/*
    Copyright 2011 MCForge
        
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using MCGalaxy.DB;

namespace MCGalaxy.Commands.Chatting {    
    public class CmdTitle : EntityPropertyCmd {        
        public override string name { get { return "Title"; } }
        public override string type { get { return CommandTypes.Chat; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
        public override CommandPerm[] ExtraPerms {
            get { return new[] { new CommandPerm(LevelPermission.Admin, "can change the title of others") }; }
        }
        public override CommandAlias[] Aliases {
            get { return new[] { new CommandAlias("XTitle", "-own") }; }
        }
        
        public override void Use(Player p, string message, CommandData data) {
            if (!MessageCmd.CanSpeak(p, name)) return;
            UsePlayer(p, data, message, "title");
        }
        
        protected override void SetPlayerData(Player p, Player who, string title) {
            if (title.Length >= 20) { p.Message("Title must be under 20 letters."); return; }

            if (title.Length == 0) {
                Chat.MessageFrom(who, "λNICK %Shad their title removed");
            } else {
                Chat.MessageFrom(who, "λNICK %Shad their title changed to &b[" + title + "&b]");
            }
            
            who.title = title;
            who.SetPrefix();
            PlayerDB.Update(who.name, PlayerData.ColumnTitle, title.UnicodeToCp437());
        }
        
        public override void Help(Player p) {
            p.Message("%T/Title [player] [title]");
            p.Message("%HSets the title of [player]");
            p.Message("%H  If [title] is not given, removes [player]'s title.");
        }
    }
}
