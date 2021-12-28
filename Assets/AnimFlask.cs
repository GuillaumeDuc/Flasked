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
    private bool move = false, rotateTo = false, rotateBack = false, moveBack = false, spill = false, fill = false;
    private float startHeight;
    float startTime;
    float spillTime = 3f;
    float speed = 4f;

    public bool IsMoving()
    {
        return move || moveBack || rotateTo || rotateBack;
    }

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

        startTime = Time.time;
        // Start animation
        move = true;
    }

    void UpdateContents()
    {
        // Animate all contents in flasks
        GetComponentInChildren<Container>().UpdateContents(transform.localRotation.eulerAngles.z, Time.time - startTime);
    }

    void AnimateSpillContent(float time)
    {
        ContentFlask contentFlask = flask.GetComponentInChildren<Container>().GetTopContent();
        // Spill Top
        if (contentFlask != null && contentFlask.currentHeight >= 0.01f && spill)
        {
            contentFlask.SetCurrentHeight((1 - time) * contentFlask.height);
        }
        ////////////// END SPILLING //////////////////
        else if (contentFlask != null && spill)
        {
            spill = false;
            Destroy(GetComponentInChildren<Container>().GetTopContent().gameObject);
        }
    }

    public void FillContent(Flask targetFlask)
    {
        ContentFlask topContent = GetComponentInChildren<Container>().GetTopContent();
        // Flask filled starting height
        startHeight = 0;
        // Flask filled new height
        if (topContent != null)
        {
            startHeight = topContent.currentHeight;
            topContent.fill = true;
            topContent.height += targetFlask.GetComponentInChildren<Container>().GetTopContent().height;
            topContent.nbPoints += targetFlask.GetComponentInChildren<Container>().GetTopContent().nbPoints;
        }
        this.targetFlask = targetFlask;
        if (!fill)
        {
            startTime = Time.time;
            fill = true;
        }
    }

    private void AnimateFillContent(float time)
    {
        // Content flask
        Container container = GetComponentInChildren<Container>();
        ContentFlask contentFlask = container.GetTopContent();
        // Spilling flask
        Container spillingContainer = targetFlask.GetComponentInChildren<Container>();
        ContentFlask spillingContentFlask = spillingContainer.GetTopContent();
        // Create content if empty
        if (contentFlask == null)
        {
            contentFlask = container.AddContentFlask(.1f, spillingContentFlask.height, spillingContentFlask.GetColor(), spillingContentFlask.GetMaterial(), spillingContentFlask.nbPoints);
            contentFlask.currentHeight = 0;
            contentFlask.fill = true;
        }
        contentFlask.SetCurrentHeight(startHeight + (time * (contentFlask.height - startHeight)));
        // End filling
        if (startHeight + (time * (contentFlask.height - startHeight)) >= contentFlask.height)
        {
            fill = false;
            contentFlask.fill = false;
            // Set cleared mat if flask is cleared
            if (flask.IsCleared())
            {
                flask.SetClearedMaterial(10);
            }
        }
        UpdateContents();
    }

    private void rotate(float angle, float ratio)
    {
        // Rotate flask
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), ratio);
        // Rotate Content relative to up position
        UpdateContents();
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
                // Start filling target
                targetFlask.GetComponent<AnimFlask>().FillContent(this.flask);
                startTime = Time.time;
                rotateTo = true;
                move = false;
                spill = true;
            }
        }
        if (rotateTo) // Rotate to Flask
        {
            rotate(rotationAngle, (Time.time - startTime) / spillTime);
            AnimateSpillContent(Time.time - startTime);
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

        if (fill)
        {
            AnimateFillContent(Time.time - startTime);
        }
    }
}
