using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using TerraTechETCUtil;

namespace Sub_Missions
{
    internal abstract class SpawnableTech
    {
        internal readonly string name;
        private Texture2D textureCached;
        internal SpawnableTech(string name)
        {
            this.name = name;
        }
        internal Texture2D GetTexture(SubMissionTree tree)
        {
            CreateTexture(tree,
            delegate (TechData techData, Texture2D techImage2)
            {
                if (techImage2.IsNotNull())
                    textureCached = techImage2;
            });
            if (textureCached == null)
                return Texture2D.whiteTexture;
            return textureCached;
        }
        internal void ReleaseTexture()
        {
            textureCached = null;
        }
        internal abstract Tank Spawn(SubMission mission, Vector3 Pos, Vector3 Fwd, int Team);
        protected abstract void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback);
        internal abstract void Remove(SubMissionTree tree);
    }
    internal class SpawnableTechRAW : SpawnableTech
    {
        internal SpawnableTechRAW(string name) : base(name) { }
        internal override Tank Spawn(SubMission mission, Vector3 Pos, Vector3 Fwd, int Team)
        {
            Tank tech = null;
            string targetPath = Path.Combine(SMissionJSONLoader.BaseDirectory , "Custom SMissions",
                mission.Tree.TreeName, "Raw Techs", name + ".json");
            if (File.Exists(targetPath))
            {
                RawTechTemplate rawTech = new RawTechTemplate(BlockIndexer.RawTechToTechData(name, File.ReadAllText(targetPath),out _));
                rawTech.SpawnRawTech(Pos, Team, Fwd, false, true, false);
                tech = null;
                SMUtil.SetTrackedTech(ref mission, tech);
                if (Team == ManSpawn.NeutralTeam)
                    tech.SetInvulnerable(true, true);
            }
            return tech;
        }
        protected override void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback)
        {
            string targetPath = Path.Combine(SMissionJSONLoader.BaseDirectory, "Custom SMissions" ,
                tree.TreeName, "Raw Techs", name + ".json");
            if (File.Exists(targetPath))
                ManScreenshot.inst.RenderTechImage(BlockIndexer.RawTechToTechData(name, File.ReadAllText(targetPath), out _), new IntVector2(64, 64), false, callback);
            else
            {
                targetPath = Path.Combine(SMissionJSONLoader.BaseDirectory, "Custom SMissions",
                    tree.TreeName, "Raw Techs", name + ".json");
                if (File.Exists(targetPath))
                    ManScreenshot.inst.RenderTechImage(BlockIndexer.RawTechToTechData(name, File.ReadAllText(targetPath), out _), new IntVector2(64, 64), false, callback);
            }
        }

        internal override void Remove(SubMissionTree tree)
        {
            string targetPath = Path.Combine(SMissionJSONLoader.BaseDirectory, "Custom SMissions",
                tree.TreeName, "Raw Techs", name + ".json");
            if (File.Exists(targetPath))
                File.Delete(targetPath);
        }
    }
    internal class SpawnableTechSnapshot : SpawnableTech
    {
        internal SpawnableTechSnapshot(string name) : base(name) { }
        internal override Tank Spawn(SubMission mission, Vector3 Pos, Vector3 Fwd, int Team)
        {
            Tank tech = null;
            string dir = Path.Combine(mission.Tree.TreeHierachy.AddressName, "Techs", name + ".png");
            if (File.Exists(dir))
            {   // Supports normal snapshots
                if (ManScreenshot.TryDecodeSnapshotRender(FileUtils.LoadTexture(dir), out TechData.SerializedSnapshotData data))
                {
                    ManSpawn.TankSpawnParams spawn = new ManSpawn.TankSpawnParams
                    {
                        isInvulnerable = Team == 0,
                        teamID = Team,
                        blockIDs = null,
                        isPopulation = Team == -1,
                        techData = data.CreateTechData(),
                        position = Pos,
                        rotation = Quaternion.LookRotation(Fwd),
                    };
                    tech = ManSpawn.inst.SpawnTank(spawn, true);
                    SMUtil.SetTrackedTech(ref mission, tech);
                    if (Team == ManSpawn.NeutralTeam)
                        tech.SetInvulnerable(true, true);
                }
            }
            return tech;
        }
        protected override void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback)
        {
            if (tree.MissionTextures.TryGetValue((name + ".png").GetHashCode(), out Texture value))
                callback.Invoke(null, (Texture2D)value);
        }
        internal override void Remove(SubMissionTree tree)
        {
            string targetPath = Path.Combine(SMissionJSONLoader.BaseDirectory, "Custom SMissions",
                tree.TreeName, name + ".png");
            if (File.Exists(targetPath))
                File.Delete(targetPath);
        }
    }

    internal class SpawnableTechBundledRAW : SpawnableTech
    {
        private TextAsset Data;
        internal SpawnableTechBundledRAW(string name, TextAsset data) : base(name) 
        {
            Data = data;
        }
        internal override Tank Spawn(SubMission mission, Vector3 Pos, Vector3 Fwd, int Team)
        {
            Tank tech;
            RawTechTemplate rawTech = new RawTechTemplate(BlockIndexer.RawTechToTechData(name, Data.text, out _));
            rawTech.SpawnRawTech(Pos, Team, Fwd, false, true, false);
            tech = null;
            SMUtil.SetTrackedTech(ref mission, tech);
            if (Team == ManSpawn.NeutralTeam)
                tech.SetInvulnerable(true, true);
            return tech;
        }
        protected override void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback)
        {
            ManScreenshot.inst.RenderTechImage(BlockIndexer.RawTechToTechData(name, Data.text, out _), new IntVector2(64, 64), false, callback);
        }

        internal override void Remove(SubMissionTree tree)
        {
            tree.TreeTechs.Remove(name);
            ((SubMissionHierachyAssetBundle)tree.TreeHierachy).TechNames.Remove(name);
        }
    }
    internal class SpawnableTechBundledSnapshot : SpawnableTech
    {
        private Texture2D snapInst;
        internal SpawnableTechBundledSnapshot(string name, Texture2D snap) : base(name)
        { snapInst = snap; }
        internal override Tank Spawn(SubMission mission, Vector3 Pos, Vector3 Fwd, int Team)
        {
            Tank tech = null;
            // Supports normal snapshots
            if (ManScreenshot.TryDecodeSnapshotRender(snapInst, out TechData.SerializedSnapshotData data))
            {
                ManSpawn.TankSpawnParams spawn = new ManSpawn.TankSpawnParams
                {
                    isInvulnerable = Team == 0,
                    teamID = Team,
                    blockIDs = null,
                    isPopulation = Team == -1,
                    techData = data.CreateTechData(),
                    position = Pos,
                    rotation = Quaternion.LookRotation(Fwd),
                };
                tech = ManSpawn.inst.SpawnTank(spawn, true);
                SMUtil.SetTrackedTech(ref mission, tech);
                if (Team == ManSpawn.NeutralTeam)
                    tech.SetInvulnerable(true, true);
            }
            return tech;
        }
        protected override void CreateTexture(SubMissionTree tree, ManScreenshot.OnTechRendered callback)
        {
            callback.Invoke(null, snapInst);
        }
        internal override void Remove(SubMissionTree tree)
        {
            tree.TreeTechs.Remove(name);
            ((SubMissionHierachyAssetBundle)tree.TreeHierachy).TechNames.Remove(name);
        }
        /*
        internal override void Remove(SubMissionTree tree)
        {
            throw new InvalidOperationException("Techs bundled within AssetBundles cannot be removed.  " +
                "Please remove it before building the mod");
        }*/
    }
}
