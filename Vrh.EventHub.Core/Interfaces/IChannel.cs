using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// Egy csatornát reprezentál, amelyen át események közlekednek
    /// </summary>
    public interface IChannel : IDisposable
    {
        /// <summary>
        /// A csatorna típusa
        /// </summary>
        string ChannelType { get; }

        /// <summary>
        /// Bejegyzi a cstornát a csatorna megvalósításnak megfefelő módon
        /// </summary>
        void RegisterMe();

        /// <summary>
        /// Visszadja a megadott szerződést megvalósító Emittert
        ///  Ha nincs még ilyen akkor létrehozza
        /// </summary>
        /// <typeparam name="TContract">Az Emitter által használt szerződés</typeparam>
        /// <param name="id">az emitter azonosítója</param>
        /// <returns>Az emitter példány</returns>
        IEmitter GetEmitter<TContract>(string id) where TContract : BaseContract, new();

        /// <summary>
        /// Visszadja a megadott szerződést megvalósító Collectort
        ///     Ha nincs még ilyen akkor létrehozza
        /// </summary>
        /// <typeparam name="TContract">A Collector által használt szerződés</typeparam>
        /// <param name="id">a collector azonosítója</param>
        /// <returns>A Collector példány</returns>
        ICollector GetCollector<TContract>(string id) where TContract : BaseContract, new();
    }

    /// <summary>
    /// A csatorna által ismert szerződések, és azok atz adott csatornán érvényes emitter és collector objektumjainak tárolására szolgáló objektum
    /// </summary>
    public class KnownContractsWithInfrastucture
    {
        /// <summary>
        /// Szerződés
        /// </summary>
        public IContract Contract { get; set; }

        /// <summary>
        /// Emitterek listája
        /// </summary>
        public List<IEmitter> Emitters { get; set; } = new List<IEmitter>();

        /// <summary>
        /// Collectorok listája
        /// </summary>
        public List<ICollector> Collectors { get; set; } = new List<ICollector>();
    } 
}
