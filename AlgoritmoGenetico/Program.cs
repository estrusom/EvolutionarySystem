using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoGenetico
{
    internal class Program
    {
        static void Main(string[] args)
        {

            AlgoritmoGenetico algoritmo = new AlgoritmoGenetico(50, 0.01); // 50 individui, 1% prob. mutazione
            algoritmo.Esegui(100); // 100 generazioni

            // Stampa i risultati
            Console.WriteLine("Migliori individui:");
            foreach (var individuo in algoritmo.Popolazione.OrderByDescending(i => i.Fitness).Take(10))
            {
                Console.WriteLine($"Cromosoma: {individuo.Cromosoma}, Fitness: {individuo.Fitness}");
            }
        }
    }
    public class Individuo
    {
        public string Cromosoma { get; set; }
        public double Fitness { get; set; }

        public Individuo(string cromosoma)
        {
            Cromosoma = cromosoma;
        }

        // Funzione per calcolare la fitness (esempio)
        public double CalcolaFitness()
        {
            // ... Logica per calcolare la fitness in base al problema
            // ... Più alto è il valore, più l'individuo è "adatto"
            return Cromosoma.Count(c => c == '1'); // Esempio: conta il numero di '1'
        }
    }
    public class AlgoritmoGenetico
    {
        private Random random = new Random();

        public List<Individuo> Popolazione { get; set; }
        public int DimensionePopolazione { get; set; }
        public double ProbabilitaMutazione { get; set; }

        public AlgoritmoGenetico(int dimensionePopolazione, double probabilitaMutazione)
        {
            DimensionePopolazione = dimensionePopolazione;
            ProbabilitaMutazione = probabilitaMutazione;

            // Inizializzazione della popolazione
            Popolazione = new List<Individuo>();
            for (int i = 0; i < DimensionePopolazione; i++)
            {
                string cromosoma = GeneraCromosomaCasuale(10); // Esempio: cromosoma di 10 bit
                Popolazione.Add(new Individuo(cromosoma));
            }
        }

        // Genera un cromosoma casuale di una certa lunghezza
        private string GeneraCromosomaCasuale(int lunghezza)
        {
            return new string(Enumerable.Repeat('0', lunghezza).Select(x => random.Next(2) == 0 ? '0' : '1').ToArray());
        }

        // Valutazione della fitness di ogni individuo
        public void ValutaFitness()
        {
            foreach (var individuo in Popolazione)
            {
                individuo.Fitness = individuo.CalcolaFitness();
            }
        }

        // Selezione degli individui migliori (esempio: roulette)
        public List<Individuo> Seleziona()
        {
            // ... Implementazione della selezione (es. roulette)
            // ... Restituisce una lista di individui selezionati
            return Popolazione.OrderByDescending(i => i.Fitness).Take(DimensionePopolazione / 2).ToList();
        }

        // Crossover (esempio: a un punto)
        public List<Individuo> Crossover(List<Individuo> genitori)
        {
            // ... Implementazione del crossover (es. a un punto)
            // ... Restituisce una lista di nuovi individui (figli)
            return new List<Individuo>(); // Da implementare
        }

        // Mutazione di un individuo
        public void Muta(Individuo individuo)
        {
            // ... Implementazione della mutazione (es. flipping di bit)
            // ... Modifica il cromosoma dell'individuo
        }

        // Esecuzione dell'algoritmo genetico
        public void Esegui(int numeroGenerazioni)
        {
            for (int generazione = 0; generazione < numeroGenerazioni; generazione++)
            {
                ValutaFitness();
                List<Individuo> selezionati = Seleziona();
                List<Individuo> figli = Crossover(selezionati);

                // Mutazione dei figli
                foreach (var figlio in figli)
                {
                    if (random.NextDouble() < ProbabilitaMutazione)
                    {
                        Muta(figlio);
                    }
                }

                // **CORREZIONE: Combinazione della nuova generazione con i migliori della precedente**
                List<Individuo> nuovaPopolazione = new List<Individuo>();
                nuovaPopolazione.AddRange(figli); // Aggiungi i figli
                nuovaPopolazione.AddRange(selezionati.Take(DimensionePopolazione / 2)); // Aggiungi i migliori della generazione precedente

                Popolazione = nuovaPopolazione; // Aggiorna la popolazione
            }
        }
    }
}
