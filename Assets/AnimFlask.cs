using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimFlask : MonoBehaviour
{
    public float selectedPositionHeight = .5f;
    private Flask flask;
    private int waitSpillSecond = 2;
    private Vector3 originalPos;

    public void MoveSelected()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + selectedPositionHeight);
    }

    public void MoveUnselected()
    {
        gameObject.transform.position = originalPos;
    }

    public void SpillAnimation(Flask targetFlask)
    {
        // Target flask
        Vector3 target = new Vector3(targetFlask.gameObject.transform.position.x - 2.2f, targetFlask.gameObject.transform.position.y + 2.5f);
        // MoveToFlask, RotateToSpill, RotateToMove and WaitAndMove
        StartCoroutine(MoveToFlask(transform.position, target, targetFlask));
    }

    IEnumerator WaitAndMove(Vector3 from, Vector3 to)
    {
        float startTime = Time.time;
        while (Time.time - startTime <= 1)
        {
            transform.position = Vector3.Lerp(from, to, Time.time - startTime);
            yield return 1;
        }
    }

    IEnumerator MoveToFlask(Vector3 from, Vector3 to, Flask targetFlask)
    {
        float startTime = Time.time; // Time.time contains current frame time, so remember starting point
        while (Time.time - startTime <= 1)
        { // until one second passed
            transform.position = Vector3.Lerp(from, to, Time.time - startTime); // lerp from A to B in one second
            yield return 1; // wait for next frame
        }
        // Rotate
        StartCoroutine(RotateToSpill(-80, targetFlask));
    }

    IEnumerator RotateToSpill(float angle, Flask targetFlask)
    {
        float startTime = Time.time;
        while ((Time.time - startTime) / waitSpillSecond <= 1)
        {
            // Rotate flask
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), (Time.time - startTime) / 20f);
            // Rotate Content relative to up position
            AnimateContentFlask(transform.localRotation.eulerAngles.z);
            AnimateFillSpillContent(targetFlask, Time.time - startTime);
            yield return 1;
        }
        // Rotate to original position
        StartCoroutine(RotateToMove(0, targetFlask));
    }

    IEnumerator RotateToMove(float angle, Flask targetFlask)
    {
        float startTime = Time.time;
        while (Time.time - startTime <= 1)
        {
            // Rotate flask
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), (Time.time - startTime) / 20f);
            // Rotate Content and Spill Content
            AnimateContentFlask(transform.localRotation.eulerAngles.z);
            yield return 1;
        }
        // Move
        StartCoroutine(WaitAndMove(gameObject.transform.position, originalPos));
    }

    void AnimateContentFlask(float eulerAngle)
    {
        List<ContentFlask> contentFlasks = flask.GetContentFlask();
        for (int i = 0; i < contentFlasks.Count; i++)
        {
            // Rotate
            if (!contentFlasks[i].isEmpty())
            {
                contentFlasks[i].RotateContent(-WrapAngle(eulerAngle));
            }
        }
    }

    void AnimateFillSpillContent(Flask targetFlask, float time)
    {
        ContentFlask contentTarget = targetFlask.GetContentToBeFilled();
        ContentFlask contentToSpill = flask.GetContentToSpill();

        if (contentTarget != null && contentTarget.fill)
        {
            contentTarget.Fill(time);
        }
        if (contentToSpill != null && contentToSpill.spill)
        {
            contentToSpill?.Spill(time);
        }
    }

    private static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

    void Start()
    {
        flask = gameObject.GetComponent<Flask>();
        originalPos = transform.position;
    }
}
