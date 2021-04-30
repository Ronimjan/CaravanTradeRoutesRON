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
            Dictionary<string, string> currentDestinationString = new Dictionary<string, string>();
            Dictionary<string, string> caravanTradeRoutesString = new Dictionary<string, string>();

            Dictionary<string, string> temp = new Dictionary<string, string>();

            if (dataStore.IsSaving)
            {
                foreach (MobileParty party in Campaign.Current.MobileParties)
                {
                    bool partyRegistredInCurrentDestination = SubModule.currentDestination.TryGetValue(party, out string destination);
                    if (partyRegistredInCurrentDestination)
                    {
                        currentDestinationString.Add(party.ToString(), destination);
                    }

                    bool partyRegistredInTradeRoutes = SubModule.caravanTradeRoutes.TryGetValue(party, out string tradeRoute);
                    if (partyRegistredInTradeRoutes)
                    {
                        caravanTradeRoutesString.Add(party.ToString(), tradeRoute);
                    }
                }

                var jsonString = JsonConvert.SerializeObject(currentDestinationString);
                dataStore.SyncData("currentDestinationString", ref jsonString);

                jsonString = JsonConvert.SerializeObject(caravanTradeRoutesString);
                dataStore.SyncData("caravanTradeRoutesString", ref jsonString);

                jsonString = JsonConvert.SerializeObject(SubModule.modVersion);
                dataStore.SyncData("modVersionCaravanTradeRoutesRON", ref jsonString);
            }

            if (dataStore.IsLoading)
            {
                string jsonString = "";

                if (dataStore.SyncData("modVersionCaravanTradeRoutesRON", ref jsonString) && !string.IsNullOrEmpty(jsonString))
                {
                    SubModule.modVersion = JsonConvert.DeserializeObject<float?>(jsonString);
                }

                if (!SubModule.modVersion.HasValue)
                {
                    if (dataStore.SyncData("currentDestination", ref jsonString) && !string.IsNullOrEmpty(jsonString))
                    {
                        temp = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                        foreach (MobileParty party in Campaign.Current.MobileParties)
                        {
                            bool partyRegistred = temp.TryGetValue(party.ToString(), out string destination);
                            if (partyRegistred)
                            {
                                SubModule.currentDestination.Add(party, destination);
                            }
                        }
                    }
                    else
                    {
                        SubModule.currentDestination = new Dictionary<MobileParty, string>();
                    }

                    if (dataStore.SyncData("caravanTradeRoutes", ref jsonString) && !string.IsNullOrEmpty(jsonString))
                    {
                        temp = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                        foreach (MobileParty party in Campaign.Current.MobileParties)
                        {
                            bool partyRegistred = temp.TryGetValue(party.ToString(), out string tradeRoute);
                            if (partyRegistred)
                            {
                                SubModule.caravanTradeRoutes.Add(party, tradeRoute);
                            }
                        }
                    }
                    else
                    {
                        SubModule.caravanTradeRoutes = new Dictionary<MobileParty, string>();
                    }

                    SubModule.modVersion = 1.3f;
                }
                else
                {
                    if (dataStore.SyncData("currentDestinationString", ref jsonString) && !string.IsNullOrEmpty(jsonString))
                    {
                        currentDestinationString = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
                        foreach (MobileParty party in Campaign.Current.MobileParties)
                        {
                            bool partyRegistred = currentDestinationString.TryGetValue(party.ToString(), out string destination);
                            if (partyRegistred)
                            {
                                SubModule.currentDestination.Add(party, destination);
                            }
                        }
                    }
                    else
                    {
                        SubModule.currentDestination = new Dictionary<MobileParty, string>();
                    }
                    if (dataStore.SyncData("caravanTradeRoutesString", ref jsonString) && !string.IsNullOrEmpty(jsonString))
                    {
                        caravanTradeRoutesString = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
                        foreach (MobileParty party in Campaign.Current.MobileParties)
                        {
                            bool partyRegistred = caravanTradeRoutesString.TryGetValue(party.ToString(), out string tradeRoute);
                            if (partyRegistred)
                            {
                                SubModule.caravanTradeRoutes.Add(party, tradeRoute);
                            }
                        }
                    }
                    else
                    {
                        SubModule.caravanTradeRoutes = new Dictionary<MobileParty, string>();
                    }
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
