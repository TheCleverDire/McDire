﻿/*
    Copyright 2011 MCForge
        
    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using MCGalaxy.Blocks.Physics;
   
namespace MCGalaxy.Events.LevelEvents {
    
    public delegate void OnLevelLoaded(Level lvl);
    public sealed class OnLevelLoadedEvent : IEvent<OnLevelLoaded> {
        
        public static void Call(Level lvl) {
            if (handlers.Count == 0) return;
            CallCommon(pl => pl(lvl));
        }
    }
    
    public delegate void OnLevelLoad(string level);
    public sealed class OnLevelLoadEvent : IEvent<OnLevelLoad> {
        
        public static void Call(string name) {
            if (handlers.Count == 0) return;
            CallCommon(pl => pl(name));
        }
    }
    
    public delegate void OnLevelSave(Level lvl);
    public sealed class OnLevelSaveEvent : IEvent<OnLevelSave> {
        
        public static void Call(Level lvl) {
            if (handlers.Count == 0) return;
            CallCommon(pl => pl(lvl));
        }
    }
    
    public delegate void OnLevelUnload(Level lvl);
    public sealed class OnLevelUnloadEvent : IEvent<OnLevelUnload> {
        
        public static void Call(Level lvl) {
            if (handlers.Count == 0) return;
            CallCommon(pl => pl(lvl));
        }
    }
    
    public delegate void OnLevelAdded(Level lvl);
    public sealed class OnLevelAddedEvent : IEvent<OnLevelAdded> {
        
        public static void Call(Level lvl) {
            if (handlers.Count == 0) return;
            CallCommon(pl => pl(lvl));
        }
    }
    
    public delegate void OnLevelRemoved(Level lvl);
    public sealed class OnLevelRemovedEvent : IEvent<OnLevelRemoved> {
        
        public static void Call(Level lvl) {
            if (handlers.Count == 0) return;
            CallCommon(pl => pl(lvl));
        }
    }
    
    public delegate void OnPhysicsStateChanged(Level lvl, PhysicsState state);
    public sealed class OnPhysicsStateChangedEvent : IEvent<OnPhysicsStateChanged> {
        
        public static void Call(Level lvl, PhysicsState state) {
            if (handlers.Count == 0) return;
            CallCommon(pl => pl(lvl, state));
        }
    }
    
    public delegate void OnPhysicsLevelChanged(Level lvl, int level);
    public sealed class OnPhysicsLevelChangedEvent : IEvent<OnPhysicsLevelChanged> {
        
        public static void Call(Level lvl, int level) {
            if (handlers.Count == 0) return;
            CallCommon(pl => pl(lvl, level));
        }
    }
    
    public delegate void OnPhysicsUpdate(ushort x, ushort y, ushort z, PhysicsArgs args, Level lvl);
    public sealed class OnPhysicsUpdateEvent : IEvent<OnPhysicsUpdate> {
        public static void Call(ushort x, ushort y, ushort z, PhysicsArgs extraInfo, Level l) {
            
            if (handlers.Count == 0) return;
            CallCommon(pl => pl(x, y, z, extraInfo, l));
        }
    }
}
