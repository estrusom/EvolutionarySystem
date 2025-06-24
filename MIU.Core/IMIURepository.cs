// File: MIU.Core/IMIURepository.cs

using System.Collections.Generic;
using System.Threading.Tasks; // Necessary for async methods
using EvolutiveSystem.Common; // For MiuStateInfo

namespace MIU.Core
{
    // Interface for the high-level MIU data repository operations
    public interface IMIURepository
    {
        // Method to load configuration parameters
        Dictionary<string, string> LoadMIUParameterConfigurator();

        // Method to load MIU states asynchronously
        Task<List<MiuStateInfo>> LoadMIUStatesAsync();

        // Add any other methods here that your MIURepository needs to expose
        // For example:
        // Task SaveMiuStateAsync(MiuStateInfo state);
        // Task<MiuStateInfo> GetMiuStateByIdAsync(long stateId);
    }
}
