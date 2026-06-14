using System;

namespace Magic_Symbol_Recognizer.Logic
{
    public class BotAI
    {
        private readonly Random _random = new Random();

        public class BotAction
        {
            public string SelectedElement { get; set; }
            public int TargetScore { get; set; }
            public double CastDuration { get; set; }
        }

        public BotAction DetermineNextMove(int botHP, int playerHP, string lastPlayerElement, int playerRanking)
        {
            string chosenElement;

            // Taktische Auswahl des Elements (Bleibt wie zuvor)
            if (botHP < 30 && _random.NextDouble() < 0.6)
            {
                chosenElement = "Light";
            }
            else if (!string.IsNullOrEmpty(lastPlayerElement) && _random.NextDouble() < 0.7)
            {
                chosenElement = GetCounterElement(lastPlayerElement);
            }
            else
            {
                string[] standardElements = { "Fire", "Dark", "Earth", "Thunder", "Water", "Wind" };
                chosenElement = standardElements[_random.Next(standardElements.Length)];
            }

            // ==========================================
            // DYNAMISCHE SKALIERUNG ANHAND DER RANKING PUNKTE
            // ==========================================

            // Ermitteln des Skalierungsfaktors (z.B. 1000 Punkte = 1.0; 1500 Punkte = 1.5)
            double rankFactor = playerRanking / 1000.0;
            if (rankFactor < 0.5) rankFactor = 0.5; // Untergrenze zum Schutz

            // 1. Genauigkeit (Score) berechnen
            // Basis-Werte verschieben sich mit höherem Rang nach oben
            int baseMinScore = (int)(60 * rankFactor);
            int baseMaxScore = (int)(90 * rankFactor);

            // Schutzgrenzen, damit der Bot nicht über 100% oder unter 40% würfelt
            int minScore = Math.Clamp(baseMinScore, 40, 95);
            int maxScore = Math.Clamp(baseMaxScore, 65, 100);

            if (minScore >= maxScore) minScore = maxScore - 5;
            int simulatedScore = _random.Next(minScore, maxScore + 1);

            // 2. Casting-Zeit berechnen (Höheres Ranking = Schnellerer Bot)
            // Basiszeit verringert sich, je höher das Ranking ist
            double maxCastTime = 4.0 / rankFactor;
            double minCastTime = 2.0 / rankFactor;

            // Absolute Unter- und Obergrenzen für die Spielbarkeit fixieren
            maxCastTime = Math.Clamp(maxCastTime, 1.2, 4.0);
            minCastTime = Math.Clamp(minCastTime, 0.8, 2.5);

            double precisionFactor = (simulatedScore - minScore) / (double)(maxScore - minScore == 0 ? 1 : maxScore - minScore);
            double duration = minCastTime + (precisionFactor * (maxCastTime - minCastTime));

            return new BotAction
            {
                SelectedElement = chosenElement,
                TargetScore = simulatedScore,
                CastDuration = Math.Clamp(duration, minCastTime, maxCastTime)
            };
        }

        private string GetCounterElement(string playerElement)
        {
            switch (playerElement)
            {
                case "Fire": return "Water";
                case "Earth": return "Fire";
                case "Wind": return "Earth";
                case "Thunder": return "Wind";
                case "Water": return "Thunder";
                case "Dark": return "Light";
                case "Light": return "Dark";
                default: return "Fire";
            }
        }
    }
}
