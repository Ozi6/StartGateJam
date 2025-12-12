using UnityEngine;

public class Archer : Person
{
    protected override void OnDestroy()
    {

    }

    protected override void Start()
    {

    }

    protected override void Update()
    {
        transform.position += Vector3.back * 0.025f * moveSpeed * Time.deltaTime;
    }
}
