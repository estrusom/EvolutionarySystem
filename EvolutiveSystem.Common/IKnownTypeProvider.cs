// File: EvolutiveSystem.Common/IKnownTypeProvider.cs
using System;

namespace EvolutiveSystem.Common
{
    /// <summary>
    /// Interfaccia per fornire i tipi noti necessari alla serializzazione XML.
    /// Questo disaccoppia il serializzatore dalla conoscenza dei tipi concreti.
    /// </summary>
    public interface IKnownTypeProvider
    {
        Type[] GetKnownTypes();
    }
}