using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KovaaksLeaderboardCollector
{
    public class KvksBenchmarkUser
    {
        public string? steamID;
        public string? steamName;
        //public KovaaksBenchmark benchmark;
    }

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
        public static async Task<List<Result>> fetchScoresForUser(string leaderboardid, string steam64id)
        {
            string url = "https://kovaaks.com/sa_leaderboard_scores_steam_ids_get";
            HttpClient client = new HttpClient();

            var payload = new
            {
                leaderboard_id = leaderboardid,
                steam_id = steam64id,
                steam_ids = new string[] { steam64id }
            };
            string jsonPayload = JsonConvert.SerializeObject(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("User-Agent", "X-UnrealEngine-Agent");
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            request.Headers.Add("Authorization", "Bearer 140000007407386b2c618e1e3eb4425b01001001a9a90b66180000000100000002000000f15b782a2e4453214f67070001000000b200000032000000040000003eb4425b01001001ce930c00c30e5fae0138a8c000000000aba90b662b5927660100b62e080000000000c9f0e8dc401c8594ca19563a6a6989c4d0d865d8538663ff4329600d3bcbc221ef3223dd25966fb3ccb71fd2a3ee94a8331ed3373c77b3fe4eab5aca10f564e4f9b9fa9e2808581ec851b9966dc30f204f6a43865979ab316c097e27109dcd87b15d72acf74e61511abbb8973c414dba51331987af9815d626367076bfd5f574");
            request.Headers.Add("GSTVersion", "3.4.2.2024-02-28-14-22-08-791139f13a");

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
                List<Result> data = JsonConvert.DeserializeObject<List<Result>>(result);
                return data;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }

            return null;
        }


        public class Attributes
        {
            public double fov { get; set; }
            public string hash { get; set; }
            public double cm360 { get; set; }
            public long epoch { get; set; }
            public int kills { get; set; }
            public int score { get; set; }
            public double avg_fps { get; set; }
            public double avg_ttk { get; set; }
            public string fov_scale { get; set; }
            public double vert_sens { get; set; }
            public double horiz_sens { get; set; }
            public string resolution { get; set; }
            public string sens_scale { get; set; }
            public int accuracy_damage { get; set; }
            public string challenge_start { get; set; }
            public string scenario_version { get; set; }
            public string client_build_version { get; set; }
        }

        public class Result
        {
            public string steam_id { get; set; }
            public string steam_account_name { get; set; }
            public int score { get; set; }
            public int rank { get; set; }
            public bool kovaaks_plus_active { get; set; }
            public Attributes attributes { get; set; }
        }


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

        public static async Task getLeaderboardByID(string leaderboardid)
        {
            string url = "https://kovaaks.com/webapp-backend/leaderboard/scores/global?leaderboardId=" + leaderboardid + "&page=0&max=50";

            HttpClient httpClient = new HttpClient();
            var result = httpClient.GetAsync(url).Result;

            using HttpResponseMessage r = await httpClient.GetAsync(url);

            var jsonResponse = await r.Content.ReadAsStringAsync();
            Console.WriteLine($"{jsonResponse}\n");


            var parts2 = jsonResponse.Split("}");
            var users = new List<kvksLeaderboardUser>();
            foreach (var part in parts2)
            {
                var info = part.Split(",");

                var u = new kvksLeaderboardUser();
                u.rank = info.FirstOrDefault(s => s.Contains("rank"));
                if (u.rank != null) u.rank = u.rank.Replace("\"rank\":", "");

                u.steamID = info.FirstOrDefault(s => s.Contains("steamId"));

                u.steamName = info.FirstOrDefault(s => s.Contains("steamAccountName"));
                if (u.steamName != null) u.steamName = u.steamName.Replace("\"steamAccountName\":\"", "").Replace("\"", string.Empty);

                u.score = info.FirstOrDefault(s => s.Contains("score"));
                if (u.score != null) u.score = u.score.Replace("\"score\":", string.Empty);

                users.Add(u);
            }

            var sb = new StringBuilder();
            users.RemoveAll(k => k.steamID == null && k.steamName == null && k.score == null && k.rank == null);
            foreach (var entry in users.Take(10))
            {
                sb.AppendLine($"``#{entry.rank}`` ``{entry.score}`` {entry.steamName} ");
            }
            //await ReplyAsync(sb.ToString());
        }

        public static async Task<string?> getkvksleaderboardIDFromName(string scenName)
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
        }
    }
}
