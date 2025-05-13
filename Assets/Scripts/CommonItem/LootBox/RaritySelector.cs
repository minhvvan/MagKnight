using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RaritySelector
{
    private static List<(ItemRarity rarity, float probability)> rarityTable = new()
    {
        (ItemRarity.Common, 0.7f),
        (ItemRarity.Uncommon, 0.15f),
        (ItemRarity.Rare, 0.07f),
        (ItemRarity.Epic, 0.05f),
        (ItemRarity.Legendary, 0.03f)
    };

    public static ItemRarity GetRandomRarity()
    {
        float rand = Random.value; // 0.0 ~ 1.0
        float cumulative = 0f;

        foreach (var entry in rarityTable)
        {
            cumulative += entry.probability;
            if (rand <= cumulative)
                return entry.rarity;
        }

        // Fallback in case of floating point error
        return rarityTable.Last().rarity;
    }
    
    public static ItemRarity GetRandomRarityExcluding(params ItemRarity[] excludedRarities)
    {
        var filteredTable = rarityTable
            .Where(entry => !excludedRarities.Contains(entry.rarity))
            .ToList();

        float total = filteredTable.Sum(entry => entry.probability);
        float rand = Random.value * total;
        float cumulative = 0f;

        foreach (var entry in filteredTable)
        {
            cumulative += entry.probability;
            if (rand <= cumulative)
                return entry.rarity;
        }

        return filteredTable.Last().rarity;
    }
}
