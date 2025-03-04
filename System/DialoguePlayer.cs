using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

using Fujin.Player;
using Fujin.Data;

namespace Fujin.System
{
    public class DialoguePlayer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textLabel;
        [SerializeField] private PlayerPhysics player;
        [SerializeField] private GameObject textbox;
        [SerializeField] private Image face;
        [SerializeField] private GameObject eventSystem;

        [SerializeField] private Sprite tonsuke;
        [SerializeField] private Sprite shaddie;
        [SerializeField] private Sprite kaguoCalm;
        [SerializeField] private Sprite kaguoSad;

        [SerializeField]private List<LogSet> logSetList = new List<LogSet>();
        
        [SerializeField]private CanvasGroup canvasGroup;

        public bool IsTalking { get; private set; }
        public bool IsResult { get; private set; }
        private DialogueExecutor dialogueExecutor;

        void Start()
        {
            if (player == null)
                Debug.LogError("Player is not set to the tonsuke asset!");
            if (logSetList.Count() < 1)
                Debug.LogWarning("No element is set to the list logSetList!");
        }

        public void ShowUI()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void HideUI()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Narrate(string signID)
        {
            player.Stun();
            IsTalking = true;
            LogSet logSet = logSetList.FirstOrDefault(_ => _.ID == signID);
            if (logSet == null)
            {
                Debug.LogError("No LogSet matching to the given ID " + signID + " was found!");
                return;
            }

            bool isFirstTime = logSet.IsReadFirstTime;
            logSet.MarkAsRead();

            StartCoroutine(PlayLogSequentially(logSet.logInfos, isFirstTime, signID));
        }

        private IEnumerator PlayLogSequentially(List<LogInfo> logInfos, bool isFirstTime, string signID)
        {
            int i = 0;
            foreach (LogInfo logInfo in logInfos)
            {
                bool shouldDisplay = logInfo.DisplayConditionLog switch
                {
                    LogInfo.DisplayCondition.Always => true,
                    LogInfo.DisplayCondition.OnlyFirstTime => isFirstTime,
                    LogInfo.DisplayCondition.FromSecondTime => !isFirstTime,
                    _ => false
                };

                if (shouldDisplay)
                {
                    SetIcon(logInfo.IconTypeLog);
                    dialogueExecutor ??= GetComponent<DialogueExecutor>();
                    dialogueExecutor.ExecuteInNeed(signID + "_" + i.ToString());
                    yield return StartCoroutine(PlayText(logInfo.Text));
                }

                ++i;
            }

            IsTalking = false;
            player.Cleanse();
            this.gameObject.SetActive(false);
        }

        private IEnumerator PlayText(string s)
        {
            textLabel.text = "";
            int r = 0, c = 0;
            int n = s.Length;
            Adjust4Ss();
            bool isWaiting4Enter = true;

            while (r < n)
            {
                if (s[r] == '$' && r < n - 1 && s[r + 1] == 'e')
                {
                    //wait for enter key
                    r += 2;
                    while (!Input.GetKeyDown(KeyCode.Return))
                        yield return null;
                }
                else if (s[r] == '$' && r < n - 2 && s[r + 1] == 't' && IsRestrictivelyDigit(s[r + 2]))
                {
                    //wait for extra seconds
                    int cd = s[r + 2] - '0';
                    r += 3;
                    while (r < n && IsRestrictivelyDigit(s[r]))
                    {
                        cd = cd * 10 + (s[r] - '0');
                        ++r;
                    }

                    yield return new WaitForSeconds(0.1f * cd);
                }
                else if (s[r] == '$' && r < n - 1 && s[r + 1] == 's')
                {
                    //skip waiting for enter key
                    r += 2;
                    isWaiting4Enter = false;
                }
                else if (s[r] == '$' && r < n - 1 && s[r + 1] == 'c')
                {
                    //cleanse him
                    r += 2; 
                    player.Cleanse();
                }
                else if (s[r] == '$' && r < n - 1 && s[r + 1] == 'g')
                {
                    //goal (dont SetActive(false) himself)
                    r += 2;
                    IsResult = true;
                    while (IsResult)
                        yield return new WaitForSeconds(10f);
                }
                else
                {
                    //display text
                    textLabel.text += s[r];
                    ++c;
                    ++r;
                    if (c > 18) Adjust4Ls();
                    yield return new WaitForSeconds(0.1f);
                }

                yield return null;
            }

            while (isWaiting4Enter && !Input.GetKeyDown(KeyCode.Return))
                yield return null;
        }


        private bool IsRestrictivelyDigit(char c)
        {
            return c is >= '0' and <= '9';
        }

        private void Adjust4Ls()
        {
            textbox.GetComponent<RectTransform>().localPosition = new Vector3(-60, 260, 0);
        }

        private void Adjust4Ss()
        {
            textbox.GetComponent<RectTransform>().localPosition = new Vector3(-60, 220, 0);
        }

        private void SetIcon(IconType iconType)
        {
            //I'm not implementing a matrix system as these two don't change their face this entire time

            switch (iconType.IconLog)
            {
                case IconType.Icon.Tonsuke:
                    face.sprite = tonsuke;
                    break;
                case IconType.Icon.Shadie:
                    face.sprite = shaddie;
                    break;

                case IconType.Icon.Kaguo:
                    switch (iconType.EmoLog) {
                        case IconType.Emo.Calm:
                            face.sprite = kaguoCalm;
                            break;
                        case IconType.Emo.Sad:
                            face.sprite = kaguoSad;
                            break;
                    }

                    break;
            }

        }
    }
}