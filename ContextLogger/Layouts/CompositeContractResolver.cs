using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace ContextLogger.Layouts
{
    public class CompositeContractResolver : IContractResolver, IEnumerable<IContractResolver>
    {
        private readonly IList<IContractResolver> _contractResolvers = new List<IContractResolver>();

        internal CompositeContractResolver()
        {
        }

        public JsonContract ResolveContract(Type type)
        {
            return _contractResolvers.Select(r => r.ResolveContract(type)).LastOrDefault(c => c != null);
        }

        public void Add(IContractResolver contractResolver)
        {
            if (contractResolver == null)
            {
                throw new ArgumentNullException(nameof(contractResolver));
            }

            _contractResolvers.Add(contractResolver);
        }

        public IEnumerator<IContractResolver> GetEnumerator()
        {
            return _contractResolvers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
