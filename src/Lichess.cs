using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ChessBot.Pgn;
using ChessBot.Pgn.Model;

namespace TwitchBot
{
    public class Lichess
    {
        private static string LichessAPIURL = "https://lichess.org/api/";
        internal static HttpClient LichessClient = new HttpClient();

        /// <summary>
        /// Gets latest game information by given username
        /// </summary>
        internal async static Task<PgnGame> GetCurrGameAsync(string username)
        {
            PgnGame game = null;
            string url = $"{LichessAPIURL}user/{username}/current-game";
            HttpResponseMessage response = await LichessClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                game = new PgnGameParser().ParsePgn(await response.Content.ReadAsStringAsync());
            }
            return game;
        }
    }
}