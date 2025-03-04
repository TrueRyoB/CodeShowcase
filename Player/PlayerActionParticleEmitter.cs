using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Fujin.Player
{
    /// <summary>
    /// When adding a new particle system...
    /// 1. SerializeField the Particle System prefab and create the dedicated Queue of ParticleSystem
    /// 2. Register the target summon transform to the dictionary at Start()
    /// 3. Create another WaitForSeconds at Start()
    /// 4. Create public function for creating a single particle system (basically a copy pasta)
    /// 5. Add EmptyPool method to OnApplicationPause()
    ///
    /// Good luck!
    /// </summary>
    public class PlayerActionParticleEmitter : MonoBehaviour
    {
        [SerializeField] private Transform backFeet;
        [SerializeField] private GameObject splashPrefab;
        [SerializeField] private Sprite normalSplash;
        [SerializeField] private Sprite rareSplash;
        [SerializeField] private GameObject starPrefab;
        [SerializeField] private Sprite normalStar;
        [SerializeField] private Sprite rareStar;
        [SerializeField] private int numKaguoAtOnce = 10;
        [SerializeField] private GameObject rocketPsPrefab;
        
        private readonly Queue<ParticleSystem> splashPool = new Queue<ParticleSystem>();
        private readonly Queue<ParticleSystem> starPool = new Queue<ParticleSystem>();
        private readonly Queue<ParticleSystem> rocketPsPool = new Queue<ParticleSystem>();

        private int frameCount;
        private int chanceThreshold;
        private int rareActivated;
        
        private readonly Dictionary<GameObject, Transform> psTransformMap = new Dictionary<GameObject, Transform>();

        private WaitForSeconds waitForSplash;
        private WaitForSeconds waitForStar;
        private WaitForSeconds waitForRocketPs;
        
        private void Start()
        {
            chanceThreshold = Random.Range(2000, 4000000);
            
            psTransformMap[splashPrefab] = backFeet;
            psTransformMap[starPrefab] = transform;
            psTransformMap[rocketPsPrefab] = transform;

            ParticleSystem ps = splashPrefab.GetComponent<ParticleSystem>();
            waitForSplash = new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);
            ps = starPrefab.GetComponent<ParticleSystem>();
            waitForStar = new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);
            ps = rocketPsPrefab.GetComponent<ParticleSystem>();
            waitForRocketPs = new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);
        }

        private void FixedUpdate()
        {
            frameCount++;

            if (frameCount >= chanceThreshold) {
                frameCount = 0;
                rareActivated = numKaguoAtOnce;
                chanceThreshold = Random.Range(20000, 4000000);
            }
        }

        public void RocketLaunch(Vector3? lookVec = null)
        {
            ParticleSystem ps = GetPooledObject(rocketPsPool, rocketPsPrefab);
            if (lookVec.HasValue)
            {
                ps.transform.rotation = Quaternion.Euler(-90, 0, 0) * Quaternion.LookRotation(lookVec.Value);
            }
            ps.Play();
            StartCoroutine(ReturnEffToPoolRemake(rocketPsPool, ps, waitForRocketPs));
        }

        public void Splash()
        {
            ParticleSystem ps = GetPooledObject(splashPool, splashPrefab);
            if(rareActivated != 0) {
                ps.textureSheetAnimation.SetSprite(0, rareSplash);
                -- rareActivated;
            } else {
                ps.textureSheetAnimation.SetSprite(0, normalSplash);
            }
            ps.Play();
            StartCoroutine(ReturnEffToPoolRemake(splashPool, ps, waitForSplash));
        }

        public void ShootStar()
        {
            ParticleSystem ps = GetPooledObject(starPool, starPrefab);
            if(rareActivated != 0) {
                ps.textureSheetAnimation.SetSprite(0, rareStar);
                -- rareActivated;
            } else {
                ps.textureSheetAnimation.SetSprite(0, normalStar);
            }
            ps.Play();
            StartCoroutine(ReturnEffToPoolRemake(starPool, ps, waitForStar));
        }
        
        private ParticleSystem GetPooledObject(Queue<ParticleSystem> pool, GameObject eff)
        {
            if (pool.TryDequeue(out ParticleSystem ps) && ps != null)
            {
                ps.gameObject.SetActive(true);
                ps.transform.position = psTransformMap[eff].position;
                ps.Clear();
                return ps;
            }
            
            return Instantiate(eff, psTransformMap[eff].position, eff.transform.rotation).GetComponent<ParticleSystem>();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            EmptyPool(splashPool);
            EmptyPool(starPool);
            EmptyPool(rocketPsPool);
        }

        private void EmptyPool(Queue<ParticleSystem> pool)
        {
            while(pool.Count > 0){
                GameObject del = pool.Dequeue().gameObject;
                Destroy(del);
            }
        }

        private IEnumerator ReturnEffToPoolRemake(Queue<ParticleSystem> pool, ParticleSystem obs, WaitForSeconds wait)
        {
            yield return wait; 
            obs.gameObject.SetActive(false);
            pool.Enqueue(obs);
        }
    }
}