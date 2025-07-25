// File: EvolutiveSystem.Synthesis/PIDOptimizer.cs
// Data di riferimento: 25 luglio 2025 (Versione Definitiva)
// Descrizione: Componente responsabile dell'ottimizzazione dei parametri del controller PID
//              e del termine di Feedforward per il MIUExplorer, in risposta a inefficienze
//              rilevate nel comportamento del sistema.

using System;
using System.Collections.Generic;
using System.Linq; // NECESSARIO per GetValueOrDefault
using System.Threading.Tasks;
using MasterLog;
using MIU.Core; // Per IMIUDataManager
using EvolutiveSystem.Common; // Per PIDParameterKeys, MiuAbstractPattern, RegolaMIU
using EvolutiveSystem.Taxonomy.Antithesis; // Per AntithesisType (se necessario per analisi inefficienze)

namespace EvolutiveSystem.Synthesis
{
    /// <summary>
    /// Il PIDOptimizer è responsabile di analizzare le inefficienze legate al controllo
    /// dell'esplorazione e di suggerire nuovi parametri per il controller PID e il Feedforward.
    /// </summary>
    public class PIDOptimizer
    {
        private readonly Logger _logger;
        private readonly IMIUDataManager _dataManager;

        // Parametri attuali del PID (verranno caricati e salvati)
        private double _kp;
        private double _ki;
        private double _kd;
        private double _feedforwardWeight;
        private int _feedforwardLookaheadDepth;

        /// <summary>
        /// Costruttore di PIDOptimizer.
        /// Inizializza l'ottimizzatore e carica i parametri PID e Feedforward attuali dal database.
        /// </summary>
        /// <param name="logger">L'istanza del logger.</param>
        /// <param name="dataManager">L'istanza del gestore dati per l'interazione con il database MIU.</param>
        public PIDOptimizer(Logger logger, IMIUDataManager dataManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _logger.Log(LogLevel.DEBUG, "PIDOptimizer istanziato.");

            // Carica i parametri attuali all'avvio
            LoadPIDParameters();
        }

        /// <summary>
        /// Carica i parametri PID e Feedforward dal MIUParameterConfigurator.
        /// Se non presenti, usa valori di default.
        /// </summary>
        private void LoadPIDParameters()
        {
            try
            {
                var config = _dataManager.LoadMIUParameterConfigurator();
                string valueStr = "";
                // Uso di TryGetValue al posto di GetValueOrDefault
                _kp = config.TryGetValue(PIDParameterKeys.Kp, out valueStr) && double.TryParse(valueStr, out double kp) ? kp : 1.0;
                _ki = config.TryGetValue(PIDParameterKeys.Ki, out valueStr) && double.TryParse(valueStr, out double ki) ? ki : 0.0;
                _kd = config.TryGetValue(PIDParameterKeys.Kd, out valueStr) && double.TryParse(valueStr, out double kd) ? kd : 0.0;
                _feedforwardWeight = config.TryGetValue(PIDParameterKeys.FeedforwardWeight, out valueStr) && double.TryParse(valueStr, out double ffWeight) ? ffWeight : 0.5;
                _feedforwardLookaheadDepth = config.TryGetValue(PIDParameterKeys.FeedforwardLookaheadDepth, out valueStr) && int.TryParse(valueStr, out int ffDepth) ? ffDepth : 3;


                _logger.Log(LogLevel.INFO, $"[PIDOptimizer] Parametri PID/FF caricati: Kp={_kp}, Ki={_ki}, Kd={_kd}, FF_Weight={_feedforwardWeight}, FF_Depth={_feedforwardLookaheadDepth}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[PIDOptimizer] Errore durante il caricamento dei parametri PID/FF: {ex.Message}. Utilizzo valori di default.");
                _kp = 1.0;
                _ki = 0.0;
                _kd = 0.0;
                _feedforwardWeight = 0.5;
                _feedforwardLookaheadDepth = 3;
            }
        }

