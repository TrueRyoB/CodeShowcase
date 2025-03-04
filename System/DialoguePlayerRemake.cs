using Fujin.Data;
using System.Threading.Tasks;
using System;
using Fujin.Constants;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Fujin.System
{
    /// <summary>
    /// Static class that displays the corresponding prefab UI
    /// (created at 13:48, Feb 2nd)
    ///
    /// Attached to the prefab gameObject DialogueUI
    /// </summary>
    public class DialoguePlayerRemake : MonoBehaviour
    {
        // Singleton for managing an outside call
        private static DialoguePlayerRemake _instance;
        private static bool _isLoadingOrLoaded;
        private static string[,] _dialogueDataMatrix;

        private void Start()
        {
            Debug.Log("Not the best way");
        }

        public static async Task<DialoguePlayerRemake> GetInstanceAsync()
        {
            if (_instance != null) return _instance;

            if (!_isLoadingOrLoaded)
            {
                _isLoadingOrLoaded = true;
                
                // Load the prefab
                var handle = Addressables.LoadAssetAsync<GameObject>(AddressablesPath.DialoguePlayer);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject obj = Instantiate(handle.Result);
                    _instance = obj.GetComponent<DialoguePlayerRemake>();
                    _audioSource = obj.GetComponent<AudioSource>();
                    DontDestroyOnLoad(_instance);
                }
                else
                {
                    Debug.LogError("Failed loading a prefab DialoguePlayer");
                }
                
                _instance.gameObject.SetActive(false);
                
                //Load CSV as well
                if (_dialogueDataMatrix == null)
                {
                    var handleCsv = Addressables.LoadAssetAsync<TextAsset>(AddressablesPath.DialogueDataCSV);
            
                    await handleCsv.Task;

                    if (handleCsv.Status == AsyncOperationStatus.Succeeded)
                    {
                        string dialogueDataCsv = handleCsv.Result.text;
                        
                        // Convert a text asset to a decent string matrix
                        string[] rows = dialogueDataCsv.Split('\n');
                        int rowCount = rows.Length;
                        int colCount = rows[0].Split(',').Length;
                        
                        _dialogueDataMatrix = new string[rowCount, colCount];
                        for (int i = 0; i < rowCount; ++i)
                        {
                            var columns = rows[i].Split(',');
                            for (int j = 0; j < columns.Length; ++j)
                            {
                                _dialogueDataMatrix[i, j] = columns[j];
                            }
                        }
                        // Initialize an accelerator array for a binary search
                        SortDDataMatrix();
                    }
                    else
                    {
                        Debug.LogError("Failed loading a prefab DialogueDataCSV");
                    }
                }
            }
            return _instance;
        }

        private static int[] _indices;

        /// <summary>
        /// Use one-dimensional array of integer storing the index of the matrix row in non-decreasing order;
        /// use a mergesort algorithm
        /// </summary>
        private static void SortDDataMatrix()
        {
            int n = _dialogueDataMatrix.GetLength(0);
            _indices = new int[n];
            for (int i = 0; i < n; ++i)
            {
                _indices[i] = i;
            }
            
            MergeSortIndices(0, n-1);
        }

        private static void MergeSortIndices(int l, int r)
        {
            if (l >= r) return;

            int m = (l + r) / 2;
            
            MergeSortIndices(l, m);
            MergeSortIndices(m + 1, r);
            
            MergeIndices(l, m, r);
        }

        private static void MergeIndices(int l, int m, int r)
        {
            // Copy substrings of both head and tail outside the range
            int n1 = m - l + 1, n2 = r - m;
            int[] left = new int[n1], right = new int[n2];
            
            Array.Copy(_indices, l, left, 0, n1);
            Array.Copy(_indices, m+1, right, 0, n2);

            int i = 0, j = 0, k = l;

            while (i < n1 && j < n2)
            {
                int comp = String.Compare(_dialogueDataMatrix[_indices[l], 0], _dialogueDataMatrix[_indices[r], 0], StringComparison.Ordinal);
                // Prioritize r when l holds the index of a bigger string
                if (comp > 0) _indices[k++] = right[i++];
                else _indices[k++] = left[i++];
            }
            
        }
        
        /// <summary>
        /// Is called by other class to free the dialogue player from the memory
        /// </summary>
        public static void Release()
        {
            if (_instance != null)
            {
                Addressables.ReleaseInstance(_instance.gameObject);
                _instance = null;
                _isLoadingOrLoaded = false;
            }
        }

        /// <summary>
        /// Called by Play() on the first round
        /// </summary>
        private void ShowOnScreen(bool status = true)
        {
            // Switch the BGM and the input action
            AudioManager.Instance.PauseEveryBGMOver(0.3f);
            
            // Display/UnDisplay the gameObject on screen
            gameObject.SetActive(status);

            if (status)
            {
                // Switch the input action
                // TODO: Load the target BGM group (only if possible)
                //
            }
            else
            {
                // Release everything
                Addressables.Release(_dialogueDataMatrix);
                Addressables.Release(_indices);
                Addressables.Release(_instance);
            }

            // TODO: Load the target BGM group (only if possible)
            //
        }

        private bool isPlaying;

        /// <summary>
        /// Is called by an outside class to start dialogues.
        /// </summary>
        /// <param name="id"></param>
        public async Task PlayAsync(string id)
        {
            if (isPlaying) return;
            
            // Turn the flag on to prevent a duplicate calling
            isPlaying = true;
            
            // Pass a concatenated id
            switch (PreferenceDataManager.Instance.Language)
            {
                case Language.English:
                    id = String.Join(id, "_en");
                    break;
                case Language.Japanese:
                    id = String.Join(id, "_jp");
                    break;
            }
            
            DialogueData current = new DialogueData();
            DialogueData next = new DialogueData();
            
            // Load the very first dialogue
            if (!await InitializeDData(id, current))
            {
                Debug.LogError($"Error: failed to initialize DialogueData of {id}");
                return;
            }

            // Load the prefab UIs and call the coroutine after a solid preparation
            ShowOnScreen();
            StartCoroutine(PlayCoroutine(current, next));
        }

        /// <summary>
        /// Helper method for InitializeDData(string id, DialogueData dData)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int GetIndexOfRowWithID(string id)
        {
            int l = 0, r = _indices.GetLength(0) - 1;

            while (l <= r)
            {
                int m = (l + r) / 2, comp = String.Compare(_dialogueDataMatrix[_indices[m], 0], id, StringComparison.Ordinal);

                if (comp == 0) return m;
                if (comp < 0) l = m + 1;
                else r = m - 1;
            }
            
            Debug.LogError($"Error: id \"{id}\" is not found in _dialogueDataMatrix! ;-;");

            return -1;
        }

        private async Task<bool> InitializeDData(string id, DialogueData dData)
        {
            if (_dialogueDataMatrix == null)
            {
                Debug.LogError("Error: dialogueDataCSV is null!");
                return false;
            }
            
            int i = GetIndexOfRowWithID(id), n=_dialogueDataMatrix.GetLength(1);
            if (i == -1) return false;
            
            string[] row = new string[n];
            for(int j=0; j<n; ++j)  row[j] = _dialogueDataMatrix[i, j];

            await dData.InitializeWith(row);
            return true;
        }
        
        // Components consisting of a dialogue prefab
        private Image pictureHolder;
        private Image background; //TODO: gradationの色指定をどうするのか調べる
        private TextMeshProUGUI textHolder;
        private static AudioSource _audioSource;

        private Coroutine textParseCoroutine;
        
        /// <summary>
        /// Assume that
        /// 1. DData "current" has loaded essential elements using Addressables already within the past calling
        /// 2. DData "next" is yet to be initialized
        /// </summary>
        /// <param name="current"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private IEnumerator PlayCoroutine(DialogueData current, DialogueData next)
        {
            // Load Image and Background color gradation
            SetImage(current.Image);
            SetGradation(current.gradationKey);
            
            // Change BGM by ceasing every pre-existing one only if the key differs
            if (current.HasBGM && AudioManager.Instance.IsSameSoundPlayed(current.bgmKey, _audioSource))
            {
                AudioManager.Instance.Play(_audioSource, current.bgmKey, true);
            }

            //本来であればenumを丁寧に作るべきだけど、今の発想では押して次に進むか選択肢を選んで次に進むかの二択なのでboolで妥協させてもらった
            bool proceedOnConfirmation = true;
            bool isLastDialogue = false;
            
            // Start parsing the text sequentially; wait for loading the next DData before allowing a skip
            textParseCoroutine = StartCoroutine(ParseTextSequentially(current.text, res => proceedOnConfirmation=res));

            if (!string.IsNullOrEmpty(current.nextId))
            {
                yield return InitializeDData(current.nextId, next);
            }
            else
            {
                isLastDialogue = true;
            }
            
            // Wait for a text parse to be done
            while(textParseCoroutine != null)   yield return null;
            
            // Allow loading a next dialogue upon a solid preparation (displaying a triangle UI, choices, etc.)
            EnableNextDialogue(proceedOnConfirmation);

            //TODO: 本当だったら入力をまった後でようやく実行なので、InputActionを作り直す
            //　さらに言えば、ActionKeyを読み込むものすらできていない　なのでHasAction?に甘えるしかない
            current.NextAction?.Invoke();

            //TODO: もし選択肢を迫ったのなら、initializeDDataをもう一回通す
            // 選択肢の結果を回収する方法を実装する
            //public List<string> altIds;
            //yield return InitializeDData(customID, next);
            
            // Release addressables of current, pass next to current, empty current, and move on
            current.Reset();
            if (!isLastDialogue) //TODO:ここをなんかして変える
            {
                StartCoroutine(PlayCoroutine(next, current));
            }
            else
            {
                yield return new WaitForSeconds(3f);
                ShowOnScreen(false);
                // TODO: Send itself to the main menu scene if not specified in the action
            }
        }

        private void EnableNextDialogue(bool proceedWithConfirmation)
        {
            // Display an upside-down triangle, etc
            //TODO: implement something here
        }

        private void SetGradation(string gradationKey)
        {
            //TODO: implement something here
        }

        private void SetImage(Sprite image, bool animateTransition = false)
        {
            //pictureHolder
            //TODO: implement something here
        }

        private IEnumerator ParseTextSequentially(string text, Action<bool> proceedOnConfirmation)
        {
            //TODO: implement a logic here
            
            /*
             * Create a set of XML tags for texts and parse them before applying it to the text box
             *
             * XML tags candidate
             * Narration Speed
             * Emphasize (red color, dot highlight?)
             * Shaking TODO: know how to make it work like a comic
             * Delete previous keys
             * Await for player confirmation
             * Character name (replacing the specific term with a variable value)
             * Delay
             * Flags for switching
             * Make players decide their options
             * Emotion (neutral, sad, angry, happy)
             */
            
            //定められた朗読速度 (c/s)にあたって、parseは特定の文字量に追いつくまでparseしたらreturnをするように設計をする
            //文字の状態を変えうるXMLのbool値を先頭で全て格納して、もしtrueであれば反映するように工夫する
            
            textParseCoroutine = null;
            proceedOnConfirmation.Invoke(true); //ここ変更するようにする
            yield break;
        }
    }
}