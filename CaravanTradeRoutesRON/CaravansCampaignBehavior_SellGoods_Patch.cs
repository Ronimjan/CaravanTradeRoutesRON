using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;

namespace CaravanTradeRoutesRON
{
    [HarmonyPatch(typeof(CaravansCampaignBehavior), "SellGoods")]
    class CaravansCampaignBehavior_SellGoods_Patch
    {
        public static bool Prefix(Town town, MobileParty caravanParty)
        {
            if (caravanParty.LeaderHero == null || Hero.MainHero.Clan == null || caravanParty.LeaderHero.Clan != Hero.MainHero.Clan) { return true; }

            bool caravanRegistered = SubModule.caravanTradeRoutes.TryGetValue(caravanParty, out var tradeRouteString);

            if (!caravanRegistered) { return true; }

            bool tradeRouteLoaded = SubModule.tradeRoutes.TryGetValue(tradeRouteString, out var tradeRoutesTownDictionary);

            if (!tradeRouteLoaded) { return true; }

            bool assigned = false;

            (Town, List<(ItemObject, int)>, List<(ItemObject, int)>, bool, bool) townSettings = (new Town(), new List<(ItemObject, int)>(), new List<(ItemObject, int)>(), false, false);

            for (int i = 0; i < tradeRoutesTownDictionary.Count; i++)
            {
                if (town != tradeRoutesTownDictionary[i].Item1) { continue; }

                townSettings = tradeRoutesTownDictionary[i];
                assigned = true;
            }

            if (!assigned) { return true; }



            foreach ((ItemObject, int) tuple in townSettings.Item3)
            {
                ItemRoster itemRoster = caravanParty.ItemRoster;
                ItemRosterElement itemCopy = new ItemRosterElement();
                bool assignedRosterElement = false;

                for (int i = 0; i < itemRoster.Count; i++)
                {
                    if (itemRoster.GetItemAtIndex(i) == tuple.Item1)
                    {
                        itemCopy = itemRoster.GetElementCopyAtIndex(i);
                        assignedRosterElement = true;
                        break;
                    }
                }

                if (!assignedRosterElement) { continue; }

                if (tuple.Item2 != -1 && tuple.Item2 > 0)
                {
                    while (tuple.Item2 <= town.GetItemPrice(tuple.Item1, caravanParty, false))
                    {
                        SellItemsAction.Apply(caravanParty.Party, town.Owner, itemCopy, 1, town.Owner.Settlement);
                    }
                }
                else if (tuple.Item2 == -1)
                {
                    continue;
                }
                else
                {
                    InformationManager.DisplayMessage(new InformationMessage("An .xml error occured in your tradeRoutes.xml file of the caravanTradeRoutes mod at the city " + town.ToString() + ". A value lower than -1 was entered at the sell option for the good " + tuple.Item1.ToString() + "for the minimum selling price."));
                }
            }

            return !townSettings.Item5;
        }
    }
}
