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
        public const int MaxBombsActive = 64;
        public const float MinimumDelayTime = 0.123f;

        public static int BombsActive => queuedBombs.Count;
        private static float nextFireTime = 0;

        private const float vertOffsetDist = 250;
        internal static List<QueuedBomb> queuedBombs = new List<QueuedBomb>();
        internal static List<Vector3> queuedExplosions = new List<Vector3>();
        private static GameObject bombController;
        public static void OrbitalLaunch(Vector3 scenePos, Vector3 forwards, Visible target = null)
        {
            if (BombsActive >= MaxBombsActive || Time.time < nextFireTime)
                return;
            nextFireTime = Time.time + MinimumDelayTime;

            //  The bomb spawns about 500 meters off the ground, this will have to predict based on that
            DeliveryBombSpawner DBS = ManSpawn.inst.SpawnDeliveryBombNew(scenePos, DeliveryBombSpawner.ImpactMarkerType.Tech);
            DBS.SetSpawnParams(scenePos + (Vector3.up * vertOffsetDist), DeliveryBombSpawner.ImpactMarkerType.Tech);
            QueuedBomb QB = new QueuedBomb(DBS, forwards, target);
            queuedBombs.Add(QB);
        }
        public static void SpawnNew(Vector3 scenePos, Vector3 forwards)
        {
            if (!bombController)
            {
                bombController = new GameObject("AirstrikeBomber");
                bombController.AddComponent<SMExplosion>();
            }
            if (ManWorld.inst.GetTerrainHeight(scenePos, out float height))
            {
                if (height > scenePos.y)
                    scenePos.y = height;
                scenePos.y += 0.75f;
                queuedExplosions.Add(scenePos);
                bombController.SetActive(true);
                //bombPrefab.transform.Spawn(null, scenePos, Quaternion.LookRotation(forwards))
            }
        }
        public void FixedUpdate()
        {
            foreach (var item in queuedExplosions)
            {
                Detonate(item);
            }
            queuedExplosions.Clear();
            bombController.SetActive(false);
        }
        public void Detonate(Vector3 pos)
        {
            ForceExplodeCopy(pos);
            ManSFX.inst.PlayMiscSFX(ManSFX.MiscSfxType.IntroExplosionHuge, pos);
        }

        private static FieldInfo explode = typeof(Projectile).GetField("m_Explosion", BindingFlags.NonPublic | BindingFlags.Instance);
        private static Transform explosionMain = null;
        private void ForceExplodeCopy(Vector3 pos)
        {
            if (explosionMain == null)
            {
                TankBlock TB = ManSpawn.inst.GetBlockPrefab(BlockTypes.GSOBigBertha_845);
                var FD = TB.GetComponent<FireData>();
                Projectile proj = FD.m_BulletPrefab.GetComponent<Projectile>();
                Transform explodo = (Transform)explode.GetValue(proj);
                if ((bool)explodo)
                {
                    var boom = explodo.GetComponent<Explosion>();
                    if ((bool)boom)
                    {
                        explosionMain = explodo;
                    }
                }
            }
            Explosion boom2 = explosionMain.UnpooledSpawnWithLocalTransform(null, pos, Quaternion.identity).GetComponent<Explosion>();
            if (boom2 != null)

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
        internal struct QueuedBomb
        {
            private Vector3 forwards;
            private KineticDriver bomb;
            internal Visible target;

            internal QueuedBomb(DeliveryBombSpawner DBS, Vector3 forward, Visible aimTarget = null)
            {
                target = aimTarget;
                forwards = forward;
                if (target)
                {
                    bomb = DBS.gameObject.AddComponent<KineticDriver>();
                    bomb.turnSped = UnityEngine.Random.Range(2, 16) + UnityEngine.Random.Range(1, 4);
                    bomb.bomb = this;
                }
                else
                    bomb = null;
                DBS.BombDeliveredEvent.Subscribe(OnImpact);
            }
            private void OnImpact(Vector3 strike)
            {
                SpawnNew(strike, forwards);
                if (bomb)
                    Destroy(bomb);
                queuedBombs.Remove(this);
            }
        }
        internal class KineticDriver : MonoBehaviour
        {
            internal QueuedBomb bomb;
            internal float turnSped;
            private void FixedUpdate()
            {
                var rbody = gameObject.GetComponent<Rigidbody>();
                if (rbody)
                {
                    if (!bomb.target)
                    {
                        enabled = false;
                        return;
                    }
                    var aiming = Quaternion.LookRotation(bomb.target.centrePosition - rbody.position, rbody.rotation * Vector3.up).normalized;
                    rbody.MoveRotation(Quaternion.RotateTowards(rbody.rotation, aiming, turnSped * Time.fixedDeltaTime)); 
                    /*
                    if (rbody.useGravity)
                        rbody.AddForceAtPosition(Physics.gravity * 0.25f, rbody.position, ForceMode.Impulse);
                    */
                }
            }
        }
    }
}
