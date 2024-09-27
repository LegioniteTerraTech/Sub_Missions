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
using Newtonsoft.Json;

namespace Sub_Missions.ModularMonuments
{
    public class MM_JSONLoader : MonoBehaviour
    {
        private struct WorldObjInfo
        {
            public SubMissionTree SMT;
            public ModContainer MC;
            public SMWorldObject worldObj;
            public GameObject baseGO => worldObj.gameObject;
            public Material baseMat;
            public Mesh baseMesh;
        }

        private static bool BuildWorldObject(SubMissionTree SMT, string name, out int hash, out SMWorldObject GO)
        {
            Exception ex = null;
            try
            {
                hash = name.GetHashCode();
                if (SMT.WorldObjects.TryGetValue(name, out SMWorldObject GO2))
                {
                    GO = GO2;
                    return false;
                }
                else
                    GO = BuildNewWorldObjectPrefab(SMT, name);
                return true;
            }
            catch (NullReferenceException e)
            {
                ex = new NullReferenceException("BuildNewWorldObjectPrefab could not put the GameObject " + name +
                    " together", e);
            }
            catch (Exception e)
            {
                hash = 0;
                GO = null;
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + name, KickStart.ModID + ": BuildWorldObject - CRITICAL ERROR WITH CREATING " + name, e);
                throw new MandatoryException(e);
            }
            hash = 0;
            GO = null;
            SMUtil.Assert(false, "WorldObject (Loading) ~ " + name, KickStart.ModID + ": BuildWorldObject - Could not build for " +
                name + " of tree " + SMT.TreeName + "!", ex);
            return false;
        }

