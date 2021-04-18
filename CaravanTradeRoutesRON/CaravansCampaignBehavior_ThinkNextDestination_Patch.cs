using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace CaravanTradeRoutesRON
{
    [HarmonyPatch(typeof(CaravansCampaignBehavior), "ThinkNextDestination")]
    public class CaravansCampaignBehavior_ThinkNextDestination_Patch
    {
        public static void Postfix(ref Town __result, MobileParty caravanParty)
        {
                if (caravanParty.LeaderHero == null || Hero.MainHero.Clan == null || caravanParty.LeaderHero.Clan != Hero.MainHero.Clan) { return; }
            

            bool caravanRegistered = SubModule.caravanTradeRoutes.TryGetValue(caravanParty, out var tradeRouteString);

            if (!caravanRegistered) { return; }

            bool tradeRouteLoaded = SubModule.tradeRoutes.TryGetValue(tradeRouteString, out var tradeRoutesTownDictionary);

            if (!tradeRouteLoaded) { return; }

            bool destinationLoaded = SubModule.currentDestination.TryGetValue(caravanParty, out string oldTownString);

            int index = -1;

            for(int i = 0; i < tradeRoutesTownDictionary.Count; i++)
            {
                if (destinationLoaded && tradeRoutesTownDictionary[i].Name.ToString() == oldTownString)
                {
                    index = i;
                    break;
                }
            }

            if (index == tradeRoutesTownDictionary.Count - 1) { index = -1; }

            bool townLoaded = tradeRoutesTownDictionary.TryGetValue(index+1, out var loadedSettlement);

            if (!townLoaded) { return; }
             
            __result = loadedSettlement;
            SubModule.currentDestination.Remove(caravanParty);
            SubModule.currentDestination.Add(caravanParty, loadedSettlement.Name.ToString());
        }
    }
}