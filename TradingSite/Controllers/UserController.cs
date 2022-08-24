using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using TradingSite.Helper;
using TradingSite.Models;

namespace TradingSite.Controllers
{
    public class UserController : ApiController
    {
        private readonly WebHelper m_webHelper = new WebHelper();

        /// <summary>
        /// Get the users inventory
        /// check if the items are tradable
        /// return the whole collection of the items
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> GetInventory([FromBody] string _ids)
        {
            List<WebItem> items = new List<WebItem>();

            var splittedIDs = _ids.Split('/');

            string url = $"https://steamcommunity.com/inventory/{splittedIDs[0]}/{splittedIDs[1]}/{splittedIDs[2]}?l=english&trading=1&count=5000";
            string webResponse = await m_webHelper.GetStringFromRequest(url).ConfigureAwait(false);

            string steamid64 = splittedIDs[0];
            List<ItemPrice> itemswithPrice;
            using (TradingSiteEntities db = new TradingSiteEntities())
            {
                itemswithPrice = db.ItemPrice.ToList();

                db.CustomerUser.First(_user => _user.SteamID.ToString() == steamid64).LastTimeActive = DateTime.Now;
                await db.SaveChangesAsync().ConfigureAwait(false);
            }

            InventoryResponse inventoryResponse = JsonConvert.DeserializeObject<InventoryResponse>(webResponse);

            while (inventoryResponse.More == 1)
            {
                url += $"&start_assetid={inventoryResponse.LastAssetID}";

                webResponse = await m_webHelper.GetStringFromRequest(url).ConfigureAwait(false);

                InventoryResponse inventoryResponseMore = JsonConvert.DeserializeObject<InventoryResponse>(webResponse);

                inventoryResponse.More = inventoryResponseMore.More;
                inventoryResponse.LastAssetID = inventoryResponseMore.LastAssetID;

                inventoryResponse.Assets.AddRange(inventoryResponseMore.Assets);
                inventoryResponse.ItemsDescriptions.AddRange(inventoryResponseMore.ItemsDescriptions);
            }

            List<Tuple<string, string>> classidinstanceidChecked = new List<Tuple<string, string>>();
            foreach (var item in inventoryResponse.Assets)
            {
                if(classidinstanceidChecked.Contains(Tuple.Create(item.ClassID, item.InstanceID)))
                {
                    continue;
                }

                classidinstanceidChecked.Add(Tuple.Create(item.ClassID, item.InstanceID));

                var itemToHandle = inventoryResponse.ItemsDescriptions.First(_itemDescription => _itemDescription.ClassID == item.ClassID && _itemDescription.InstanceID == item.InstanceID);
                //TODO check if classid or instanceid are different if tradable = 0
                if (itemToHandle.Tradable == 0)
                {
                    continue;
                }

                //We do not want to get the price for cards
                ItemPrice itemPrice = itemswithPrice.FirstOrDefault(_itemWithPrice => _itemWithPrice.ItemName == itemToHandle.MarketHashName && _itemWithPrice.AppID == itemToHandle.AppID);

                //Decimal, therefore we have to put the "m" at the end
                if (itemPrice?.ItemValue == null || itemPrice.ItemValue.Value < 0.01m)
                {
                    continue;
                }

                WebItem itemToShow = new WebItem
                {
                    Image = itemToHandle.IconURL,
                    Name = itemToHandle.Name,
                    Price = itemPrice?.ItemValue ?? 0,
                    AppId = splittedIDs[1],
                    ContextId = splittedIDs[2],
                    AssetId = item.AssetID
                };

                var amountOfItems = inventoryResponse.Assets.Count(_item => _item.ClassID == item.ClassID && _item.InstanceID == item.InstanceID);

                if (splittedIDs[1].Equals("753"))
                {
                    if (!itemToHandle.Tags.Any(_tag => !string.IsNullOrEmpty(_tag.LocalizedTagName) && _tag.LocalizedTagName.ToLower() == "trading card"))
                    {
                        continue;
                    }

                    itemToShow.Type = itemToHandle.Tags.FirstOrDefault(_tag => _tag.Category.ToLower() == "item_class")?.LocalizedTagName;
                    itemToShow.Game = itemToHandle.Tags.FirstOrDefault(_tag => _tag.Category.ToLower() == "game")?.LocalizedTagName;
                    itemToShow.Foil = itemToHandle.MarketHashName.ToUpper().Contains("(FOIL");
                }
                else
                {
                    itemToShow.Souvenir = itemToHandle.MarketHashName.ToUpper().Contains("SOUVENIR");
                    itemToShow.StatTrak = itemToHandle.MarketHashName.ToUpper().Contains("STATTRAK");
                    itemToShow.Type = itemToHandle.Tags.FirstOrDefault(_tag => _tag.Category.ToLower() == "type")?.LocalizedTagName;
                    itemToShow.Wear = itemToHandle.Tags.FirstOrDefault(_tag => _tag.Category.ToLower() == "exterior")?.LocalizedTagName;
                }

                for (int i = 0; i < amountOfItems; i++)
                {
                    items.Add(itemToShow);
                }
            }

            var allitemsSerialized = string.Join(",", JsonConvert.SerializeObject(items));

            var response = new HttpResponseMessage
            {
                Content = new StringContent(allitemsSerialized),
                StatusCode = HttpStatusCode.Accepted
            };

            return response;
        }

