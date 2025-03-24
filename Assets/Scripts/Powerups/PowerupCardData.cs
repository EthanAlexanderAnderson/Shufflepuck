using UnityEngine;

public static class PowerupCardData
{
    private static string[] cardImportNames = { "PlusOne", "Foresight", "Block", "Bolt", "ForceField", "Phase", "Cull", "Growth", "Lock", "Explosion", "Fog", "Hydra", "Factory", "Shield", "Shuffle", "Chaos", "TimesTwo", "Resurrect", "Mill", "Research", "Insanity", "Triple", "Exponent", "Laser", "Aura", "Push", "Erratic", "Deny", "Investment", "Omniscience", null };
    public static string GetCardName(int index) { return cardImportNames[index]; }
    public static int GetCardCount() { return cardImportNames.Length; }
    public static bool CheckIfCardIsOwned(int index)
    {
        string name = PowerupCardData.GetCardName(index);
        if
        (
            PlayerPrefs.GetInt($"{name}CardOwned") > 0 ||
            PlayerPrefs.GetInt($"{name}CardOwnedHolo") > 0 ||
            PlayerPrefs.GetInt($"{name}CardOwnedBronze") > 0 ||
            PlayerPrefs.GetInt($"{name}CardOwnedBronzeHolo") > 0 ||
            PlayerPrefs.GetInt($"{name}CardOwnedGold") > 0 ||
            PlayerPrefs.GetInt($"{name}CardOwnedGoldHolo") > 0 ||
            PlayerPrefs.GetInt($"{name}CardOwnedDiamond") > 0 ||
            PlayerPrefs.GetInt($"{name}CardOwnedDiamondHolo") > 0 ||
            PlayerPrefs.GetInt($"{name}CardOwnedCelestial") > 0 ||
            PlayerPrefs.GetInt($"{name}CardOwnedCelestialHolo") > 0
        )
        {
            return true;
        }
        return false;
    }
    public static int GetCardOwnedCount(int index)
    {
        string name = PowerupCardData.GetCardName(index);
        int sum = 0;

        sum += PlayerPrefs.GetInt($"{name}CardOwned");
        sum += PlayerPrefs.GetInt($"{name}CardOwnedHolo");
        sum += PlayerPrefs.GetInt($"{name}CardOwnedBronze");
        sum += PlayerPrefs.GetInt($"{name}CardOwnedBronzeHolo");
        sum += PlayerPrefs.GetInt($"{name}CardOwnedGold");
        sum += PlayerPrefs.GetInt($"{name}CardOwnedGoldHolo");
        sum += PlayerPrefs.GetInt($"{name}CardOwnedDiamond");
        sum += PlayerPrefs.GetInt($"{name}CardOwnedDiamondHolo");
        sum += PlayerPrefs.GetInt($"{name}CardOwnedCelestial");
        sum += PlayerPrefs.GetInt($"{name}CardOwnedCelestialHolo");

        return sum;
    }

    private static int[] cardRarities = { 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 2, 0, 1, 2, 2, 2, 1, 2, 2, 3, 3, 3, 0, 0, 0, 0, 1, 4, -1 };
    //private static int[] cardRarities = { 0, 0, 0, 2, 2, 1, 1, 1, 0, 1, 1, 1, 2, 0, 1, 2, 2, 2, 1, 2, 2, 3, 3, 3, 0, 0, 0, 0, 1, 4, -1 };
    public static int GetCardRarity(int index) { return cardRarities[index]; }
}
