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
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using MCGalaxy.Games;

namespace MCGalaxy.Gui {
    public partial class PropertyWindow : Form {
        GamesHelper lsHelper, ctfHelper, twHelper;
        
        void LoadGameProps() {
            string[] allMaps = LevelInfo.AllMapNames();
            LoadCTFSettings(allMaps);
            LoadLSSettings(allMaps);
            LoadTWSettings(allMaps);
        }

        void SaveGameProps() {
            SaveCTFSettings();
            SaveLSSettings();
            SaveTWSettings();
        }
        
        GamesHelper GetGameHelper(IGame game) {
            // TODO: Find a better way of doing this
            if (game == CTFGame.Instance) return ctfHelper;
            if (game == LSGame.Instance)  return lsHelper;
            if (game == TWGame.Instance)  return twHelper;
            return null;
        }
        
        void HandleMapsChanged(RoundsGame game) {
            GamesHelper helper = GetGameHelper(game);
            if (helper == null) return;
            RunOnUI_Async(() => helper.UpdateMaps());
        }
        
        void HandleStateChanged(IGame game) {
            GamesHelper helper = GetGameHelper(game);
            if (helper == null) return;
            RunOnUI_Async(() => helper.UpdateButtons());
        }
        
        
        void LoadCTFSettings(string[] allMaps) {
            ctfHelper = new GamesHelper(
                CTFGame.Instance, ctf_cbStart, ctf_cbMap, ctf_cbMain,
                ctf_btnStart, ctf_btnStop, ctf_btnEnd,
                ctf_btnAdd, ctf_btnRemove, ctf_lstUsed, ctf_lstNotUsed);
            ctfHelper.Load(allMaps);
        }
        
        void SaveCTFSettings() {
            try {
                ctfHelper.Save();
            } catch (Exception ex) {
                Logger.LogError("Error saving CTF settings", ex);
            }
        }
        

        void LoadLSSettings(string[] allMaps) {
             lsHelper = new GamesHelper(
                LSGame.Instance, ls_cbStart, ls_cbMap, ls_cbMain,
                ls_btnStart, ls_btnStop, ls_btnEnd,
                ls_btnAdd, ls_btnRemove, ls_lstUsed, ls_lstNotUsed);            
            lsHelper.Load(allMaps);
            
            LSConfig cfg = LSGame.Config;
            ls_numMax.Value = cfg.MaxLives;
        }
        
        void SaveLSSettings() {
            try {
                LSConfig cfg = LSGame.Config;
                cfg.MaxLives = (int)ls_numMax.Value;
                
                lsHelper.Save();
                SaveLSMapSettings();
            } catch (Exception ex) {
                Logger.LogError("Error saving Lava Survival settings", ex);
            }
        }
        
        string lsCurMap;
        LSMapConfig lsCurCfg;
        void lsMapUse_SelectedIndexChanged(object sender, EventArgs e) {
            SaveLSMapSettings();
            if (ls_lstUsed.SelectedIndex == -1) {
                ls_grpMapSettings.Text = "Map settings";
                ls_grpMapSettings.Enabled = false;
                lsCurCfg = null;
                return;
            }
            
            lsCurMap = ls_lstUsed.SelectedItem.ToString();
            ls_grpMapSettings.Text = "Map settings (" + lsCurMap + ")";
            ls_grpMapSettings.Enabled = true;
            
            try {
                lsCurCfg = new LSMapConfig();
                lsCurCfg.Load(lsCurMap);
            } catch (Exception ex) {
                Logger.LogError(ex);
                lsCurCfg = null;
            }
            
            if (lsCurCfg == null) return;
            ls_numKiller.Value  = lsCurCfg.KillerChance;
            ls_numFast.Value    = lsCurCfg.FastChance;
            ls_numWater.Value   = lsCurCfg.WaterChance;
            ls_numDestroy.Value = lsCurCfg.DestroyChance;
            
            ls_numLayer.Value = lsCurCfg.LayerChance;
            ls_numCount.Value = lsCurCfg.LayerCount;
            ls_numHeight.Value = lsCurCfg.LayerHeight;
            
            ls_numRound.Value = lsCurCfg.RoundTime;
            ls_numFlood.Value = lsCurCfg.FloodTime;
            ls_numLayerTime.Value = lsCurCfg.LayerInterval;
        }
        
