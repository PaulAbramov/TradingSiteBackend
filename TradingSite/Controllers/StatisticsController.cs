using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using TradingSite.Models;

namespace TradingSite.Controllers
{
    public class StatisticsController : ApiController
    {
        /// <summary>
        /// Get statistics from the database and return it to the website
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/statistics")]
        public HttpResponseMessage GetStatistics()
        {
            HttpResponseMessage response;
            using (TradingSiteEntities db = new TradingSiteEntities())
            {
                Statistics newStatistics = new Statistics
                {
                    SuccessfulTrades = db.TradeOffer.Count(_tradeOffer => _tradeOffer.TradeOffersStatus.ToUpper() == "ACCEPTED"),
                    UsersTotal = db.CustomerUser.Count(),
                };

                List<string> itemsSearched = new List<string>();
                foreach(var item in db.Item)
                {
                    if(itemsSearched.Contains(item.MarketHashName))
                    {
                        continue;
                    }

                    itemsSearched.Add(item.MarketHashName);

                    decimal? itemValue = db.ItemPrice.FirstOrDefault(_itemPrice => _itemPrice.ItemName == item.MarketHashName &&
                                                                          _itemPrice.AppID == item.GameID)?.ItemValue;

                    var amount = db.Item.Count(_item => _item.MarketHashName == item.MarketHashName);

                    if (itemValue != null)
                    {
                        newStatistics.InventoryValue += (itemValue.Value * amount);
                    }
                }

                foreach(var user in db.CustomerUser)
                {
                    if(user.Registered.Date == DateTime.Today)
                    {
                        newStatistics.NewUsersToday++;
                    }
                    if((user.LastTimeActive - DateTime.Now).Hours < 12)
                    {
                        newStatistics.ActiveUsers++;
                    }
                }

                response = new HttpResponseMessage
                {
                    Content = new ObjectContent(typeof(Statistics), newStatistics, new JsonMediaTypeFormatter()),
                    StatusCode = HttpStatusCode.Accepted
                };
            }
            return response;
        }
    }
}
