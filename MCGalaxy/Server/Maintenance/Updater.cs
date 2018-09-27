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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using MCGalaxy.Network;
using MCGalaxy.Tasks;

namespace MCGalaxy {
    public static class Updater {
        
        public static string parent = Path.GetFileName(Assembly.GetEntryAssembly().Location);
        public const string BaseURL = "https://raw.githubusercontent.com/UnknownShadow200/MCGalaxy/master/";
        public const string UploadsURL = "https://github.com/UnknownShadow200/MCGalaxy/tree/master/Uploads";
        const string CurrentVersionFile = BaseURL + "Uploads/current_version.txt";
        const string DLLLocation = BaseURL + "Uploads/MCGalaxy_.dll?raw=true";
        const string ChangelogLocation = BaseURL + "Changelog.txt";
        const string EXELocation = BaseURL + "Uploads/MCGalaxy.exe?raw=true";
        const string CLILocation = BaseURL + "Uploads/MCGalaxyCLI.exe?raw=true";

        public static event EventHandler NewerVersionDetected;
        
        public static void UpdaterTask(SchedulerTask task) {
            UpdateCheck();
            task.Delay = TimeSpan.FromHours(2);
        }

        static void UpdateCheck() {
            if (!ServerConfig.CheckForUpdates) return;
            WebClient client = HttpUtil.CreateWebClient();

            try {
                string raw = client.DownloadString(CurrentVersionFile);
                Version latestVersion = new Version(raw);
                
                if (latestVersion <= Server.Version) {
                    Logger.Log(LogType.SystemActivity, "No update found!");
                } else if (NewerVersionDetected != null) {
                    NewerVersionDetected(null, EventArgs.Empty);
                }
            } catch (Exception ex) {
                Logger.LogError("Error checking for updates", ex);
            }
            
            client.Dispose();
        }

        public static void PerformUpdate() {
            try {
                try {
                    DeleteFiles("Changelog.txt", "MCGalaxy_.update", "MCGalaxy.update", "MCGalaxyCLI.update");
                } catch {
                }
                
                WebClient client = HttpUtil.CreateWebClient();
                client.DownloadFile(DLLLocation, "MCGalaxy_.update");
                client.DownloadFile(EXELocation, "MCGalaxy.update");
                client.DownloadFile(CLILocation, "MCGalaxyCLI.update");
                client.DownloadFile(ChangelogLocation, "Changelog.txt");

                Level[] levels = LevelInfo.Loaded.Items;
                foreach (Level lvl in levels) {
                    if (!lvl.SaveChanges) continue;
                    lvl.Save();
                    lvl.SaveBlockDBChanges();
                }

                Player[] players = PlayerInfo.Online.Items;
                foreach (Player pl in players) pl.save();
                
                bool mono = Type.GetType("Mono.Runtime") != null;
                if (!mono) {
                    Process.Start("Updater.exe", "securitycheck10934579068013978427893755755270374" + parent);
                } else {
                    string path = Path.Combine(Utils.FolderPath, "Updater.exe");
                    Process.Start("mono", path + " securitycheck10934579068013978427893755755270374" + parent);
                }
                Server.Stop(false, "Updating server.");
            } catch (Exception ex) {
                Logger.LogError("Error performing update", ex);
            }
        }
        
        static void DeleteFiles(params string[] files) {
            foreach (string f in files) {
                if (File.Exists(f)) File.Delete(f);
            }
        }
    }
}