        void SaveLSMapSettings() {
            if (lsCurCfg == null) return;
            lsCurCfg.KillerChance  = (int)ls_numKiller.Value;
            lsCurCfg.FastChance    = (int)ls_numFast.Value;
            lsCurCfg.WaterChance   = (int)ls_numWater.Value;
            lsCurCfg.DestroyChance = (int)ls_numDestroy.Value;
            
            lsCurCfg.LayerChance = (int)ls_numLayer.Value;
            lsCurCfg.LayerCount  = (int)ls_numCount.Value;
            lsCurCfg.LayerHeight = (int)ls_numHeight.Value;
            
            lsCurCfg.RoundTime = ls_numRound.Value;
            lsCurCfg.FloodTime = ls_numFlood.Value;
            lsCurCfg.LayerInterval = ls_numLayerTime.Value;
            lsCurCfg.Save(lsCurMap);
            
            LSGame game = LSGame.Instance;
            if (game.Running && game.Map.name == lsCurMap) {
                game.UpdateMapConfig();
            }
        }
        
        
        void LoadTWSettings(string[] allMaps) {
             twHelper = new GamesHelper(
                TWGame.Instance, tw_cbStart, tw_cbMap, tw_cbMain,
                tw_btnStart, tw_btnStop, tw_btnEnd,
                tw_btnAdd, tw_btnRemove, tw_lstUsed, tw_lstNotUsed);
            twHelper.Load(allMaps);
            
            TWConfig cfg = TWGame.Config;
            tw_cmbDiff.SelectedIndex = (int)cfg.Difficulty;
            tw_cmbMode.SelectedIndex = (int)cfg.Mode;
        }
        
        void SaveTWSettings() {
            try {
                TWConfig cfg = TWGame.Config;
                if (tw_cmbDiff.SelectedIndex >= 0) 
                    cfg.Difficulty = (TWDifficulty)tw_cmbDiff.SelectedIndex;
                if (tw_cmbMode.SelectedIndex >= 0)
                    cfg.Mode = (TWGameMode)tw_cmbMode.SelectedIndex;
                twHelper.Save();
                SaveTWMapSettings();
            } catch (Exception ex) {
                Logger.LogError("Error saving TNT wars settings", ex);
            }
        }
        
        string twCurMap;
        TWMapConfig twCurCfg;
        void twMapUse_SelectedIndexChanged(object sender, EventArgs e) {
            SaveTWMapSettings();
            if (tw_lstUsed.SelectedIndex == -1) {
                tw_grpMapSettings.Text = "Map settings";
                tw_grpMapSettings.Enabled = false;
                twCurCfg = null;
                return;
            }
            
            twCurMap = tw_lstUsed.SelectedItem.ToString();
            tw_grpMapSettings.Text = "Map settings (" + twCurMap + ")";
            tw_grpMapSettings.Enabled = true;
            
            try {
                twCurCfg = new TWMapConfig();
                twCurCfg.Load(twCurMap);
            } catch (Exception ex) {
                Logger.LogError(ex);
                twCurCfg = null;
            }
            
            if (twCurCfg == null) return;
            tw_numScoreLimit.Value = twCurCfg.ScoreRequired;
            tw_numScorePerKill.Value = twCurCfg.ScorePerKill;
            tw_numScoreAssists.Value = twCurCfg.AssistScore;
            tw_numMultiKills.Value = twCurCfg.MultiKillBonus;
            tw_cbStreaks.Checked = twCurCfg.Streaks;
            
            tw_cbGrace.Checked = twCurCfg.GracePeriod;
            tw_numGrace.Value = twCurCfg.GracePeriodTime;
            tw_cbBalance.Checked = twCurCfg.BalanceTeams;
            tw_cbKills.Checked = twCurCfg.TeamKills;
        }
        
        void SaveTWMapSettings() {
            if (twCurCfg == null) return;
            twCurCfg.ScoreRequired = (int)tw_numScoreLimit.Value;
            twCurCfg.ScorePerKill = (int)tw_numScorePerKill.Value;
            twCurCfg.AssistScore = (int)tw_numScoreAssists.Value;
            twCurCfg.MultiKillBonus = (int)tw_numMultiKills.Value;
            twCurCfg.Streaks = tw_cbStreaks.Checked;
            
            twCurCfg.GracePeriod = tw_cbGrace.Checked;
            twCurCfg.GracePeriodTime = tw_numGrace.Value;
            twCurCfg.BalanceTeams = tw_cbBalance.Checked;
            twCurCfg.TeamKills = tw_cbKills.Checked;
            twCurCfg.Save(twCurMap);
            
            TWGame game = TWGame.Instance;
            if (game.Running && game.Map.name == twCurMap) {
                game.UpdateMapConfig();
            }
        }       

        void tw_btnAbout_Click(object sender, EventArgs e) {
            string msg = "Difficulty:";
            msg += Environment.NewLine;
            msg += "Easy (2 Hits to die, TNT has long delay)";
            msg += Environment.NewLine;
            msg += "Normal (2 Hits to die, TNT has normal delay)";
            msg += Environment.NewLine;
            msg += "Hard (1 Hit to die, TNT has short delay and team kills on)";
            msg += Environment.NewLine;
            msg += "Extreme (1 Hit to die, TNT has short delay, big explosion and team kills on)";
            
            Popup.Message(msg, "Difficulty");
        }
    }
}
