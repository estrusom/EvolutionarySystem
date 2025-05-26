using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIU.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public class TempoAnalisiRisultati
    {
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? Media { get; set; }
        public double? DeviazioneStandard { get; set; }
    }

    public static class DataAnalysis
    {
        public static Dictionary<(string iniziale, string target), TempoAnalisiRisultati> AnalizzaTempi(List<EvolutiveSystem.Core.SerializableDictionary<string, object>> records)
        {
            var groupedData = records
                .Where(r => r.ContainsKey("StringaIniziale") && r.ContainsKey("StringaTarget") && r.ContainsKey("TempoMs"))
                .GroupBy(r => ((string)r["StringaIniziale"], (string)r["StringaTarget"]))
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var tempi = g.Select(r => Convert.ToDouble(r["TempoMs"])).ToList();
                        var risultati = new TempoAnalisiRisultati();

                        if (tempi.Any())
                        {
                            risultati.Min = tempi.Min();
                            risultati.Max = tempi.Max();
                            risultati.Media = tempi.Average();
                            risultati.DeviazioneStandard = CalcolaDeviazioneStandard(tempi, risultati.Media.Value);
                        }

                        return risultati;
                    }
                );
            return groupedData;
        }

        private static double? CalcolaDeviazioneStandard(List<double> valori, double media)
        {
            if (valori.Count <= 1)
            {
                return null;
            }
            double sumOfSquares = valori.Sum(valore => Math.Pow(valore - media, 2));
            return Math.Sqrt(sumOfSquares / (valori.Count - 1));
        }
    }
}
