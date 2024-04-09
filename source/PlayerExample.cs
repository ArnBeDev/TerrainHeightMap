using UnityEngine;

public class PlayerExample : MonoBehaviour
{
    // A simple class to demonstrate the TerrainHeightMap
    // The Gameobject will be moved to the targetPosition  using the TerrainHeightMap

    TerrainHeightMap terrain;

    public Vector3 targetPosition;

    void Start()
    {
        terrain = FindObjectOfType<TerrainHeightMap>();

        //gets the correct height of the targetPosition on the terrain
        targetPosition = terrain.GetPosition(targetPosition);

        //gets the correct height of this gameobject on the terrain
        this.transform.position = terrain.GetPosition(this.transform.position);
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector3 nextPosition = Vector3.MoveTowards(
            this.transform.position,
            targetPosition,
            Time.deltaTime * 5
        );

        // Note that we have to turn the sign of the x-coordinate
        float height = terrain.GetHeightOfPosition(
            new Vector3(nextPosition.x * (-1f), 0, nextPosition.z)
        );

        // Move only if the nextPosition has a stored height
        if (height > 0)
        {
            this.transform.position = new Vector3(nextPosition.x, height, nextPosition.z);
        }
    }
}
