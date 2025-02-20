using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using TerraTechETCUtil;
using Newtonsoft.Json;
using TAC_AI.Templates;
using System.Security.Cryptography;

namespace Sub_Missions
{
    internal abstract class SpawnableTech
    {
        protected IntVector2 imageSize = new IntVector2(256, 256);
        /// <summary>
        /// Also used as the ID of the Tech!!!
        /// </summary>
        internal readonly string name;
        private Texture2D textureCached;
        protected bool textureCachCalled = false;
        internal SpawnableTech(string name)
        {
            this.name = name;
        }
        internal Texture2D GetTexture(SubMissionTree tree)
        {
            if (!textureCachCalled)
            {
                textureCachCalled = true;
                CreateTexture(tree,
                    delegate (TechData techData, Texture2D techImage2)
                    {
                        if (techImage2.IsNotNull())
                            textureCached = techImage2;
                    });
            }
            if (textureCached == null)
                return Texture2D.whiteTexture;
            return textureCached;
        }
        internal void GetTextureAsync(SubMissionTree tree, ManScreenshot.OnTechRendered rend)
        {
            CreateTexture(tree,
            delegate (TechData techData, Texture2D techImage2)
            {
                if (techImage2.IsNotNull())
                    rend.Invoke(techData, techImage2);
            });
        }
        internal void ReleaseTexture()
        {
            textureCached = null;
        }
        internal abstract Tank Spawn(SubMissionTree tree, Vector3 Pos, Vector3 Fwd, int Team);
        internal Tank Spawn(SubMission mission, Vector3 Pos, Vector3 Fwd, int Team)
        {
            Tank tech = Spawn(mission.Tree, Pos, Fwd, Team);
            /*
            if (tech != null)
            {
                SMUtil.SetTrackedTech(ref mission, tech);
                if (Team == ManSpawn.NeutralTeam)
                    tech.SetInvulnerable(true, true);
            }*/
            return tech;
        }
        protected abstract void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback);
        internal abstract void Remove(SubMissionTree tree);
    }
    internal class SpawnableTechRAW : SpawnableTech
    {
        internal SpawnableTechRAW(string name) : base(name)
        {
            Debug_SMissions.Log(KickStart.ModID + ": Registered " + name + " as SpawnableTechRAW");
        }
        protected string GetDirectory(SubMissionTree tree) => Path.Combine(SMissionJSONLoader.BaseDirectory,
                "Custom SMissions", tree.TreeName, "Raw Techs", name);
        internal override Tank Spawn(SubMissionTree tree, Vector3 Pos, Vector3 Fwd, int Team)
        {
            string targetPath = GetDirectory(tree);
            try
            {
                if (File.Exists(targetPath))
                {
                    RawTechTemplate rawTech = JsonConvert.DeserializeObject<RawTechTemplate>(File.ReadAllText(targetPath), SMissionJSONLoader.JSONSaver);
                    if (rawTech == null)
                        throw new NullReferenceException("rawTech failed to generate");
                    var tanker = rawTech.SpawnRawTech(Pos, Team, Fwd, false, true, false);
                    tanker.SetName(rawTech.techName);
                    SMUtil.Log(false, tanker.name);
                    return tanker;
                }
            }
            catch (Exception e)
            {
                SMUtil.Error(true, "SpawnableTechRAW.Spawn()", "Mission RawTech " + name + " is corrupted!" + 
                    "\n" + e.ToString());
            }
            return null;
        }
        protected override void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback)
        {
            string targetPath = GetDirectory(tree);
            try
            {
                if (File.Exists(targetPath))
                {
                    RawTechTemplate rawTech = JsonConvert.DeserializeObject<RawTechTemplate>(File.ReadAllText(targetPath), SMissionJSONLoader.JSONSaver);
                    ManScreenshot.inst.RenderTechImage(BlockIndexer.RawTechToTechData(name, rawTech.savedTech, out _),
                        imageSize, false, callback);
                }
            }
            catch (Exception e)
            {
                SMUtil.Error(true, "SpawnableTechRAW.CreateTexture()", "Mission RawTech " + name + " is corrupted!" +
                    "\n" + e);
            }
        }

        internal override void Remove(SubMissionTree tree)
        {
            string targetPath = GetDirectory(tree);
            tree.TreeTechs.Remove(name);
            if (File.Exists(targetPath))
                File.Delete(targetPath);
        }
    }
    internal class SpawnableTechSnapshot : SpawnableTech
    {
        // Node: "name" in this case INCLUDES the extension!
        internal SpawnableTechSnapshot(string name) : base(name)
        {
            Debug_SMissions.Log(KickStart.ModID + ": Registered " + name + " as SpawnableTechSnapshot");
        }
        protected string GetDirectory(SubMissionTree tree) => Path.Combine(SMissionJSONLoader.BaseDirectory,
                "Custom SMissions", tree.TreeName, "Techs", name);
        internal override Tank Spawn(SubMissionTree tree, Vector3 Pos, Vector3 Fwd, int Team)
        {
            Tank tech = null;
            if (tree.MissionTextures.TryGetValue(name, out Texture value))
            {   // Supports normal snapshots
                if (ManScreenshot.TryDecodeSnapshotRender((Texture2D)value, out TechData.SerializedSnapshotData data,
                     name, false))
                {
                    ManSpawn.TankSpawnParams spawn = new ManSpawn.TankSpawnParams
                    {
                        isInvulnerable = Team == ManSpawn.NeutralTeam,
                        teamID = Team,
                        blockIDs = null,
                        isPopulation = Team == ManSpawn.NewEnemyTeam,
                        techData = data.CreateTechData(),
                        position = Pos,
                        forceSpawn = true,
                        rotation = Quaternion.LookRotation(Fwd),
                    };
                    tech = ManSpawn.inst.SpawnTank(spawn, true);
                }
                else
                    Debug_SMissions.LogError("Could not spawn tech " + name + " - decode renders failed");
            }
            else
            {
                Debug_SMissions.LogError("Could not get Tech " + name);
                Debug_SMissions.Log("PRESENT TEXTURES:");
                foreach (var item in tree.MissionTextures)
                {
                    Debug_SMissions.Log(" - " + item.Key);
                }
            }
            return tech;
        }
        protected override void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback)
        {
            if (tree.MissionTextures.TryGetValue(name, out Texture value))
                callback.Invoke(null, (Texture2D)value);
            else
            {
                Debug_SMissions.LogError("Could not get texture for " + name);
                Debug_SMissions.Log("PRESENT TEXTURES:");
                foreach (var item in tree.MissionTextures)
                {
                    Debug_SMissions.Log(" - " + item.Key);
                }
            }
        }
        internal override void Remove(SubMissionTree tree)
        {
            string targetPath = GetDirectory(tree);
            tree.TreeTechs.Remove(name);
            tree.MissionTextures.Remove(name);
            if (File.Exists(targetPath))
                File.Delete(targetPath);
        }
    }

    internal class SpawnableTechBundledRAW : SpawnableTech
    {
        private TextAsset Data;
        private RawTechTemplate tech;
        internal SpawnableTechBundledRAW(string name, TextAsset data) : base(name) 
        {
            Data = data;
            Debug_SMissions.Log(KickStart.ModID + ": Registered " + name + " as SpawnableTechBundledRAW");
            tech = JsonConvert.DeserializeObject<RawTechTemplate>(Data.text);
        }
        internal override Tank Spawn(SubMissionTree tree, Vector3 Pos, Vector3 Fwd, int Team)
        {
            return tech.SpawnRawTech(Pos, Team, Fwd, false, true, false);
        }
        protected override void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback)
        {
            ManScreenshot.inst.RenderTechImage(BlockIndexer.RawTechToTechData(name, tech.savedTech, out _), 
                imageSize, false, callback);
        }

        internal override void Remove(SubMissionTree tree)
        {
            ((SubMissionHierachyAssetBundle)tree.TreeHierachy).TechNames.Remove(name);
            tree.TreeTechs.Remove(name);
        }
    }
    internal class SpawnableTechBundledSnapshot : SpawnableTech
    {
        private Texture2D snapInst;
        internal SpawnableTechBundledSnapshot(string name, Texture2D snap) : base(name)
        {
            snapInst = snap;
            Debug_SMissions.Log(KickStart.ModID + ": Registered " + name + " as SpawnableTechBundledSnapshot");
        }
        internal override Tank Spawn(SubMissionTree tree, Vector3 Pos, Vector3 Fwd, int Team)
        {
            Tank tech = null;
            // Supports normal snapshots
            if (ManScreenshot.TryDecodeSnapshotRender(snapInst, out TechData.SerializedSnapshotData data,
                name, false))
            {
                ManSpawn.TankSpawnParams spawn = new ManSpawn.TankSpawnParams
                {
                    isInvulnerable = Team == -2,
                    teamID = Team,
                    blockIDs = null,
                    isPopulation = Team == -1,
                    techData = data.CreateTechData(),
                    position = Pos,
                    rotation = Quaternion.LookRotation(Fwd),
                };
                tech = ManSpawn.inst.SpawnTank(spawn, true);
            }
            return tech;
        }
        protected override void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback)
        {
            callback.Invoke(null, snapInst);
        }
        internal override void Remove(SubMissionTree tree)
        {
            ((SubMissionHierachyAssetBundle)tree.TreeHierachy).TechNames.Remove(name);
            tree.TreeTechs.Remove(name);
        }
        /*
        internal override void Remove(SubMissionTree tree)
        {
            throw new InvalidOperationException("Techs bundled within AssetBundles cannot be removed.  " +
                "Please remove it before building the mod");
        }*/
    }
    internal class SpawnableTechFromPool : SpawnableTech
    {
        private RawTechPopParams popParams;
        internal SpawnableTechFromPool(string name, RawTechPopParams popParams) : base(name)
        {
            this.popParams = popParams;
            Debug_SMissions.Log(KickStart.ModID + ": Registered " + name + " as SpawnableTechFromPool");
        }
        internal override Tank Spawn(SubMissionTree tree, Vector3 Pos, Vector3 Fwd, int Team)
        {
            return RawTechLoader.SpawnRandomTechAtPosHead(Pos, Fwd, Team, popParams, false);
        }
        protected override void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback)
        {
            callback.Invoke(null, ManIngameWiki.BlocksSprite.texture);
        }
        internal override void Remove(SubMissionTree tree)
        {
            if (tree.TreeHierachy is SubMissionHierachyDirectory SMHD)
            {
                string path = Path.Combine(SMHD.TreeDirectory, "Pop Params", name);
                if (File.Exists(path))
                    File.Delete(path);
            }
            else if (tree.TreeHierachy is SubMissionHierachyAssetBundle SMHAB)
                SMHAB.TechNames.Remove(name);
            tree.TreeTechs.Remove(name);
        }
        /*
        internal override void Remove(SubMissionTree tree)
        {
            throw new InvalidOperationException("Techs bundled within AssetBundles cannot be removed.  " +
                "Please remove it before building the mod");
        }*/
        internal void ReloadFromDisk(SubMissionTree tree)
        {
            if (tree.TreeHierachy is SubMissionHierachyDirectory SMHD)
            {
                string path = Path.Combine(SMHD.TreeDirectory, "Pop Params", name);
                if (File.Exists(path))
                {
                    string fileData = File.ReadAllText(path);
                    var temp = JsonConvert.DeserializeObject<RawTechPopParams>(fileData);
                    if (temp != null)
                        popParams = temp;
                }
            }
        }
    }
}
