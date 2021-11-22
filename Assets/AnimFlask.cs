using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimFlask : MonoBehaviour
{
    public float selectedPositionHeight = .5f;
    public float wait = 1f;
    private Flask flask;
    private Vector3 originalPos;

    public void MoveSelected()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + selectedPositionHeight);
    }

    public void MoveUnselected()
    {
        gameObject.transform.position = originalPos;
    }

    public void SpillAnimation(Flask flask)
    {
        // Target flask
        Vector3 target = new Vector3(flask.gameObject.transform.position.x - 1.5f, flask.gameObject.transform.position.y + 1);
        StartCoroutine(MoveToAndReturn(transform.position, target));
    }

    IEnumerator WaitAndMove(float wait, Vector3 from, Vector3 to)
    {
        float startTime = Time.time;
        yield return wait;
        while (Time.time - startTime <= 1)
        {
            transform.position = Vector3.Lerp(from, to, Time.time - startTime);
            yield return 1;
        }
    }

    IEnumerator MoveToAndReturn(Vector3 from, Vector3 to)
    {
        float startTime = Time.time; // Time.time contains current frame time, so remember starting point
        while (Time.time - startTime <= 1)
        { // until one second passed
            transform.position = Vector3.Lerp(from, to, Time.time - startTime); // lerp from A to B in one second
            yield return 1; // wait for next frame
        }
        // Return to original position
        StartCoroutine(WaitAndMove(wait, to, originalPos));
    }

    IEnumerator RotateToSpill(float angle)
    {
        float startTime = Time.time;
        while (Time.time - startTime <= 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), (Time.time - startTime) / 20f);
            yield return 1;
        }
    }

    void Start()
    {
        flask = gameObject.GetComponent<Flask>();
        originalPos = transform.position;
    }
}
