public interface CPUPathInterface {

    (float, float, float) GetPath();

    bool DoesPathRequirePhasePowerup();

    bool DoesPathRequireExplosionPowerup();

    bool IsPathAContactShot();

    int CalculateValue();

    void EnablePathVisualization(int mode = 0);

    void DisablePathVisualization();
}
