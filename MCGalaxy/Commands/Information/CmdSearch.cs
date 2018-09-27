/*
Copyright 2011-2014 MCGalaxy
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
using System.Text.RegularExpressions;
using BlockID = System.UInt16;

namespace MCGalaxy.Commands.Info {
    public class CmdSearch : Command2 {
        public override string name { get { return "Search"; } }
        public override string type { get { return CommandTypes.Information; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public override bool UseableWhenFrozen { get { return true; } }
        
        public override void Use(Player p, string message, CommandData data) {
            string[] args = message.SplitSpaces(3);
            if (args.Length < 2) { Help(p); return; }
            args[0] = args[0].ToLower();
            string keyword = args[1];
            string modifier = args.Length > 2 ? args[2] : "";
            
            if (args[0] == "block" || args[0] == "blocks") {
                SearchBlocks(p, keyword, modifier);
            } else if (args[0] == "rank" || args[0] == "ranks") {
                SearchRanks(p, keyword, modifier);
            } else if (args[0] == "command" || args[0] == "commands") {
                SearchCommands(p, keyword, modifier);
            } else if (args[0] == "player" || args[0] == "players") {
                SearchPlayers(p, keyword, modifier);
            } else if (args[0] == "loaded") {
                SearchLoaded(p, keyword, modifier);
            } else if (args[0] == "level" || args[0] == "levels" || args[0] == "maps") {
                SearchMaps(p, keyword, modifier);
            } else {
                Help(p);
            }
        }
        
        static void SearchBlocks(Player p, string keyword, string modifier) {
            List<BlockID> blocks = new List<BlockID>();
            BlockDefinition[] defs = p.IsSuper ? BlockDefinition.GlobalDefs : p.level.CustomBlockDefs;
            
            for (int b = 0; b < Block.ExtendedCount; b++) {
                BlockID block = (BlockID)b;
                if (Block.IsPhysicsType(block)) {
                    if (!Block.Undefined(block)) blocks.Add(block);
                } else {
                    if (defs[block] != null) blocks.Add(block);
                }
            }

            List<string> blockNames = FilterList(blocks, keyword, 
                                                 b => Block.GetName(p, b), null,
                                                 b => CmdBlocks.FormatBlockName(p, b));
            OutputList(p, keyword, "search blocks", "blocks", modifier, blockNames);
        }
        
        static void SearchCommands(Player p, string keyword, string modifier) {
            List<string> commands = FilterList(Command.allCmds, keyword, cmd => cmd.name,
                                               null, cmd => CmdHelp.GetColor(cmd) + cmd.name);
            List<string> shortcuts = FilterList(Command.allCmds, keyword, cmd => cmd.shortcut,
                                                cmd => !String.IsNullOrEmpty(cmd.shortcut), 
                                                cmd => CmdHelp.GetColor(cmd) + cmd.name);
            
            // Match both names and shortcuts
            foreach (string shortcutCmd in shortcuts) {
                if (commands.CaselessContains(shortcutCmd)) continue;
                commands.Add(shortcutCmd);
            }
            
            OutputList(p, keyword, "search commands", "commands", modifier, commands);
        }
        
        static void SearchRanks(Player p, string keyword, string modifier) {
            List<string> ranks = FilterList(Group.GroupList, keyword, grp => grp.Name,
                                            null, grp => grp.ColoredName);
            OutputList(p, keyword, "search ranks", "ranks", modifier, ranks);
        }
        
        static void SearchPlayers(Player p, string keyword, string modifier) {
            Player[] online = PlayerInfo.Online.Items;
            List<string> players = FilterList(online, keyword, pl => pl.name,
                                              pl => Entities.CanSee(p, pl), pl => pl.ColoredName);
            OutputList(p, keyword, "search players", "players", modifier, players);
        }
        
        static void SearchLoaded(Player p, string keyword, string modifier) {
            Level[] loaded = LevelInfo.Loaded.Items;
            List<string> levels = FilterList(loaded, keyword, level => level.name);
            OutputList(p, keyword, "search loaded", "loaded levels", modifier, levels);
        }
        
        static void SearchMaps(Player p, string keyword, string modifier) {
            string[] allMaps = LevelInfo.AllMapNames();
            List<string> maps = FilterList(allMaps, keyword, map => map);
            OutputList(p, keyword, "search levels", "maps", modifier, maps);
        }
        
        internal static List<string> FilterList<T>(IList<T> input, string keyword, StringFormatter<T> formatter,
                                          Predicate<T> filter = null, StringFormatter<T> listFormatter = null) {
            List<string> matches = new List<string>();
            Regex regex = null;
            // wildcard matching
            if (keyword.Contains("*") || keyword.Contains("?")) {
                string pattern = "^" + Regex.Escape(keyword).Replace("\\?", ".").Replace("\\*", ".*") + "$";
                regex = new Regex(pattern, RegexOptions.IgnoreCase);
            }
            
            foreach (T item in input) {
                if (filter != null && !filter(item)) continue;
                string name = formatter(item);
                
                if (regex != null) { if (!regex.IsMatch(name))  continue; }
                else { if (!name.CaselessContains(keyword))     continue; }
                
                // format this item for display
                if (listFormatter != null) name = listFormatter(item);
                matches.Add(name);
            }
            return matches;
        }
        
        static void OutputList(Player p, string keyword, string cmd, string type, string modifier, List<string> items) {
            if (items.Count == 0) {
                p.Message("No {0} found containing \"{1}\"", type, keyword);
            } else {
                MultiPageOutput.Output(p, items, item => item, cmd + " " + keyword, type, modifier, false);
            }
        }
        
        public override void Help(Player p) {
            p.Message("%T/Search [list] [keyword]");
            p.Message("%HFinds entries in a list that match the given keyword");
            p.Message("%H  keyword can also include wildcard characters:");
            p.Message("%H    * - placeholder for zero or more characters");
            p.Message("%H    ? - placeholder for exactly one character");
            p.Message("%HLists available: &fblocks/commands/ranks/players/loaded/maps");
        }
    }
}
