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
using MCGalaxy.Games;

namespace MCGalaxy.Eco {
    
    public sealed class ReviveItem : SimpleItem {
        
        public ReviveItem() {
            Aliases = new string[] { "revive", "rev" };
            Price = 7;
        }
        
        public override string Name { get { return "Revive"; } }
        
        protected internal override void OnBuyCommand(Player p, string message, string[] args) {
            if (p.money < Price) {
                p.Message("%WYou don't have enough &3" + ServerConfig.Currency + "%W to buy a " + Name + "."); return;
            }
            if (!ZSGame.Instance.Running || !ZSGame.Instance.RoundInProgress) {
                p.Message("You can only buy an revive potion " +
                                   "when a round of zombie survival is in progress."); return;
            }
            
            ZSData data = ZSGame.Get(p);
            if (!data.Infected) {
                p.Message("You are already a human."); return;
            }            
            
            DateTime end = ZSGame.Instance.RoundEnd;
            if (DateTime.UtcNow.AddSeconds(ZSGame.Config.ReviveNoTime) > end) {
                p.Message(ZSGame.Config.ReviveNoTimeMessage); return;
            }
            int count = ZSGame.Instance.Infected.Count;
            if (count < ZSGame.Config.ReviveFewZombies) {
                p.Message(ZSGame.Config.ReviveFewZombiesMessage); return;
            }
            if (data.RevivesUsed >= ZSGame.Config.ReviveTimes) {
                p.Message("You cannot buy any more revive potions."); return;
            }
            if (data.TimeInfected.AddSeconds(ZSGame.Config.ReviveTooSlow) < DateTime.UtcNow) {
                p.Message("%WYou can only revive within the first {0} seconds after you were infected.",
                               ZSGame.Config.ReviveTooSlow); return;
            }
            
            int chance = new Random().Next(1, 101);
            if (chance <= ZSGame.Config.ReviveChance) {
                ZSGame.Instance.DisinfectPlayer(p);
                ZSGame.Instance.Map.Message(p.ColoredName + " %Sused a revive potion. &aIt was super effective!");
            } else {
                ZSGame.Instance.Map.Message(p.ColoredName + " %Stried using a revive potion. &cIt was not very effective..");
            }
            Economy.MakePurchase(p, Price, "%3Revive:");
            data.RevivesUsed++;
        }
        
        protected override void DoPurchase(Player p, string message, string[] args) { }
        
        protected internal override void OnStoreCommand(Player p) {
            int time = ZSGame.Config.ReviveNoTime, expiry = ZSGame.Config.ReviveTooSlow;
            int potions = ZSGame.Config.ReviveTimes;
            p.Message("%T/Buy " + Name);
            OutputItemInfo(p);
            
            p.Message("Lets you rejoin the humans - %Wnot guaranteed to always work");
            p.Message("  Cannot be used in the last &a" + time + " %Sseconds of a round.");
            p.Message("  Can only be used within &a" + expiry + " %Sseconds after being infected.");
            p.Message("  Can only buy &a" + potions + " %Srevive potions per round.");
        }
    }
}
