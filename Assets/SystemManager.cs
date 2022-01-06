using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SystemManager : MonoBehaviour
{
    public int nbContent = 4;
    public int nbEmpty = 2;
    public float contentHeight = 1;
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
    void Start()
    {
        // Load store from file 
        // Store store = new Store();
        // SaveDataManager.LoadJsonData(store);
        Store = new Store();
        Store.FetchData();

        Init();
        SetInfo();
        InitButtons();
    }

    void InitButtons()
    {
        InitRefreshButton();
        InitRetryButton();
        InitUndoButton();
    }

    void InitRefreshButton()
    {
        RefreshButton.onClick.AddListener(RefreshScene);
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
        bool solved;
        int tentative = 1;
        // Create flasks
        flasks = FlaskCreator.CreateFlasks(flaskPrefab, Store.level, nbContent, nbEmpty, contentHeight);
        // Load existing flasks 
        if (Store.savedScenes.Count > 0)
        {
            // Load top scenes from saved scenes
            FlaskCreator.RefillFlask(flasks, Store.savedScenes[Store.savedScenes.Count - 1].ToList(), contentHeight);
        }
        else
        {
            FlaskCreator.FillFlasksRandom(flasks, flasks.Count, nbContent, nbEmpty, contentHeight);
            //Try to solve them
            solved = Solver.Solve(flasks);
            while (!solved)
            {
                FlaskCreator.FillFlasksRandom(flasks, flasks.Count, nbContent, nbEmpty, contentHeight);
                solved = Solver.Solve(flasks);
                tentative += 1;
            }
            // Save flasks in store
            Store.SaveFlasksBeginLevel(flasks);
            Store.retryCount = nbRetry;
            Store.SaveCurrentScene(flasks);
            Store.undoCount = nbUndo;
            Store.SaveData();
        }

        Debug.Log("Tentative " + tentative);
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
            FlaskCreator.RefillFlask(flasks, Store.savedFlasks.ToList(), contentHeight);
            Store.RetryScene();
            retryCount.text = "" + Store.retryCount;
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
            FlaskCreator.RefillFlask(flasks, previousScene, contentHeight);
            // Remove current scene
            Store.savedScenes.RemoveAt(Store.savedScenes.Count - 1);
            Store.undoCount -= 1;
            undoCount.text = "" + Store.undoCount;
            Store.SaveData();
        }
    }

    void RefreshScene()
    {
        Store.Reset();
        ReloadScene();
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(0);
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
                // GameObject clicked is flask
                if (clickedFlask != null && !clickedFlask.IsMoving())
                {
                    // Flask clicked is not already selected and is not moving
                    if (!clickedFlask.Equals(selectedFlask))
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
