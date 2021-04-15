using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace CaravanTradeRoutesRON
{
    class SaveTypeDefiner : SaveableTypeDefiner
    {
        public SaveTypeDefiner() : base(2136100) { }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<MobileParty, string>));
        }
    }
}
