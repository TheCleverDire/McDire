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
using System.Security.Cryptography;
using MCGalaxy.Events;
using MCGalaxy.Network;

namespace MCGalaxy.Core {
    internal static class ConnectingHandler {
        
        internal static void HandleConnecting(Player p, string mppass) {
            bool success = HandleConnectingCore(p, mppass);
            if (success) return;
            p.cancelconnecting = true;
        }
        
        static bool HandleConnectingCore(Player p, string mppass) {
            if (p.truename.Length > 16) {
                p.Leave(null, "Usernames must be 16 characters or less", true); return false;
            }
            if (!Player.ValidName(p.truename)) {
                p.Leave(null, "Invalid player name", true); return false;
            }
            
            if (!VerifyName(p, mppass)) return false;
            if (!IPThrottler.CheckIP(p)) return false;
            if (!CheckTempban(p)) return false;

            bool whitelisted = CheckWhitelist(p);
            if (!whitelisted) {
                p.Leave(null, "This is a private server!", true);
                return false;
            }
            
            p.group = Group.GroupIn(p.name);
            if (!CheckBanned(p, whitelisted)) return false;
            if (!CheckPlayersCount(p)) return false;
            return true;
        }
        
        static System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        static object md5Lock = new object();
        
        static bool VerifyName(Player p, string mppass) {
            if (!ServerConfig.VerifyNames) return true;
            
            byte[] hash = null;
            lock (md5Lock)
                hash = md5.ComputeHash(enc.GetBytes(Server.salt + p.truename));
            
            string hashHex = BitConverter.ToString(hash);
            if (!mppass.CaselessEq(hashHex.Replace("-", ""))) {
                if (!HttpUtil.IsPrivateIP(p.ip)) {
                    p.Leave(null, "Login failed! Close the game and sign in again.", true); return false;
                }
            } else {
                p.verifiedName = true;
            }
            return true;
        }
        
        static bool CheckTempban(Player p) {
            try {
                string data = Server.tempBans.FindData(p.name);
                if (data == null) return true;
                
                string banner, reason;
                DateTime expiry;
                Ban.UnpackTempBanData(data, out reason, out banner, out expiry);
                
                if (expiry < DateTime.UtcNow) {
                    Server.tempBans.Remove(p.name);
                    Server.tempBans.Save();
                } else {
                    reason = reason.Length == 0 ? "" :" (" + reason + ")";
                    string delta = (expiry - DateTime.UtcNow).Shorten(true);
                    
                    p.Kick(null, "Banned by " + banner + " for another " + delta + reason, true);
                    return false;
                }
            } catch { }
            return true;
        }

        static bool CheckWhitelist(Player p) {
            if (!ServerConfig.WhitelistedOnly) return true;
            if (!Server.whiteList.Contains(p.name)) return false;
            
            // If verify names is off, check if the player is on the same IP.
            return ServerConfig.VerifyNames || PlayerInfo.FindAccounts(p.ip).Contains(p.name);
        }
        
        static bool CheckPlayersCount(Player p) {
            if (Server.vip.Contains(p.name)) return true;
            
            Player[] online = PlayerInfo.Online.Items;
            if (online.Length >= ServerConfig.MaxPlayers && !HttpUtil.IsPrivateIP(p.ip)) {
                p.Leave(null, "Server full!", true); return false;
            }
            if (p.Rank > LevelPermission.Guest) return true;
            
            online = PlayerInfo.Online.Items;
            int guests = 0;
            foreach (Player pl in online) {
                if (pl.Rank <= LevelPermission.Guest) guests++;
            }
            if (guests < ServerConfig.MaxGuests) return true;
            
            if (ServerConfig.GuestLimitNotify) Chat.MessageOps("Guest " + p.truename + " couldn't log in - too many guests.");
            Logger.Log(LogType.Warning, "Guest {0} couldn't log in - too many guests.", p.truename);
            p.Leave(null, "Server has reached max number of guests", true);
            return false;
        }
        
        static bool CheckBanned(Player p, bool whitelisted) {
            if (Server.bannedIP.Contains(p.ip) && (!ServerConfig.WhitelistedOnly || !whitelisted)) {
                p.Kick(null, ServerConfig.DefaultBanMessage, true);
                return false;
            }
            if (p.Rank != LevelPermission.Banned) return true;
            
            string banner, reason, prevRank;
            DateTime time;
            Ban.GetBanData(p.name, out banner, out reason, out time, out prevRank);
            
            if (banner != null) {
                p.Kick(null, "Banned by " + banner + ": " + reason, true);
            } else {
                p.Kick(null, ServerConfig.DefaultBanMessage, true);
            }
            return false;
        }
    }
}
