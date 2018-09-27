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
using MCGalaxy.DB;
using MCGalaxy.Maths;
using BlockID = System.UInt16;

namespace MCGalaxy.Commands.Building {
    public sealed class CmdSPlace : Command2 {
        public override string name { get { return "SPlace"; } }
        public override string shortcut { get { return "set"; } }
        public override string type { get { return CommandTypes.Building; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Builder; } }
        public override bool SuperUseable { get { return false; } }

        public override void Use(Player p, string message, CommandData data) {
            ushort distance = 0, interval = 0;
            if (message.Length == 0) { Help(p); return; }
            
            string[] parts = message.SplitSpaces();
            if (!CommandParser.GetUShort(p, parts[0], "Distance", ref distance)) return;
            if (parts.Length > 1 && !CommandParser.GetUShort(p, parts[1], "Interval", ref interval)) return;

            if (distance < 1) {
                p.Message("Enter a distance greater than 0."); return;
            }
            if (interval >= distance) {
                p.Message("The Interval cannot be greater than the distance."); return;
            }

            DrawArgs dArgs = new DrawArgs();
            dArgs.distance = distance; dArgs.interval = interval;
            p.Message("Place or break two blocks to determine direction.");
            p.MakeSelection(2, dArgs, DoSPlace);
        }
        
        bool DoSPlace(Player p, Vec3S32[] m, object state, BlockID block) {
            DrawArgs dArgs = (DrawArgs)state;
            ushort distance = dArgs.distance, interval = dArgs.interval;
            if (m[0] == m[1]) { p.Message("No direction was selected"); return false; }
            
            int dirX = 0, dirY = 0, dirZ = 0;
            int dx = Math.Abs(m[1].X - m[0].X), dy = Math.Abs(m[1].Y - m[0].Y), dz = Math.Abs(m[1].Z - m[0].Z);
            if (dy > dx && dy > dz) {
                dirY = m[1].Y > m[0].Y ? 1 : -1;
            } else if (dx > dz) {
                dirX = m[1].X > m[0].X ? 1 : -1;
            } else {
                dirZ = m[1].Z > m[0].Z ? 1 : -1;
            } 
            
            ushort endX = (ushort)(m[0].X + dirX * distance);
            ushort endY = (ushort)(m[0].Y + dirY * distance);
            ushort endZ = (ushort)(m[0].Z + dirZ * distance);
            
            BlockID held = p.GetHeldBlock();
            if (!CommandParser.IsBlockAllowed(p, "place", held)) return false;
            p.level.UpdateBlock(p, endX, endY, endZ, held, BlockDBFlags.Drawn, true);
            
            if (interval > 0) {
                int x = m[0].X, y = m[0].Y, z = m[0].Z;
                int delta = 0;
                while (p.level.IsValidPos(x, y, z) && delta < distance) {
                    p.level.UpdateBlock(p, (ushort)x, (ushort)y, (ushort)z, held, BlockDBFlags.Drawn, true);
                    x += dirX * interval; y += dirY * interval; z += dirZ * interval;
                    delta = Math.Abs(x - m[0].X) + Math.Abs(y - m[0].Y) + Math.Abs(z - m[0].Z);
                }
            } else {
                p.level.UpdateBlock(p, (ushort)m[0].X, (ushort)m[0].Y, (ushort)m[0].Z, held, BlockDBFlags.Drawn, true);
            }

            if (!p.Ignores.DrawOutput) {
                p.Message("Placed {1} blocks {0} apart.",
                               interval > 0 ? interval : distance, Block.GetName(p, held));
            }
            return true;
        }
        
        class DrawArgs { public ushort distance, interval; }

        public override void Help(Player p) {
            p.Message("%T/SPlace [distance] <interval>");
            p.Message("%HMeasures a set [distance] and places your held block at each end.");
            p.Message("%HOptionally place a block at set <interval> between them.");
        }
    }
}
