using System;
using System.Collections.Generic;
using System.Linq;

namespace KovaaksLeaderboardCollector
{
    public class kvksLeaderboardUser
    {
        public string? rank;
        public string? steamID;
        public string? steamName;
        public string? score;
        public string? accuracy;
    }

    public static class KvksAPI
    {
        public static async Task<List<kvksLeaderboardUser>> getScenarioLeaderboardByID(string leaderboardid, int page = 0, int amount = 100)
        {
            string url = "https://kovaaks.com/webapp-backend/leaderboard/scores/global?leaderboardId=" + leaderboardid + "&page=" + page + "&max=" + amount;

            HttpClient httpClient = new HttpClient();
            var result = httpClient.GetAsync(url).Result;

            using HttpResponseMessage r = await httpClient.GetAsync(url);

            var jsonResponse = await r.Content.ReadAsStringAsync();
            //Console.WriteLine($"{jsonResponse}\n");

            var parts2 = jsonResponse.Split("}");

            var totalParts = parts2[0].Split(",")[0];
            int.TryParse(totalParts.Split(":")[1], out int totalEntries);

            var users = new List<kvksLeaderboardUser>();
            foreach (var part in parts2)
            {
                var info = part.Split(",");

                var u = new kvksLeaderboardUser();
                u.rank = info.FirstOrDefault(s => s.Contains("rank"));
                if (u.rank != null) u.rank = u.rank.Replace("\"rank\":", "");

                var id = info.FirstOrDefault(s => s.Contains("steamId"));
                if (id != null)
                    id = id.Replace("\"data\":[{\"steamId\":\"", string.Empty).Replace("\"", string.Empty).Replace("{steamId:", string.Empty).Trim();

                u.steamID = id;

                u.steamName = info.FirstOrDefault(s => s.Contains("steamAccountName"));
                if (u.steamName != null) u.steamName = u.steamName.Replace("\"steamAccountName\":\"", "").Replace("\"", string.Empty);

                u.score = info.FirstOrDefault(s => s.Contains("score"));
                if (u.score != null) u.score = u.score.Replace("\"score\":", string.Empty);

                users.Add(u);
            }

            users.RemoveAll(k => k.steamID == null && k.steamName == null && k.score == null && k.rank == null);
            return users;
        }

        /*public static async Task<string?> getkvksleaderboardIDFromName(string scenName)
        {
            string? foundid = null;

            string url = "https://kovaaks.com/webapp-backend/scenario/popular?page=0&max=20&scenarioNameSearch=" + scenName;

            HttpClient httpClient = new HttpClient();
            var result = httpClient.GetAsync(url).Result;

            using HttpResponseMessage r = await httpClient.GetAsync(url);

            var jsonResponse = await r.Content.ReadAsStringAsync();
            if (jsonResponse.Contains(scenName))
            {
                var index = jsonResponse.IndexOf(scenName);
                if (index != -1)
                {
                    var matchPart = jsonResponse.Substring(0, index);

                    var backwardsContent = new List<char>();

                    for (int i = matchPart.Length; i-- > 0;)
                    {
                        backwardsContent.Insert(0, matchPart[i]);

                        var resultSoFar = new string(backwardsContent.ToArray());
                        if (resultSoFar.Contains("leaderboardId")) break;
                    }

                    var found = new string(backwardsContent.ToArray());
                    var leaderboardID = found.Split(',');
                    var parts2 = leaderboardID[0].Split(':');
                    foundid = parts2[1];
                }
            }

            return foundid;
        }*/
    }
}
