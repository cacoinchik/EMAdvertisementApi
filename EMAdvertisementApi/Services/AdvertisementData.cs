using System.Collections.Immutable;

namespace EMAdvertisementApi.Services
{

    public class AdvertisementData
    {
        private ImmutableDictionary<string, ImmutableHashSet<string>> advertisementData =
            ImmutableDictionary<string, ImmutableHashSet<string>>.Empty.WithComparers(StringComparer.Ordinal);

        public void Replace(ImmutableDictionary<string, ImmutableHashSet<string>> newAdvertisementData) => Interlocked.Exchange(ref advertisementData, newAdvertisementData);

        public IReadOnlyCollection<string> FindAdvertisementByLocation(string location)
        {
            var loc = (location ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(loc))
                return Array.Empty<string>();

            var result = new HashSet<string>(StringComparer.Ordinal);

            foreach (var prefix in GetPrefixes(loc))
                if (advertisementData.TryGetValue(prefix, out var set))
                    result.UnionWith(set);

            return result.Count == 0 ? Array.Empty<string>() : result.OrderBy(x => x, StringComparer.Ordinal).ToArray();
        }

        private IEnumerable<string> GetPrefixes(string loc)
        {
            yield return loc;

            for (int i = loc.Length - 1; i > 0; i--)
            {
                if (loc[i] == '/')
                    yield return loc[..i];
            }
        }
    }
}
