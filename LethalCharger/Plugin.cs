using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace LethalCharger
{
    [BepInPlugin("LethalCharger", "LethalCharger", "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Logger.LogInfo($"LethalCharger loaded!");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}