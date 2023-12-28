using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace LethalTerminalExtender
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "us.domkalan.lethalextendedterminal";
        public const string NAME = "Lethal Extended Terminal";
        public const string VERSION = "1.1.0";
        
        public static Plugin instance;
        private void Awake()
        {
            Plugin.instance = this;
            
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        
        public void Log(LogType type, string contents)
        {
            Logger.LogInfo(contents);
        }
    }
}
