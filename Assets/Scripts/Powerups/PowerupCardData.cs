using UnityEngine;

public static class PowerupCardData
{
    private static string[] cardImportNames = { "PlusOne", "Foresight", "Block", "Bolt", "ForceField", "Phase", "Cull", "Growth", "Lock", "Explosion", "Fog", "Hydra", "Factory", "Shield", "Shuffle", "Chaos", "TimesTwo", "Resurrect", "Mill", "Research", "Insanity", "Triple", "Exponent", "Laser", "Aura", "Push", "Erratic", "Deny", "Investment", "Omniscience", null };
    public static string GetCardName(int index) { return cardImportNames[index]; }
    public static int GetCardCount() { return cardImportNames.Length; }
    public static bool CheckIfCardIsOwned(int index)
    {
        var name = PowerupCardData.GetCardName(index);
        if
        (
            PlayerPrefs.GetInt($"{name}CardOwnedStandard") > 0 ||
            PlayerPrefs.GetInt($"{name}CardOwnedStandardHolo") > 0 ||
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
}
