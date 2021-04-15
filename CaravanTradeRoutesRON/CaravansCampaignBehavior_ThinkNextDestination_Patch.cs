using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace CaravanTradeRoutesRON
{
    [HarmonyPatch(typeof(CaravansCampaignBehavior), "ThinkNextDestination")]
    public class CaravansCampaignBehavior_ThinkNextDestination_Patch
    {
        public static void Postfix(ref Town __result, MobileParty caravanParty)
        { 
            if(caravanParty.LeaderHero != Hero.MainHero) {  return; }

            bool tradeRouteLoaded = SubModule.tradeRoutes.TryGetValue("westernTradeRoute", out var tradeRoutesTownDictionary);

            if (!tradeRouteLoaded)
            {
                return;
            }

            bool townLoaded = tradeRoutesTownDictionary.TryGetValue(0, out var loadedSettlement);

            if (!townLoaded)
            {
                return;
            }
             
            __result = loadedSettlement;
        }
    }
}