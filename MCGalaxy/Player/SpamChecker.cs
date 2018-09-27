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
using System;
using System.Collections.Generic;
using MCGalaxy.Events;
using MCGalaxy.Tasks;

namespace MCGalaxy {
    internal sealed class SpamChecker {
        
        public SpamChecker(Player p) {
            this.p = p;
            blockLog = new List<DateTime>(ServerConfig.BlockSpamCount);
            chatLog = new List<DateTime>(ServerConfig.ChatSpamCount);
            cmdLog = new List<DateTime>(ServerConfig.CmdSpamCount);
        }
        
        Player p;
        readonly object chatLock = new object(), cmdLock = new object();
        readonly List<DateTime> blockLog, chatLog, cmdLog;
        
        public void Clear() {
            blockLog.Clear();
            lock (chatLock)
                chatLog.Clear();
            lock (cmdLock)
                cmdLog.Clear();
        }
        
        public bool CheckBlockSpam() {
            if (p.ignoreGrief || !ServerConfig.BlockSpamCheck) return false;
            if (blockLog.AddSpamEntry(ServerConfig.BlockSpamCount, ServerConfig.BlockSpamInterval)) 
                return false;

            TimeSpan oldestDelta = DateTime.UtcNow - blockLog[0];
            Chat.MessageFromOps(p, "λNICK %Wwas kicked for suspected griefing.");

            Logger.Log(LogType.SuspiciousActivity, 
                       "{0} was kicked for block spam ({1} blocks in {2} seconds)",
                       p.name, blockLog.Count, oldestDelta);
            p.Kick("You were kicked by antigrief system. Slow down.");
            return true;            
        }
        
        public bool CheckChatSpam() {
            Player.lastMSG = p.name;
            if (!ServerConfig.ChatSpamCheck || p.IsSuper) return false;
            
            lock (chatLock) {
                if (chatLog.AddSpamEntry(ServerConfig.ChatSpamCount, ServerConfig.ChatSpamInterval)) 
                    return false;
                
                TimeSpan duration = ServerConfig.ChatSpamMuteTime;
                ModAction action = new ModAction(p.name, Player.Console, ModActionType.Muted, "&0Auto mute for spamming", duration);
                OnModActionEvent.Call(action);
                return true;
            }
        }
        
        public bool CheckCommandSpam() {
            if (!ServerConfig.CmdSpamCheck || p.IsSuper) return false;
            
            lock (cmdLock) {
                if (cmdLog.AddSpamEntry(ServerConfig.CmdSpamCount, ServerConfig.CmdSpamInterval)) 
                    return false;
                
                string blockTime = ServerConfig.CmdSpamBlockTime.Shorten(true, true);
                p.Message("You have been blocked from using commands for "
                          + blockTime + " due to spamming");
                p.cmdUnblocked = DateTime.UtcNow.Add(ServerConfig.CmdSpamBlockTime);
                return true;
            }
        }
    }
}
