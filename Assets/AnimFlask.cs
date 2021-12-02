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
    private bool move = false, rotateTo = false, rotateBack = false, moveBack = false, spill = false;
    private float startHeight;
    float startTime;

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
        // Init variables
        this.targetFlask = targetFlask;
        this.targetPosition = new Vector3(targetFlask.gameObject.transform.position.x - 2.2f, targetFlask.gameObject.transform.position.y + 2.5f);

        ContentFlask topContent = targetFlask.GetComponentInChildren<Container>().GetTopContent();
        // Flask filled starting height
        startHeight = 0;
        // Flask filled new height
        if (topContent != null)
        {
            startHeight = topContent.height;
            topContent.height += flask.GetComponentInChildren<Container>().GetTopContent().height;
        }

        startTime = Time.time;
        // Start animation
        move = true;

    }

    void UpdateContents(Flask targetFlask)
    {
        // Animate all contents in flasks
        GetComponentInChildren<Container>().UpdateContents(transform.localRotation.eulerAngles.z);
        targetFlask.GetComponentInChildren<Container>().UpdateContents(targetFlask.transform.localRotation.eulerAngles.z);
    }

    void AnimateFillSpillContent(Flask targetFlask, float time)
    {
        ContentFlask contentFlask = flask.GetComponentInChildren<Container>().GetTopContent();
        // Spill Top
        if (contentFlask != null && contentFlask.currentHeight >= 0.01f && spill)
        {
            contentFlask.SetCurrentHeight((1 - time) * contentFlask.height);
        }
        else if (contentFlask != null && spill) // end
        {
            spill = false;
            Destroy(GetComponentInChildren<Container>().GetTopContent().gameObject);
        }

        // Target flask
        Container container = targetFlask.GetComponentInChildren<Container>();
        ContentFlask targetContentFlask = container.GetTopContent();
        // Create content if empty
        if (targetContentFlask == null && spill)
        {
            targetContentFlask = container.AddContentFlask(.1f, contentFlask.height, contentFlask.GetColor(), contentFlask.GetMaterial(), flask.nbPoints);
            targetContentFlask.currentHeight = 0;
            UpdateContents(targetFlask);
        }
        // Fill Top
        if (spill)
        {
            targetContentFlask.SetCurrentHeight(startHeight + (time * (targetContentFlask.height - startHeight)));
        }
    }

    private void rotate(float angle, float time)
    {
        // Rotate flask
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), (Time.time - startTime) / spillTime);
        // Rotate Content relative to up position
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
                spill = true;
            }
        }
        if (rotateTo)
        {
            rotate(rotationAngle, Time.time - startTime);
            AnimateFillSpillContent(targetFlask, Time.time - startTime);
            if (WrapAngle(transform.localRotation.eulerAngles.z) == rotationAngle && !spill)
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
