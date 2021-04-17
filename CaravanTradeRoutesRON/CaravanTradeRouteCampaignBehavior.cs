using System;
using TaleWorlds.CampaignSystem;

namespace CaravanTradeRoutesRON
{
    class CaravanTradeRouteCampaignBehavior : CampaignBehaviorBase
    {
        private static string temp = "";
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnAfterNewGameCreated));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnAfterNewGameCreated));
        }

        private void OnAfterNewGameCreated(CampaignGameStarter starter)
        {
            starter.AddDialogLine("change_caravan_behavior", "caravan_companion_talk_start", "change_caravan_behavior", "I want to to change the way to trade.", new ConversationSentence.OnConditionDelegate(IsOwnedByHeroCondition), null);
            starter.AddDialogLine("caravan_trade_routes", "change_caravan_behavior", "caravan_trade_routes", "I want you to use a fixed trade Route.", null, null);
            starter.AddDialogLine("caravan_normal_trade", "change_caravan_behavior", "caravan_normal_trade", "", null, new ConversationSentence.OnConsequenceDelegate(SetBackToNormalBehaviorConsequence));
            foreach(string name in SubModule.tradeRouteList)
            {
                temp = name;
                starter.AddDialogLine("trade_route_" + name, "caravan_trade_routes", "trade_route_" + name, name, null, new ConversationSentence.OnConsequenceDelegate(AddTradeRouteConsequence));
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("currentDestinationDictionary", ref SubModule.currentDestination);
            dataStore.SyncData("tradeRouteDictionary", ref SubModule.caravanTradeRoutes);
        }

        private static bool IsOwnedByHeroCondition()
        {
            if(MobileParty.ConversationParty.LeaderHero.Clan == Hero.MainHero.Clan) { return true; }
            
            return false;
        }

        private static void SetBackToNormalBehaviorConsequence()
        {
            bool caravanRegistered = SubModule.caravanTradeRoutes.TryGetValue(MobileParty.ConversationParty, out string tradeRoute);

            if (!caravanRegistered) { return; }

            SubModule.caravanTradeRoutes.Remove(MobileParty.ConversationParty);
        }

        private static void AddTradeRouteConsequence()
        {
            SubModule.caravanTradeRoutes.Add(MobileParty.ConversationParty, temp);
        }
    }
}
