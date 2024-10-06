using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Unity;
using UnityEngine;
using TerraTechETCUtil;
using Sub_Missions.Steps;
using Sub_Missions.ModularMonuments;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using TAC_AI.Templates;

namespace Sub_Missions
{
    public class MissionTypeEnumConverter : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is SubMissionStep)
            {
                writer.WriteValue(Enum.GetName(typeof(SubMissionStep), (SubMissionStep)value));
                return;
            }

            base.WriteJson(writer, value, serializer);
        }
    }

    /// <summary>
    /// Handles all JSON Mission loading for JSON Mission modders. 
    /// Does not handle saving save games.
    /// Cannot decode existing missions - uScript leaves behind an apocalyptic aftermath of hair-pulling 
    ///   methods and fields that are nearly impossible to retrace.
    ///    I believe the unreadable format is intentional, but it goes strongly 
    ///    against TerraTech's normal coding accessability.
    /// </summary>
    public class SMissionJSONLoader : MonoBehaviour
    {
        public static string corpJsonPostFix { get; } = "_MissionCorp";
        public static string hierachyPostFix { get; } = "_Hierarchy";
        public static string worldObjectPostFix { get; } = "_MM";
        public static string DLLDirectory;
        public static string BaseDirectory;
        public static string MissionsDirectory;
        public static string MissionSavesDirectory;
        public static string MissionCorpsDirectory;

        internal static JsonSerializerSettings JSONSaver = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };
        private static JsonSerializerSettings JSONSaverMission = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = new List<JsonConverter>{ new MissionTypeEnumConverter() },
        };
        private static JsonSerializerSettings JSONSafe = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 12,
        };

        public static void SetupWorkingDirectories()
        {
            DirectoryInfo di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            di = di.Parent; // off of this DLL
            DLLDirectory = di.ToString();
            DirectoryInfo game = new DirectoryInfo(Application.dataPath);
            game = game.Parent; // out of the game folder
            BaseDirectory = game.ToString();
            MissionsDirectory = Path.Combine(BaseDirectory, "Custom SMissions");
            MissionSavesDirectory = Path.Combine(BaseDirectory, "SMissions Saves");
            MissionCorpsDirectory = Path.Combine(BaseDirectory, "SMissions Corps");
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(MissionSavesDirectory);
            ValidateDirectory(MissionCorpsDirectory);

            if (!ManSMCCorps.hasScanned)
            {
                ManSMCCorps.LoadAllCorps();
            }
#if DEBUG
            Debug_SMissions.Log(KickStart.ModID + ": DLL folder is at: " + DLLDirectory);
            Debug_SMissions.Log(KickStart.ModID + ": Custom SMissions is at: " + MissionsDirectory);
            //SMCCorpLicense.SaveTemplateToDisk();
#endif
            foreach (var item in KickStartSubMissions.oInst.GetModObjects<Mesh>())
            {
                if (!GeneralMeshDatabase.ContainsKey(item.name))
                    GeneralMeshDatabase.Add(item.name, item);
            }
    }
        public static void SetupWorkingDirectoriesLegacy()
        {
            DirectoryInfo di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            di = di.Parent; // off of this DLL
            DLLDirectory = di.ToString();
            DirectoryInfo game = new DirectoryInfo(Application.dataPath);
            game = game.Parent; // out of the game folder
            BaseDirectory = game.ToString();
            MissionsDirectory = Path.Combine(game.ToString(), "Custom SMissions");
            MissionSavesDirectory = Path.Combine(game.ToString(), "SMissions Saves");
            MissionCorpsDirectory = Path.Combine(game.ToString(), "SMissions Corps");
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(MissionSavesDirectory);
            ValidateDirectory(MissionCorpsDirectory);

            if (!ManSMCCorps.hasScanned)
            {
                ManSMCCorps.LoadAllCorps();
            }
#if DEBUG
            Debug_SMissions.Log(KickStart.ModID + ": DLL folder is at: " + DLLDirectory);
            Debug_SMissions.Log(KickStart.ModID + ": Custom SMissions is at: " + MissionsDirectory);
            //SMCCorpLicense.SaveTemplateToDisk();
#endif
        }



        // First Startup


        // Majors
        public static List<SubMissionTree> LoadAllTrees()
        {
            List<SubMissionTree> temps = new List<SubMissionTree>();
            Debug_SMissions.Log(KickStart.ModID + ": Searching Official Mods Folder...");
            List<string> directories = GetTreeDirectoriesOfficial();
            Debug_SMissions.Log(KickStart.ModID + ": Found " + directories.Count + " trees...");
            foreach (string directed in directories)
            {
                if (GetName(directed, out string name, true))
                {
                    try
                    {
                        TreeLoader(new SubMissionHierachyDirectory(name, directed), out SubMissionTree Tree);
                        Debug_SMissions.Log(KickStart.ModID + ": Added Tree " + name);
                        temps.Add(Tree);
                    }
                    catch (MandatoryException e)
                    {
                        throw e;
                    }
                    catch (Exception e)
                    {
                        SMUtil.Assert(false, "Mission Tree ~ " + name, "Could not load mission tree " + name, e);
                    }
                }

            }
            // LOAD OFFICIAL
            foreach (var item in ResourcesHelper.IterateAllMods())
            {
                foreach (var item2 in ResourcesHelper.IterateAssetsInModContainer<TextAsset>(item.Value))
                {
                    if (item2 != null && item2.name.Contains(hierachyPostFix))
                    {
                        try
                        {
                            var bundleDir = SubMissionHierachyAssetBundle.MakeInstance(item.Value, item2.text);
                            TreeLoader(bundleDir, out SubMissionTree Tree);
                            Debug_SMissions.Log(KickStart.ModID + ": Added Bundled Tree " + bundleDir.TreeName);
                            temps.Add(Tree);
                        }
                        catch (MandatoryException e)
                        {
                            throw e;
                        }
                        catch (Exception e)
                        {
                            SMUtil.Assert(false, "Mission Tree ~ UNKNOWN", "Could not load Bundled mission tree for " + item.Key, e);
                        }
                    }
                }
            }
            //
            ValidateDirectory(MissionsDirectory);
            Debug_SMissions.Log(KickStart.ModID + ": Searching Custom SMissions Folder...");
            List<string> namesUnofficial = GetCleanedNamesInDirectory();
            Debug_SMissions.Log(KickStart.ModID + ": Found " + namesUnofficial.Count + " trees...");
            foreach (string name in namesUnofficial)
            {
                try
                {
                    TreeLoader(new SubMissionHierachyDirectory(name, Path.Combine(MissionsDirectory, name)), out SubMissionTree Tree);
                    Debug_SMissions.Log(KickStart.ModID + ": Added Tree " + name);
                    temps.Add(Tree);
                }
                catch (MandatoryException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    SMUtil.Assert(false, "Mission Tree ~ " + name, "Could not load mission tree " + name, e);
                }
            }
            return temps;
        }

        public static SubMissionTree LoadTree(string directed)
        {
            ValidateDirectory(MissionsDirectory);
            if (GetName(directed, out string name, true))
            {
                string treeDir = Path.Combine(MissionsDirectory, name);
                if (File.Exists(treeDir))
                {
                    try
                    {
                        TreeLoader(new SubMissionHierachyDirectory(name, Path.Combine(MissionsDirectory, name)), out SubMissionTree Tree);
                        Debug_SMissions.Log(KickStart.ModID + ": Added Tree " + name); 
                        return Tree;
                    }
                    catch (MandatoryException e)
                    {
                        throw e;
                    }
                    catch (Exception e)
                    {
                        SMUtil.Assert(false, "Mission Tree ~ " + name, "Could not load mission tree " + name, e);
                    }
                }
                // LOAD OFFICIAL
                foreach (var item in ResourcesHelper.IterateAllMods())
                {
                    foreach (var item2 in ResourcesHelper.IterateAssetsInModContainer<TextAsset>(item.Value))
                    {
                        if (item2 != null && item2.name.Contains(name + hierachyPostFix))
                        {
                            try
                            {
                                var bundleDir = SubMissionHierachyAssetBundle.MakeInstance(item.Value, item2.text);
                                TreeLoader(bundleDir, out SubMissionTree Tree);
                                Debug_SMissions.Log(KickStart.ModID + ": Added Bundled Tree " + bundleDir.TreeName);
                                return Tree;
                            }
                            catch (MandatoryException e)
                            {
                                throw e;
                            }
                            catch (Exception e)
                            {
                                SMUtil.Assert(false, "Mission Tree ~ UNKNOWN", "Could not load Bundled mission tree for " + item.Key, e);
                            }
                        }
                    }
                }
                try
                {
                    TreeLoader(new SubMissionHierachyDirectory(name, directed), out SubMissionTree Tree);
                    Debug_SMissions.Log(KickStart.ModID + ": Added Tree " + name);
                    return Tree;
                }
                catch (MandatoryException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    SMUtil.Assert(false, "Mission Tree ~ " + name, "Could not load mission tree " + name, e);
                }
            }
            return null;
        }

        public static IEnumerable<SubMission> LoadAllMissions(SubMissionTree tree)
        {
            ValidateDirectory(MissionsDirectory);
            List<string> names = GetCleanedNamesInDirectory(Path.Combine(tree.TreeName, "Missions"), true);
            foreach (string name in names)
            {
                var mission = MissionLoader(tree, name);
                if (mission == null)
                {
                    SMUtil.Error(false, "Mission(Load) ~ " + name, "<b> CRITICAL ERROR IN HANDLING MISSION " + 
                        name + " - UNABLE TO IMPORT ANY INFORMATION! </b>");
                    continue;
                }
                yield return mission;
            }
        }
        public static IEnumerable<SubMissionStandby> LoadAllMissionsToStandby(SubMissionTree tree)
        {
            ValidateDirectory(MissionsDirectory);
            List<string> names = GetCleanedNamesInDirectory(Path.Combine(tree.TreeName, "Missions"), true);
            foreach (string name in names)
            {
                var mission = MissionLoader(tree, name);
                if (mission == null)
                {
                    SMUtil.Error(false, "Mission(Load) ~ " + name, "<b> CRITICAL ERROR IN HANDLING MISSION " +
                        name + " - UNABLE TO IMPORT ANY INFORMATION! </b>");
                    continue;
                }
                yield return SubMissionTree.CompileToStandby(mission);
            }
        }



        // Utilities
        public static bool TryGetCorpInfoData(string factionShort,  out string results)
        {
            string dataName = factionShort + corpJsonPostFix;
            foreach (var contained in ResourcesHelper.IterateAllMods())
            {
                if (contained.Value == null)
                    continue;
                foreach (var item in ResourcesHelper.IterateAssetsInModContainer<TextAsset>(contained.Value))
                {
                    if (item.name.Contains(dataName))
                    {
                        results = item.text;
                        return true;
                    }
                }
            }
            string location = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.ToString();
            // Goes to the cluster directory where all the mods are
            string fileName = factionShort + corpJsonPostFix + ".json";

            int attempts = 0;
            foreach (string directoryLoc in Directory.GetDirectories(location))
            {
                while (true)
                {
                    try
                    {
                        string GO;
                        GO = directoryLoc + "\\" + fileName;
                        if (File.Exists(GO))
                        {
                            attempts++;
                            results = File.ReadAllText(GO);
                            return true;
                        }
                        else
                            break;
                    }
                    catch (Exception e)
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": TryGetCorpInfoDirectory - Error on MissionCorp search " + factionShort + " | " + e);
                        break;
                    }
                }
                attempts = 0;
            }

            results = null;
            return false;
        }
        public static List<string> GetTreeDirectoriesOfficial()
        {
            List<string> Cleaned = new List<string>();
            string location = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.ToString();
            // Goes to the cluster directory where all the mods are

            foreach (int cCorp in ManMods.inst.GetCustomCorpIDs())
            {
                int attempts = 0;
                foreach (string directoryLoc in Directory.GetDirectories(location))
                {
                    while (true)
                    {
                        try
                        {
                            string GO;
                            string fileName = ManMods.inst.FindCorpShortName((FactionSubTypes)cCorp) + "_MissionTree_" + attempts + ".json";
                            GO = directoryLoc + "\\" + fileName;
                            if (File.Exists(GO))
                            {
                                Debug_SMissions.Log(KickStart.ModID + ": GetTreeNamesOfficial - " + GO);
                                attempts++;
                                if (GetName(fileName, out string cleanName, true))
                                {
                                    Cleaned.Add(cleanName);
                                }
                            }
                            else
                                break;
                        }
                        catch (Exception e)
                        {
                            Debug_SMissions.Log("LocalModCorpAudio: RegisterCorpMusics - Error on Music search " + cCorp + " | " + e);
                            break;
                        }
                    }
                    attempts = 0;
                }
            }
            return Cleaned;
        }
        public static List<string> GetCleanedNamesInDirectory(string directoryFromMissionsDirectory = "", bool doJSON = false)
        {
            string search;
            if (directoryFromMissionsDirectory == "")
                search = MissionsDirectory;
            else
                search = Path.Combine(MissionsDirectory, directoryFromMissionsDirectory);
            IEnumerable<string> toClean;
            if (doJSON)
                toClean = Directory.GetFiles(search);
            else
                toClean = Directory.GetDirectories(search);
            //Debug_SMissions.Log(KickStart.ModID + ": Cleaning " + toClean.Count);
            List<string> Cleaned = new List<string>();
            foreach (string cleaning in toClean)
            {
                if (GetName(cleaning, out string cleanName, doJSON))
                {
                    Cleaned.Add(cleanName);
                }
            }
            return Cleaned;
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


        // -------------------------------- JSON Handlers --------------------------------
        public static void TreeLoader(SubMissionHierachy SMH, out SubMissionTree Tree)
        {
            try
            {
                string output = SMH.LoadMissionTreeTrunkFromFile();
                Tree = JsonConvert.DeserializeObject<SubMissionTree>(output, new MissionTypeEnumConverter());
                Tree.TreeHierachy = SMH;
                SMH.LoadMissionTreeDataFromFile(ref Tree.MissionTextures,
                    ref Tree.MissionMeshes, ref Tree.TreeTechs);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new DirectoryNotFoundException(KickStart.ModID + ": Check your Tree file names, " +
                    "cases where you referenced the names and make sure they match!!!", e);
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentNullException(KickStart.ModID + ": Unexpected tree hierachy", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        public static SubMission MissionLoader(SubMissionTree tree, string MissionName)
        {
            try
            {
                string output = tree.TreeHierachy.LoadMissionTreeMissionFromFile(MissionName);
                if (output == null)
                    throw new NullReferenceException("MissionName " + MissionName + " is the name of the mission INSIDE the json, " +
                        "but not the actual name of the json itself.  They must match!");
                SubMission mission = JsonConvert.DeserializeObject<SubMission>(output, JSONSaverMission);
                mission.Tree = tree;
                return mission;
            }
            catch (DirectoryNotFoundException e)
            {
                SMUtil.Assert(false, "Mission (Loading) ~ " + MissionName, KickStart.ModID + ": Check your Mission file names, cases where you referenced " +
                    "the names and make sure they match!!!  Tree: " +
                    tree.TreeName + ", Mission: " + MissionName, e);
                return null;
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        // Tech loading is handled elsewhere - either PopulationInjector or TACtical_AIs.

        /// <summary>
        /// Returns true if it should be added to the list
        /// </summary>
        /// <param name="SMT"></param>
        /// <param name="name"></param>
        /// <param name="hash"></param>
        /// <param name="GO"></param>
        /// <returns></returns>

        /*
        public static void TreeLoaderLEGACY(string TreeName, string TreeDirectory, out SubMissionTree Tree)
        {
            try
            {
                string output = LoadMissionTreeTrunkFromFile(TreeName, TreeDirectory);
                Tree = JsonConvert.DeserializeObject<SubMissionTree>(output, new MissionTypeEnumConverter());
                //Tree.TreeHierachy = TreeDirectory;
                LoadMissionTreeDataFromFile(TreeName, TreeDirectory, ref Tree.MissionTextures,
                    ref Tree.MissionMeshes, ref Tree.TreeTechs);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new DirectoryNotFoundException(KickStart.ModID + ": Check your Tree file names, " +
                    "cases where you referenced the names and make sure they match!!!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }


        // Loaders
        private static string LoadMissionTreeTrunkFromFile(string TreeName, string TreeDirectory)
        {
            ValidateDirectory(TreeDirectory);
            try
            {
                string output = File.ReadAllText(Path.Combine(TreeDirectory, "MissionTree.json "));

                Debug_SMissions.Log(KickStart.ModID + ": Loaded MissionTree.json  trunk for " + TreeName + " successfully.");
                return output;
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName + "." +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json  destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName + "." +
                    "\n - File MissionTree.json  is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName + "." +
                    "\n - File MissionTree.json  is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName + "." +
                    "\n - File MissionTree.json  is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
            return null;
        }
        private static void LoadMissionTreeDataFromFile(string TreeName, string TreeDirectory,
            ref Dictionary<int, Texture> album, ref Dictionary<int, Mesh> models,
            ref Dictionary<string, SpawnableTech> techs)
        {
            ValidateDirectory(TreeDirectory);
            try
            {
                LoadTreePNGs(TreeName, TreeDirectory, ref album);
                LoadTreeMeshes(TreeName, TreeDirectory, ref models);
                LoadTreeTechs(TreeName, TreeDirectory, ref techs);

                Debug_SMissions.Log(KickStart.ModID + ": Loaded MissionTree.json  for " + TreeName + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName + "." +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json  destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName + "." +
                    "\n - File MissionTree.json  is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName + "." +
                    "\n - File MissionTree.json  is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName + "." +
                    "\n - File MissionTree.json  is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
       

        // ETC
        private static void LoadTreePNGs(string TreeName, string TreeDirectory, 
            ref Dictionary<int, Texture> dictionary)
        {
            dictionary.Clear();
            try
            {
                string[] outputs = Directory.GetFiles(TreeDirectory);
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (GetName(str, out string name) && str.EndsWith(".png"))
                    {
                        try
                        {
                            dictionary.Add(name.GetHashCode(), FileUtils.LoadTexture(str));
                            foundAny = true;
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, KickStart.ModID + ": Could not load " + name + ".png for " + TreeName + "." +
                                "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json  destination", e);
                        }
                        catch (PathTooLongException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, KickStart.ModID + ": Could not load " + name + ".png for " + TreeName + "." +
                                "\n - File MissionTree.json  is located in a directory that makes it too deep and long" +
                                " for the OS to navigate correctly", e);
                        }
                        catch (DirectoryNotFoundException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, KickStart.ModID + ": Could not load " + name + ".png for " + TreeName + "." +
                                "\n - Path to place Folder \"" + name + "\" is incomplete (there are missing folders in" +
                                " the target folder hierachy)", e);
                        }
                        catch (FileNotFoundException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, KickStart.ModID + ": Could not load " + name + ".png for " + TreeName + "." +
                                "\n - File MissionTree.json  is not at destination", e);
                        }
                        catch (IOException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Textures (Loading) ~ " + TreeName, KickStart.ModID + ": Could not load " + name + ".png for " + TreeName + "." +
                                "\n - File MissionTree.json  is not accessable because IOException(?) was thrown!", e);
                        }
                        catch (Exception e)
                        {
                            throw new MandatoryException("Encountered exception not properly handled", e);
                        }
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Loaded " + dictionary.Count + " PNG files for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log(KickStart.ModID + ": " + TreeName + " does not have any PNG files to load.");
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Textures (Loading) ~ " + TreeName, KickStart.ModID + ": CASCADE FAILIURE ~ Could not load PNG files for " + TreeName + 
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        private static void LoadTreeMeshes(string TreeName, string TreeDirectory, 
            ref Dictionary<int, Mesh> dictionary)
        {
            dictionary.Clear();
            try
            {
                string[] outputs = Directory.GetFiles(TreeDirectory);
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (GetName(str, out string name) && str.EndsWith(".obj"))
                    {
                        dictionary.Add(name.GetHashCode(), LoadMesh(str));
                        foundAny = true;
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Loaded " + dictionary.Count + " .obj files for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log(KickStart.ModID + ": " + TreeName + " does not have any .obj files to load.");
            }
            catch (NotImplementedException e)
            {
                SMUtil.Assert(false, "Mission Tree Meshes (Loading) ~ " + TreeName, KickStart.ModID + ": Could not load .obj files for " + TreeName +
                    ".  \n   You need the mod \"LegacyBlockLoader\" to import non-AssetBundle models.", e);
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Meshes (Loading) ~ " + TreeName, KickStart.ModID + ": Could not load .obj files for " + TreeName + 
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        private static void LoadTreeTechs(string TreeName, string TreeDirectory, 
            ref Dictionary<string, SpawnableTech> dictionary)
        {
            dictionary.Clear();
            try
            {
                string[] outputs = Directory.GetFiles(TreeDirectory + "Techs");
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (GetName(str, out string name) && str.EndsWith(".png"))
                    {
                        if (dictionary.ContainsKey(name))
                        {
                            SMUtil.Error(false, "Mission Tree SnapTechs (Loading) ~ " + TreeName + ", tech " + name,
                                "Tech of name " + name + " already is assigned to the tree.  Cannot add " +
                                "multiple Techs of same name!");
                        }
                        else
                            dictionary.Add(name, new SpawnableTechSnapshot(name));
                        foundAny = true;
                    }
                }
                outputs = Directory.GetFiles(TreeDirectory + "Raw Techs");
                foreach (string str in outputs)
                {
                    if (GetName(str, out string name) && (str.EndsWith(".json") || str.EndsWith(".RAWTECH")))
                    {
                        if (dictionary.ContainsKey(name))
                        {
                            SMUtil.Error(false, "Mission Tree RawTechs (Loading) ~ " + TreeName + ", tech " + name,
                                "Tech of name " + name + " already is assigned to the tree.  Cannot add " +
                                "multiple Techs of same name!");
                        }
                        else
                            dictionary.Add(name, new SpawnableTechRAW(name));
                        foundAny = true;
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Loaded " + dictionary.Count + " Techs for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log(KickStart.ModID + ": " + TreeName + " does not have any Techs to load.");
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Techs (Loading) ~ " + TreeName, KickStart.ModID + ": Could not load Techs for " + TreeName +
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        */

        private static Dictionary<string, Mesh> GeneralMeshDatabase = new Dictionary<string, Mesh>();
        internal static bool TryGetBuiltinMesh(string nameWithExt, out Mesh mesh)
        {
            if (GeneralMeshDatabase.TryGetValue(nameWithExt, out mesh))
                return true;
            else
            {
                mesh = null;
                return false;
            }
        }

        internal static Mesh LoadMeshFromFile(string MeshDirectory)
        {
#if !STEAM
            Mesh mesh = ObjImporter.ImportFileFromPath(TreeDirectory);
            if (!mesh)
            {
                throw new NullReferenceException("The object could not be imported at all: ");
            }
            return mesh;
#else
            try
            {
                Mesh mesh = LoadMeshFromFile_Encapsulated(MeshDirectory);
                if (!mesh)
                {
                    throw new NullReferenceException("The .object could not be imported at all");
                }
                return mesh;
            }
            catch
            {
                throw new NotImplementedException("SMissionJSONLoader.LoadMesh requires the mod " +
                    "\"LegacyBlockLoader\" to load models.");
            }
            //throw new NotImplementedException("SMissionJSONLoader.LoadMesh is not currently supported in Official.");

#endif
        }
        private static Mesh LoadMeshFromFile_Encapsulated(string MeshDirectory)
        {
            return LegacyBlockLoader.FastObjImporter.Instance.ImportFileFromPath(MeshDirectory);
        }



        // Savers
        public static void SaveEntireTreeToAssetBundle(SubMissionTree tree, string targetDirectory)
        {
            string RawTreeJSON = JsonConvert.SerializeObject(tree, Formatting.Indented, JSONSaver);
            try
            {
                if (!DirectoryExists(Path.Combine(targetDirectory, tree.TreeName)))
                    Directory.CreateDirectory(Path.Combine(targetDirectory, tree.TreeName));
                File.WriteAllText(Path.Combine(targetDirectory, tree.TreeName, tree.TreeName + ".json"), RawTreeJSON);

                string assetBundleable = JsonConvert.SerializeObject(tree.TreeHierachy.CreateAssetBundleable(), Formatting.Indented, JSONSaver);
                File.WriteAllText(Path.Combine(targetDirectory, tree.TreeName, tree.TreeName + hierachyPostFix + ".json"), assetBundleable);
                Debug_SMissions.Log(KickStart.ModID + ": Saved MissionTree.json  for " + tree.TreeName + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Export MissionTree to AssetBundle", KickStart.ModID + ": Could not edit MissionTree.json  for " + tree.TreeName +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json  destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Export MissionTree to AssetBundle", KickStart.ModID + ": Could not edit MissionTree.json  for " + tree.TreeName +
                    "\n - File MissionTree.json  is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Export MissionTree to AssetBundle", KickStart.ModID + ": Could not edit MissionTree.json  for " + tree.TreeName +
                    "\n - File MissionTree.json  is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Export MissionTree to AssetBundle", KickStart.ModID + ": Could not edit MissionTree.json  for " + tree.TreeName +
                    "\n - File MissionTree.json  is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }


        public static void SaveTree(SubMissionTree tree)
        {
            string RawTreeJSON = JsonConvert.SerializeObject(tree, Formatting.Indented, JSONSaver);
            try
            {
                if (!DirectoryExists(Path.Combine(MissionsDirectory, tree.TreeName)))
                    Directory.CreateDirectory(Path.Combine(MissionsDirectory, tree.TreeName));
                File.WriteAllText(Path.Combine(MissionsDirectory, tree.TreeName, "MissionTree.json"), RawTreeJSON);

                string assetBundleable = JsonConvert.SerializeObject(tree.TreeHierachy.CreateAssetBundleable(), Formatting.Indented, JSONSaver);
                File.WriteAllText(Path.Combine(MissionsDirectory, tree.TreeName, tree.TreeName + hierachyPostFix + ".json"), assetBundleable);
                Debug_SMissions.Log(KickStart.ModID + ": Saved  " + tree.TreeName + hierachyPostFix + ".json for " + tree.TreeName + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Save MissionTree", KickStart.ModID + ": Could not edit MissionTree.json for " + tree.TreeName +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json  destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Save MissionTree", KickStart.ModID + ": Could not edit MissionTree.json  for " + tree.TreeName +
                    "\n - File MissionTree.json  is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Save MissionTree", KickStart.ModID + ": Could not edit MissionTree.json  for " + tree.TreeName +
                    "\n - File MissionTree.json  is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Save MissionTree", KickStart.ModID + ": Could not edit MissionTree.json  for " + tree.TreeName +
                    "\n - File MissionTree.json  is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        public static void SaveMission(SubMissionTree tree, SubMission mission)
        {
            if (!DirectoryExists(Path.Combine(MissionsDirectory, tree.TreeName, "Missions")))
                Directory.CreateDirectory(Path.Combine(MissionsDirectory, tree.TreeName, "Missions"));
            var missionJSON = JsonConvert.SerializeObject(mission, Formatting.Indented, JSONSaverMission);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, tree.TreeName, "Missions", mission.Name), missionJSON);
            tree.ActiveMissions.Add(mission);
            ManSubMissions.Selected = mission;
        }
        public static bool SaveNewTechSnapshot(SubMissionTree tree, string name, Texture2D Tech)
        {
            if (!tree.TreeTechs.ContainsKey(name))
            {
                if (!DirectoryExists(Path.Combine(MissionsDirectory, tree.TreeName, "Techs")))
                    Directory.CreateDirectory(Path.Combine(MissionsDirectory, tree.TreeName, "Techs"));
                FileUtils.SaveTexture(Tech, Path.Combine(MissionsDirectory, tree.TreeName, "Techs", name + ".png"));
                if (tree.TreeTechs.ContainsKey(name))
                {
                    tree.MissionTextures.Remove(name);
                    tree.TreeTechs.Remove(name);
                }
                tree.MissionTextures.Add(name, Tech);
                tree.TreeTechs.Add(name, new SpawnableTechSnapshot(name));
                return true;
            }
            return false;
        }
        public static bool SaveNewTechRaw(SubMissionTree tree, string name, TechData data)
        {
            name = name.Replace(".json", string.Empty).Replace(".RAWTECH", string.Empty) + ".RAWTECH";
            if (!tree.TreeTechs.ContainsKey(name))
            {
                tree.TreeTechs.Add(name, new SpawnableTechRAW(name));
                RawTech RT = new RawTech(data);
                var dataString = JsonConvert.SerializeObject(RT.ToTemplate());
                if (!DirectoryExists(Path.Combine(MissionsDirectory, tree.TreeName, "Raw Techs")))
                    Directory.CreateDirectory(Path.Combine(MissionsDirectory, tree.TreeName, "Raw Techs"));
                TryWriteToFile(Path.Combine(MissionsDirectory, tree.TreeName, "Raw Techs", name), dataString);
                return true;
            }
            return false;
        }
        public static string TryDecodeRawTechType(string rawTech)
        {
            string outp = JsonConvert.DeserializeObject<RawTechTemplate>(rawTech).techName;
            if (outp == null)
            {
                return JsonConvert.DeserializeObject<RawTechTemplateFast>(rawTech).Blueprint;
            }
            return outp;
        }
        internal static bool SaveNewSMWorldObject(SubMissionTree tree, SMWorldObjectJSON SMWO)
        {
            if (!tree.WorldObjects.ContainsKey(SMWO.Name))
            {
                if (!DirectoryExists(Path.Combine(MissionsDirectory, tree.TreeName, "Pieces")))
                    Directory.CreateDirectory(Path.Combine(MissionsDirectory, tree.TreeName, "Pieces"));
                string SMWorldObjectJSON = JsonConvert.SerializeObject(SMWO, Formatting.Indented, JSONSafe);
                SMWO.Tree = tree.TreeName;
                TryWriteToJSONFile(Path.Combine(MissionsDirectory, tree.TreeName, "Pieces", SMWO.Name), SMWorldObjectJSON);
                var prefab = MM_JSONLoader.BuildNewWorldObjectPrefabJSON(tree, SMWO);
                tree.WorldObjects.Add(SMWO.Name, prefab);
                tree.WorldObjectFileNames.Add(SMWO.Name);
                return true;
            }
            return false;
        }
        internal static void OpenInExplorer(string directory)
        {
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.MacOSX:
                    Process.Start(new ProcessStartInfo("file://" + directory));
                    break;
                case OperatingSystemFamily.Linux:
                case OperatingSystemFamily.Windows:
                    Process.Start(new ProcessStartInfo("explorer.exe", directory));
                    break;
                default:
                    throw new Exception("This operating system is UNSUPPORTED by Sub_Missions");
            }
        }
    }
}
