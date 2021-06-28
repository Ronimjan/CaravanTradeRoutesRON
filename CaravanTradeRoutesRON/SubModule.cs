using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using System.Xml.Linq;
using TaleWorlds.Library;
using HarmonyLib;

namespace CaravanTradeRoutesRON
{
    public class SubModule : MBSubModuleBase
    {
        public static readonly string xmlPath = string.Concat(BasePath.Name, "Modules/CaravanTradeRoutesRON/tradeRoutes.xml");

        public static float? modVersion;
        public static List<string> tradeRouteList = new List<string>();
        public static Dictionary<string, Dictionary<int, (Town, List<(ItemObject, int)>, List<(ItemObject, int)>, bool, bool)>> tradeRoutes = new Dictionary<string, Dictionary<int, (Town, List<(ItemObject, int)>, List<(ItemObject, int)>, bool, bool)>>();
        public static Dictionary<MobileParty, string> currentDestination = new Dictionary<MobileParty, string>();
        public static Dictionary<MobileParty, string> caravanTradeRoutes = new Dictionary<MobileParty, string>();

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
            Dictionary<int, (Town, List<(ItemObject, int)>, List<(ItemObject, int)>, bool, bool)> tempTownList = new Dictionary<int, (Town, List<(ItemObject, int)>, List<(ItemObject, int)>, bool, bool)>();
            int count;
            List<(ItemObject, int)> buyItemList = new List<(ItemObject, int)>();
            List<(ItemObject, int)> sellItemList = new List<(ItemObject, int)>();
            bool buyOnly = false;
            bool sellOnly = false;

            foreach (XElement tradeRouteCount in listTradeRoutes)
            {
                tempListTown = tradeRouteCount.Element("towns").Elements();
                count = 0;

                foreach (XElement townCount in tempListTown)
                {
                    foreach (Settlement town in Campaign.Current.Settlements)
                    {
                        if (town.IsTown && town.Name.ToString() == townCount.Attribute("name").Value)
                        {
                            XElement buyElement = townCount.Element("Buy Goods");
                            XElement sellElement = townCount.Element("Sell Goods");

                            bool buyInt = int.TryParse(buyElement.Attribute("only").Value, out int BuyValue);
                            if (buyInt == false)
                            {
                                InformationManager.DisplayMessage(new InformationMessage("A .xml problem occured regarding the <buy only=> element at " + town.Name.ToString() + ". A parameter which is not a number was parsed."));
                            }
                            else if (BuyValue == 0)
                            {
                                buyOnly = false;
                            }
                            else if(BuyValue == 1)
                            {
                                buyOnly = true;
                            }
                            else
                            {
                                InformationManager.DisplayMessage(new InformationMessage("A .xml problem occured regarding the <buy only=> element at " + town.Name.ToString() + ". A parameter not 0 or 1 was parsed"));
                            }

                            bool sellInt = int.TryParse(sellElement.Attribute("only").Value, out int SellValue);
                            if (sellInt == false)
                            {
                                InformationManager.DisplayMessage(new InformationMessage("A .xml problem occured regarding the <sell only=> element at " + town.Name.ToString() + ". A parameter which is not a number was parsed."));
                            }
                            else if (SellValue == 0)
                            {
                                sellOnly = false;
                            }
                            else if (SellValue == 1)
                            {
                                sellOnly = true;
                            }
                            else
                            {
                                InformationManager.DisplayMessage(new InformationMessage("A .xml problem occured regarding the <sell only=> element at " + town.Name.ToString() + ". A parameter not 0 or 1 was parsed"));
                            }

                            IEnumerable<XElement> buyItems = buyElement.Elements();
                            IEnumerable<XElement> sellItems = sellElement.Elements();
                            foreach(XElement buyItem in buyItems)
                            {
                                foreach(ItemObject item in ItemObject.All)
                                {
                                    if (buyItem.Attribute("name").Value == item.Name.ToString())
                                    {
                                        if (int.TryParse(buyItem.Attribute("price").Value, out int price))
                                        {
                                            buyItemList.Add((item, int.Parse(buyItem.Attribute("price").Value)));
                                        }
                                    }
                                }
                            }
                            foreach (XElement sellItem in sellItems)
                            {
                                foreach (ItemObject item in ItemObject.All)
                                {
                                    if (sellItem.Attribute("name").Value == item.Name.ToString())
                                    {
                                        if (int.TryParse(sellItem.Attribute("price").Value, out int price))
                                        {
                                            buyItemList.Add((item, int.Parse(sellItem.Attribute("price").Value)));
                                        }
                                    }
                                }
                            }
                            tempTownList.Add(count, (town.Town, buyItemList, sellItemList, buyOnly, sellOnly));
                            count++;
                            buyItemList.Clear();
                            sellItemList.Clear();
                        }
                    }
                }

                bool tradeRouteErrorAlreadyRegistered = tradeRoutes.TryGetValue(tradeRouteCount.Attribute("name").Value, out Dictionary<int, (Town, List<(ItemObject, int)>, List<(ItemObject, int)>, bool, bool)> tradeRouteError);
                if(tradeRouteErrorAlreadyRegistered)
                {
                    bool errorTown1 = true;
                    bool errorTown2 = true;
                    int num = 0;
                    while(errorTown1 && errorTown2)
                    {
                        errorTown1 = tradeRouteError.TryGetValue(num, out (Town, List<(ItemObject, int)>, List<(ItemObject, int)>, bool, bool) townError1);
                        errorTown2 = tempTownList.TryGetValue(num, out (Town, List<(ItemObject, int)>, List<(ItemObject, int)>, bool, bool) townError2);
                        num++;
                    }
                    if (num < tradeRouteError.Count || num < tempTownList.Count)
                    {
                        InformationManager.DisplayMessage(new InformationMessage("While loading the trade route " + tradeRouteCount.Attribute("name").Value + ", a non-identical trade route was found. It one got replaced with the new loaded one."));
                    }
                    else
                    {
                        InformationManager.DisplayMessage(new InformationMessage("While loading the trade route " + tradeRouteCount.Attribute("name").Value + ", an indentical trade route was found. It got replaced with the new loaded one."));
                    }
                    tradeRoutes.Remove(tradeRouteCount.Attribute("name").Value);
                }
                tradeRoutes.Add(tradeRouteCount.Attribute("name").Value, new Dictionary<int, (Town, List<(ItemObject, int)>, List<(ItemObject, int)>, bool, bool)>(tempTownList));
                tradeRouteList.Add(tradeRouteCount.Attribute("name").Value);
                tempTownList.Clear();
            }
        }
    }
}