        /// <summary>
        /// get all items from db and send them to the site
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/user/botsinventories")]
        public HttpResponseMessage GetBotsInventories([FromBody] string _gameid)
        {
            List<WebItem> items = new List<WebItem>();
            List<Item> listOfItems;
            List<ItemPrice> itemswithPrice;
            int appID = int.Parse(_gameid);

            using (TradingSiteEntities db = new TradingSiteEntities())
            {
                listOfItems = db.Item.Where(_item => _item.GameID.Equals(appID)).ToList();
                itemswithPrice = db.ItemPrice.ToList();
            }

            List<string> itemsSearched = new List<string>();
            foreach (var item in listOfItems)
            {
                if (itemsSearched.Contains(item.MarketHashName))
                {
                    continue;
                }

                itemsSearched.Add(item.MarketHashName);

                var itemPrice = itemswithPrice.FirstOrDefault(_itemWithPrice => _itemWithPrice.ItemName == item.MarketHashName && _itemWithPrice.AppID == item.GameID);

                //Decimal, therefore we have to put the "m" at the end
                if (itemPrice?.ItemValue == null || itemPrice.ItemValue.Value < 0.01m)
                {
                    continue;
                }

                //TODO go trough every item, so we have the itemids, or do select them all and get the ids
                var amount = listOfItems.Count(_item => _item.MarketHashName == item.MarketHashName);

                WebItem itemToShow = new WebItem
                {
                    Image = item.IconURL,
                    Name = item.ItemName,
                    Price = itemPrice?.ItemValue ?? 0,
                    Type = item.ItemType,
                    ItemId = item.ItemID.ToString()
                };

                if (item.GameID.Equals(753))
                {
                    itemToShow.Game = item.GameName;
                    itemToShow.Foil = item.MarketHashName.ToUpper().Contains("(FOIL");
                }
                else
                {
                    itemToShow.StatTrak = item.MarketHashName.ToUpper().Contains("STATTRAK");
                    itemToShow.Souvenir = item.MarketHashName.ToUpper().Contains("SOUVENIR");
                    itemToShow.Wear = item.Condition;
                }

                for(int i = 0; i < amount; i++)
                {
                    items.Add(itemToShow);
                }
            }

            var allitemsSerialized = string.Join(",", JsonConvert.SerializeObject(items));

            var response = new HttpResponseMessage
            {
                Content = new StringContent(allitemsSerialized),
                StatusCode = HttpStatusCode.Accepted
            };
            return response;
        }

        [HttpPost]
        [Route("api/user/updatetradetoken")]
        public HttpResponseMessage UpdateTradeToken([FromBody] string _tokenWithUrl)
        {
            string tradeUrl = _tokenWithUrl.Split('|')[0];
            string token = _tokenWithUrl.Split('|')[1];

            string steamid = "";
            long? userID = JWTAuth.ValidateToken(token);

            if (userID == null)
            {
                return AuthController.RequestToken();
            }

            using (TradingSiteEntities db = new TradingSiteEntities())
            {
                foreach (var user in db.TemporaryUser)
                {
                    if (user.UserID == userID)
                    {
                        steamid = user.SteamID.ToString();
                        break;
                    }
                }

                var userToChange = db.CustomerUser.FirstOrDefault(_user => _user.SteamID.ToString() == steamid);

                if (userToChange != null)
                {
                    userToChange.TradeURL = tradeUrl;
                    userToChange.LastTimeActive = DateTime.Now;
                }

                db.SaveChanges();
            }

            var response = Request.CreateResponse(HttpStatusCode.Accepted);

            return response;
        }

        [HttpPost]
        [Route("api/user/maketrade")]
        public HttpResponseMessage MakeTrade([FromBody] string _items)
        {
            var parts = _items.Split('/');

            var theirItems = JsonConvert.DeserializeObject<List<WebItem>>(parts[0]);
            var ourItems = JsonConvert.DeserializeObject<List<WebItem>>(parts[1]);

            var theirItemsString = string.Join(",", theirItems.Select(_item => $"{_item.AppId}_{_item.ContextId}_{_item.AssetId}"));
            var ourItemsString = string.Join(",", ourItems.Select(_item => _item.ItemId));

            long? userID = JWTAuth.ValidateToken(parts.Last());

            if (userID == null)
            {
                return AuthController.RequestToken();
            }

            //TODO validate trade

            using (TradingSiteEntities db = new TradingSiteEntities())
            {
                long steamid = 0;

                foreach (var user in db.TemporaryUser)
                {
                    if (user.UserID == userID)
                    {
                        if(user.SteamID != null)
                        {
                            steamid = user.SteamID.Value;
                        }

                        break;
                    }
                }

                if(steamid != 0)
                {
                    db.TradeOffer.Add(new TradeOffer
                    {
                        SteamID = steamid,
                        TheirItems = theirItemsString,
                        OurItems = ourItemsString,
                        TradeOffersStatus = "Pending",
                        TimeCreated = DateTime.Now
                    });

                    db.SaveChanges();
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.Accepted);

            return response;
        }
    }
}
