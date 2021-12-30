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
    public GameObject flaskPrefab;
    public Button RefreshButton;
    public Button RetryButton;
    public Button UndoButton;
    public GameObject EndPanel;
    public Text textCount;
    public Text retryCount;
    private List<Flask> flasks = new List<Flask>();
    private Flask selectedFlask;
    void Start()
    {
        Init();
        SetInfo();
        InitButtons();
    }

    void InitButtons()
    {
        InitRefreshButton();
        InitRetryButton();
    }

    void InitRefreshButton()
    {
        RefreshButton.onClick.AddListener(RefreshScene);
    }

    void InitRetryButton()
    {
        RetryButton.onClick.AddListener(RetryScene);
    }

    void SetInfo()
    {
        textCount.text = "" + (Store.level + 1);
        retryCount.text = "" + Store.retryCount;
    }

    void Init()
    {
        bool solved;
        int tentative = 1;
        flasks = FlaskCreator.CreateFlasks(flaskPrefab, Store.level, nbContent, nbEmpty, contentHeight);
        solved = Solver.Solve(flasks);
        while (!solved)
        {
            FlaskCreator.FillFlasksRandom(flasks, flasks.Count, nbContent, nbEmpty, contentHeight);
            solved = Solver.Solve(flasks);
            tentative += 1;
        }

        Store.SaveFlasksBeginLevel(flasks);
        Store.retryCount = nbRetry;

        Debug.Log(tentative);
    }

    bool SpillBottle(Flask giver, Flask receiver)
    {
        bool spilled = (giver != null) ? giver.SpillTo(receiver) : false;
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
        Store.level += 1;
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
            FlaskCreator.RefillFlask(flasks, Store.savedFlasks, contentHeight);
            Store.retryCount -= 1;
            retryCount.text = "" + Store.retryCount;
        }
    }

    void RefreshScene()
    {
        Store.level = 0;
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
