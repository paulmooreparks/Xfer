using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ParksComputing.Xfer.Lang.ContractResolvers {
    public class DefaultContractResolver : IContractResolver {
        public List<PropertyInfo> ResolveProperties(Type type) {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
        }

        public virtual string ResolvePropertyName(string propertyName) {
            return propertyName;
        }
    }
}
