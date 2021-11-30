using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimFlask : MonoBehaviour
{
    public float selectedPositionHeight = .5f;
    private float rotationAngle = -65f, defaultAngle = 0;
    private Flask flask;
    float spillTime = 5f;
    private Vector3 originalPos;
    private Flask targetFlask;
    private Vector3 targetPosition;
    private int count, index, indexContent, countContent, maxCount;
    private bool move = false, rotateTo = false, rotateBack = false, moveBack = false;
    float startTime;

    public void MoveSelected()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + selectedPositionHeight);
    }

    public void MoveUnselected()
    {
        gameObject.transform.position = originalPos;
    }

    public void SpillAnimation(Flask targetFlask, int count, int index, int indexContent)
    {
        this.targetFlask = targetFlask;
        this.targetPosition = new Vector3(targetFlask.gameObject.transform.position.x - 2.2f, targetFlask.gameObject.transform.position.y + 2.5f);
        startTime = Time.time;
        this.count = count;
        this.index = index;
        this.indexContent = indexContent;
        this.countContent = 0;
        this.maxCount = count;
        move = true;
    }

    void UpdateContents(Flask targetFlask)
    {
        // Animate all contents in flasks
        flask.GetContentFlask().ForEach(content =>
        {
            content.UpdateContent(transform.localRotation.eulerAngles.z);
        });
        targetFlask.GetContentFlask().ForEach(content =>
        {
            content.UpdateContent(targetFlask.transform.localRotation.eulerAngles.z);
        });
    }

    void AnimateFillSpillContent(Flask targetFlask, float time)
    {
        ContentFlask contentFlask = flask.GetContentFlask()[index];
        // Spill Top
        if (contentFlask.height >= 0.01f)
        {
            contentFlask.SetHeight((1 - time) * flask.contentHeight);
        }
        else // Spill next content
        {
            startTime = Time.time;
            flask.RemoveContentFlask(contentFlask);
            index--;
            count--;
        }

        // If empty, create content
        Container container = targetFlask.GetComponentInChildren<Container>();
        ContentFlask targetContentFlask = container.GetContentAt(indexContent + countContent);
        if (targetContentFlask == null)
        {
            targetContentFlask = targetFlask.CreateContentFlask(.1f, 0, contentFlask.GetMaterial(), targetFlask.nbPoints);
            targetFlask.GetContentFlask().Add(targetContentFlask);
        }
        // Fill Top
        if (targetContentFlask.height <= .99f)
        {
            targetContentFlask.SetHeight(time * flask.contentHeight);
        }
        // Fill Next content
        else if (countContent < maxCount)
        {
            countContent++;
        }
    }

    private void rotate(float angle, float time)
    {
        // Rotate flask
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), (Time.time - startTime) / spillTime);
        // Rotate Content relative to up position
        if (count > 0)
        {
            AnimateFillSpillContent(targetFlask, time);
        }
        UpdateContents(targetFlask);
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

    void Update()
    {
        // Move to target, rotate left, rotate back, move to original position
        if (move)
        {
            transform.position = Vector3.Lerp(originalPos, targetPosition, Time.time - startTime);

            if (transform.position == targetPosition)
            {
                startTime = Time.time;
                rotateTo = true;
                move = false;
            }
        }
        if (rotateTo)
        {
            rotate(rotationAngle, Time.time - startTime);
            if (WrapAngle(transform.localRotation.eulerAngles.z) == rotationAngle && countContent == maxCount && count == 0)
            {
                startTime = Time.time;
                rotateTo = false;
                rotateBack = true;
            }
        }
        // Wait for content to finish spilling / filling
        if (rotateBack)
        {
            rotate(defaultAngle, Time.time - startTime);
            if (WrapAngle(transform.localRotation.eulerAngles.z) <= defaultAngle + .01f && WrapAngle(transform.localRotation.eulerAngles.z) >= defaultAngle - .01f)
            {
                startTime = Time.time;
                rotateBack = false;
                moveBack = true;
            }
        }
        if (moveBack)
        {
            transform.position = Vector3.Lerp(targetPosition, originalPos, Time.time - startTime);
            if (transform.position == originalPos)
            {
                startTime = Time.time;
                moveBack = false;
            }
        }
    }
}
