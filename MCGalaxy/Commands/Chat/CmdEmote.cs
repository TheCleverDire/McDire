/*
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
namespace MCGalaxy.Commands.Chatting {
    public sealed class CmdEmote : Command2 {
        public override string name { get { return "Emote"; } }
        public override string shortcut { get { return "<3"; } }
        public override string type { get { return CommandTypes.Chat; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data) {
            p.parseEmotes = !p.parseEmotes;
            bool addToList = p.parseEmotes != ServerConfig.ParseEmotes;
            if (!addToList) Server.noEmotes.Remove(p.name);
            else Server.noEmotes.AddUnique(p.name);
            Server.noEmotes.Save();
            p.Message("Emote parsing is {0}.", p.parseEmotes ? "enabled" : "disabled");
        }
        
        public override void Help(Player p) {
            p.Message("%T/Emote");
            p.Message("%HEnables or disables emoticon parsing");
        }
    }
}
