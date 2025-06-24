// File: SemanticProcessor.csproj (o un nuovo file MiuExplorationDataManager.cs nel tuo progetto)
// Descrizione: Routine per l l'accesso e la gestione degli stati MIU e dei parametri di esplorazione
//              dal database, pensate per essere integrate nel SemanticProcessor.
// Dipendenze: Necessita dell'accesso alle istanze di IMIUDataManager e del Logger.
// MODIFICATO 24.06.2025: Corretta l'iniezione della dipendenza per utilizzare IMIUDataManager
//                       al posto di MIURepository, risolvendo l'errore finale 'CS1061'.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvolutiveSystem.Common; // Assicurati che questo namespace contenga MiuStateInfo
using MasterLog; // Per il Logger
using MIU.Core; // Per l'interazione con il core, ora include IMIUDataManager

namespace SemanticProcessor // O il namespace appropriato per la tua logica di gestione dati
{
    /// <summary>
    /// Classe di gestione dati per l'esplorazione MIU, focalizzata sul recupero
    /// degli stati e sulla persistenza dei puntatori di esplorazione.
    /// </summary>
    public class MiuExplorationDataManager
    {
        // CORREZIONE CRITICA: Cambiato il tipo di dipendenza da MIURepository a IMIUDataManager.
        // Questo risolve l'errore CS1061 perché ora chiamiamo metodi sull'interfaccia corretta.
        private readonly IMIUDataManager _miuDataManager;
        private readonly Logger _logger;

        /// <summary>
        /// Costruttore. Inietta le dipendenze necessarie.
        /// </summary>
        /// <param name="miuDataManager">Istanza del data manager per l'accesso ai dati MIU.</param>
        /// <param name="logger">Istanza del logger per la registrazione degli eventi.</param>
        public MiuExplorationDataManager(IMIUDataManager miuDataManager, Logger logger)
        {
            _miuDataManager = miuDataManager ?? throw new ArgumentNullException(nameof(miuDataManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.Log(LogLevel.DEBUG, "[MiuExplorationDataManager] Manager di esplorazione istanziato.");
        }

        /// <summary>
        /// Carica tutti gli stati MIU dal database.
        /// Gli stati vengono restituiti come stringhe standard (decompresse).
        /// </summary>
        /// <returns>Una lista di stringhe che rappresentano gli stati MIU.</returns>
        public async Task<List<string>> LoadAllMiuStatesAsync()
        {
            _logger.Log(LogLevel.DEBUG, "[MiuExplorationDataManager] Richiesta caricamento di tutti gli stati MIU.");
            try
            {
                // Chiamata al metodo asincrono LoadMIUStatesAsync() sull'interfaccia IMIUDataManager.
                // Assicurati che IMIUDataManager definisca Task<List<MiuStateInfo>> LoadMIUStatesAsync();
                List<MiuStateInfo> states = await _miuDataManager.LoadMIUStatesAsync();
                List<string> standardStrings = states
                                                    .Select(s => s.CurrentString) // Proprietà CurrentString in MiuStateInfo
                                                    .ToList();

                _logger.Log(LogLevel.INFO, $"[MiuExplorationDataManager] Caricati {standardStrings.Count} stati MIU dal database.");
                return standardStrings;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MiuExplorationDataManager] Errore durante il caricamento degli stati MIU: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Recupera i puntatori di esplorazione (source e target index) dal database.
        /// </summary>
        /// <returns>
        /// Un oggetto anonimo contenente ExplorerCurrentSourceIndex e ExplorerCurrentTargetIndex,
        /// o valori predefiniti (0, 0) se non trovati.
        /// </returns>
        public async Task<(long sourceIndex, long targetIndex)> GetExplorerPointersAsync()
        {
            _logger.Log(LogLevel.DEBUG, "[MiuExplorationDataManager] Richiesta recupero puntatori di esplorazione.");
            try
            {
                // Chiamata al metodo sincrono LoadMIUParameterConfigurator() sull'interfaccia IMIUDataManager.
                // Assicurati che IMIUDataManager definisca Dictionary<string, string> LoadMIUParameterConfigurator();
                Dictionary<string, string> config = _miuDataManager.LoadMIUParameterConfigurator();

                long sourceIndex = 0;
                long targetIndex = 0;

                if (config.TryGetValue("Explorer_CurrentSourceIndex", out string sourceIndexStr) && long.TryParse(sourceIndexStr, out sourceIndex))
                {
                    // Valore già assegnato a sourceIndex
                }
                if (config.TryGetValue("Explorer_CurrentTargetIndex", out string targetIndexStr) && long.TryParse(targetIndexStr, out targetIndex))
                {
                    // Valore già assegnato a targetIndex
                }

                _logger.Log(LogLevel.INFO, $"[MiuExplorationDataManager] Puntatori recuperati: Source={sourceIndex}, Target={targetIndex}.");
                return (sourceIndex, targetIndex);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MiuExplorationDataManager] Errore durante il recupero dei puntatori di esplorazione: {ex.Message}");
                return (0, 0); // Valori predefiniti in caso di errore
            }
        }

        /// <summary>
        /// Salva i nuovi puntatori di esplorazione (source e target index) nel database.
        /// </summary>
        /// <param name="newSourceIndex">Il nuovo indice della stringa sorgente.</param>
        /// <param name="newTargetIndex">Il nuovo indice della stringa target.</param>
        public async Task SaveExplorerPointersAsync(long newSourceIndex, long newTargetIndex)
        {
            _logger.Log(LogLevel.DEBUG, $"[MiuExplorationDataManager] Richiesta salvataggio puntatori di esplorazione: Source={newSourceIndex}, Target={newTargetIndex}.");
            try
            {
                Dictionary<string, string> configToSave = new Dictionary<string, string>
                {
                    { "Explorer_CurrentSourceIndex", newSourceIndex.ToString() },
                    { "Explorer_CurrentTargetIndex", newTargetIndex.ToString() },
                    { "Explorer_LastExplorationTimestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff") } // Aggiorna timestamp
                };

                // Chiamata al metodo sincrono SaveMIUParameterConfigurator() sull'interfaccia IMIUDataManager.
                // Assicurati che IMIUDataManager definisca void SaveMIUParameterConfigurator(Dictionary<string, string> config);
                _miuDataManager.SaveMIUParameterConfigurator(configToSave);

                _logger.Log(LogLevel.INFO, "[MiuExplorationDataManager] Puntatori di esplorazione salvati con successo.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.ERROR, $"[MiuExplorationDataManager] Errore durante il salvataggio dei puntatori di esplorazione: {ex.Message}");
            }
        }
    }
}
