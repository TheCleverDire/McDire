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
using System;

namespace MCGalaxy {

    /// <summary> Importance. Higher priority plugins have their handlers called before lower priority plugins. </summary>
    public enum Priority : byte {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3,
        System_Level = 4
    }
  
    /// <summary> This class provides for more advanced modification to MCGalaxy </summary>
    public abstract partial class Plugin {

        /// <summary> Hooks into events and initalises states/resources etc </summary>
        /// <param name="startup"> True if plugin is being auto loaded due to server starting up, false if manually. </param>
        public abstract void Load(bool startup);
        
        /// <summary> Unhooks from events and disposes of state/resources etc </summary>
        /// <param name="shutdown"> True if plugin is being auto unloaded due to server shutting down, false if manually. </param>
        public abstract void Unload(bool shutdown);
        
        /// <summary> Called when a player does /Help on the plugin. Typically, shows to the player what this plugin is about. </summary>
        /// <param name="p"> Player who is doing /Help. </param>
        public abstract void Help(Player p);
        
        /// <summary> Name of the plugin. </summary>
        public abstract string name { get; }        
        /// <summary> Your website. </summary>
        public abstract string website { get; }        
        /// <summary> Oldest version of MCGalaxy the plugin is compatible with. </summary>
        public abstract string MCGalaxy_Version { get; }        
        /// <summary> Version of your plugin. </summary>
        public abstract int build { get; }      
        /// <summary> Message to display once plugin is loaded. </summary>
        public abstract string welcome { get; }        
        /// <summary> The creator/author of this plugin. (Your name) </summary>
        public abstract string creator { get; }        
        /// <summary> Whether or not to auto load this plugin on server startup. </summary>
        public abstract bool LoadAtStartup { get; }
    }
    
    public abstract class Plugin_Simple : Plugin {

        public override void Help(Player p) {
            p.Message("No help is available for this plugin.");
        }
        
        public override string website { get { return "http://www.example.org"; } }
        public override int build { get { return 0; } }
        public override string welcome { get { return "Plugin " + name + " loaded."; } }
        public override bool LoadAtStartup { get { return true; } }
    }
}

