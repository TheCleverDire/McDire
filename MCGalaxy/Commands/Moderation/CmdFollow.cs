/*
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
using System;

namespace MCGalaxy.Commands.Moderation {
    public sealed class CmdFollow : Command2 {
        public override string name { get { return "Follow"; } }
        public override string type { get { return CommandTypes.Moderation; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data) {
            if (!p.canBuild) { p.Message("You're currently being &4possessed%S!"); return; }
            string[] args = message.SplitSpaces(2);
            string name = args[0];
            
            bool stealth = false;
            if (message == "#") {
                if (p.following.Length > 0) { stealth = true; name = ""; }
                else { Help(p); return; }
            } else if (args.Length > 1 && args[0] == "#") {
                if (p.hidden) stealth = true;
                name = args[1];
            }
            
            if (name.Length == 0 && p.following.Length == 0) { Help(p); return; }            
            if (name.CaselessEq(p.following) || (name.Length == 0 && p.following.Length > 0)) {
                Unfollow(p, data, stealth);
            } else {
                Follow(p, name, data, stealth);
            }
        }
        
        static void Unfollow(Player p, CommandData data, bool stealth) {
            Player who = PlayerInfo.FindExact(p.following);
            p.Message("Stopped following ", who == null ? p.following : who.ColoredName);
            p.following = "";
            if (who != null) Entities.Spawn(p, who);
            
            if (!p.hidden) return;
            if (!stealth) {
                Command.Find("Hide").Use(p, "", data);
            } else {
                p.Message("You are still hidden.");
            }
        }
        
        static void Follow(Player p, string name, CommandData data, bool stealth) {
            Player who = PlayerInfo.FindMatches(p, name);
            if (who == null) return;
            if (who == p) { p.Message("Cannot follow yourself."); return; }
            if (!CheckRank(p, data, who, "follow", false)) return;
            
            if (who.following.Length > 0) { 
                p.Message(who.ColoredName+ " %Sis already following " + who.following); return; 
            }

            if (!p.hidden) Command.Find("Hide").Use(p, "", data);

            if (p.level != who.level) Command.Find("TP").Use(p, who.name, data);
            if (p.following.Length > 0) {
                Player old = PlayerInfo.FindExact(p.following);
                if (old != null) Entities.Spawn(p, old);
            }
            
            p.following = who.name;
            p.Message("Following " + who.ColoredName + "%S. Use %T/Follow %Sto stop.");
            Entities.Despawn(p, who);
        }
        
        public override void Help(Player p) {
            p.Message("%T/Follow [name]");
            p.Message("%HFollows <name> until the command is cancelled");
            p.Message("%T/Follow # [name]");
            p.Message("%HWill cause %T/Hide %Hnot to be toggled");
        }
    }
}
