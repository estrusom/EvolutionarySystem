// File: MIUDataService.cs
// Posizione: progetto EvolutionarySystem.Data
// Assicurati che i progetti "EvolutionarySystem.Core.Models" e "Microsoft.EntityFrameworkCore" siano referenziati.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EvolutionarySystem.Core.Models;

namespace EvolutionarySystem.Data
{
    // L'interfaccia IMIUDataService si trova nel progetto Core.Models.
    public class MIUDataService : IMIUDataService
    {
        private readonly EvolutionarySystemContext _context;

        // Inietta il DbContext nel costruttore
        public MIUDataService(EvolutionarySystemContext context)
        {
            _context = context;
        }

        // Metodo corretto che implementa l'interfaccia
        public async Task<(List<MIUPath> Paths, List<MIURuleApplication> RuleApplications)> GetAllDataAsync()
        {
            // Uso i nomi delle proprietà corretti: MIUPaths e MIURuleApplications
            var paths = await _context.MIUPaths.ToListAsync();
            var ruleApplications = await _context.MIURuleApplications.ToListAsync();

            return (paths, ruleApplications);
        }
    }
}
