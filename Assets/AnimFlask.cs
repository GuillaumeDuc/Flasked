using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimFlask : MonoBehaviour
{
    public float selectedPositionHeight = .5f;
    public float maxAngle = 80f, defaultAngle = 0;
    private Flask flask;
    float spillTime = 5f;
    private Vector3 originalPos;
    private Flask targetFlask;
    private Vector3 targetPosition;
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

    public void SpillAnimation(Flask targetFlask)
    {
        this.targetFlask = targetFlask;
        this.targetPosition = new Vector3(targetFlask.gameObject.transform.position.x - 2.2f, targetFlask.gameObject.transform.position.y + 2.5f);
        startTime = Time.time;
        move = true;
    }

    void AnimateContentFlask(float eulerAngle)
    {
        // Animate all contents in flask
        flask.GetContentFlask().ForEach(content =>
        {
            content.UpdateContent();
        });
    }

    void AnimateFillSpillContent(Flask targetFlask, float time)
    {
    }

    private void rotate(float angle)
    {
        // Rotate flask
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), (Time.time - startTime) / spillTime);
        // Rotate Content relative to up position
        AnimateContentFlask(transform.localRotation.eulerAngles.z);
        AnimateFillSpillContent(targetFlask, Time.deltaTime);
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
            rotate(-80);
            if (WrapAngle(transform.localRotation.eulerAngles.z) == -80)
            {
                startTime = Time.time;
                rotateTo = false;
                rotateBack = true;
            }
        }
        if (rotateBack)
        {
            rotate(0);
            if (WrapAngle(transform.localRotation.eulerAngles.z) <= .01f && WrapAngle(transform.localRotation.eulerAngles.z) >= -.01f)
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
