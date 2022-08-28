using HarmonyLib;
using VRage.Plugins;

namespace avaness.MESInhibitorPlugin
{
    public class Main : IPlugin
    {
        public static Harmony HarmonyPatcher { get; private set; }
        public void Dispose()
        {

        }

        public void Init(object gameInstance)
        {
            HarmonyPatcher = new Harmony("avaness.MESInhibitorPlugin");
        }

        public void Update()
        {

        }
    }
}
