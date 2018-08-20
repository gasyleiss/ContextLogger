using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ContextLogger.Serialization
{
    class ShouldSerializeContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            var skippedProperties = new[] {"Obj", "Value"};
            property.ShouldSerialize = instance => !skippedProperties.Contains(property.PropertyName);
            return property;
        }
    }
}