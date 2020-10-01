using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KestrelMockServer.Domain
{
    public class PathMappingRegexKey
    {
        public Regex Regex { get; set; }

        public string Method { get; set; }

        public override bool Equals(object obj)
        {
            return obj is PathMappingRegexKey key &&
                   EqualityComparer<Regex>.Default.Equals(Regex, key.Regex) &&
                   Method == key.Method;
        }

        public override int GetHashCode()
        {
            var hashCode = 1571525928;
            hashCode = hashCode * -1521134295 + EqualityComparer<Regex>.Default.GetHashCode(Regex);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Method);
            return hashCode;
        }
    }
}
