﻿/*
    Copyright 2015 MCGalaxy
        
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
using System.Collections.Generic;

namespace MCGalaxy.Commands.Moderation {
    public class CmdNotes : Command2 {
        public override string name { get { return "Notes"; } }
        public override string type { get { return CommandTypes.Moderation; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }

        public override void Use(Player p, string name, CommandData data) {
            if (!ServerConfig.LogNotes) {
                p.Message("The server does not have notes logging enabled."); return;
            }
            
            if (CheckSuper(p, name, "player name")) return;
            if (name.Length == 0) name = p.name;
            
            name = PlayerInfo.FindMatchesPreferOnline(p, name);
            if (name == null) return;
            
            List<string> notes = Server.Notes.FindAllExact(name);
            string target = PlayerInfo.GetColoredName(p, name);
            
            if (notes.Count == 0) {
                p.Message("{0} %Shas no notes.", target); return;
            } else {
                p.Message("  Notes for {0}:", target);
            }
            
            foreach (string line in notes) {
                string[] args = line.SplitSpaces();
                if (args.Length <= 3) continue;
                
                if (args.Length == 4) {
                    p.Message(Action(args[1]) + " by " + args[2] + " on " + args[3]);
                } else {
                    p.Message(Action(args[1]) + " by " + args[2] + " on " + args[3]
                                   + " - " + args[4].Replace("%20", " "));
                }
            }
        }
        
        static string Action(string arg) {
            if (arg.CaselessEq("W")) return "Warned";
            if (arg.CaselessEq("K")) return "Kicked";
            if (arg.CaselessEq("M")) return "Muted";
            if (arg.CaselessEq("B")) return "Banned";
            if (arg.CaselessEq("J")) return "Jailed";
            if (arg.CaselessEq("F")) return "Frozen";
            if (arg.CaselessEq("T")) return "Temp-Banned";
            return arg;
        }

        public override void Help(Player p) {
            p.Message("%T/Notes [name] %H- views that player's notes.");
            p.Message("%HNotes are things such as bans, kicks, warns, mutes.");
        }
    }
    
    public sealed class CmdMyNotes : CmdNotes {
        public override string name { get { return "MyNotes"; } }
        public override string type { get { return CommandTypes.Other; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data) { 
            base.Use(p, p.name, data); 
        }

        public override void Help(Player p) {
            p.Message("%T/MyNotes %H- views your own notes.");
            p.Message("%HNotes are things such as bans, kicks, warns, mutes.");
        }
    }
}
