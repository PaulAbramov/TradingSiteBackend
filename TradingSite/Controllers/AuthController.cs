using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using TradingSite.Helper;
using TradingSite.Models;
using TradingSite.Models.Steam;

namespace TradingSite.Controllers
{
    public class AuthController : ApiController
    {
        private readonly WebHelper m_webHelper = new WebHelper();

        /// <summary>
        /// User receives a token and a temporaryID when he joins the site
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/auth/requesttoken")]
        public static HttpResponseMessage RequestToken()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            DateTime expirationDate;

            using (TradingSiteEntities db = new TradingSiteEntities())
            {
                expirationDate = DateTime.UtcNow.AddDays(1);

                db.TemporaryUser.Add(new TemporaryUser { ExpirationTime = expirationDate });
                db.SaveChanges();
            }

            using (TradingSiteEntities db = new TradingSiteEntities())
            {
                foreach (var user in db.TemporaryUser)
                {
                    user.ExpirationTime = user.ExpirationTime.AddTicks(-user.ExpirationTime.Ticks % TimeSpan.TicksPerSecond);
                    expirationDate = expirationDate.AddTicks(-expirationDate.Ticks % TimeSpan.TicksPerSecond);

                    if (user.ExpirationTime == expirationDate)
                    {
                        long userID = user.UserID;

                        response.Content = new StringContent(JWTAuth.GenerateToken(userID, expirationDate));
                        response.StatusCode = HttpStatusCode.Accepted;

                        break;
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// User sends his token and gets his data to show
        /// If the token is not valid then request a new one
        /// </summary>
        /// <param name="_token"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/auth/requestdata")]
        public HttpResponseMessage RequestData([FromBody] string _token)
        {
            long? userID = JWTAuth.ValidateToken(_token);
            CustomerUserJson userJson = null;

            if (userID == null)
            {
                return RequestToken();
            }

            using (TradingSiteEntities db = new TradingSiteEntities())
            {
                string steamid = db.TemporaryUser.FirstOrDefault(_user => _user.UserID == userID)?.SteamID.ToString();

                foreach(var user in db.CustomerUser)
                {
                    if(user.SteamID.ToString() == steamid)
                    {
                        userJson = new CustomerUserJson
                        {
                            SteamID = user.SteamID.ToString(),
                            Avatar = user.Avatar,
                            TradeURL = user.TradeURL,
                            UserName = user.UserName,
                            Registered = user.Registered
                        };

                        user.LastTimeActive = DateTime.Now;

                        break;
                    }
                }

                db.SaveChanges();
            }

            if(userJson == null)
            {
                return RequestToken();
            }

            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(userJson)),
                StatusCode = HttpStatusCode.Accepted
            };

            return response;
        }

        /// <summary>
        /// Steam openID page
        /// If the user doesn't have a valid token anymore, redirect him back to get a new token
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/auth/steamlogin")]
        public HttpResponseMessage Login(HttpRequestMessage _body)
        {
            var queryParams = Uri.UnescapeDataString(_body.RequestUri.Query).Split('=');
            long? userID = JWTAuth.ValidateToken(queryParams.Last());

            var response = Request.CreateResponse(HttpStatusCode.Redirect);

            if (userID == null)
            {
                response.Headers.Location = new Uri("http://localhost:4200/");
                return response;
            }

            response.Headers.Location = new Uri($"https://steamcommunity.com/openid/login?openid.mode=checkid_setup&openid.ns=http://specs.openid.net/auth/2.0&openid.identity=http://specs.openid.net/auth/2.0/identifier_select&openid.claimed_id=http://specs.openid.net/auth/2.0/identifier_select&openid.return_to=http://localhost:43410/api/auth/authverify/{userID}&openid.realm=http://localhost:43410/");

            return response;
        }

        /// <summary>
        /// get the users profile data, save it to the db and return to the tradingsite
        /// </summary>
        /// <param name="id"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/auth/authverify/{id}")]
        public async Task<HttpResponseMessage> AuthVerify(long id, HttpRequestMessage path)
        {
            string steamid = "";

            var queryParams = Uri.UnescapeDataString(path.RequestUri.Query).Split('&');

            foreach(string param in queryParams)
            {
                if(param.StartsWith("openid.identity"))
                {
                    steamid = param.Split('/').Last();
                    break;
                }
            }

            string url = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=&steamids={steamid}";
            var steamid64 = Convert.ToInt64(steamid);

            var stringresponse = await m_webHelper.GetStringFromRequest(url).ConfigureAwait(false);
            APIResponse<PlayerSummariesJson> playerSummariesResponse = JsonConvert.DeserializeObject<APIResponse<PlayerSummariesJson>>(stringresponse);

            using (TradingSiteEntities db = new TradingSiteEntities())
            {
                var temporaryUser = db.TemporaryUser.FirstOrDefault(_user => _user.UserID == id);
                
                if(temporaryUser != null)
                {
                    temporaryUser.SteamID = steamid64;
                }

                if(db.CustomerUser.FirstOrDefault(_x => _x.SteamID == steamid64) == null)
                {
                    db.CustomerUser.Add(new CustomerUser
                    {
                        SteamID = steamid64,
                        Avatar = playerSummariesResponse.Response.Players.First().AvatarFull,
                        Banned = false,
                        UserName = playerSummariesResponse.Response.Players.First().PersonaName,
                        Registered = DateTime.Now,
                        LastTimeActive = DateTime.Now
                    });
                }

                db.SaveChanges();
            }

            var response = Request.CreateResponse(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri("http://localhost:4200/");

            return response;
        }

        
    }

    /// <summary>
    /// If we receive a response, there is a "response:" tag, which will be here excluded, so we do not get a nullreference
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class APIResponse<T>
    {
        public T Response { get; set; }
    }
}
