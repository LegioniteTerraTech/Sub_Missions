using Newtonsoft.Json;
using Sub_Missions.Steps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TerraTechETCUtil;
using Sub_Missions.SimpleMissions;

namespace Sub_Missions
{
    public class SimpleSMissions
    {
        public static string DLLDirectory => SMissionJSONLoader.DLLDirectory;
        public static string BaseDirectory => SMissionJSONLoader.BaseDirectory;
        public static string MissionsDirectory => SMissionJSONLoader.MissionsDirectory;
        public static string MissionSavesDirectory => SMissionJSONLoader.MissionSavesDirectory;
        public static string MissionCorpsDirectory => SMissionJSONLoader.MissionCorpsDirectory;

        private static JsonSerializerSettings JSONSaver = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };
        private static JsonSerializerSettings JSONSaverMission = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = new List<JsonConverter> { new MissionTypeEnumConverter() },
        };
        private static JsonSerializerSettings JSONSafe = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 12,
        };

        public static SMWorldObjectJSON MakePrefabWorldObject()
        {
            SMWorldObjectJSON SMWO = new SMWorldObjectJSON();
            SMWO.Name = "ModularBrickCube_(636)";
            SMWO.GameMaterialName = "AncientRuins";
            SMWO.VisualMeshName = "ModularBrickCube_6x3x6";
            SMWO.ColliderMeshName = "ModularBrickCube_6x3x6";
            SMWO.WorldObjectJSON = new Dictionary<string, object>();
            GameObject GOInst = UnityEngine.Object.Instantiate(new GameObject("Temp"), null);
            string lightName = typeof(Light).FullName;
            Light light = GOInst.AddComponent<Light>();
            light.intensity = 1337;
            Dictionary<string, object> pair = new Dictionary<string, object> {
                { "GameObject|nextLevel2", null },
                { lightName, MakeCompat(light) },
            };
            SMWO.WorldObjectJSON.Add("GameObject|nextLevel", pair);
            UnityEngine.Object.Destroy(GOInst);
            return SMWO;
        }
        public static void MakePrefabMissionTreeToFile(string TreeName)
        {

            Debug_SMissions.Log(KickStart.ModID + ": Setting up template reference...");

            SubMission mission1 = PrefabNPC_Mission.MakeMission();
            SubMission mission2 = PrefabCombat_Mission.MakeMission();
            SubMission mission3 = PrefabHarvest_Mission.MakeMission();
            SubMission mission4 = PrefabRewards_Mission.MakeMission();

            SMWorldObjectJSON SMWO = MakePrefabWorldObject();

            SubMissionTree tree = new SubMissionTree();
            tree.TreeName = TreeName;
            tree.Faction = "GSO";
            tree.ModID = KickStart.ModID;
            tree.MissionNames.Add("NPC Mission");
            tree.MissionNames.Add("Harvest Mission");
            tree.MissionNames.Add("GSO Blocks Kit");
            tree.RepeatMissionNames.Add("Combat Mission");
            tree.WorldObjectFileNames.Add("ModularBrickCube_(636)");

            string RawTreeJSON = JsonConvert.SerializeObject(tree, Formatting.Indented, JSONSaver);
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName));
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName, "Raw Techs"));
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName, "Missions"));
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName, "Pieces"));
            string one = JsonConvert.SerializeObject(mission1, Formatting.Indented, JSONSaverMission);
            string two = JsonConvert.SerializeObject(mission2, Formatting.Indented, JSONSaverMission);
            string three = JsonConvert.SerializeObject(mission3, Formatting.Indented, JSONSaverMission);
            string four = LargeStringData.GarrettGruntle;
            string five = LargeStringData.TestTarget;
            string six = JsonConvert.SerializeObject(mission4, Formatting.Indented, JSONSaverMission);
            string seven = JsonConvert.SerializeObject(SMWO, Formatting.Indented, JSONSafe);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", mission1.Name), one);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", mission2.Name), two);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", mission3.Name), three);
            TryWriteToFile(Path.Combine(MissionsDirectory, TreeName, "Raw Techs", "Garrett Gruntle.RAWTECH"), four);
            TryWriteToFile(Path.Combine(MissionsDirectory, TreeName, "Raw Techs", "TestTarget.RAWTECH"), five);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", mission4.Name), six);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Pieces", "ModularBrickCube_(636)"), seven);
            TryWriteToTextFile(Path.Combine(MissionsDirectory, "SubMissionHelp"), SubMission.GetDocumentation());
            TryWriteToTextFile(Path.Combine(MissionsDirectory, "SubMissionsSteps"), SubMissionStep.GetALLStepDocumentations());
            TryCopyFile(Path.Combine(DLLDirectory, "Garrett Gruntle.png"), Path.Combine(MissionsDirectory, TreeName, "Garrett Gruntle.png"));
            TryCopyFile(Path.Combine(DLLDirectory, "ModularBrickCube_6x3x6.obj"), Path.Combine(MissionsDirectory, TreeName, "ModularBrickCube_6x3x6.obj"));
            try
            {
                File.WriteAllText(Path.Combine(MissionsDirectory, TreeName, "MissionTree.json "), RawTreeJSON);
                Debug_SMissions.Log(KickStart.ModID + ": Saved MissionTree.json  for " + TreeName + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json  destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName +
                    "\n - File MissionTree.json  is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName +
                    "\n - File MissionTree.json  is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName +
                    "\n - File MissionTree.json  is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
            Debug_SMissions.Log(KickStart.ModID + ": Setup template reference successfully.");
        }


        private static Dictionary<string, object> MakeCompat<T>(T convert)
        {
            IEnumerable<PropertyInfo> PI = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Debug_SMissions.Log(KickStart.ModID + ": MakeCompat - Compiling " + typeof(T) + " which has " + PI.Count() + " properties");
            Dictionary<string, object> converted = new Dictionary<string, object>();
            foreach (PropertyInfo PIC in PI)
            {
                //if (FI.IsPublic)
                converted.Add(PIC.Name, PIC.GetValue(convert));
            }
            return converted;
        }


        public static bool GetName(string FolderDirectory, out string output, bool doJSON = false)
        {
            StringBuilder final = new StringBuilder();
            foreach (char ch in FolderDirectory)
            {
                if (ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar)
                {
                    final.Clear();
                }
                else
                    final.Append(ch);
            }
            if (doJSON)
            {
                if (!final.ToString().Contains(".json"))
                {
                    output = "error";
                    return false;
                }
                final.Remove(final.Length - 5, 5);// remove ".json"
            }
            output = final.ToString();
            //Debug_SMissions.Log(KickStart.ModID + ": Cleaning Name " + output);
            return true;
        }
        public static bool DirectoryExists(string DirectoryIn)
        {
            return Directory.Exists(DirectoryIn);
        }
        public static void ValidateDirectory(string DirectoryIn)
        {
            if (!GetName(DirectoryIn, out string name))
                return;// error
            if (!Directory.Exists(DirectoryIn))
            {
                Debug_SMissions.Log(KickStart.ModID + ": Generating " + name + " folder.");
                try
                {
                    Directory.CreateDirectory(DirectoryIn);
                    Debug_SMissions.Log(KickStart.ModID + ": Made new " + name + " folder successfully.");
                }
                catch (UnauthorizedAccessException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", KickStart.ModID + ": Could not create new " + name + " folder" +
                        "\n - TerraTech + SubMissions was not permitted to access Folder \"" + name + "\"", e);
                }
                catch (PathTooLongException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", KickStart.ModID + ": Could not create new " + name + " folder" +
                        "\n - Folder \"" + name + "\" is located in a directory that makes it too deep and long" +
                        " for the OS to navigate correctly", e);
                }
                catch (DirectoryNotFoundException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", KickStart.ModID + ": Could not create new " + name + " folder" +
                        "\n - Path to place Folder \"" + name + "\" is incomplete (there are missing folders in" +
                        " the target folder hierachy)", e);
                }
                catch (IOException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", KickStart.ModID + ": Could not create new " + name + " folder" +
                        "\n - Folder \"" + name + "\" is not accessable because IOException(?) was thrown!", e);
                }
                catch (Exception e)
                {
                    throw new MandatoryException("Encountered exception not properly handled", e);
                }
            }
        }
        public static void TryWriteToFile(string FileDirectory, string ToOverwrite)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.WriteAllText(FileDirectory, ToOverwrite);
                Debug_SMissions.Log(KickStart.ModID + ": Saved " + name + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name +
                    "\n - TerraTech + SubMissions was not permitted to access the " + name + " destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name +
                    "\n - File " + name + " is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name +
                    "\n - File " + name + " is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name +
                    "\n - File " + name + " is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        public static void TryWriteToJSONFile(string FileDirectory, string ToOverwrite)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.WriteAllText(FileDirectory + ".json", ToOverwrite);
                Debug_SMissions.Log(KickStart.ModID + ": Saved " + name + ".json successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name + ".json" +
                    "\n - TerraTech + SubMissions was not permitted to access the " + name + ".json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name + ".json" +
                    "\n - File " + name + ".json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name + ".json" +
                    "\n - File " + name + ".json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name + ".json" +
                    "\n - File " + name + ".json is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        public static void TryWriteToTextFile(string FileDirectory, string ToOverwrite)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.WriteAllText(FileDirectory + ".txt", ToOverwrite);
                Debug_SMissions.Log(KickStart.ModID + ": Saved " + name + ".txt successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not write to " + name + ".txt" +
                    "\n - TerraTech + SubMissions was not permitted to access the target file", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not write to " + name + ".txt" +
                    "\n - File " + name + ".txt is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not write to " + name + ".txt" +
                    "\n -  File " + name + ".txt is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not write to " + name + ".txt" +
                    "\n -  File " + name + ".txt is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        private static void TryCopyFile(string FileDirectory, string FileDirectoryEnd)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.Copy(FileDirectory, FileDirectoryEnd);
                Debug_SMissions.Log(KickStart.ModID + ": Copied " + name + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not copy " + name +
                    "\n   TerraTech + SubMissions was not permitted to access the target file", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not copy " + name +
                    "\n - Target file is too deep and long for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not copy " + name +
                    "\n - Target file is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not copy " + name +
                    "\n - Target file is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
    }
}
