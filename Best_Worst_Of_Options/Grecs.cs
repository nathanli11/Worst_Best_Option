using System;
using System.Collections.Generic;
using System.Linq;

namespace Best_Worst_Of_Options
{
    public static class Greeks
    {
        // ---------------------------------------------------------
        // 🔥 Delta Pathwise
        // ---------------------------------------------------------
        public static double Delta(Option option, int nbSimulations = 10000)
        {
            // 1️⃣ Simuler les paths
            var paths = MonteCarlo.MonteCarloSimulations(
                option.Underlyings,
                option.PricingDate,
                option.MaturityDate,
                nbSimulations,
                option.Rate
            );

            int lastDay = paths[option.Underlyings[0]].GetLength(1) - 1;

            double[] deltas = new double[nbSimulations];

            // 2️⃣ Boucle Monte Carlo
            for (int p = 0; p < nbSimulations; p++)
            {
                // Stockage des prix finaux
                Dictionary<Stock, double> finalPrices = new();
                foreach (var s in option.Underlyings)
                    finalPrices[s] = paths[s][p, lastDay];

                // Sélection Best-Of / Worst-Of
                var selected = option.PayoffType == PayoffType.BestOf
                    ? finalPrices.OrderByDescending(x => x.Value).First().Key
                    : finalPrices.OrderBy(x => x.Value).First().Key;

                double ST = finalPrices[selected];
                double S0 = selected.HistoricalPrices.OrderBy(h => h.Key).Last().Value;

                // dPayoff/dST → dépend du type Call/Put
                double dPayoff_dST = DerivativePayoff(option, ST);

                // Pathwise: dST/dS0 = ST / S0
                double dST_dS0 = ST / S0;

                deltas[p] = dPayoff_dST * dST_dS0;
            }

            // 3️⃣ Moyenne + actualisation
            return Math.Exp(-option.Rate  option.TimeToMaturity)  deltas.Average();
        }


        // ---------------------------------------------------------
        // 🔥 Dérivée du payoff par rapport à S_T
        // (appelle Call/Put via un cast sécurisé)
        // ---------------------------------------------------------
        private static double DerivativePayoff(Option option, double ST)
        {
            return option switch
            {
                Call c => ST > c.Strike ? 1.0 : 0.0,
                Put p  => ST < p.Strike ? -1.0 : 0.0,
                _      => throw new NotSupportedException("Type d’option non supporté pour Delta Pathwise.")
            };
        }
    }
}