        /// <summary>
        /// Salva i parametri PID e Feedforward aggiornati nel MIUParameterConfigurator.
        /// </summary>
        private void SavePIDParameters()
        {
            try
            {
                var config = _dataManager.LoadMIUParameterConfigurator(); // Ricarica per non sovrascrivere altri parametri
                config[PIDParameterKeys.Kp] = _kp.ToString(System.Globalization.CultureInfo.InvariantCulture);
                config[PIDParameterKeys.Ki] = _ki.ToString(System.Globalization.CultureInfo.InvariantCulture);
                config[PIDParameterKeys.Kd] = _kd.ToString(System.Globalization.CultureInfo.InvariantCulture);
                config[PIDParameterKeys.FeedforwardWeight] = _feedforwardWeight.ToString(System.Globalization.CultureInfo.InvariantCulture);
                config[PIDParameterKeys.FeedforwardLookaheadDepth] = _feedforwardLookaheadDepth.ToString();

                _dataManager.SaveMIUParameterConfigurator(config);
                _logger.Log(LogLevel.INFO, $"[PIDOptimizer] Parametri PID/FF salvati: Kp={_kp}, Ki={_ki}, Kd={_kd}, FF_Weight={_feedforwardWeight}, FF_Depth={_feedforwardLookaheadDepth}.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[PIDOptimizer] Errore durante il salvataggio dei parametri PID/FF: {ex.Message}");
            }
        }

        /// <summary>
        /// Analizza le inefficienze e suggerisce nuovi parametri PID e Feedforward.
        /// Questa è la logica di "sintesi" per il controllo.
        /// Per ora, una logica molto semplice. In futuro, qui andranno algoritmi di ottimizzazione.
        /// </summary>
        /// <param name="inefficiencies">La lista delle inefficienze identificate dal TaxonomyEngine.</param>
        /// <returns>True se i parametri sono stati modificati, False altrimenti.</returns>
        public bool OptimizePIDParameters(IReadOnlyList<MiuAbstractPattern> inefficiencies)
        {
            _logger.Log(LogLevel.INFO, $"[PIDOptimizer] Avvio ottimizzazione parametri PID/FF. Inefficienze ricevute: {inefficiencies.Count}.");
            bool parametersChanged = false;

            foreach (var inefficiency in inefficiencies)
            {
                // Ora possiamo accedere a inefficiency.Nome correttamente
                _logger.Log(LogLevel.DEBUG, $"[PIDOptimizer] Analisi inefficienza: {inefficiency.Nome} (Pattern: {inefficiency.Type}:{inefficiency.Value})");

                // Esempio di logica di ottimizzazione molto semplice:
                // Se c'è una qualsiasi inefficienza, facciamo un piccolo aggiustamento di esempio
                // In futuro, qui andranno algoritmi di ottimizzazione più sofisticati
                // basati sul tipo e gravità dell'inefficienza (es. AntithesisType.ControlOscillation, ControlStall, ecc.).
                // Per ora, un aggiustamento generico per dimostrazione.
                _kp += 0.01; // Piccolissimo aggiustamento per dimostrazione
                _feedforwardWeight = Math.Min(1.0, _feedforwardWeight + 0.01); // Aumenta il peso del FF
                parametersChanged = true;
                _logger.Log(LogLevel.INFO, $"[PIDOptimizer] Aggiustamento di esempio: Kp={_kp}, Ki={_ki}, Kd={_kd}, FF_Weight={_feedforwardWeight}."); // Log all params
            }

            if (parametersChanged)
            {
                SavePIDParameters(); // Salva i nuovi parametri se sono stati modificati
                _logger.Log(LogLevel.INFO, "[PIDOptimizer] Parametri PID/FF aggiornati e salvati.");
            }
            else
            {
                _logger.Log(LogLevel.INFO, "[PIDOptimizer] Nessun aggiustamento significativo ai parametri PID/FF necessario in questo ciclo.");
            }

            return parametersChanged;
        }

        // Metodi per accedere ai parametri attuali (potrebbero essere usati dal MIUExplorer)
        public double GetKp() => _kp;
        public double GetKi() => _ki;
        public double GetKd() => _kd;
        public double GetFeedforwardWeight() => _feedforwardWeight;
        public int GetFeedforwardLookaheadDepth() => _feedforwardLookaheadDepth;
    }
}
