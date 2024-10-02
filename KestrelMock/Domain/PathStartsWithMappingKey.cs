using System.Collections.Generic;

namespace KestrelMockServer.Domain
{
    public class PathStartsWithMappingKey
	{
        public string PathStartsWith { get; set; }

        public string Method { get; set; }

        public override bool Equals(object obj)
        {
            return obj is PathStartsWithMappingKey key &&
				   PathStartsWith == key.PathStartsWith &&
                   Method == key.Method;
        }

        public override int GetHashCode()
        {
            var hashCode = -1266948330;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PathStartsWith);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Method);
            return hashCode;
        }
    }
}
