// Standard Elo formula

using UnityEngine;
public static class EloCalculator
{
    // TODO: adjust K, perhaps based on score difference (32 = fast growth, 16 = slow)
    private const int K = 32;

    // Minimum allowed Elo
    private const int MinElo = 100;

    public static (int newA, int newB) CalculateElo(int ratingA, int ratingB, float scoreA)
    {
        // scoreA: 1 = A wins, 0 = A loses, 0.5 = draw
        float scoreB = 1f - scoreA;

        // Expected scores
        float expectedA = 1f / (1f + Mathf.Pow(10f, (ratingB - ratingA) / 400f));
        float expectedB = 1f / (1f + Mathf.Pow(10f, (ratingA - ratingB) / 400f));

        // New ratings
        int newA = Mathf.RoundToInt(ratingA + K * (scoreA - expectedA));
        int newB = Mathf.RoundToInt(ratingB + K * (scoreB - expectedB));

        return (Mathf.Max(newA, MinElo), Mathf.Max(newB, MinElo));
    }
}
