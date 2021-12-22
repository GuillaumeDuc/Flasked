using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimFlask : MonoBehaviour
{
    public float selectedPositionHeight = .5f;
    private float rotationAngle = -60f, defaultAngle = 0;
    private Flask flask;
    private Vector3 originalPos;
    private Flask targetFlask;
    private Vector3 targetPosition;
    private bool move = false, rotateTo = false, rotateBack = false, moveBack = false, spill = false;
    private float startHeight;
    float startTime;
    float spillTime = 3f;
    float speed = 4f;

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
        this.targetPosition = new Vector3(targetFlask.gameObject.transform.position.x - 2f, targetFlask.gameObject.transform.position.y + 3f);

        ContentFlask topContent = targetFlask.GetComponentInChildren<Container>().GetTopContent();
        // Flask filled starting height
        startHeight = 0;
        // Flask filled new height
        if (topContent != null)
        {
            startHeight = topContent.height;
            topContent.fill = true;
            topContent.height += flask.GetComponentInChildren<Container>().GetTopContent().height;
            topContent.nbPoints += flask.GetComponentInChildren<Container>().GetTopContent().nbPoints;
        }

        startTime = Time.time;
        // Start animation
        move = true;

    }

    void UpdateContents(Flask targetFlask)
    {
        // Animate all contents in flasks
        GetComponentInChildren<Container>().UpdateContents(transform.localRotation.eulerAngles.z, Time.time - startTime);
        targetFlask.GetComponentInChildren<Container>().UpdateContents(targetFlask.transform.localRotation.eulerAngles.z, Time.time - startTime);
    }

    void AnimateFillSpillContent(Flask targetFlask, float time)
    {
        ContentFlask contentFlask = flask.GetComponentInChildren<Container>().GetTopContent();

        // Target flask
        Container container = targetFlask.GetComponentInChildren<Container>();
        ContentFlask targetContentFlask = container.GetTopContent();

        // Spill Top
        if (contentFlask != null && contentFlask.currentHeight >= 0.01f && spill)
        {
            contentFlask.SetCurrentHeight((1 - time) * contentFlask.height);
        }
        ////////////// END SPILLING //////////////////
        else if (contentFlask != null && spill)
        {
            spill = false;
            targetContentFlask.fill = false;
            Destroy(GetComponentInChildren<Container>().GetTopContent().gameObject);
            // Set cleared mat if flask is cleared
            if (targetFlask.IsCleared())
            {
                targetFlask.SetClearedMaterial(10);
            }
        }

        // Create content if empty
        if (targetContentFlask == null && spill)
        {
            targetContentFlask = container.AddContentFlask(.1f, contentFlask.height, contentFlask.GetColor(), contentFlask.GetMaterial(), contentFlask.nbPoints);
            targetContentFlask.currentHeight = 0;
            targetContentFlask.fill = true;
            UpdateContents(targetFlask);
        }
        // Fill Top
        if (spill)
        {
            targetContentFlask.SetCurrentHeight(startHeight + (time * (targetContentFlask.height - startHeight)));
        }
    }

    private void rotate(float angle, float ratio)
    {
        // Rotate flask
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), ratio);
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

    void CreateSpillShape(float angle)
    {
        Container container = flask.GetComponentInChildren<Container>();
        // .002 : edge radius
        SpillShape spillShape = GetComponentInChildren<SpillShape>();
        // Pre-create shape
        if (spillShape == null)
        {
            Bounds boxBounds = container.GetComponent<EdgeCollider2D>().bounds;
            Vector2 topRight = container.transform.TransformPoint(GetMaxRightHeight(container.GetComponent<EdgeCollider2D>().points));

            GameObject meshObj = new GameObject("Spill");
            meshObj.transform.parent = gameObject.transform;
            spillShape = meshObj.AddComponent<SpillShape>();
            spillShape.Init(container.GetTopContent().GetMaterial(), topRight, flask.nbPoints);
        }
        else
        {
            int layerMask = 1 << 6;
            // Try to hit bottom flask content
            Vector2 pos = spillShape.gameObject.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, 100, layerMask);
            if (hit.collider != null)
            {
                spillShape.UpdateShape(hit.point, angle);
            }
        }
    }

    public Vector2 GetMaxRightHeight(Vector2[] points)
    {
        Vector2 maxHeight = points[0];
        for (int i = 0; i < points.Length; i++)
        {
            maxHeight = points[i].y > maxHeight.y && points[i].x >= maxHeight.x ? points[i] : maxHeight;
        }
        return maxHeight;
    }

    void DestroySpillShape()
    {
        GetComponentInChildren<SpillShape>().DestroySpillShape();
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
            transform.position = Vector3.Lerp(originalPos, targetPosition, (Time.time - startTime) * speed);

            if (transform.position == targetPosition)
            {
                startTime = Time.time;
                rotateTo = true;
                move = false;
                spill = true;
            }
        }
        if (rotateTo) // Rotate to Flask
        {
            rotate(rotationAngle, (Time.time - startTime) / spillTime);
            AnimateFillSpillContent(targetFlask, Time.time - startTime);
            CreateSpillShape(rotationAngle);
            // End rotation to spill, start rotating back
            if (!spill)
            {
                DestroySpillShape();
                startTime = Time.time;
                rotateTo = false;
                rotateBack = true;
            }
        }
        // Wait for content to finish spilling / filling
        if (rotateBack)
        {
            rotate(defaultAngle, (Time.time - startTime) / (spillTime / 2));
            float round = Mathf.Round(WrapAngle(transform.localRotation.eulerAngles.z) * 10f) / 10f;
            // End rotating back
            if (round == defaultAngle)
            {
                startTime = Time.time;
                rotateBack = false;
                moveBack = true;
            }
        }
        if (moveBack)
        {
            transform.position = Vector3.Lerp(targetPosition, originalPos, (Time.time - startTime) * speed);
            if (transform.position == originalPos)
            {
                startTime = Time.time;
                moveBack = false;
            }
        }
    }
}
