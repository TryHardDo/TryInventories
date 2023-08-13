using TryInventories.Models;

namespace TryInventories;

public class ProxyPool
{
    public ProxyPool()
    {
    }

    public ProxyPool(IEnumerable<ProxyEntry> entries)
    {
        AddAll(entries);
    }

    private HashSet<ProxyEntry> Pool { get; set; } = new();
    public int SelectedIndex { get; private set; }
    private readonly Dictionary<string, DateTime> _blackList = new();

    public void Add(ProxyEntry entry)
    {
        if (Pool.Contains(entry)) return;
        Pool.Add(entry);
    }

    public void AddAll(IEnumerable<ProxyEntry> list)
    {
        foreach (var proxyEntry in list)
        {
            if (Pool.Contains(proxyEntry)) continue;
            Pool.Add(proxyEntry);
        }
    }

    /// <summary>
    /// Determines whether a proxy is blacklisted and if its time value is still within
    /// the defined expiration threshold.
    /// </summary>
    /// <param name="entry">The proxy entry to be checked.</param>
    /// <param name="threshold">The threshold time in minutes.</param>
    /// <returns>
    ///   <c>true</c> if the address is on the blacklist and has not exceeded the 
    ///   specified threshold; otherwise, <c>false</c>.
    /// </returns>
    public bool IsBlackListed(ProxyEntry entry, int threshold = 30)
    {
        var address = $"{entry.ProxyAddress}:{entry.Port}";
        return _blackList.ContainsKey(address) && _blackList[address].AddMinutes(threshold) > DateTime.UtcNow;
    }

    public void RemoveFromBlackListCurrent()
    {
        var selected = GetSelected();
        if (selected == null) return;
        var address = $"{selected.ProxyAddress}:{selected.Port}";
        if (!_blackList.ContainsKey(address)) return;
        _blackList.Remove(address);
    }

    public void BlackListCurrent()
    {
        var selected = GetSelected();
        if (selected == null) return;
        var address = $"{selected.ProxyAddress}:{selected.Port}";
        if (_blackList.ContainsKey(address))
        {
            _blackList[address] = DateTime.UtcNow;
            return;
        }

        _blackList.Add(address, DateTime.UtcNow);
    }

    [Obsolete("This method is not used anymore. Use ShufflePoolV2() instead!")]
    public void ShufflePool()
    {
        var poolList = Pool.ToList();
        var random = new Random();
        for (var i = poolList.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (poolList[i], poolList[j]) = (poolList[j], poolList[i]);
        }

        Pool = poolList.ToHashSet();
    }

    public void ShufflePoolV2()
    {
        var random = new Random();
        SelectedIndex = random.Next(0, Pool.Count - 1);
    }

    public ProxyEntry? GetSelected()
    {
        return Pool.Count == 0 ? null : Pool.ElementAt(SelectedIndex);
    }

    public int Rotate()
    {
        var loopCount = 0;

        while (loopCount < Pool.Count - 1)
        {
            if (SelectedIndex + 1 >= Pool.Count)
            {
                SelectedIndex = 0;
            }
            else
            {
                SelectedIndex++;
            }

            if (IsBlackListed(GetSelected() ?? throw new InvalidOperationException("Rotation not possible: No elements found inside the pool!")))
            {
                loopCount++;
                continue;
            }

            RemoveFromBlackListCurrent();
            break;
        }

        if (loopCount == Pool.Count - 1) throw new InvalidOperationException("Rotation not possible: All elements are on black list!");

        return SelectedIndex;
    }
}