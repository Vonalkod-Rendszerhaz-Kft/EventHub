using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.EventHub.Core.Exceptions;

namespace Vrh.EventHub.Core.Interfaces
{
    /// <summary>
    /// Csatorna ősosztály, minden megvalósított csatorna ebből származik!
    /// </summary>
    public class BaseChannel : IChannel, IDisposable
    {
        /// <summary>
        /// Visszadja a csatorna típusát
        /// </summary>
        public string ChannelType
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        /// <summary>
        /// Felülírandó!!!
        /// </summary>
        public virtual void Dispose()
        {
            throw new NotImplementedException("Must override this!!!");
        }

        /// <summary>
        /// Felülírandó!!!
        /// </summary>
        public virtual void RegisterMe()
        {
            if (this.GetType().FullName != typeof(BaseChannel).FullName)
            {
                throw new NotImplementedException("Must override this!!!");
            }
        }

        /// <summary>
        /// Visszadja a megadott szerződést megvalósító Emittert
        ///  Ha nincs még ilyen akkor létrehozza
        /// </summary>
        /// <typeparam name="TContract">Az Emitter által használt szerződés</typeparam>
        /// <param name="id">az emitter azonosítója</param>
        /// <returns>Emitter példány</returns>
        public IEmitter GetEmitter<TContract>(string id) where TContract : BaseContract, new()
        {
            KnownContractsWithInfrastucture contract = _knownContracts.FirstOrDefault(x => x.Contract.Id == typeof(TContract).FullName);
            if (contract == null)
            {
                contract = new KnownContractsWithInfrastucture()
                {
                    Contract = new TContract(),
                };
                _knownContracts.Add(contract);
            }
            IEmitter emitter = contract.Emitters.FirstOrDefault(x => x.Id == id);
            if (emitter == null)
            {
                emitter = EmitterFactory(id);
                contract.Emitters.Add(emitter);
            }
            return emitter;           
        }

        /// <summary>
        /// Visszadja a megadott szerződést megvalósító Collectort
        ///  Ha nincs még ilyen akkor létrehozza
        /// </summary>
        /// <typeparam name="TContract">A collector által használt szerződés</typeparam>
        /// <param name="id">A collector azonosítója</param>
        /// <returns>Collector példány</returns>
        public ICollector GetCollector<TContract>(string id) where TContract : BaseContract, new()
        {
            KnownContractsWithInfrastucture contract = _knownContracts.FirstOrDefault(x => x.Contract.Id == typeof(TContract).FullName);
            if (contract == null)
            {
                contract = new KnownContractsWithInfrastucture()
                {
                    Contract = new TContract(),
                };
                _knownContracts.Add(contract);
            }
            ICollector collector = contract.Collectors.FirstOrDefault(x => x.Id == id);
            if (collector == null)
            {
                collector = CollectorFactory(id);
                contract.Collectors.Add(collector);
            }
            return collector;
        }

        /// <summary>
        /// Az ismert szerződések listája
        /// </summary>
        internal List<KnownContractsWithInfrastucture> KnownContracts
        {
            get
            {
                return _knownContracts;
            }
        }

        /// <summary>
        /// Legyárt egy a csatornának megfelellő emittert
        /// </summary>
        /// <param name="id">Az emitter példány azonosítója</param>
        /// <returns>Emitter példány</returns>
        protected virtual IEmitter EmitterFactory(string id)
        {
            if (this.GetType().FullName == typeof(BaseChannel).FullName)
            {
                return new BaseEmitter()
                {
                    Id = id,
                };
            }
            else
            {
                throw new NotImplementedException("Must override this!!!");
            }
        }

        /// <summary>
        /// Legyárt egy  acsatornának megfelellő collectort
        /// </summary>
        /// <param name="id">A collector példány azonosítója</param>
        /// <returns>Collector példány</returns>
        protected virtual ICollector CollectorFactory(string id)
        {
            if (this.GetType().FullName == typeof(BaseChannel).FullName)
            {
                return new BaseCollector()
                {
                    Id = id,
                };
            }
            else
            {
                throw new NotImplementedException("Must override this!!!");
            }
        }

        /// <summary>
        /// A csatorna által ismert szerződések, és azok infrastruktúrája
        /// </summary>
        protected List<KnownContractsWithInfrastucture> _knownContracts = new List<KnownContractsWithInfrastucture>();
    }
}
