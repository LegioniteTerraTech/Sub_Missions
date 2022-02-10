using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

namespace Sub_Missions
{
    internal class SMWorldObject : MonoBehaviour, IWorldTreadmill
    {
        private const float MaxTransitionSpeedUp = 4;
        private const float MaxTransitionSpeedDown = 16;
        private const float TransitionLerpSmooth = 45;
        private const float TransitionSnap = 5;
        [JsonIgnore]
        public bool animateUp = true;
        [JsonIgnore]
        public float distDiff = -9999;


        public string Name = "Unset";
        public string TextureName;
        public string GameMaterialName;
        public string VisualMeshName;
        public string ColliderMeshName;
        public bool aboveGround;

        public float aimedHeight;
        public IntVector2 tilePos;
        public Vector3 offsetFromTile;

        public Dictionary<string, object> WorldObjectJSON;

        public void Activate(Vector3 scenePos)
        {
            Singleton.Manager<ManWorldTreadmill>.inst.AddListener(this);
            IntVector3 posSet = new IntVector3(scenePos);
            aimedHeight = posSet.y;
            transform.position = posSet;
            if (ManWorld.inst.GetTerrainHeight(transform.position, out float height))
                transform.position = transform.position.SetY(height - GetComponent<Collider>().bounds.size.y);
            tilePos = ManWorld.inst.TileManager.SceneToTileCoord(scenePos);
            offsetFromTile = new IntVector3(scenePos - ManWorld.inst.TileManager.CalcTileCentreScene(tilePos));
            animateUp = true;
            enabled = true;
        }
        /// <summary>
        /// For Spawning back in from save
        /// </summary>
        /// <param name="tilePos"></param>
        /// <param name="posInTile"></param>
        public void Activate(IntVector2 tilePos, Vector3 posInTile)
        {
            Singleton.Manager<ManWorldTreadmill>.inst.AddListener(this);
            IntVector3 posSet = new IntVector3(posInTile + ManWorld.inst.TileManager.CalcTileCentreScene(tilePos));
            aimedHeight = posSet.y;
            transform.position = posSet;
            this.tilePos = tilePos;
            offsetFromTile = posInTile;
        }
        public void FixedUpdate()
        {   // only used to update the position while loading in/out
            if (animateUp)
            {
                if (aimedHeight - 0.05f < transform.position.y)
                {
                    transform.position = transform.position.SetY(aimedHeight);
                    enabled = false;
                }
                float heightAdd = aimedHeight - transform.position.y;
                heightAdd = Mathf.Clamp((heightAdd * heightAdd) / TransitionLerpSmooth, 0.01f, MaxTransitionSpeedUp);
                transform.position = transform.position.SetY(transform.position.y + heightAdd);
            }
            else
            {
                float aimFor = GetTerrainHideDepth();
                if (aimFor > transform.position.y)
                {
                    Singleton.Manager<ManWorldTreadmill>.inst.RemoveListener(this);
                    Destroy(gameObject);
                }
                float heightAdd = transform.position.y - aimedHeight;
                heightAdd = Mathf.Clamp(-((heightAdd * heightAdd) / TransitionLerpSmooth), -MaxTransitionSpeedDown, -0.01f);
                transform.position = transform.position.SetY(transform.position.y + heightAdd);
            }
        }
        public void OnMoveWorldOrigin(IntVector3 moveDist)
        {
            transform.position += moveDist;
        }
        public float GetTerrainHideDepth()
        {
            if (distDiff == -9999)
            {
                if (ManWorld.inst.GetTerrainHeight(transform.position, out float height))
                    distDiff = height - GetComponent<Collider>().bounds.size.magnitude;
                else
                    distDiff = transform.position.y - 64;
            }
            return distDiff;
        }

        public void SetFromJSON(SMWorldObjectJSON toSetFrom)
        {
            WorldObjectJSON = toSetFrom.WorldObjectJSON;
            Name = toSetFrom.Name;
            TextureName = toSetFrom.TextureName;
            GameMaterialName = toSetFrom.GameMaterialName;
            VisualMeshName = toSetFrom.VisualMeshName;
            ColliderMeshName = toSetFrom.ColliderMeshName;
            aboveGround = toSetFrom.aboveGround;
        }
        public SMWorldObjectJSON GetJSON()
        {
            SMWorldObjectJSON final = new SMWorldObjectJSON
            {
                WorldObjectJSON = WorldObjectJSON,
                Name = Name,
                TextureName = TextureName,
                GameMaterialName = GameMaterialName,
                VisualMeshName = VisualMeshName,
                ColliderMeshName = ColliderMeshName,
                aboveGround = aboveGround,
            };
            return final;
        }
        public void Remove(bool immedeate,bool RemoveFromManager = true)
        {
            if (RemoveFromManager)
                ManModularMonuments.Unregister(this);
            if (immedeate)
            {
                Singleton.Manager<ManWorldTreadmill>.inst.RemoveListener(this);
                Destroy(gameObject);
                return;
            }
            animateUp = false;
            enabled = true;
        }
    }
    public class SMWorldObjectJSON
    {
        public string Name = "Unset";
        public string TextureName;
        public string GameMaterialName;
        public string VisualMeshName;
        public string ColliderMeshName;
        public bool aboveGround = false;

        public Dictionary<string, object> WorldObjectJSON;
    }
}
