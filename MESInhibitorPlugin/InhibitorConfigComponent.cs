using HarmonyLib;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace avaness.MESInhibitorPlugin
{

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class InhibitorConfigComponent : MySessionComponentBase
    {
        private const string ModularEncountersId = "1521905890";

        private bool init = false;
        private InhibitorConfig config;

        private static InhibitorConfigComponent Instance { get; set; }

        public override void UpdateBeforeSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;
            if (!init)
                Start();
        }

        private void Start()
        {
            init = true;
            Instance = this;
            if (MyAPIGateway.Session.IsServer)
                config = InhibitorConfig.Load();

            foreach (var kv in MyScriptManager.Static.Scripts)
            {
                if (kv.Key.String.Contains(ModularEncountersId))
                {
                    Patch(kv.Value);
                    break;
                }
            }
        }

        protected override void UnloadData()
        {
            Instance = null;
        }

        private void Patch(Assembly mes)
        {
            try
            {
                Type tBlockLogic = mes.GetType("ModularEncountersSystems.BlockLogic.InhibitorCore", false);
                if (tBlockLogic == null)
                    tBlockLogic = mes.GetType("ModularEncountersSystems.BlockLogic.InhibitorBase", false);
                if (tBlockLogic == null)
                    tBlockLogic = mes.GetType("ModularEncountersSystems.BlockLogic.InhibitorLogic", false);
                if (tBlockLogic != null)
                {
                    int inhibitors = 0;
                    HarmonyMethod inhibitorTickDisable = new HarmonyMethod(typeof(InhibitorConfigComponent), nameof(RunTick100));
                    foreach (Type t in mes.GetTypes().Where(t => t != tBlockLogic && tBlockLogic.IsAssignableFrom(t)))
                    {
                        MethodInfo m = t.GetMethod("RunTick100", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        if(m != null)
                        {
                            Main.HarmonyPatcher.Patch(m, inhibitorTickDisable);
                            config?.AddInhibitor(t.Name);
                            MyLog.Default.WriteLine("Disabled MES inhibitor: " + t.Name);
                            inhibitors++;
                        }
                    }

                    if(inhibitors == 0)
                    {
                        MyLog.Default.WriteLine("ERROR: No MES inhibitors were patched");
                        MyAPIGateway.Utilities.ShowMessage("MES Inhibitor Plugin", "ERROR: No MES inhibitors were found!");
                    }
                }
                else
                {
                    MyLog.Default.WriteLine("ERROR: Unable to find MES inhbitors");
                    MyAPIGateway.Utilities.ShowMessage("MES Inhibitor Plugin", "ERROR: No MES inhibitors were found!");
                }

                config?.Save();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine("ERROR: An error occurred while trying to patch MES: " + e);
                MyAPIGateway.Utilities.ShowMessage("MES Inhibitor Plugin", "ERROR: No MES inhibitors were found!");
            }
        }

        public static bool RunTick100(object __instance)
        {
            if (Instance == null || MySession.Static == null || !MySession.Static.IsServer)
                return true;
            InhibitorConfig config = Instance.config;
            if (config == null)
                return true;
            return config.IsEnabled(__instance.GetType().Name);
        }
    }
}