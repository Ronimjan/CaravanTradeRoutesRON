using System;
using TaleWorlds.CampaignSystem;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CaravanTradeRoutesRON
{
    class CaravanTradeRouteCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnAfterNewGameCreated));
        }

        private void OnAfterNewGameCreated(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("change_caravan_behavior", "caravan_talk", "change_caravan_behavior", "I want you to change the way you trade.", new ConversationSentence.OnConditionDelegate(IsOwnedByHeroCondition), null);
            starter.AddDialogLine("change_caravan_behavior_answer", "change_caravan_behavior", "change_caravan_behavior_answer", "Of course. Which way should we trade in the future?", null, null);

            starter.AddPlayerLine("caravan_trade_routes", "change_caravan_behavior_answer", "caravan_trade_routes", "I want you to use a fixed trade route.", null, null);
            starter.AddDialogLine("caravan_trade_routes_answer", "caravan_trade_routes", "caravan_trade_routes_answer", "Okay, which route should we follow?", null, null);

            starter.AddPlayerLine("caravan_normal_trade", "change_caravan_behavior_answer", "caravan_normal_trade", "I want you to trade without a fixed trade route", null, new ConversationSentence.OnConsequenceDelegate(SetBackToNormalBehaviorConsequence));
            starter.AddDialogLine("caravan_normal_trade_answer", "caravan_normal_trade", "caravan_pretalk", "Okay, I gonna decide which towns we are visiting from now on.", null, null);

            foreach (string name in SubModule.tradeRouteList)
            {
                void AddTradeRouteConsequence()
                {
                    bool alreadyIn = SubModule.caravanTradeRoutes.TryGetValue(MobileParty.ConversationParty, out var currentTradeRoute);
                    if (alreadyIn)
                    {
                        SubModule.caravanTradeRoutes.Remove(MobileParty.ConversationParty);
                    }
                    SubModule.caravanTradeRoutes.Add(MobileParty.ConversationParty, name);
                }

                starter.AddPlayerLine("trade_route_" + name, "caravan_trade_routes_answer", "trade_route_" + name, "Follow the " + name, null, new ConversationSentence.OnConsequenceDelegate(AddTradeRouteConsequence));
                starter.AddDialogLine("trade_route_" + name + "_answer", "trade_route_" + name, "caravan_pretalk", "Okay, we will follow the " + name, null, null);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (dataStore.IsSaving)
            {
                var jsonString = JsonConvert.SerializeObject(SubModule.currentDestination);
                dataStore.SyncData("currentDestination", ref jsonString);

                jsonString = JsonConvert.SerializeObject(SubModule.caravanTradeRoutes);
                dataStore.SyncData("caravanTradeRoutes", ref jsonString);
            }

            if (dataStore.IsLoading)
            {
                var jsonString = "";
                if (dataStore.SyncData("currenDestination", ref jsonString) && !string.IsNullOrEmpty(jsonString))
                {
                    SubModule.currentDestination = JsonConvert.DeserializeObject<Dictionary<MobileParty, string>>(jsonString);
                }
                else
                {
                    SubModule.currentDestination = new Dictionary<MobileParty, string>();
                }
                if (dataStore.SyncData("caravanTradeRoutes", ref jsonString) && !string.IsNullOrEmpty(jsonString))
                {
                    SubModule.caravanTradeRoutes = JsonConvert.DeserializeObject<Dictionary<MobileParty, string>>(jsonString);
                }
                else
                {
                    SubModule.caravanTradeRoutes = new Dictionary<MobileParty, string>();
                }
            }
        }

        private static bool IsOwnedByHeroCondition()
        {
            if (MobileParty.ConversationParty.LeaderHero == null) { return false; }

            if (MobileParty.ConversationParty.LeaderHero.Clan == Hero.MainHero.Clan) { return true; }

            return false;
        }

        private static void SetBackToNormalBehaviorConsequence()
        {
            bool caravanRegistered = SubModule.caravanTradeRoutes.TryGetValue(MobileParty.ConversationParty, out string tradeRoute);

            if (!caravanRegistered) { return; }

            SubModule.caravanTradeRoutes.Remove(MobileParty.ConversationParty);
        }
    }
}
