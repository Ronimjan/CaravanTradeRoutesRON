using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using System.Xml.Linq;
using TaleWorlds.Library;
using HarmonyLib;
using TaleWorlds.SaveSystem;

namespace CaravanTradeRoutesRON
{
    public class SubModule : MBSubModuleBase
    {
        public static readonly string xmlPath = string.Concat(BasePath.Name, "Modules/CaravanTradeRoutesRON/tradeRoutes.xml");

        public static List<string> tradeRouteList = new List<string>();
        public static Dictionary<string, Dictionary<int, Town>> tradeRoutes = new Dictionary<string, Dictionary<int, Town>>();
        public static Dictionary<MobileParty, string> currentDestination;
        public static Dictionary<MobileParty, string> caravanTradeRoutes;

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            if (!(game.GameType is Campaign)) { return; }

            if (gameStarterObject is CampaignGameStarter starter)
            {
                starter.AddBehavior(new CaravanTradeRouteCampaignBehavior());
            }

            new Harmony("CaravanTradeRoutesRON.patcher").PatchAll();
        }

        public override void OnGameInitializationFinished(Game game)
        {
            if (!(game.GameType is Campaign)) { return; }

            LoadTradeRoutesFromXml();
        }

        public static void LoadTradeRoutesFromXml()
        {
            XDocument doc = XDocument.Load(xmlPath);
            XElement tradeRoutesElement = doc.Root.Element("tradeRoutes");
            IEnumerable<XElement> listTradeRoutes = tradeRoutesElement.Elements();
            IEnumerable<XElement> tempListTown;
            Dictionary<int, Town> tempTownList = new Dictionary<int, Town>();
            int count;

            foreach (XElement tradeRouteCount in listTradeRoutes)
            {

                FileLog.Log(tradeRouteCount.Attribute("name").Value + " is loaded");

                tempListTown = tradeRouteCount.Element("towns").Elements();
                count = 0;

                foreach (XElement townCount in tempListTown)
                {
                    FileLog.Log(townCount.Attribute("name").Value + " is loaded");

                    foreach (Settlement town in Campaign.Current.Settlements)
                    {
                        if (town.IsTown && town.Name.ToString() == townCount.Attribute("name").Value)
                        {
                            tempTownList.Add(count, town.Town);
                            count++;
                            FileLog.Log(town.Name.ToString() + " is loaded into the tempTownList");
                        }
                    }
                }
                tradeRoutes.Add(tradeRouteCount.Attribute("name").Value, new Dictionary<int, Town>(tempTownList));
                tradeRouteList.Add(tradeRouteCount.Attribute("name").Value);
                FileLog.Log(tradeRouteCount.Attribute("name").Value + " is loaded into Dictionary containing " + tempTownList.Count.ToString() + " Elements");
                tempTownList.Clear();
            }
        }
    }
}