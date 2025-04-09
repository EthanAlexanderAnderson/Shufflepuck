public interface CPUPathInterface {

    (float angle, float power, float spin) GetPath();

    bool DoesPathRequirePhasePowerup();

    bool DoesPathRequireExplosionPowerup();

    int CalculateValue(int modifiedDifficulty);

    void EnablePathVisualization(int mode = 0);

    void DisablePathVisualization();
}