        internal static SMWorldObject BuildNewWorldObjectPrefabJSON(SubMissionTree SMT, SMWorldObjectJSON ObjectJSON)
        {
            try
            {
                GameObject gameObj = Instantiate(new GameObject("Unset"), null);
                SMWorldObject worldObj = gameObj.AddComponent<SMWorldObject>();
                worldObj.SetFromJSON(SMT, ObjectJSON);
                gameObj.name = worldObj.Name;
                var mod = ManMods.inst.FindMod(SMT.ModID);
                WorldObjInfo WOI = new WorldObjInfo
                {
                    MC = mod,
                    SMT = SMT,
                    worldObj = worldObj,
                };
                AssignMaterialBase(ref WOI);
                AssignMeshBase(ref WOI);
                AssignColliderBase(ref WOI);
                Damageable dmg = gameObj.AddComponent<Damageable>();
                dmg.SetInvulnerable(true, true);
                if (worldObj.WorldObjectJSON != null)
                {
                    RecursiveGameObjectBuilder(ref WOI, gameObj, worldObj.WorldObjectJSON);
                }
                int layerSet = Globals.inst.layerTerrain;
                if (worldObj.aboveGround)
                    layerSet = Globals.inst.layerLandmark;  // no longer accepts anchors but does not mess with anchors.
                foreach (Collider Col in gameObj.GetComponentsInChildren<Collider>())
                {
                    Col.gameObject.layer = layerSet;
                    switch (worldObj.TerrainType)
                    {
                        case SMWOTerrain.Rubber:
                            Col.sharedMaterial = PMR;
                            break;
                        case SMWOTerrain.Ice:
                            Col.sharedMaterial = PMI;
                            break;
                        case SMWOTerrain.Frictionless:
                            Col.sharedMaterial = PMN;
                            break;
                        default:
                            Col.sharedMaterial = PM;
                            break;
                    }
                }
                gameObj.transform.position = Vector3.down * 50;
                gameObj.SetActive(false);
                return worldObj;
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " +
                    (ObjectJSON.Name.NullOrEmpty() ? "UNKNOWN" : ObjectJSON.Name) +
                    " in " + SMT.TreeName, KickStart.ModID + ": BuildNewWorldObjectPrefabJSON - Check your WorldObject.json for errors!  " +
                    "Tree: " + SMT.TreeName, e);
                Debug_SMissions.Log(e);
                return null;
            }
        }

        internal static SMWorldObject BuildNewWorldObjectPrefab(SubMissionTree SMT, string ObjectName)
        {
            try
            {
                string ObjectJSON = LoadMissionTreeWorldObjectFromFile(SMT, ObjectName);
                return BuildNewWorldObjectPrefabJSON(SMT, JsonConvert.DeserializeObject<SMWorldObjectJSON>(ObjectJSON));
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ UNKNOWN in " + SMT.TreeName, KickStart.ModID + ": BuildNewWorldObjectPrefab - Check your WorldObject.json for errors!  " +
                    "Tree: " + SMT.TreeName, e);
                return null;
            }
        }

        private static void AssignMaterialBase(ref WorldObjInfo info)
        {
            var MRender = info.baseGO.AddComponent<MeshRenderer>();
            MRender.sharedMaterial = GetMaterial(info, info.worldObj.GameMaterialName, info.worldObj.TextureName);
            info.baseMat = MRender.sharedMaterial;
        }
        private static void AssignMeshBase(ref WorldObjInfo info)
        {
            var MFilter = info.baseGO.AddComponent<MeshFilter>();
            if (info.worldObj.VisualMeshName != null)
            {
                MFilter.sharedMesh = GetMesh(info, info.worldObj.VisualMeshName);
            }
            else
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + info.worldObj.name, KickStart.ModID + ": Check your WorldObject.json " + info.worldObj.Name + " for a mesh!  " +
                    "It MUST have a valid VisualMeshName (with .obj included) provided in it's base SubMission folder or " +
                            "a valid in-game mesh to use!  Tree: " + info.SMT.TreeName,
                            new NullReferenceException("AssignMeshBase(ref WorldObjInfo info) info.worldObj.VisualMeshName is null"));
                //info.worldObj.Remove(true);
            }
            info.baseMesh = MFilter.sharedMesh;
        }
        private static void AssignColliderBase(ref WorldObjInfo info)
        {
            if (info.worldObj.ColliderMeshName != null)
            {
                var meshCol = info.baseGO.AddComponent<MeshCollider>();
                meshCol.sharedMesh = GetMesh(info, info.worldObj.ColliderMeshName);
            }
            else
            {
                var boxCol = info.baseGO.AddComponent<BoxCollider>();
                if (info.baseMesh)
                {
                    boxCol.size = info.baseMesh.bounds.size;
                    boxCol.center = info.baseMesh.bounds.center;
                }
                else
                    SMUtil.Assert(false, "WorldObject (Loading) ~ " + info.worldObj.name, 
                        KickStart.ModID + ": Check your WorldObject.json " + info.worldObj.Name + 
                        " for a mesh!  It MUST have a valid ColliderMeshName (with .obj included) provided in it's base SubMission folder!  Tree: " +
                        info.SMT.TreeName, 
                        new NullReferenceException("AssignColliderBase(ref WorldObjInfo info) info.worldObj.VisualMeshName is null"));
                //return null;
            }
        }

        private static int depth = 0;
        private const int MaxDepth = 10;
        private static void RecursiveGameObjectBuilder(ref WorldObjInfo info, GameObject parent, Dictionary<string, object> WorldObject)
        {
            depth++;
            try
            {
                foreach (KeyValuePair<string, object> entry in WorldObject)
                {
                    try
                    {
                        if (entry.Key.Where(delegate (char cCase) { return cCase != ' '; }).ToString().StartsWith("GameObject|"))
                        {
                            if (depth > MaxDepth)
                            {
                                SMUtil.Error(false, "WorldObjectJSON(Load) ~ " + info.baseGO.name, KickStart.ModID + ": Error in " + info.baseGO.name + " WorldObjectJSON Tree: " +
                                    info.SMT.TreeName + "\n You have exceeded the maximum safe GameObject depth of " + MaxDepth + "!");
                                continue;
                            }
                            GameObject nextLevel = Instantiate(new GameObject(entry.Key.Skip(11).ToString()), parent.transform);

                            RecursiveGameObjectBuilder(ref info, nextLevel, (Dictionary<string, object>)entry.Value);
                        }
                        else
                        {
                            try
                            {
                                Type type = KickStart.LookForType(entry.Key);
                                if (type == null)
                                    continue;
                                if (!type.IsClass)
                                    continue;
                                var comp = parent.GetComponent(type);
                                if (!comp)
                                    comp = parent.AddComponent(type);

                                foreach (KeyValuePair<string, object> pair in (Dictionary<string, object>)entry.Value)
                                {
                                    PropertyInfo PI = type.GetProperty(pair.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (PI != null)
                                    {
                                        Construct(ref info, comp, PI, pair.Value);
                                    }
                                    else
                                        SMUtil.Error(false, "WorldObjectJSON(Load) ~ " + info.baseGO.name, KickStart.ModID + ": Error in " + info.baseGO.name +
                                            " WorldObjectJSON Tree: " + info.SMT.TreeName + "\n No such variable exists: " +
                                            (pair.Key.NullOrEmpty() ? pair.Key : "ENTRY IS NULL OR EMPTY"));
                                }
                            }
                            catch (Exception e)
                            {   // report missing component 
                                SMUtil.Assert(false, "WorldObject (Loading) ~ " + info.baseGO.name, KickStart.ModID + ": Error in " + info.baseGO.name + " WorldObjectJSON Tree: " +
                                    info.SMT.TreeName + "\n No such type exists: " + (entry.Key.NullOrEmpty() ? entry.Key :
                                    "ENTRY IS NULL OR EMPTY"), e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        SMUtil.Assert(false, "WorldObject (Loading) ~ " + info.baseGO.name, KickStart.ModID + ": Error in " + info.baseGO.name + " WorldObjectJSON Tree: " +
                            info.SMT.TreeName, e);
                    }
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "WorldObject (Loading) ~ " + info.baseGO.name, KickStart.ModID + ": (GameObject case) Error in " + info.baseGO.name +
                    " WorldObjectJSON Tree: " + info.SMT.TreeName, e);
            }
            depth--;
        }
        private static void Construct(ref WorldObjInfo info, Component comp, PropertyInfo PI, object obj)
        {
            if (PI.PropertyType == typeof(Mesh))
            {
                PI.SetValue(comp, GetMesh(info, obj.ToString()));
            }
            else if (PI.PropertyType == typeof(Material))
            {
                PI.SetValue(comp, info.baseMat);
            }
            else
                PI.SetValue(comp, obj);
        }


        private static Material GetMaterial(WorldObjInfo info, string GameMaterialName, string nameWithExt)
        {
            Material Mat = null;
            if (GameMaterialName != null)
            {
                Material[] mats = Resources.FindObjectsOfTypeAll<Material>();
                mats = mats.Where(cases => cases.name == GameMaterialName).ToArray();
                if (mats != null && mats.Count() > 0)
                    Mat = mats.FirstOrDefault();
                if (!Mat)
                {
                    SMUtil.Error(false, "WorldObjectJSON(Load) ~ " + info.baseGO.name,
                        KickStart.ModID + ": Check your WorldObject.json " + info.baseGO.name +
                        "'s GameMaterialName is not a valid material!  Tree: " + info.SMT.TreeName);
                    Mat = (Material)Resources.Load("GSO_Scenery");
                }
            }
            else
                Mat = (Material)Resources.Load("GSO_Scenery");
            Texture tex;
            if (info.MC != null && nameWithExt != null)
            {
                tex = ResourcesHelper.GetTextureFromModAssetBundle(info.MC, nameWithExt.Replace(".png", ""), false);
                if (tex != null)
                {
                    Material newMat = new Material(Mat);
                    newMat.mainTexture = tex;
                    return newMat;
                }
            }
            if (nameWithExt != null && info.SMT.MissionTextures.TryGetValue(nameWithExt, out tex))
            {
                Material newMat = new Material(Mat);
                newMat.mainTexture = tex;
                return newMat;
            }
            else
            {
                return Mat;
            }
        }
        private static Mesh GetMesh(WorldObjInfo info, string nameWithExt)
        {
            //Debug_SMissions.Log("1");
            if (nameWithExt != null)
            {
                //Debug_SMissions.Log("2");
                if (info.MC != null)
                {
                    //Debug_SMissions.Log("3");
                    var mesh = ResourcesHelper.GetMeshFromModAssetBundle(info.MC, nameWithExt.Replace(".obj", ""), false);
                    if (mesh != null)
                        return mesh;
                }
                //Debug_SMissions.Log("4");
                if (info.SMT.TryGetMesh(nameWithExt, out Mesh obj))
                    return obj;
                else
                {
                    //Debug_SMissions.Log("5");
                    var mes = Resources.FindObjectsOfTypeAll<Mesh>().FirstOrDefault(x => !x.name.NullOrEmpty() && nameWithExt.CompareTo(x.name) == 0);
                    if (mes != null)
                        return mes;
                }
            }
            //Debug_SMissions.Log("6");
            SMUtil.Error(false, "WorldObjectJSON(Load) ~ " + info.baseGO.name,
                KickStart.ModID + ": Check your WorldObject.json " + info.baseGO.name + " for the mesh!  " +
                "Mesh name " + nameWithExt + " does not exists  " +
                "It MUST have a valid VisualMeshName (with .obj included) provided in it's base SubMission folder or " +
                "a valid in-game mesh to use!  Tree: " + info.SMT.TreeName);
            return null;
        }


        // WorldObjects
        public static void BuildAllWorldObjects(List<SubMissionTree> SMTs)
        {
            try
            {
                int counter = 0;
                foreach (SubMissionTree tree in SMTs)
                {
                    tree.WorldObjects.Clear();
                    Dictionary<string, SMWorldObject> iGO = tree.WorldObjects;
                    List<string> outputs = tree.WorldObjectFileNames;
                    foreach (string nameCase in outputs)
                    {
                        string nameFiltered = nameCase.Replace("%", string.Empty);
                        if (BuildWorldObject(tree, nameFiltered, out int hash, out SMWorldObject GO))
                        {
                            iGO.Add(nameFiltered, GO);
                            counter++;
                        }
                    }
                }
                if (counter > 0)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": BuildAllWorldObjects - Loaded " + counter + " WorldObj.json files from all trees successfully.");
                }
                else
                    Debug_SMissions.Log(KickStart.ModID + ": BuildAllWorldObjects - There were no WorldObj.json files to load.");
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "WorldObjects (Loading)", KickStart.ModID + ": BuildAllWorldObjects - CRITICAL ERROR", e);
            }
        }
        private static string LoadMissionTreeWorldObjectFromFile(SubMissionTree tree, string ObjectName)
        {
            return tree.TreeHierachy.LoadMissionTreeWorldObjectFromFile(ObjectName);
        }



        public static readonly PhysicMaterial PM = new PhysicMaterial("MMPM")
        {
            dynamicFriction = 0.75f,
            bounciness = 0.1f,
            bounceCombine = PhysicMaterialCombine.Maximum,
            frictionCombine = PhysicMaterialCombine.Maximum,
            staticFriction = 0.75f,
        };
        public static readonly PhysicMaterial PMR = new PhysicMaterial("RUBR")
        {
            dynamicFriction = 0.75f,
            bounciness = 0.9f,
            bounceCombine = PhysicMaterialCombine.Maximum,
            frictionCombine = PhysicMaterialCombine.Maximum,
            staticFriction = 0.75f,
        };
        public static readonly PhysicMaterial PMI = new PhysicMaterial("ICE")
        {
            dynamicFriction = 0.1f,
            bounciness = 0.0f,
            bounceCombine = PhysicMaterialCombine.Average,
            frictionCombine = PhysicMaterialCombine.Average,
            staticFriction = 0.1f,
        };
        public static readonly PhysicMaterial PMN = new PhysicMaterial("NOFRICT")
        {
            dynamicFriction = 0.0f,
            bounciness = 0.0f,
            bounceCombine = PhysicMaterialCombine.Average,
            frictionCombine = PhysicMaterialCombine.Minimum,
            staticFriction = 0.0f,
        };

    }
}
