#if UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using UnityEngine;

public static class RuntimeConfigTestFactory
{
    public static GameBalanceConfig CreateDefault()
    {
        GameBalanceConfig config = ScriptableObject.CreateInstance<GameBalanceConfig>();

        SetField(config, "_maxBatteryCharge", 100f);
        SetField(config, "_startBatteryCharge", 100f);
        SetField(config, "_batteryDrainPerSecond", 8f);
        SetField(config, "_batteryRestoreAmount", 25f);
        SetField(config, "_trapDamageAmount", 35f);
        SetField(config, "_scorePerDistanceUnit", 1f);
        SetField(config, "_boosterSpeedBonus", 3f);
        SetField(config, "_boosterDurationSeconds", 4f);
        SetField(config, "_baseForwardSpeed", 7f);

        return config;
    }

    private static void SetField<T>(GameBalanceConfig config, string fieldName, T value)
    {
        FieldInfo fieldInfo = typeof(GameBalanceConfig).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (fieldInfo == null)
        {
            throw new InvalidOperationException($"Field '{fieldName}' was not found on {nameof(GameBalanceConfig)}.");
        }

        fieldInfo.SetValue(config, value);
    }
}
#endif
