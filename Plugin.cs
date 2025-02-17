using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ClipperSaveCleaner;

internal class Patches
{
    [HarmonyPatch(typeof(SaveLoadManager))]
    internal static class SaveCleaner
    {
        [HarmonyPatch("LoadGame")]
        [HarmonyPrefix]
        private static void CleanSave(int backupIndex)
        {   //removes all references to the modded boat before the game loads
            Debug.LogWarning("Save Cleaner Running");
            Debug.LogWarning("Cleaning Slot " + SaveSlots.currentSlot);
            string path = ((backupIndex != 0) ? SaveSlots.GetBackupPath(SaveSlots.currentSlot, backupIndex) : SaveSlots.GetCurrentSavePath());
            SaveContainer saveContainer;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {   // Deserialize the save container from the file
                saveContainer = (SaveContainer)binaryFormatter.Deserialize(fileStream);
            }
            //Cleans Hull & Relevant Mooring Lines
            saveContainer.savedPrefabs.RemoveAll(x => x.itemParentObject == 153 || x.itemParentObject == 154 || x.itemParentObject == 155 || x.itemParentObject == 156 || x.itemParentObject == 157);
            saveContainer.savedObjects.RemoveAll(x => x.sceneIndex == 153 || x.sceneIndex == 154 || x.sceneIndex == 155 || x.sceneIndex == 156 || x.sceneIndex == 157);
            using (FileStream fileStream = File.Open(path, FileMode.Create))
            {
                binaryFormatter.Serialize(fileStream, saveContainer);
            }
            Debug.LogWarning("Save Cleaned");
        }

    }
}

[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
public class BoatPatcher : BaseUnityPlugin
{
    public const string pluginGuid = "com.TheOriginOfAllEvil.ClipperCleaner";
    internal const string pluginName = "Clipper Beta Save Cleaner";
    public const string pluginVersion = "0.1.0";
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {pluginGuid} is loaded!");

        //Initialising Harmony Instance
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), pluginGuid);
    }
}
