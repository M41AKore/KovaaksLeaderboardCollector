using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KovaaksLeaderboardCollector
{
    internal class Stuff
    {
        static List<KvksScenario> scenarios = new List<KvksScenario>()
        {
            new KvksScenario()
            {
                Name = "1w6ts reload v2",
                LeaderboardID = "666",
            },
            new KvksScenario()
            {
                Name = "5 Sphere Hipfire Small",
                LeaderboardID = "125",
            }, 
        };

        static int TaskResultsLimitKvks = 300; // amount of results per taskleaderboard


        public static async Task CollectScenLeaderboards()
        {
            var taskleaderboards = new Dictionary<string, List<kvksLeaderboardUser>>();
            var tasks = new List<Task>();

            foreach (var scen in scenarios)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await collectEntriesFromLdb(scen, taskleaderboards);
                }));
            }
            await Task.WhenAll(tasks);

            Console.WriteLine($"found {taskleaderboards.Count} taskleaderboards!");

            if(!Directory.Exists("./leaderboards/")) Directory.CreateDirectory("./leaderboards/");

            foreach (var leaderboard in taskleaderboards)
            {
                var lines = new List<string>();
                
                foreach (var entry in leaderboard.Value)
                {
                    lines.Add($"{entry.rank}, {entry.score}, {entry.steamID}");
                }

                File.WriteAllLines($"./leaderboards/{leaderboard.Key}.txt", lines.ToArray());
            }
        }
      
        private static async Task collectEntriesFromLdb(KvksScenario scen, Dictionary<string, List<kvksLeaderboardUser>> taskleaderboards)
        {
            int callCount = TaskResultsLimitKvks / 100;
            var result = await KvksAPI.getScenarioLeaderboardByID(scen.LeaderboardID, 0, 100);
            if (result != null)
            {
                for (int i = 1; i < callCount; i++)
                    result.AddRange(await KvksAPI.getScenarioLeaderboardByID(scen.LeaderboardID, i, 100));
            }

            taskleaderboards.Add(scen.Name, result);
            Console.WriteLine($"added {scen.Name} with {result.Count} entries!");
        }    
    }
}
