
using FishNet.Connection;
using UnityEngine;

public interface IVulnerable
{
    void TakeDamage(int damage, NetworkConnection conn, string parthit, bool setOnFire, GameObject origin, bool staggers, Vector3 hitLocation, int uniqueItemID);

    int GetCurrentHealth();

    int GetMaxHealth();

    bool IsBelowHalfHealth();

    int GetUniqueID();

    string GetCreatureType();


    Vector3 GetDamageNumberLocation();
}
