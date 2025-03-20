public static class PowerupCardData
{
    private static string[] cardImportNames = { "PlusOne", "Foresight", "Block", "Bolt", "ForceField", "Phase", "Cull", "Growth", "Lock", "Explosion", "Fog", "Hydra", "Factory", "Shield", "Shuffle", "Chaos", "TimesTwo", "Resurrect", "Mill", "Research", "Insanity", "Triple", "Exponent", "Laser", "Aura", "Push", "Erratic", "Deny", "Investment", "Omniscience", null };
    public static string GetCardName(int index) { return cardImportNames[index]; }
    public static int GetCardCount() { return cardImportNames.Length; }
}
