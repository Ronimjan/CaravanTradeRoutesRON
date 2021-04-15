using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using System.Xml.Linq;
using TaleWorlds.Library;
using System;
using System.Data;
using System.Diagnostics;

namespace CaravanTradeRoutesRON
{
    public class SubModule : MBSubModuleBase
    {
        public static readonly string xmlPath = string.Concat(BasePath.Name, "Modules/CaravanTradeRoutesRON/tradeRoutes.xml");

        public static Dictionary<string, Dictionary<int, Town>> tradeRoutes = new Dictionary<string, Dictionary<int, Town>>();
        public static Dictionary<MobileParty, Town> currentDestination = new Dictionary<MobileParty, Town>();

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is Campaign)) { return; }

            LoadTradeRoutesFromXml();

            new HarmonyLib.Harmony("CaravanTradeRoutesRON.patcher").PatchAll();
        }

        public static void LoadTradeRoutesFromXml()
        {
            XDocument doc = XDocument.Load(xmlPath);
            XElement tradeRoutes = doc.Root.Element("tradeRoutes");
            IEnumerable<XElement> listTradeRoutes = tradeRoutes.Elements();
            IEnumerable<XElement> tempListTown;
            Dictionary<int, Town> tempTownList = new Dictionary<int, Town>();
            int count;

            foreach (XElement tradeRouteCount in listTradeRoutes)
            {
                Debug.DisplayDebugMessage(tradeRouteCount.Attribute("name").Value + "is loaded");

                tempListTown = tradeRouteCount.Element("towns").Elements();
                count = 0;

                foreach (XElement townCount in tempListTown)
                {
                    Debug.DisplayDebugMessage(townCount.Attribute("name").Value + "is loaded");

                    foreach (Settlement town in Campaign.Current.Settlements)
                    {
                        if (town.IsTown && town.Name == townCount.Attributes("name"))
                        {
                            tempTownList.Add(count, town.Town);

                            Debug.DisplayDebugMessage(town.Name + "is loaded into the tempTownList");
                        }
                    }
                    count++;
                }
                tradeRoutes.Add(tradeRouteCount.Attributes("name"), tempTownList);
            }
        }
    }
}