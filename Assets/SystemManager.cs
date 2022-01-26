using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class SystemManager : MonoBehaviour
{
    public int nbContent = 4;
    public float contentHeight = 1;
    public int nbEmpty = 1;
    public int nbRetry = 3;
    public int nbUndo = 5;
    public GameObject flaskPrefab;
    public Button RefreshButton;
    public Button RetryButton;
    public Button UndoButton;
    public GameObject EndPanel;
    public Text textCount;
    public Text retryCount;
    public Text undoCount;
    private List<Flask> flasks = new List<Flask>();
    private Flask selectedFlask;
    private Store Store;
    private List<List<Color>> nextSceneColors = new List<List<Color>>();
    private Thread searchNextSceneThread;

    void Start()
    {
        // Load store from file
        Store = new Store();
        Store.FetchData();

        Init();
        SetInfo();
        InitButtons();
    }

    void InitButtons()
    {
        InitResetButton();
        InitRetryButton();
        InitUndoButton();
    }

    void InitResetButton()
    {
        RefreshButton.onClick.AddListener(ResetScene);
    }

    void InitRetryButton()
    {
        RetryButton.onClick.AddListener(RetryScene);
    }

    void InitUndoButton()
    {
        UndoButton.onClick.AddListener(UndoMove);
    }

    void SetInfo()
    {
        textCount.text = "" + (Store.level + 1);
        retryCount.text = "" + Store.retryCount;
        undoCount.text = "" + Store.undoCount;
    }

    void Init()
    {
        // Create flasks GameObjects
        flasks = FlaskCreator.CreateFlasks(flaskPrefab, FlaskCreator.GetNbFlask(Store.level), Store.nbContent, Store.nbEmptyFlask, contentHeight);

        // Load save
        if (Store.savedScenes.Count > 0)
        {
            // Load top scenes from saved scenes
            FlaskCreator.RefillFlasks(flasks, Store.savedScenes[Store.savedScenes.Count - 1].ToList(), contentHeight);
        }
        // Load previously generated next scene
        else if (nextSceneColors.Count > 0)
        {
            FlaskCreator.RefillFlasks(flasks, nextSceneColors, contentHeight);
            // Init flask data in store
            Store.SaveFlasksBeginLevel(flasks);
            Store.retryCount = nbRetry;
            Store.SaveCurrentScene(flasks);
            Store.undoCount = nbUndo;
            Store.nbContent = nbContent;
            // Save store
            Store.SaveData();
        }
        // Load scene
        else
        {
            Store.nbEmptyFlask = nbEmpty;
            List<List<Color>> listColorFlasks = FlaskCreator.GetSolvedRandomFlasks(flasks.Count, Store.nbContent, ref Store.nbEmptyFlask);
            // Fill flasks
            FlaskCreator.RefillFlasks(flasks, listColorFlasks, contentHeight);
            // Init flask data in store
            Store.SaveFlasksBeginLevel(flasks);
            Store.retryCount = nbRetry;
            Store.SaveCurrentScene(flasks);
            Store.undoCount = nbUndo;
            Store.nbContent = nbContent;
            // Save store
            Store.SaveData();
        }

        // Search next scene
        searchNextSceneThread = new Thread(new ThreadStart(GenerateNextLevel));
        searchNextSceneThread.Start();
    }

    bool SpillBottle(Flask giver, Flask receiver)
    {
        bool spilled = (giver != null) ? giver.SpillTo(receiver) : false;
        // If spilled, save current scene
        if (spilled)
        {
            Store.SaveCurrentScene(flasks);
            Store.SaveData();
        }
        return spilled;
    }

    void End()
    {
        bool end = true;
        flasks.ForEach(flask =>
        {
            if (!flask.IsCleared() && !flask.IsEmpty())
            {
                end = false;
            }
        });
        if (end)
        {
            EndPanel.SetActive(true);
            StartCoroutine(NextLevel());
        }
    }

    IEnumerator NextLevel()
    {
        Store.NextLevel();
        yield return new WaitForSeconds(3.5f);
        // Reload flasks
        ReloadScene();
    }

    void RetryScene()
    {
        bool isMoving = false;
        // Can't retry if a flask is moving
        flasks.ForEach(flask =>
        {
            if (flask.IsMoving())
            {
                isMoving = true;
            }
        });
        if (!isMoving && Store.retryCount > 0)
        {
            FlaskCreator.RefillFlasks(flasks, Store.savedFlasks.ToList(), contentHeight);
            Store.RetryScene();
            Store.undoCount = nbUndo;
            SetInfo();
        }
    }

    void UndoMove()
    {
        bool isMoving = false;
        // Can't undo if a flask is moving
        flasks.ForEach(flask =>
        {
            if (flask.IsMoving())
            {
                isMoving = true;
            }
        });
        if (!isMoving && Store.undoCount > 0 && Store.savedScenes.Count > 1)
        {
            // Get previous scene 
            List<List<Color>> previousScene = Store.savedScenes[Store.savedScenes.Count - 2].ToList();
            // Set previous scene
            FlaskCreator.RefillFlasks(flasks, previousScene, contentHeight);
            // Remove current scene
            Store.savedScenes.RemoveAt(Store.savedScenes.Count - 1);
            Store.undoCount -= 1;
            undoCount.text = "" + Store.undoCount;
            Store.SaveData();
        }
    }

    void ResetScene()
    {
        nextSceneColors = new List<List<Color>>();
        Store.Reset();
        ReloadScene();
    }

    void ReloadScene()
    {
        FlaskCreator.DeleteFlasks(flasks);
        EndPanel.SetActive(false);
        searchNextSceneThread.Join();
        Init();
        SetInfo();
    }

    void GenerateNextLevel()
    {
        Debug.Log("start");
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        // nextSceneColors = FlaskCreator.BenchMark();
        nextSceneColors = FlaskCreator.GetSolvedRandomFlasks(FlaskCreator.GetNbFlask(Store.level + 1), Store.nbContent, ref Store.nbEmptyFlask);
        Thread.Sleep(0);
        stopwatch.Stop();
        System.TimeSpan ts = stopwatch.Elapsed;
        Debug.Log("generated in " + ts.Minutes + "m " + ts.Seconds + "s " + ts.Milliseconds + "ms");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Flask clickedFlask = hit.transform.gameObject.GetComponent<Flask>();
                bool spilled = false;
                // GameObject clicked is flask and is not moving
                if (clickedFlask != null && !clickedFlask.IsMoving())
                {
                    // Flask clicked is not already selected and selected flask is not filling
                    bool selectedIsFilling = selectedFlask == null ? false : selectedFlask.IsFilling();
                    if (!clickedFlask.Equals(selectedFlask) && !selectedIsFilling)
                    {
                        // Clicked flask, try to spill
                        spilled = SpillBottle(selectedFlask, clickedFlask);
                        // If not spilled, select
                        if (!spilled)
                        {
                            clickedFlask.SetSelected();
                        }
                        else
                        {
                            // Spilled, try to end
                            End();
                        }
                    }
                    selectedFlask?.SetUnselected();
                }
                else // GameObject clicked is not flask
                {
                    selectedFlask?.SetUnselected();
                    selectedFlask = null;
                }
                // Change selected flask to new clicked flask
                // If clicked on the same flask two times, unselect
                // If spilled, unselect
                if (spilled || clickedFlask.Equals(selectedFlask))
                {
                    selectedFlask = null;
                }
                else
                {
                    selectedFlask = clickedFlask;
                }
            }
            else
            { // No object selected, unselect
                selectedFlask?.SetUnselected();
                selectedFlask = null;
            }
        }
    }
}
