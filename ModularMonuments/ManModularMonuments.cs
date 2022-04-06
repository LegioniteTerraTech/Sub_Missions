using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sub_Missions
{
    public class ManModularMonuments : MonoBehaviour
    {
        public static Dictionary<int, GameObject> WorldObjects = new Dictionary<int, GameObject>();

        internal static List<GameObject> ActiveWorldObjects = new List<GameObject>();
        private static List<SMWorldObject> PermWorldObjects = new List<SMWorldObject>();
        private static Dictionary<IntVector2, List<ModularMonumentSave>> PermWorldObjectsUnloaded = new Dictionary<IntVector2, List<ModularMonumentSave>>();



        public static bool SpawnMM(string name, Vector3 scenePos, Vector3 forwardsLookEulers, Vector3 scale, out GameObject MM)
        {
            int hash = name.GetHashCode();
            if (WorldObjects.TryGetValue(hash, out GameObject inst))
            {
                scale.x = scale.x <= 0 ? 1 : scale.x;
                scale.y = scale.x <= 0 ? 1 : scale.y;
                scale.z = scale.x <= 0 ? 1 : scale.z;
                MM = Instantiate(inst, null, true);
                MM.GetComponent<SMWorldObject>().Activate(scenePos);
                ActiveWorldObjects.Add(MM);
                MM.transform.eulerAngles = forwardsLookEulers;
                MM.transform.localScale = scale;
                MM.SetActive(true);
                return true;
            }
            MM = null;
            SMUtil.Assert(false, "SubMissions: ManModularMonuments - WorldObject " + name + " does not exists");
            return false;
        }
        public static bool SpawnMM(ModularMonumentSave MMS, out GameObject MM)
        {
            int hash = MMS.name.GetHashCode();
            if (WorldObjects.TryGetValue(hash, out GameObject inst))
            {
                MMS.scale.x = MMS.scale.x <= 0 ? 1 : MMS.scale.x;
                MMS.scale.y = MMS.scale.x <= 0 ? 1 : MMS.scale.y;
                MMS.scale.z = MMS.scale.x <= 0 ? 1 : MMS.scale.z;
                MM = Instantiate(inst, null, true);
                MM.GetComponent<SMWorldObject>().Activate(MMS.tilePos, MMS.offsetFromTile);
                ActiveWorldObjects.Add(MM);
                MM.transform.eulerAngles = MMS.eulerAngles;
                MM.transform.localScale = MMS.scale;
                MM.SetActive(true);
                return true;
            }
            MM = null;
            SMUtil.Assert(false, "SubMissions: ManModularMonuments - WorldObject " + MMS.name + " does not exists");
            return false;
        }


        internal static void Unregister(SMWorldObject remove)
        {
            try
            {
                ActiveWorldObjects.Remove(remove.gameObject);
            }
            catch { }
        }
        internal static void GraduateToPerm(List<SMWorldObject> makePerm)
        {
            if (makePerm == null)
                return;
            foreach (SMWorldObject SMWO in makePerm)
                PermWorldObjects.Add(SMWO);
            Debug.Log("SubMissions: ManModularMonuments - Graduated " + makePerm.Count + " pieces to perm");
        }
        internal static ModularMonumentSave Save(SMWorldObject SMWO)
        {
            if (SMWO != null)
            {
                ModularMonumentSave MMS1 = new ModularMonumentSave();
                MMS1.name = SMWO.Name;
                MMS1.eulerAngles = SMWO.transform.eulerAngles;
                MMS1.tilePos = SMWO.tilePos;
                MMS1.offsetFromTile = SMWO.offsetFromTile;
                MMS1.scale = SMWO.transform.localScale;
                return MMS1;
            }
            return null;
        }
        public static List<ModularMonumentSave> SaveAll()
        {
            List<ModularMonumentSave> MMS = new List<ModularMonumentSave>();
            foreach (SMWorldObject SMWO in PermWorldObjects)
            {
                if (SMWO != null)
                {
                    MMS.Add(Save(SMWO));
                }
            }

            foreach (List<ModularMonumentSave> SMWOl in PermWorldObjectsUnloaded.Values)
            {
                MMS.AddRange(SMWOl);
            }
            return MMS;
        }
        public static void LoadAll(List<ModularMonumentSave> MMS)
        {
            PurgeAllPermOnly();
            foreach (ModularMonumentSave MMS1 in MMS)
            {
                if (MMS1 != null)
                {
                    if (PermWorldObjectsUnloaded.TryGetValue(MMS1.tilePos, out List<ModularMonumentSave> val))
                        val.Add(MMS1);
                    else
                        PermWorldObjectsUnloaded.Add(MMS1.tilePos, new List<ModularMonumentSave> { MMS1 });
                }
            }
        }

        public static void LoadAllAtTile(IntVector2 tilePos)
        {
            if (PermWorldObjectsUnloaded.TryGetValue(tilePos, out List<ModularMonumentSave> val))
            {
                foreach (ModularMonumentSave MMS in val)
                    SpawnMM(MMS, out GameObject GO);
                PermWorldObjectsUnloaded.Remove(tilePos);   
            }
        }
        public static void UnloadAllAtTile(IntVector2 tilePos)
        {
            List<SMWorldObject> toUnload = PermWorldObjects.FindAll(delegate (SMWorldObject cand) { return cand.tilePos == tilePos; });
            if (toUnload.Count > 0)
            {
                foreach (SMWorldObject SMWO in toUnload)
                {
                    if (PermWorldObjectsUnloaded.TryGetValue(tilePos, out List<ModularMonumentSave> val))
                        val.Add(Save(SMWO));
                    else
                        PermWorldObjectsUnloaded.Add(tilePos, new List<ModularMonumentSave> { Save(SMWO) });
                    PermWorldObjects.Remove(SMWO);
                    SMWO.Remove(true);
                }
            }
            UpdateActive();
        }

        public static void PurgeAllPermOnly()
        {
            foreach (SMWorldObject SMWO in PermWorldObjects)
            {
                if (SMWO)
                    SMWO.Remove(false);
            }
            PermWorldObjects.Clear();
            PermWorldObjectsUnloaded.Clear();
            UpdateActive();
        }
        public static void PurgeAllActive()
        {
            foreach (GameObject GO in ActiveWorldObjects)
            {
                if (GO)
                    GO.GetComponent<SMWorldObject>().Remove(false);
            }
            ActiveWorldObjects.Clear();
        }

        private static void UpdateActive()
        {
            int count = ActiveWorldObjects.Count;
            for (int step = 0; step < count; )
            {
                if (!ActiveWorldObjects.ElementAt(step))
                {
                    ActiveWorldObjects.RemoveAt(step);
                    count--;
                }
                else
                    step++;
            }
            ActiveWorldObjects.Clear();
        }
    }
    public class ModularMonumentSave
    {
        public string name;
        public IntVector2 tilePos;
        public Vector3 offsetFromTile;
        public Vector3 eulerAngles;
        public Vector3 scale;
    }
}
