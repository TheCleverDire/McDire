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
using System.Data;
using MCGalaxy.SQL;

namespace MCGalaxy.DB {
    
    /// <summary> Converts names to integer ids and back </summary>
    public static class NameConverter {
        
        // NOTE: this restriction is due to BlockDBCacheEntry
        public const int MaxPlayerID = 0x00FFFFFF;
        
        public static string FindName(int id) {
            List<string> invalid = Server.invalidIds.All();
            if (id > MaxPlayerID - invalid.Count)
                return invalid[MaxPlayerID - id];
            
            string name = Database.ReadString("Players", "Name", "WHERE ID=@0", id);
            return name != null ? name : "ID#" + id;
        }
        
        static object ListIds(IDataRecord record, object arg) {
            ((List<int>)arg).Add(record.GetInt32(0));
            return arg;
        }
        
        public static int[] FindIds(string name) {
            List<string> invalid = Server.invalidIds.All();
            List<int> ids = new List<int>();
            
            int i = invalid.CaselessIndexOf(name);
            if (i >= 0) ids.Add(MaxPlayerID - i);
            
            Database.Backend.ReadRows("Players", "ID", ids, ListIds, "WHERE Name=@0", name);
            return ids.ToArray();
        }
        
        public static int InvalidNameID(string name) {
            bool added = Server.invalidIds.AddUnique(name);
            if (added) Server.invalidIds.Save();
            
            int index = Server.invalidIds.All().CaselessIndexOf(name);
            return MaxPlayerID - index;
        }
    }
}