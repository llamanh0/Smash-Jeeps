public interface IDamageable
{
    void Damage(PlayerVehicleController playerVehicleController);
    ulong GetKillerClientId();

    int GetRespawnTimer();
}
