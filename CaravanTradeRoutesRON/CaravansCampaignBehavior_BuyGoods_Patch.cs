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
using TaleWorlds.Library;

namespace CaravanTradeRoutesRON
{
    [HarmonyPatch(typeof(CaravansCampaignBehavior), "BuyGoods")]
    class CaravansCampaignBehavior_BuyGoods_Patch
    {
        public static bool Prefix(MobileParty caravanParty, Town town)
        {
            if (caravanParty.LeaderHero == null || Hero.MainHero.Clan == null || caravanParty.LeaderHero.Clan != Hero.MainHero.Clan) { return true; }

            bool caravanRegistered = SubModule.caravanTradeRoutes.TryGetValue(caravanParty, out var tradeRouteString);

            if (!caravanRegistered) { return true; }

            bool tradeRouteLoaded = SubModule.tradeRoutes.TryGetValue(tradeRouteString, out var tradeRoutesTownDictionary);

            if (!tradeRouteLoaded) { return true; }

            int caravanCapacity = caravanParty.InventoryCapacity;
            bool assigned = false;
            List<(ItemObject, int, int)> items = new List<(ItemObject, int, int)>();
            int totalItemsPrice = 0;
            float totalItemsWeight = 0f;

            (Town, List<(ItemObject, int)>, List<(ItemObject, int)>, bool, bool) townSettings = (new Town(), new List<(ItemObject, int)>(), new List<(ItemObject, int)>(), false, false);

            for (int i = 0; i < tradeRoutesTownDictionary.Count; i++)
            {
                if (town != tradeRoutesTownDictionary[i].Item1) { continue; }

                townSettings = tradeRoutesTownDictionary[i];
                assigned = true;
            }

            if (!assigned) { return true; }

            foreach ((ItemObject, int) tuple in townSettings.Item2)
            {
                items.Add((tuple.Item1, town.GetItemPrice(tuple.Item1, caravanParty, false), tuple.Item2));
                totalItemsPrice += town.GetItemPrice(tuple.Item1, caravanParty, false);
                totalItemsWeight += tuple.Item1.Weight;
            }

            foreach ((ItemObject, int, int) tuple in items)
            {
                ItemRoster itemRoster = town.Owner.ItemRoster;
                ItemRosterElement itemCopy = new ItemRosterElement();
                bool assignedRosterElement = false;
                int amount = 0;

                if (tuple.Item2 > tuple.Item3 && tuple.Item3 != -1) { continue; }

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

                if (!tuple.Item1.IsAnimal)
                {
                    float maxPriceF = ((tuple.Item2 / totalItemsPrice) * caravanParty.PartyTradeGold);
                    int maxAmountByPrice = (int)maxPriceF;
                    float maxWeightF = (tuple.Item1.Weight / totalItemsWeight) * caravanCapacity;
                    int maxAmountByWeight = (int)maxWeightF;
                    amount = Math.Min(maxAmountByPrice, maxAmountByWeight);
                }
                else if (tuple.Item1.HorseComponent != null && (tuple.Item1.HorseComponent.IsLiveStock || tuple.Item1.HorseComponent.IsPackAnimal))
                {
                    float maxPriceF = ((tuple.Item2 / totalItemsPrice) * caravanParty.PartyTradeGold);
                    amount = (int)maxPriceF;

                    int numberOfPackAnimals = caravanParty.ItemRoster.NumberOfPackAnimals;
                    int numberOfLivestockAnimals = caravanParty.ItemRoster.NumberOfLivestockAnimals;
                    if (itemCopy.EquipmentElement.Item.HorseComponent.IsLiveStock && (float)(numberOfLivestockAnimals + amount) > (float)caravanParty.Party.NumberOfAllMembers * 0.6f)
                    {
                        amount = (int)((float)caravanParty.Party.NumberOfAllMembers * 0.6f) - numberOfLivestockAnimals;
                    }
                }
                else if (tuple.Item1.HorseComponent != null && tuple.Item1.HorseComponent.IsMount)
                {
                    float maxPriceF = ((tuple.Item2 / totalItemsPrice) * caravanParty.PartyTradeGold);
                    amount = (int)maxPriceF;
                }

                SellItemsAction.Apply(town.Owner, caravanParty.Party, itemCopy, amount, town.Owner.Settlement);
            }

            return townSettings.Item4;
        }
    }
}
