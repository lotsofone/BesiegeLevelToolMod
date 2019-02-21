using System;
using Modding;
using UnityEngine;

namespace lto_leveltools
{
	public class Mod : ModEntryPoint
	{
        public static GameObject Instance;
        public override void OnLoad()
		{
            Mod.Instance = new GameObject("lto Level Tools Mod");
            UnityEngine.Object.DontDestroyOnLoad(Mod.Instance);
            Mod.Instance.AddComponent<ModBehaviour>();

            GameObject gameObject1 = new GameObject("ToolBox");
            gameObject1.transform.parent = Mod.Instance.transform;
            gameObject1.AddComponent<ToolBoxBehaviour>();
            //SingleInstance<ModSettingUI>.get_Instance().transform.SetParent(BlockEnhancementMod.mod.transform);
            //SingleInstance<LanguageManager>.get_Instance().transform.SetParent(BlockEnhancementMod.mod.transform);
            //SingleInstance<MessageController>.get_Instance().transform.SetParent(BlockEnhancementMod.mod.transform);
            //SingleInstance<RocketsController>.get_Instance().transform.SetParent(BlockEnhancementMod.mod.transform);
        }
    }
    public class ClkDefination
    {
        public string key;
        public float value;
        public bool global;
        public ClkDefination(string varName, float value, bool global=false)
        {
            this.key = varName;
            this.value = value;
            this.global = global;
        }
    }
}
