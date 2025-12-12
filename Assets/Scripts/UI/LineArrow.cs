using UnityEngine;

public class LineArrow : MonoBehaviour
{
    public LineRenderer line;
    public Transform arrowHead;
    public float arrowSize = 0.2f;

    void LateUpdate()
    {
        if (line.positionCount < 2) return;
        Vector3 end = line.GetPosition(line.positionCount - 1);
        Vector3 prev = line.GetPosition(line.positionCount - 2);
        arrowHead.position = end;
        Vector3 dir = (end - prev).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrowHead.rotation = Quaternion.Euler(0, 0, angle - 90f);
        arrowHead.localScale = Vector3.one * arrowSize;
    }
}
