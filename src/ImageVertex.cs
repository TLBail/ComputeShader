using System.Numerics;

namespace ComputeShader;

public struct ImageVertex
{
    public Vector3 position;
    public Vector2 texCoords;

    public ImageVertex(Vector3 position, Vector2 texCoords) {
        this.position = position;
        this.texCoords = texCoords;
    }
}