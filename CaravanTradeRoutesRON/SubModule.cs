using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using System.Xml.Linq;
using TaleWorlds.Library;
using System;

namespace CaravanTradeRoutesRON
{
    public class SubModule : MBSubModuleBase
    {
        public Dictionary<string, List<Town>> tradeRoutes = new Dictionary<string, List<Town>>();
        public readonly string xmlPath = String.Concat(BasePath.Name, "Modules/tradeRoutes.xml");

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is Campaign)) { return; }

            XDocument doc = XDocument.Load(xmlPath);           
            XElement tradeRoutes = doc.Root.Element("tradeRoutes");
            IEnumerable<XElement> listTradeRoutes = tradeRoutes.Elements();
            IEnumerable<XElement> tempTown;
            List<Town> tempTownList = new List<Town>();

            foreach(XElement tradeRouteCount in listTradeRoutes)
            {
                tempTown = tradeRouteCount.Element("Towns").Elements();
                foreach(XElement townCount in tempTown)
                {
                    tempTownList.Add(Campaign.);
                }
            }

            new HarmonyLib.Harmony("WorkshopStashRON.patcher").PatchAll();
        }
    }
}