using System;
using System.IO;
using System.Text.Json;

namespace Magic_Symbol_Recognizer
{
    internal class RankingManager
    {
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ranking.json");
        private const int DefaultRanking = 1000;

        public static int LoadRanking()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    SaveRanking(DefaultRanking);
                    return DefaultRanking;
                }

                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<int>(json);
            }
            catch
            {
                return DefaultRanking;
            }
        }

        public static void SaveRanking(int currentRanking)
        {
            try
            {
                string json = JsonSerializer.Serialize(currentRanking);
                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // Fehler beim Schreiben ignorieren oder loggen
            }
        }
    }
}
