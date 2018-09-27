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

namespace MCGalaxy.Blocks.Physics {

    public static class LeafPhysics {
        
        public static void DoLeaf(Level lvl, ref PhysInfo C) {
            if (lvl.physics > 1) { //Adv physics kills flowers and mushroos in water/lava
                ActivateablePhysics.CheckNeighbours(lvl, C.X, C.Y, C.Z);
            }

            // Just immediately remove from physics list
            if (!lvl.Config.LeafDecay) {
                lvl.leaves.Clear();
                C.Data.Data = PhysicsArgs.RemoveFromChecks; return;
            }
            
            // Delay checking for decay for a random amount of time
            if (C.Data.Data < 5) {
                Random rand = lvl.physRandom;
                if (rand.Next(10) == 0) C.Data.Data++;
                return;
            }
            
            // Perform actual leaf decay, then remove from physics list
            if (DoLeafDecay(lvl, ref C)) lvl.AddUpdate(C.Index, Block.Air, default(PhysicsArgs));
            C.Data.Data = PhysicsArgs.RemoveFromChecks;
        }
        
        static bool DoLeafDecay(Level lvl, ref PhysInfo C) {
            const int dist = 4;
            ushort x = C.X, y = C.Y, z = C.Z;

            for (int xx = -dist; xx <= dist; xx++)
                for (int yy = -dist; yy <= dist; yy++)
                    for (int zz = -dist; zz <= dist; zz++)
            {
                int index = lvl.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz));
                if (index < 0) continue;
                byte type = lvl.blocks[index];
                
                if (type == Block.Log)
                    lvl.leaves[index] = 0;
                else if (type == Block.Leaves)
                    lvl.leaves[index] = -2;
                else
                    lvl.leaves[index] = -1;
            }

            for (int i = 1; i <= dist; i++)
                for (int xx = -dist; xx <= dist; xx++)
                    for (int yy = -dist; yy <= dist; yy++)
                        for (int zz = -dist; zz <= dist; zz++)
            {
                int index = lvl.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz));
                if (index < 0) continue;
                
                if (lvl.leaves[index] == i - 1) {
                    CheckLeaf(lvl, i, x + xx - 1, y + yy, z + zz);
                    CheckLeaf(lvl, i, x + xx + 1, y + yy, z + zz);
                    CheckLeaf(lvl, i, x + xx, y + yy - 1, z + zz);
                    CheckLeaf(lvl, i, x + xx, y + yy + 1, z + zz);
                    CheckLeaf(lvl, i, x + xx, y + yy, z + zz - 1);
                    CheckLeaf(lvl, i, x + xx, y + yy, z + zz + 1);
                }
            }
            return lvl.leaves[C.Index] < 0;
        }
        
        static void CheckLeaf(Level lvl, int i, int x, int y, int z) {
            int index = lvl.PosToInt((ushort)x, (ushort)y, (ushort)z);
            if (index < 0) return;
            
            sbyte type;
            if (lvl.leaves.TryGetValue(index, out type) && type == -2)
                lvl.leaves[index] = (sbyte)i;
        }
    }
}
