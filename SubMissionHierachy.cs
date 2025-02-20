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
using System.Collections;
using System.Xml.Linq;
using TAC_AI.Templates;
using Sub_Missions.ModularMonuments;

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
        public abstract bool IsEditable();

        private static Dictionary<string, string> nameCollisionDetector = new Dictionary<string, string>();
        internal SubMissionHierachyAssetBundle CreateAssetBundleable()
        {
            try
            {
                SubMissionHierachyAssetBundle newC = new SubMissionHierachyAssetBundle(tree);
                foreach (var item in tree.TerrainEdits)
                {
                    string name = item.Key;
                    if (nameCollisionDetector.TryGetValue(name, out string conflict))
                    {
                        SMUtil.Error(false, "Mission Tree AssetBundle (Building) ~ " + TreeName,
                            "Terrain of name " + item.Key + " clashes with item of same name in - " + conflict);
                        SMUtil.PushErrors();
                        return newC;
                    }
                    nameCollisionDetector.Add(name, "Terrains");
                    newC.TerrainNames.Add(name);
                }
                foreach (var item in tree.TreeTechs)
                {
                    string name = item.Key;
                    if (nameCollisionDetector.TryGetValue(name, out string conflict))
                    {
                        SMUtil.Error(false, "Mission Tree AssetBundle (Building) ~ " + TreeName,
                            "Tech of name " + item.Key + " clashes with item of same name in - " + conflict);
                        SMUtil.PushErrors();
                        return newC;
                    }
                    nameCollisionDetector.Add(name, "Techs");
                    if (item.Value is SpawnableTechFromPool)
                        newC.PoolParamNames.Add(name);
                    else
                        newC.TechNames.Add(name);
                }
                foreach (var item in tree.WorldObjectFileNames)
                {
                    string name = item;
                    if (nameCollisionDetector.TryGetValue(name, out string conflict))
                    {
                        SMUtil.Error(false, "Mission Tree AssetBundle (Building) ~ " + TreeName,
                            "WorldObject of name " + item + " clashes with item of same name in - " + conflict);
                        SMUtil.PushErrors();
                        return newC;
                    }
                    nameCollisionDetector.Add(name, "Pieces");
                    newC.ObjectNames.Add(name);
                }
                foreach (var item in tree.MissionNames)
                {
                    string name = item;
                    newC.MissionNames.Add(name);
                }
                return newC;
            }
            finally
            {
                nameCollisionDetector.Clear();
            }
        }

        internal abstract string LoadMissionTreeTrunkFromFile();
        internal abstract void LoadMissionTreeDataFromFile(
            ref Dictionary<string, Texture> album, ref Dictionary<string, Mesh> models,
            ref Dictionary<string, SpawnableTech> techs);
        internal abstract string LoadMissionTreeMissionFromFile(string MissionName);
        internal abstract string LoadMissionTreeWorldObjectFromFile(string ObjectName);


        // ETC
        internal abstract void LoadTreePNGs(ref Dictionary<string, Texture> dictionary);
        internal abstract void LoadTreeMeshes(ref Dictionary<string, Mesh> dictionary);
        internal abstract Mesh LoadMesh(string MeshName);
        internal abstract void LoadTreeTerrains(ref Dictionary<string, Dictionary<IntVector2, TerrainModifier>> dictionary);
        internal abstract void LoadTreeTechs(ref Dictionary<string, Texture> dictTexs, ref Dictionary<string, SpawnableTech> dictionary);
        internal abstract void LoadTreeWorldObjects(ref List<string> entries);
    }
    public class SubMissionHierachyAssetBundle : SubMissionHierachy
    {
        [JsonIgnore]
        internal ModContainer container { get; private set; }
        internal ModContents contents => container.Contents;
        public List<string> MissionNames = new List<string>();
        public List<string> TechNames = new List<string>();
        public List<string> PoolParamNames = new List<string>();
        public List<string> TerrainNames = new List<string>();
        public List<string> ObjectNames = new List<string>();
        public override bool IsEditable() => false;


        /// <summary> SERIALIZATION ONLY </summary>
        public SubMissionHierachyAssetBundle()
        {
        }
        public SubMissionHierachyAssetBundle(SubMissionTree Treee) : base(Treee.TreeName)
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
            return ((TextAsset)contents.m_AdditionalAssets.FirstOrDefault(x => x.name == TreeName)).text;
        }
        internal override void LoadMissionTreeDataFromFile(
                    ref Dictionary<string, Texture> album, ref Dictionary<string, Mesh> models,
                    ref Dictionary<string, SpawnableTech> techs)
        {
            try
            {
                LoadTreePNGs(ref album);
                LoadTreeMeshes(ref models);
                LoadTreeTechs(ref album, ref techs);

                Debug_SMissions.Log(KickStart.ModID + ": Loaded MissionTree.json  for " + TreeName + " successfully.");
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
        internal override void LoadTreePNGs(ref Dictionary<string, Texture> dictionary)
        {
            foreach (var item in ResourcesHelper.IterateAssetsInModContainer<Texture2D>(container))
            {
                if (!dictionary.ContainsKey(item.name))
                    dictionary.Add(item.name, item);
            }
        }
        internal override void LoadTreeMeshes(ref Dictionary<string, Mesh> dictionary)
        {
            foreach (var item in ResourcesHelper.IterateAssetsInModContainer<Mesh>(container))
            {
                if (!dictionary.ContainsKey(item.name))
                    dictionary.Add(item.name, item);
            }
        }
        internal override Mesh LoadMesh(string MeshName)
        {
            return ResourcesHelper.GetMeshFromModAssetBundle(container, MeshName);
        }
        internal override void LoadTreeTerrains(ref Dictionary<string, Dictionary<IntVector2, TerrainModifier>> dictionary)
        {
            dictionary.Clear();
            try
            {
                foreach (var item in ResourcesHelper.IterateAssetsInModContainer<TextAsset>(container))
                {
                    if (TechNames.Contains(item.name) && !dictionary.ContainsKey(item.name))
                    {
                        try
                        {
                            string nameFiltered = item.name.Replace("%", string.Empty);
                            if (!item.text.NullOrEmpty())
                            {
                                dictionary.Add(nameFiltered, JsonConvert.DeserializeObject<Dictionary<IntVector2, TerrainModifier>>(
                                    item.text));
                            }
                        }
                        catch (Exception e)
                        {
                            SMUtil.Error(false, "Mission Tree Terrain (Loading) ~ " + TreeName + ", terrain " + item.name,
                                "Terrain of name " + item.name + " is corrupted, unable to load! - " + e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Techs (Loading) ~ " + TreeName, KickStart.ModID + ": Could not load Techs for " + TreeName +
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        internal override void LoadTreeTechs(ref Dictionary<string, Texture> dictTexs, ref Dictionary<string, SpawnableTech> dictionary)
        {
            foreach (var item in dictionary)
            {
                dictTexs.Remove(item.Key);
            }
            dictionary.Clear();
            foreach (var item in ResourcesHelper.IterateAssetsInModContainer<Texture2D>(container))
            {
                if (TechNames.Contains(item.name) && !dictionary.ContainsKey(item.name))
                {
                    if (ManScreenshot.TryDecodeSnapshotRender(item, out _))
                    {
                        dictionary.Add(item.name, new SpawnableTechBundledSnapshot(item.name, item));
                    }
                }
            }
            foreach (var item in ResourcesHelper.IterateAssetsInModContainer<TextAsset>(container))
            {
                if (TechNames.Contains(item.name) && !dictionary.ContainsKey(item.name))
                {
                    if (!item.text.NullOrEmpty())
                    {
                        string nameFiltered = item.name.Replace("%", string.Empty);
                        dictionary.Add(nameFiltered, new SpawnableTechBundledRAW(nameFiltered, item));
                    }
                }
            }
            foreach (var item in ResourcesHelper.IterateAssetsInModContainer<TextAsset>(container))
            {
                if (PoolParamNames.Contains(item.name) && !dictionary.ContainsKey(item.name))
                {
                    if (!item.text.NullOrEmpty())
                    {
                        string nameFiltered = item.name.Replace("%", string.Empty);
                        SMissionJSONLoader.RawTechPopParamsEx RTPP = JsonConvert.DeserializeObject<SMissionJSONLoader.RawTechPopParamsEx>(item.text);
                        if (RTPP != null)
                            dictionary.Add(nameFiltered, new SpawnableTechFromPool(nameFiltered, RTPP.ToInst()));
                    }
                }
            }
        }

        internal override void LoadTreeWorldObjects(ref List<string> entries)
        {
            entries.Clear();
            try
            {
                bool foundAny = false;
                foreach (var item in ResourcesHelper.IterateAssetsInModContainer<Texture2D>(container))
                {
                    if (item.name.EndsWith(SMissionJSONLoader.worldObjectPostFix + ".json"))
                    {
                        if (entries.Contains(item.name))
                        {
                            SMUtil.Error(false, "Mission Tree WorldObjects (Loading) ~ " + TreeName + ", piece " +
                                item.name, "Piece of name " + item.name + " already is assigned to the tree.  Cannot add " +
                                "multiple Pieces of same name!");
                        }
                        else
                        {
                            string nameFiltered = item.name.Replace("%", string.Empty);
                            entries.Add(item.name);
                        }
                        foundAny = true;
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Loaded " + entries.Count + " Pieces for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log(KickStart.ModID + ": " + TreeName + " does not have any Pieces to load.");
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree WorldObjects (Loading) ~ " + TreeName, KickStart.ModID + ": Could not load Pieces for " + TreeName +
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
    }
    public class SubMissionHierachyDirectory : SubMissionHierachy
    {
        public string TreeDirectory;
        public override bool IsEditable() => true;
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
        internal override void LoadMissionTreeDataFromFile(
            ref Dictionary<string, Texture> album, ref Dictionary<string, Mesh> models,
            ref Dictionary<string, SpawnableTech> techs)
        {
            SMissionJSONLoader.ValidateDirectory(TreeDirectory);
            try
            {
                LoadTreePNGs(ref album);
                LoadTreeMeshes(ref models);
                LoadTreeTechs(ref album, ref techs);

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
        internal override string LoadMissionTreeMissionFromFile(string MissionName)
        {
            string destination = Path.Combine(TreeDirectory, "Missions");

            SMissionJSONLoader.ValidateDirectory(destination);
            try
            {
                string output = File.ReadAllText(Path.Combine(destination, MissionName + ".json"));
                Debug_SMissions.Log(KickStart.ModID + ": Loaded Mission.json for " + MissionName + " successfully.");
                return output;
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Mission (Loading) ~ " + MissionName, KickStart.ModID + ": Could not read " + MissionName + ".json for " + TreeName + "." +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json  destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Mission (Loading) ~ " + MissionName, KickStart.ModID + ": Could not read " + MissionName + ".json for " + TreeName + "." +
                    "\n - File MissionTree.json  is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Mission (Loading) ~ " + MissionName, KickStart.ModID + ": Could not read " + MissionName + ".json for " + TreeName + "." +
                    "\n - File " + MissionName + ".json is not at destination, this needs to have " +
                    "both main Name and the file's name should match, minus the \".json\" for " +
                    "the Name within the file", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Mission (Loading) ~ " + MissionName, KickStart.ModID + ": Could not read " + MissionName + ".json for " + TreeName + "." +
                    "\n - File MissionTree.json  is not accessable because IOException(?) was thrown!", e);
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
                Debug_SMissions.Log(KickStart.ModID + ": Loaded WorldObject.json for " + ObjectName + " from file.");
                return output;
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + ObjectName, KickStart.ModID + ": Could not read " + ObjectName + ".json for " + tree.TreeName + "." +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json  destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + ObjectName, KickStart.ModID + ": Could not read " + ObjectName + ".json for " + tree.TreeName + "." +
                    "\n - File MissionTree.json  is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + ObjectName, KickStart.ModID + ": Could not read " + ObjectName + ".json for " + tree.TreeName + "." +
                    "\n - File MissionTree.json  is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + ObjectName, KickStart.ModID + ": Could not read " + ObjectName + ".json for " + tree.TreeName + "." +
                    "\n - File MissionTree.json  is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
            return null;
        }


        // ETC
        internal override void LoadTreePNGs(ref Dictionary<string, Texture> dictionary)
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
                            dictionary.Add(name, FileUtils.LoadTexture(str));
                            Debug_SMissions.Log("Loaded " + name + " as a Tech");
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
        internal override void LoadTreeMeshes(ref Dictionary<string, Mesh> dictionary)
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
                        dictionary.Add(name, LoadMesh(str));
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
        internal override void LoadTreeTerrains(ref Dictionary<string, Dictionary<IntVector2, TerrainModifier>> dictionary)
        {
            dictionary.Clear();
            try
            {
                bool foundAny = false;
                string[] outputs;
                if (Directory.Exists(Path.Combine(TreeDirectory, "Terrain")))
                {
                    outputs = Directory.GetFiles(Path.Combine(TreeDirectory, "Terrain"));
                    foreach (string str in outputs)
                    {
                        if (SMissionJSONLoader.GetName(str, out string name) && str.EndsWith(".json"))
                        {
                            if (dictionary.ContainsKey(name))
                            {
                                SMUtil.Error(false, "Mission Tree Terrain (Loading) ~ " + TreeName + ", terrain " + name,
                                    "Terrain of name " + name + " already is assigned to the tree.  Cannot add " +
                                    "multiple terrains of same name!");
                            }
                            else
                            {
                                try
                                {
                                    dictionary.Add(name, JsonConvert.DeserializeObject<Dictionary<IntVector2, TerrainModifier>>(
                                        File.ReadAllText(str)));
                                }
                                catch (Exception e)
                                {
                                    SMUtil.Error(false, "Mission Tree Terrain (Loading) ~ " + TreeName + ", terrain " + name,
                                        "Terrain of name " + name + " is corrupted, unable to load! - " + e);
                                }
                            }
                            foundAny = true;
                        }
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Loaded " + dictionary.Count + " Terrains for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log(KickStart.ModID + ": " + TreeName + " does not have any terrain to load.");
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Techs (Loading) ~ " + TreeName, KickStart.ModID + ": Could not load Techs for " + TreeName +
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        internal override void LoadTreeTechs(ref Dictionary<string, Texture> dictTexs, ref Dictionary<string, SpawnableTech> dictionary)
        {
            foreach (var item in dictionary)
            {
                dictTexs.Remove(item.Key);
            }
            dictionary.Clear();
            try
            {
                bool foundAny = false;
                string[] outputs;
                if (Directory.Exists(Path.Combine(TreeDirectory, "Techs")))
                {
                    outputs = Directory.GetFiles(Path.Combine(TreeDirectory, "Techs"));
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
                            {
                                if (dictTexs.ContainsKey(name))
                                    SMUtil.Error(false, "Mission Tree SnapTechs (Loading) ~ " + TreeName + ", tech " + name,
                                        "Tech of name " + name + " tried to be added to the tree but a Texture of the same name" +
                                        " is already assigned!  Cannot add multiple Textures of same name!");
                                else
                                {
                                    dictionary.Add(name, new SpawnableTechSnapshot(name));
                                    dictTexs.Add(name, FileUtils.LoadTexture(str));
                                }
                            }
                            foundAny = true;
                        }
                    }
                }
                if (Directory.Exists(Path.Combine(TreeDirectory, "Raw Techs")))
                {
                    outputs = Directory.GetFiles(Path.Combine(TreeDirectory, "Raw Techs"));
                    foreach (string str in outputs)
                    {
                        if (SMissionJSONLoader.GetName(str, out string name) && (name.EndsWith(".json") || name.EndsWith(".RAWTECH")))
                        {
                            if (dictionary.ContainsKey(name))
                            {
                                SMUtil.Error(false, "Mission Tree RawTechs (Loading) ~ " + TreeName + ", tech " + name,
                                    "Tech of name " + name + " already is assigned to the tree.  Cannot add " +
                                    "multiple Techs of same name!");
                            }
                            else
                            {
                                try
                                {
                                    string fileData = File.ReadAllText(str);
                                    RawTechTemplate temp = JsonConvert.DeserializeObject<RawTechTemplate>(fileData);
                                    if (temp.techName == "!!!!null!!!!")
                                    { // Convert to the correct format 
                                        RawTechTemplateFast temp2 = JsonConvert.DeserializeObject<RawTechTemplateFast>(fileData);

                                        temp.techName = name.Replace(".json", string.Empty).Replace(".RAWTECH", string.Empty);
                                        temp.savedTech = temp2.Blueprint;
                                        if (temp2.Blueprint == null)
                                        {
                                            SMUtil.Log(false, "Tech of name " + name + " could not be converted from \"Fast\" RawTech format - " +
                                                "JsonConvert failed to fetch Blueprint");
                                        }
                                        else
                                        {
                                            File.WriteAllText(str, JsonConvert.SerializeObject(temp));
                                            SMUtil.Log(false, "Tech of name " + name + " has been converted from \"Fast\" RawTech format.");
                                        }
                                    }
                                    dictionary.Add(name, new SpawnableTechRAW(name));
                                }
                                catch (Exception)
                                {
                                    SMUtil.Error(false, "Mission Tree RawTechs (Loading) ~ " + TreeName + ", tech " + name,
                                        "Tech of name " + name + " Is corrupted or in invalid format.  Cannot load!");
                                }
                            }
                            foundAny = true;
                        }
                    }
                }

                if (Directory.Exists(Path.Combine(TreeDirectory, "Pop Params")))
                {
                    outputs = Directory.GetFiles(Path.Combine(TreeDirectory, "Pop Params"));
                    foreach (string str in outputs)
                    {
                        if (SMissionJSONLoader.GetName(str, out string name) && name.EndsWith(".json"))
                        {
                            if (dictionary.ContainsKey(name))
                            {
                                SMUtil.Error(false, "Mission Tree Pop Params (Loading) ~ " + TreeName + ", tech " + name,
                                    "Pop Param of name " + name + " already is assigned to the tree.  Cannot add " +
                                    "multiple Techs of same name!");
                            }
                            else
                            {
                                try
                                {
                                    string fileData = File.ReadAllText(str);
                                    SMissionJSONLoader.RawTechPopParamsEx temp = JsonConvert.DeserializeObject<SMissionJSONLoader.RawTechPopParamsEx>(fileData);
                                    if (temp == null)
                                        throw new NullReferenceException("RawTechPopParams is in invalid format");
                                    dictionary.Add(name, new SpawnableTechFromPool(name, temp.ToInst()));
                                }
                                catch (Exception e)
                                {
                                    SMUtil.Error(false, "Mission Tree RawTechs (Loading) ~ " + TreeName + ", tech " + name,
                                        "Tech of name " + name + " Is corrupted or in invalid format.  Cannot load! + " + e);
                                }
                            }
                            foundAny = true;
                        }
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
        internal override void LoadTreeWorldObjects(ref List<string> entries)
        {
            entries.Clear();
            try
            {
                string[] outputs = Directory.GetFiles(Path.Combine(TreeDirectory, "Pieces"));
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (SMissionJSONLoader.GetName(str, out string name) && str.EndsWith(".json"))
                    {
                        if (entries.Contains(name))
                        {
                            SMUtil.Error(false, "Mission Tree WorldObjects (Loading) ~ " + TreeName + ", piece " + name,
                                "Piece of name " + name + " already is assigned to the tree.  Cannot add " +
                                "multiple Pieces of same name!");
                        }
                        else
                            entries.Add(name);

                        foundAny = true;
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Loaded " + entries.Count + " Pieces for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log(KickStart.ModID + ": " + TreeName + " does not have any Pieces to load.");
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree WorldObjects (Loading) ~ " + TreeName, KickStart.ModID + ": Could not load Pieces for " + TreeName +
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }

    }
}
