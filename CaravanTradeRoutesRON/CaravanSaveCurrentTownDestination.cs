using TaleWorlds.CampaignSystem;

namespace CaravanTradeRoutesRON
{
    class CaravanSaveCurrentTownDestination : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            return;
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("currentDestinationDictionary", ref SubModule.currentDestination);
        }
    }
}
