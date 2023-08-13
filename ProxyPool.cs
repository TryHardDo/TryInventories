using TryInventories.Models;

namespace TryInventories;

public class ProxyPool
{
    public ProxyPool()
    {
        Pool = new HashSet<ProxyEntry>();
    }

    public ProxyPool(IEnumerable<ProxyEntry> entries)
    {
        Pool = new HashSet<ProxyEntry>();
        AddAll(entries);
    }

    private HashSet<ProxyEntry> Pool { get; set; }
    public int SelectedIndex { get; private set; }

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
        if (SelectedIndex + 1 >= Pool.Count)
            SelectedIndex = 0;
        else
            SelectedIndex++;

        return SelectedIndex;
    }
}