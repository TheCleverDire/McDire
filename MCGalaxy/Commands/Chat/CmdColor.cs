﻿/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
    
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
using MCGalaxy.Bots;
using MCGalaxy.DB;

namespace MCGalaxy.Commands.Chatting {    
    public class CmdColor : EntityPropertyCmd {
        public override string name { get { return "Color"; } }
        public override string type { get { return CommandTypes.Chat; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override CommandPerm[] ExtraPerms {
            get { return new[] { new CommandPerm(LevelPermission.Operator, "can change the color of others"),
                    new CommandPerm(LevelPermission.Operator, "can change the color of bots") }; }
        }
        public override CommandAlias[] Aliases {
            get { return new[] { new CommandAlias("Colour"), new CommandAlias("XColor", "-own") }; }
        }        
        public override void Use(Player p, string message, CommandData data) { 
            UseBotOrPlayer(p, data, message, "color"); 
        }

        protected override void SetBotData(Player p, PlayerBot bot, string colName) {
            string color = colName.Length == 0 ? "&1" : Matcher.FindColor(p, colName);
            if (color == null) return;
            
            p.Message("You changed the color of bot " + bot.ColoredName + 
                      " %Sto " + color + Colors.Name(color));
            bot.color = color;
            
            bot.GlobalDespawn();
            bot.GlobalSpawn();
            BotsFile.Save(p.level);
        }
        
        protected override void SetPlayerData(Player p, Player who, string colName) {
            string color = "";
            if (colName.Length == 0) {
                Chat.MessageFrom(who, "λNICK %Shad their color removed");
                who.color = who.group.Color;
            } else {
                color = Matcher.FindColor(p, colName);
                if (color == null) return;
                if (color == who.color) { p.Message(who.ColoredName + " %Salready has that color."); return; }
                
                Chat.MessageFrom(who, "λNICK %Shad their color changed to " + color + Colors.Name(color));
                who.color = color;
            }
            
            Entities.GlobalRespawn(who);
            who.SetPrefix();
            PlayerDB.Update(who.name, PlayerData.ColumnColor, color);
        }
        
        public override void Help(Player p) {
            p.Message("%T/Color [player] [color]");
            p.Message("%HSets the nick color of that player");
            p.Message("%H  If [color] is not given, reverts to player's rank color.");
            p.Message("%H/Color bot [bot] [color]");
            p.Message("%TSets the name color of that bot.");
            p.Message("%HTo see a list of all colors, use /Help colors.");
        }
    }
}
