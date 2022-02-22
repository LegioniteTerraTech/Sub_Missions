using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class SMExplosion : Sub_Missions.SMExplosion { }

namespace Sub_Missions
{
    // General-Purpose explosion for use beyond vanilla for SubMissions
    public class SMExplosion : MonoBehaviour
    {
        private const float vertOffsetDist = 250;
        internal static List<QueuedBomb> queuedBombs = new List<QueuedBomb>();
        private static GameObject bombPrefab;
        public static void OrbitalLaunch(Vector3 scenePos, Vector3 forwards)
        {
            //  The bomb spawns about 500 meters off the ground, this will have to predict based on that
            DeliveryBombSpawner DBS = ManSpawn.inst.SpawnDeliveryBombNew(scenePos, DeliveryBombSpawner.ImpactMarkerType.Tech);
            DBS.SetSpawnParams(scenePos + (Vector3.up * vertOffsetDist), DeliveryBombSpawner.ImpactMarkerType.Tech);
            QueuedBomb QB = new QueuedBomb(DBS, forwards);
            queuedBombs.Add(QB);
        }
        public static void SpawnNew(Vector3 scenePos, Vector3 forwards)
        {
            if (!bombPrefab)
            {
                bombPrefab = new GameObject("AirstrikeBomb");
                bombPrefab.AddComponent<SMExplosion>();
                bombPrefab.SetActive(false);
            }
            if (ManWorld.inst.GetTerrainHeight(scenePos, out float height))
            {
                if (height > scenePos.y)
                    scenePos.y = height;
                scenePos.y += 0.75f;
                Instantiate(bombPrefab, scenePos, Quaternion.LookRotation(forwards), null).SetActive(true);
            }
        }
        public void FixedUpdate()
        {
            Detonate();
        }
        public void Detonate()
        {
            TankBlock TB = ManSpawn.inst.GetBlockPrefab(BlockTypes.GSOBigBertha_845);
            var FD = TB.GetComponent<FireData>();
            ForceExplodeCopy(FD.m_BulletPrefab.GetComponent<Projectile>());
            ManSFX.inst.PlayMiscSFX(ManSFX.MiscSfxType.IntroExplosion);
            enabled = false;
            Destroy(this);
        }

        private static FieldInfo explode = typeof(Projectile).GetField("m_Explosion", BindingFlags.NonPublic | BindingFlags.Instance);
        private void ForceExplodeCopy(Projectile proj)
        {
            Transform explodo = (Transform)explode.GetValue(proj);
            if ((bool)explodo)
            {
                var boom = explodo.GetComponent<Explosion>();
                if ((bool)boom)
                {
                    Explosion boom2 = explodo.UnpooledSpawnWithLocalTransform(null, transform.position, Quaternion.identity).GetComponent<Explosion>();
                    if (boom2 != null)
                    {
                        boom2.SetDamageSource(null);
                        boom2.SetDirectHitTarget(null);
                        boom2.m_MaxDamageStrength *= 3;
                        boom2.m_MaxImpulseStrength *= 3;
                        boom2.m_EffectRadius *= 1.5f;
                        boom2.DoDamage = true;
                        foreach (ParticleSystem PS in boom2.GetComponentsInChildren<ParticleSystem>())
                        {
                            var main = PS.main;
                            main.startSizeMultiplier *= 1.5f;
                            main.startSpeedMultiplier *= 2f;
                        }
                        boom2.gameObject.SetActive(true);
                    }
                }
            }
        }
        internal class QueuedBomb
        {
            private Vector3 forwards = Vector3.forward;

            internal QueuedBomb(DeliveryBombSpawner DBS, Vector3 forward)
            {
                DBS.BombDeliveredEvent.Subscribe(OnImpact);
                //DBS.gameObject.AddComponent<KineticDriver>();
                forwards = forward;
            }
            private void OnImpact(Vector3 strike)
            {
                SpawnNew(strike, forwards);
                queuedBombs.Remove(this);
            }
        }
        internal class KineticDriver : MonoBehaviour
        {
            private void FixedUpdate()
            {
                var rbody = gameObject.GetComponent<Rigidbody>();
                if (rbody)
                {
                    if (rbody.useGravity)
                        rbody.AddForceAtPosition(Physics.gravity * 0.25f, rbody.position, ForceMode.Impulse);
                }
            }
        }
    }
}
