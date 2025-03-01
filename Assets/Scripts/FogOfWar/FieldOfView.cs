using System;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float fov;
    [SerializeField] private int rayCount;
    [SerializeField] private float angle;
    [SerializeField] private float viewDistance;
    [SerializeField] private LayerMask layerMask;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uv;
    private Vector3 origin;
    private int[] triangles;
    private int vertexIndex;
    private int triangleIndex;
    private float startingAngle;
    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
    }

    private void LateUpdate()
    {
        CastFieldOfView();
    }

    private void CastFieldOfView()
    {
        angle = startingAngle;
        float angleIncrease = fov / rayCount;
        //viewDistance = 50f;
        vertexIndex = 1;
        triangleIndex = 0;
        vertices = new Vector3[rayCount + 1 + 1];
        uv = new Vector2[vertices.Length];
        triangles = new int[rayCount * 3];

        vertices[0] = origin;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex = origin + GetVectorFromAngle(angle) * viewDistance;
            RaycastHit2D rayCastHit2D =  Physics2D.Raycast(origin, GetVectorFromAngle(angle), viewDistance, layerMask);

            if (rayCastHit2D.collider == null)
            {
                //No hit
                vertex = origin + GetVectorFromAngle(angle) * viewDistance;
            }
            else
            {
                //Hit Object
                vertex = rayCastHit2D.point;
            }
                        
            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                
                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }
        
        /*vertices[1] = new Vector3(viewDistance, 0);
        vertices[2] = new Vector3(0, -viewDistance);*/

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private Vector3 GetVectorFromAngle(float angle)
    {
        //angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    private float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0)
        {
            n += 360;
        }
        return n;
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        startingAngle = GetAngleFromVectorFloat(aimDirection) + (fov / 2f);
    }
}
