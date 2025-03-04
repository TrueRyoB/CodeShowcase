// using System.Collections;
// using System;
// using TMPro;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// namespace Fujin.Mobs
// {
//     [RequireComponent(typeof(AudioSource))]
//     public class BonusScoreDisplayManager : MonoBehaviour
//     {
//         [SerializeField] TextMeshProUGUI scoreHolder;
//         [SerializeField] private float randomRange = 20f;
//         [SerializeField] private GameObject hanabiPrefab;
//         [SerializeField] private AudioClip hanabiSound;
//         [SerializeField] private AudioSource audioSource;
//
//         public void Initialize(int addedScore, string supportText, string colorCode)
//         {
//             hanabiColorCode = colorCode;
//             scoreHolder.text = $"<color={colorCode}>{supportText}</color>   +<color=#FFFFFF>{addedScore}</color>";
//         }
//
//         private string hanabiColorCode = "#FFFFFFF";
//         
//         private Coroutine coroutine;
//         
//         public void FloatAndExplode(float floatDuration, float waitTime, Action callback = null)
//         {
//             coroutine ??= StartCoroutine(PerformFloating(floatDuration, waitTime, () =>
//             {
//                 // Callback for calculation
//                 callback?.Invoke();
//                 
//                 // Summon hanabi and destroy itself
//                 LaunchHanabi();
//             }));
//         }
//
//         private void LaunchHanabi()
//         {
//             audioSource.PlayOneShot(hanabiSound); // This may not be played as intended because the gameObject is displayed before it's fully played
//             
//             Vector3 randomPos = new Vector3(
//                 Random.Range(-randomRange/2, randomRange/2),
//                 Random.Range(-randomRange/2, randomRange/2),
//                 Random.Range(-randomRange/2, randomRange/2)
//                 );
//             
//             GameObject hanabiObject = Instantiate(hanabiPrefab, Vector3.zero, Quaternion.identity);
//             hanabiObject.transform.SetParent(transform, false);
//             hanabiObject.transform.localPosition = randomPos;
//             ParticleSystem hanabiParticleSystem = hanabiObject.GetComponent<ParticleSystem>();
//             SetGradientColor(hanabiParticleSystem, hanabiColorCode);
//             hanabiParticleSystem.Play();
//             Destroy(hanabiObject, hanabiParticleSystem.main.duration);
//             StartCoroutine(FadeOutAndDestroyItself(hanabiParticleSystem.main.duration - 1f, hanabiParticleSystem.main.duration));
//         }
//
//         private void SetGradientColor(ParticleSystem ps, string colorCode)
//         {
//             ParticleSystem.ColorOverLifetimeModule refstant = ps.colorOverLifetime;
//             refstant.enabled = true;
//             refstant.color = new ParticleSystem.MinMaxGradient(GetGradientFadingOut(colorCode));
//         }
//
//         private Gradient GetGradientFadingOut(string colorCode)
//         {
//             ColorUtility.TryParseHtmlString(colorCode, out Color color);
//             Gradient res = new Gradient();
//             res.SetKeys(
//                 new[]
//                 {
//                     new GradientColorKey(color, 0.0f)
//                 },
//                 new[]
//                 {
//                     new GradientAlphaKey(1f, 0.0f),
//                     new GradientAlphaKey(0f, 1.0f)
//                 }
//             );
//             
//             return res;
//         }
//
//         /// <summary>
//         /// FadeOutBy should be assigned a smaller value compared to DestroyBy btw
//         /// </summary>
//         /// <param name="fadeOutBy"></param>
//         /// <param name="destroyBy"></param>
//         /// <returns></returns>
//         private IEnumerator FadeOutAndDestroyItself(float fadeOutBy, float destroyBy)
//         {
//             Color originalColor = scoreHolder.color;
//             float elapsedTime = 0;
//             float smaller = fadeOutBy < destroyBy ? fadeOutBy : destroyBy;
//             while (elapsedTime < smaller)
//             {
//                 elapsedTime += Time.deltaTime;
//                 originalColor.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutBy);
//                 scoreHolder.color = originalColor;
//                 yield return null;
//             }
//             
//             yield return new WaitForSeconds(destroyBy - fadeOutBy);
//
//             Destroy(gameObject);
//         }
//
//         private IEnumerator PerformFloating(float duration, float waitTime, Action callback = null)
//         {
//             Vector3 startPos = transform.position;
//             Vector3 endPos = transform.position + new Vector3(0, 10f, 0);
//             float elapsedTime = 0;
//             
//             while (elapsedTime < duration)
//             {
//                 elapsedTime += Time.deltaTime;
//                 transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
//                 yield return null;
//             }
//             transform.position = endPos;
//             yield return new WaitForSeconds(waitTime);
//             
//             callback?.Invoke();
//         }
//     }
//}