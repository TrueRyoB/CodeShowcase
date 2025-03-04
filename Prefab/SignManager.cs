using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

namespace Fujin.Prefab
{
    public class SignManager : MonoBehaviour
    {
        [SerializeField][Tooltip("stage # followed by the order ig.) 1_1")]private string id;
        [SerializeField][Tooltip("Optional Text displayed as an UI")] private string caption;
        [SerializeField][Tooltip("Optional Text displayed only if a player is nearby")] private string nearCaption;
        [SerializeField][Tooltip("Other optional texts possibly triggered by external collisions")] private List<string> altTexts;
        [SerializeField][Tooltip("Other optional ids possibly...")]private List<string> altIDs;
        private TMP_Text myTMPText;
        private readonly int chaLimit = 6;
        private bool isNearby;
        private string oldCap;

        public string ID
        {
            get => id;
            set => id = value;
        }
        public bool IsNearby
        {
            set
            {
                isNearby = value;
                if(!string.IsNullOrEmpty(nearCaption)) {
                    ModifyCaption2(id, value?nearCaption:caption);
                }
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            if (string.IsNullOrEmpty(id)) {
                Debug.LogError("One of signs isn't registered its id");
                return;
            }
            ModifyCaption2(id, caption);
        }

        private string Modified(string input)
        {
           string result = ""; 
            for(int i = 0; i < input.Length; ++i) {
                if(i != 0 && i % chaLimit == 0)
                    result += Environment.NewLine;
                result += input[i];
            }
            return result;
        }

        public void ModifyCaption2(string newid, string newcap)
        {
            if((newcap == caption || newcap == nearCaption || altTexts.Contains(newcap) || newcap == "" || newcap == "same") && (newid == id || altIDs.Contains(newid)|| newid == "same")) {
                if(newid != "same")
                    id = newid;
                if(newcap != "same")
                    transform.GetChild(0).GetComponent<TMP_Text>().text = Modified(newcap);
            } else
                Debug.LogError("Either the passed caption or the text is not pre-registered to the list");
        }
    }
}