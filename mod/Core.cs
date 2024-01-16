using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace NoTutorial
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Core : BaseUnityPlugin
    {
        public const string PluginGUID = "trpg.uk.notutorial";
        public const string PluginName = "NoTutorial";
        public const string PluginVersion = "1.0.0";

        public static new ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("NoTutorial");

        private void Awake()
        {
            Harmony Harmony = new Harmony("NoTutorial");
            Harmony.PatchAll();
            Logger.LogInfo($"NoTutorial is loaded.");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FindIntroButtons();
        }

        private void FindIntroButtons()
        {
            if (SceneHelper.CurrentScene != "Main Menu") return;

            GameObject canvas = GameObject.Find("/Canvas");
            GameObject prelude = null;

            foreach (LayerSelect ls in canvas.GetComponentsInChildren<LayerSelect>(true))
            {
                if (ls.layerNumber == 0)
                {
                    prelude = ls.transform.parent.gameObject;
                    break;
                }
            }

            //Button yes = prelude.transform.Find("FullIntroPopup").Find("Panel").Find("Button (1)").GetComponent<Button>();
            Button no = prelude.transform.Find("FullIntroPopup").Find("Panel").Find("Button").GetComponent<Button>();

            no.onClick.AddListener(delegate { SetTutorial(); });
        }

        private void SetTutorial()
        {
            if (!GameProgressSaver.GetTutorial()) GameProgressSaver.SetTutorial(true);
        }
    }

    [HarmonyPatch(typeof(OptionsMenuToManager), "CheckIfTutorialBeaten")]
    public class OptionsMenuToManager_CheckIfTutorialBeaten_Patch
    {
        public static bool Prefix()
        {
            if (!GameProgressSaver.GetTutorial()) Core.Logger.LogInfo("No tutorial!");
            return false;
        }
    }

    [HarmonyPatch(typeof(Bootstrap), "Start")]
    public class Bootstrap_Start_Patch
    {
        public static bool Prefix()
        {
            Debug.Log(Addressables.RuntimePath);
            if (!GameProgressSaver.GetTutorial()) Core.Logger.LogInfo("No tutorial!");
            SceneHelper.LoadScene("Intro", true);
            return false;
        }
    }

    [HarmonyPatch(typeof(IntroViolenceScreen), "GetTargetScene")]
    public class IntroViolenceScreen_GetTargetScene_Patch
    {
        public static void Postfix(ref string __result)
        {
            __result = "Main Menu";
        }
    }
}
