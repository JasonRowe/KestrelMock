using System.Collections.Generic;

namespace KestrelMockServer.Domain
{
    public class PathMappingKey
    {
        public string Path { get; set; }

        public string Method { get; set; }

        public override bool Equals(object obj)
        {
            return obj is PathMappingKey key &&
                   Path == key.Path &&
                   Method == key.Method;
        }

        public override int GetHashCode()
        {
            var hashCode = -1266948330;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Method);
            return hashCode;
        }
    }
}
