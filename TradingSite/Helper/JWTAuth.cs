using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using TradingSite.Models.Auth;

namespace TradingSite.Helper
{
    public class JWTAuth
    {
        private static string secret = @"";

        /// <summary>
        /// Generate a token with the current time and the a temporaryUserID
        /// </summary>
        /// <param name="_temporaryUserID"></param>
        /// <returns></returns>
        public static string GenerateToken(long _temporaryUserID, DateTime _expirationDate)
        {
            var payload = new JwtPayload
            {
                {"token", new Token{ExpirationDate = _expirationDate, TemporaryUserID = _temporaryUserID}}
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var header = new JwtHeader(credentials);
            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();
            var tokenstring = handler.WriteToken(secToken);

            return tokenstring;
        }

        /// <summary>
        /// Validate the token, check the remaining time and if it is valid return the users ID
        /// </summary>
        /// <param name="_accessToken"></param>
        /// <returns></returns>
        public static long? ValidateToken(string _accessToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(_accessToken);
                JToken jtoken = (JToken) token.Payload.First().Value;
                var tokenObject = jtoken.ToObject<Token>();

                TimeSpan ts = DateTime.UtcNow - tokenObject.ExpirationDate;

                if (ts.Minutes < 1)
                {
                    return tokenObject.TemporaryUserID;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}