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
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MCGalaxy.Util {
    internal sealed class PasswordHasher {
        public static string GetPath(string salt) { return "extra/passwords/" + salt + ".dat"; }
        public static bool Exists(string salt) { return File.Exists(GetPath(salt)); }

        internal static byte[] Compute(string salt, string plainText) {
            salt = salt.Replace("<", "(");
            salt = salt.Replace(">", ")");
            plainText = plainText.Replace("<", "(");
            plainText = plainText.Replace(">", ")");

            MD5 hash = MD5.Create();
            byte[] saltB = hash.ComputeHash(Encoding.ASCII.GetBytes(salt));
            byte[] textB = hash.ComputeHash(Encoding.ASCII.GetBytes(plainText));
           
            byte[] data = new byte[saltB.Length + textB.Length];
            Array.Copy(saltB, 0, data, 0, saltB.Length);
            Array.Copy(textB, 0, data, saltB.Length, textB.Length);
            return hash.ComputeHash(data);
        }

        internal static void StoreHash(string salt, string plainText) {
            byte[] hashed = Compute(salt, plainText);
            using (Stream stream = File.Create(GetPath(salt))) {
                stream.Write(hashed, 0, hashed.Length);
            }
        }

        internal static bool MatchesPass(string salt, string plainText) {
            if (!Exists(salt)) return false;
            
            byte[] hashed = File.ReadAllBytes(GetPath(salt));
            byte[] computed = Compute(salt, plainText);
            // Old passwords stored UTF8 string instead of just the raw 16 byte hashes
            // We need to support both since this behaviour was accidentally changed
            if (hashed.Length != computed.Length) {
                return Encoding.UTF8.GetString(hashed) == Encoding.UTF8.GetString(computed);
            }             
            
            for (int i = 0; i < hashed.Length; i++) {
                if (hashed[i] != computed[i]) return false;
            }
            return true;
        }
    }
}
