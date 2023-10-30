using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using TerraTechETCUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Sub_Missions
{
    public abstract class SubMissionHierachy
    {
        public string TreeName;
        [JsonIgnore]
        public string AddressName;
        [JsonIgnore]
        public SubMissionTree tree => ManSubMissions.GetTree(TreeName);
        /// <summary> SERIALIZATION ONLY </summary>
        public SubMissionHierachy() { }
        public SubMissionHierachy(string treeName)
        {
            TreeName = treeName;
        }

        internal SubMissionHierachyAssetBundle CreateAssetBundleable()
        {
            SubMissionHierachyAssetBundle newC = new SubMissionHierachyAssetBundle();
            newC.TreeName = tree.TreeName;
            newC.AddressName = "(Bundled) " + tree.TreeName;
            foreach (var item in tree.TreeTechs)
            {
                newC.TechNames.Add(item.Key);
            }
            foreach (var item in tree.WorldObjectFileNames)
            {
                newC.ObjectNames.Add(item);
            }
            foreach (var item in tree.MissionNames)
            {
                newC.MissionNames.Add(item);
            }
            return newC;
        }

        internal abstract string LoadMissionTreeTrunkFromFile();
        internal abstract void LoadMissionTreeDataFromFile(
            ref Dictionary<int, Texture> album, ref Dictionary<int, Mesh> models,
            ref Dictionary<string, SpawnableTech> techs);
        internal abstract string LoadMissionTreeMissionFromFile(string MissionName);
        internal abstract string LoadMissionTreeWorldObjectFromFile(string ObjectName);


        // ETC
        internal abstract void LoadTreePNGs(ref Dictionary<int, Texture> dictionary);
        internal abstract void LoadTreeMeshes(ref Dictionary<int, Mesh> dictionary);
        internal abstract Mesh LoadMesh(string MeshName);
        internal abstract void LoadTreeTechs(ref Dictionary<string, SpawnableTech> dictionary);

    }
    public class SubMissionHierachyAssetBundle : SubMissionHierachy
    {
        [JsonIgnore]
        internal ModContainer container { get; private set; }
        internal ModContents contents => container.Contents;
        public List<string> MissionNames = new List<string>();
        public List<string> TechNames = new List<string>();
        public List<string> ObjectNames = new List<string>();


        /// <summary> SERIALIZATION ONLY </summary>
        public SubMissionHierachyAssetBundle()
        {
            AddressName = "(Bundled) " + tree.TreeName;
        }
        public static SubMissionHierachyAssetBundle MakeInstance(ModContainer MC, string textData)
        {
            var inst = JsonConvert.DeserializeObject<SubMissionHierachyAssetBundle>(textData);
            inst.Setup(MC);
            return inst;
        }
        internal void Setup(ModContainer MC)
        {
            container = MC;
        }
        internal override string LoadMissionTreeTrunkFromFile()
        {
            return ((TextAsset)contents.m_AdditionalAssets.First(x => x.name == TreeName)).text;
        }
        internal override void LoadMissionTreeDataFromFile(
                    ref Dictionary<int, Texture> album, ref Dictionary<int, Mesh> models,
                    ref Dictionary<string, SpawnableTech> techs)
        {
            try
            {
                LoadTreePNGs(ref album);
                LoadTreeMeshes(ref models);
                LoadTreeTechs(ref techs);

                Debug_SMissions.Log("SubMissions: Loaded MissionTree.json for " + TreeName + " successfully.");
            }
            catch (Exception e)
            {
                throw new MandatoryException("(BUNDLE) Encountered exception not properly handled", e);
            }
        }
        internal override string LoadMissionTreeMissionFromFile(string MissionName)
        {
            throw new NotImplementedException();
        }

        internal override string LoadMissionTreeWorldObjectFromFile(string ObjectName)
        {
            throw new NotImplementedException();
        }


        // ETC
        internal override void LoadTreePNGs(ref Dictionary<int, Texture> dictionary)
        {
            foreach (var item in ResourcesHelper.IterateObjectsFromModContainer<Texture>(container))
            {
                if (!dictionary.ContainsKey(item.name.GetHashCode()))
                    dictionary.Add(item.name.GetHashCode(), item);
            }
        }
        internal override void LoadTreeMeshes(ref Dictionary<int, Mesh> dictionary)
        {
            foreach (var item in ResourcesHelper.IterateObjectsFromModContainer<Mesh>(container))
            {
                if (!dictionary.ContainsKey(item.name.GetHashCode()))
                    dictionary.Add(item.name.GetHashCode(), item);
            }
        }
        internal override Mesh LoadMesh(string MeshName)
        {
            return ResourcesHelper.GetMeshFromModAssetBundle(container, MeshName);
        }
        internal override void LoadTreeTechs(ref Dictionary<string, SpawnableTech> dictionary)
        {
            foreach (var item in ResourcesHelper.IterateObjectsFromModContainer<Texture2D>(container))
            {
                if (TechNames.Contains(item.name) && !dictionary.ContainsKey(item.name))
                {
                    if (ManScreenshot.TryDecodeSnapshotRender(item, out _))
                    {
                        dictionary.Add(item.name, new SpawnableTechBundledSnapshot(item.name, item));
                    }
                }
            }
            foreach (var item in ResourcesHelper.IterateObjectsFromModContainer<TextAsset>(container))
            {
                if (TechNames.Contains(item.name) && !dictionary.ContainsKey(item.name))
                {
                    if (!item.text.NullOrEmpty())
                    {
                        dictionary.Add(item.name, new SpawnableTechBundledRAW(item.name, item));
                    }
                }
            }
        }
    }
    public class SubMissionHierachyDirectory : SubMissionHierachy
    {
        public string TreeDirectory;
        /// <summary> SERIALIZATION ONLY </summary>
        public SubMissionHierachyDirectory()
        {
            AddressName = TreeDirectory;
        }
        public SubMissionHierachyDirectory(string treeName, string treeDirectory) : base(treeName)
        {
            TreeDirectory = treeDirectory;
            AddressName = TreeDirectory;
        }

        // Loaders
        internal override string LoadMissionTreeTrunkFromFile()
        {
            SMissionJSONLoader.ValidateDirectory(TreeDirectory);
            try
            {
                string output = File.ReadAllText(Path.Combine(TreeDirectory, "MissionTree.json"));

                Debug_SMissions.Log("SubMissions: Loaded MissionTree.json trunk for " + TreeName + " successfully.");
                return output;
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
            return null;
        }
        internal override void LoadMissionTreeDataFromFile(
            ref Dictionary<int, Texture> album, ref Dictionary<int, Mesh> models,
            ref Dictionary<string, SpawnableTech> techs)
        {
            SMissionJSONLoader.ValidateDirectory(TreeDirectory);
            try
            {
                LoadTreePNGs(ref album);
                LoadTreeMeshes(ref models);
                LoadTreeTechs(ref techs);

                Debug_SMissions.Log("SubMissions: Loaded MissionTree.json for " + TreeName + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        internal override string LoadMissionTreeMissionFromFile(string MissionName)
        {
            string destination = Path.Combine(TreeDirectory, "Missions");

            SMissionJSONLoader.ValidateDirectory(destination);
            try
            {
                string output = File.ReadAllText(Path.Combine(destination, MissionName + ".json"));
                Debug_SMissions.Log("SubMissions: Loaded Mission.json for " + MissionName + " successfully.");
                return output;
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Mission (Loading) ~ " + MissionName, "SubMissions: Could not read " + MissionName + ".json for " + TreeName + "." +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Mission (Loading) ~ " + MissionName, "SubMissions: Could not read " + MissionName + ".json for " + TreeName + "." +
                    "\n - File MissionTree.json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Mission (Loading) ~ " + MissionName, "SubMissions: Could not read " + MissionName + ".json for " + TreeName + "." +
                    "\n - File MissionTree.json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Mission (Loading) ~ " + MissionName, "SubMissions: Could not read " + MissionName + ".json for " + TreeName + "." +
                    "\n - File MissionTree.json is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
            return null;
        }

        internal override string LoadMissionTreeWorldObjectFromFile(string ObjectName)
        {
            string destination = Path.Combine(TreeDirectory, "Pieces");

            SMissionJSONLoader.ValidateDirectory(destination);
            try
            {
                string output = File.ReadAllText(Path.Combine(destination, ObjectName + ".json"));
                Debug_SMissions.Log("SubMissions: Loaded WorldObject.json for " + ObjectName + " successfully.");
                return output;
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + ObjectName, "SubMissions: Could not read " + ObjectName + ".json for " + tree.TreeName + "." +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + ObjectName, "SubMissions: Could not read " + ObjectName + ".json for " + tree.TreeName + "." +
                    "\n - File MissionTree.json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + ObjectName, "SubMissions: Could not read " + ObjectName + ".json for " + tree.TreeName + "." +
                    "\n - File MissionTree.json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + ObjectName, "SubMissions: Could not read " + ObjectName + ".json for " + tree.TreeName + "." +
                    "\n - File MissionTree.json is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
            return null;
        }


        // ETC
        internal override void LoadTreePNGs(ref Dictionary<int, Texture> dictionary)
        {
            dictionary.Clear();
            try
            {
                string[] outputs = Directory.GetFiles(TreeDirectory);
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (SMissionJSONLoader.GetName(str, out string name) && str.EndsWith(".png"))
                    {
                        try
                        {
                            dictionary.Add(name.GetHashCode(), FileUtils.LoadTexture(str));
                            foundAny = true;
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, "SubMissions: Could not load " + name + ".png for " + TreeName + "." +
                                "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json destination", e);
                        }
                        catch (PathTooLongException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, "SubMissions: Could not load " + name + ".png for " + TreeName + "." +
                                "\n - File MissionTree.json is located in a directory that makes it too deep and long" +
                                " for the OS to navigate correctly", e);
                        }
                        catch (DirectoryNotFoundException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, "SubMissions: Could not load " + name + ".png for " + TreeName + "." +
                                "\n - Path to place Folder \"" + name + "\" is incomplete (there are missing folders in" +
                                " the target folder hierachy)", e);
                        }
                        catch (FileNotFoundException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, "SubMissions: Could not load " + name + ".png for " + TreeName + "." +
                                "\n - File MissionTree.json is not at destination", e);
                        }
                        catch (IOException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Textures (Loading) ~ " + TreeName, "SubMissions: Could not load " + name + ".png for " + TreeName + "." +
                                "\n - File MissionTree.json is not accessable because IOException(?) was thrown!", e);
                        }
                        catch (Exception e)
                        {
                            throw new MandatoryException("Encountered exception not properly handled", e);
                        }
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log("SubMissions: Loaded " + dictionary.Count + " PNG files for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log("SubMissions: " + TreeName + " does not have any PNG files to load.");
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Textures (Loading) ~ " + TreeName, "SubMissions: CASCADE FAILIURE ~ Could not load PNG files for " + TreeName +
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        internal override void LoadTreeMeshes(ref Dictionary<int, Mesh> dictionary)
        {
            dictionary.Clear();
            try
            {
                string[] outputs = Directory.GetFiles(TreeDirectory);
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (SMissionJSONLoader.GetName(str, out string name) && str.EndsWith(".obj"))
                    {
                        dictionary.Add(name.GetHashCode(), LoadMesh(str));
                        foundAny = true;
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log("SubMissions: Loaded " + dictionary.Count + " .obj files for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log("SubMissions: " + TreeName + " does not have any .obj files to load.");
            }
            catch (NotImplementedException e)
            {
                SMUtil.Assert(false, "Mission Tree Meshes (Loading) ~ " + TreeName, "SubMissions: Could not load .obj files for " + TreeName +
                    ".  \n   You need the mod \"LegacyBlockLoader\" to import non-AssetBundle models.", e);
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Meshes (Loading) ~ " + TreeName, "SubMissions: Could not load .obj files for " + TreeName +
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        internal override Mesh LoadMesh(string TreeDirectory)
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
                Mesh mesh = LoadMesh_Encapsulated(TreeDirectory);
                if (!mesh)
                {
                    throw new NullReferenceException("The object could not be imported at all");
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
        private Mesh LoadMesh_Encapsulated(string TreeDirectory)
        {
            return LegacyBlockLoader.FastObjImporter.Instance.ImportFileFromPath(TreeDirectory);
        }
        internal override void LoadTreeTechs(ref Dictionary<string, SpawnableTech> dictionary)
        {
            dictionary.Clear();
            try
            {
                string[] outputs = Directory.GetFiles(TreeDirectory + "Techs");
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (SMissionJSONLoader.GetName(str, out string name) && str.EndsWith(".png"))
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
                    if (SMissionJSONLoader.GetName(str, out string name) && (str.EndsWith(".json") || str.EndsWith(".RAWTECH")))
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
                    Debug_SMissions.Log("SubMissions: Loaded " + dictionary.Count + " Techs for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log("SubMissions: " + TreeName + " does not have any Techs to load.");
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Techs (Loading) ~ " + TreeName, "SubMissions: Could not load Techs for " + TreeName +
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }

    }
}
