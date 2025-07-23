using ParksComputing.Xfer.Lang.ContractResolvers;
using ParksComputing.Xfer.Lang.Converters;
using System.Collections.Generic;

namespace ParksComputing.Xfer.Lang.Configuration {
    public class XferSerializerSettings {
        public NullValueHandling NullValueHandling { get; set; } = NullValueHandling.Include;
        public IContractResolver ContractResolver { get; set; } = new DefaultContractResolver();
        public IList<IXferConverter> Converters { get; } = new List<IXferConverter>();
    }
}
