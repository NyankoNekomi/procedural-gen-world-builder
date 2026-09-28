using Godot;
using System;

public partial class TerrainSand : Node3D
{
    [Export] public FastNoiseLite Noise { get; set; }
    [Export] public int Resolution { get; set; } = 32;
    [Export] public float ChunkSize { get; set; } = 32.0f;
    
    // Lower MaxHeight keeps the terrain below the shader's grass threshold
    [Export] public float MaxHeight { get; set; } = 3.0f; 
    
    [Export] public ShaderMaterial TerrainShader { get; set; } 
    [Export] public FoliageManager Foliage { get; set; }

    public override void _Ready()
    {
        if (Noise == null)
        {
            // Lower frequency creates smoother, wider rolling dunes
            Noise = new FastNoiseLite { NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex, Seed = 777, Frequency = 0.015f };
        }

        MeshInstance3D meshInstance = new MeshInstance3D { Name = "TerrainMesh" };
        AddChild(meshInstance);

        var chunkMesh = GenerateChunkMesh(GlobalPosition, Resolution, ChunkSize, MaxHeight);
        meshInstance.Mesh = chunkMesh;

        if (TerrainShader != null)
        {
            meshInstance.MaterialOverride = TerrainShader;
        }

        StaticBody3D staticBody = new StaticBody3D { Name = "TerrainCollider" };
        CollisionShape3D colShape = new CollisionShape3D { Name = "CollisionShape3D" };

        var trimesh = chunkMesh.CreateTrimeshShape();
        if (trimesh is ConcavePolygonShape3D concave)
        {
            concave.BackfaceCollision = true;
        }
    
        colShape.Shape = trimesh;
        staticBody.AddChild(colShape);
        AddChild(staticBody);
    }

    public float GetHeight(float worldX, float worldZ)
    {
        return Noise.GetNoise2D(worldX, worldZ) * MaxHeight;
    }

    // Analytical normals are maintained so the shader can still detect slopes accurately
    public Vector3 GetNormal(float worldX, float worldZ, float step)
    {
        float heightL = GetHeight(worldX - step, worldZ);
        float heightR = GetHeight(worldX + step, worldZ);
        float heightD = GetHeight(worldX, worldZ - step);
        float heightU = GetHeight(worldX, worldZ + step);

        Vector3 normal = new Vector3(heightL - heightR, 2.0f * step, heightD - heightU);
        return normal.Normalized();
    }

    public ArrayMesh GenerateChunkMesh(Vector3 worldOffset, int resolution, float chunkSize, float maxHeight)
    {
        int width = resolution + 1;
        int totalVertices = width * width;
        float step = chunkSize / resolution;

        Vector3[] vertices = new Vector3[totalVertices];
        Vector2[] uvs = new Vector2[totalVertices];
        Vector3[] normals = new Vector3[totalVertices];

        int vIdx = 0;
        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float worldX = worldOffset.X + (x * step);
                float worldZ = worldOffset.Z + (z * step);
                
                float height = GetHeight(worldX, worldZ);

                vertices[vIdx] = new Vector3(x * step, height, z * step);
                uvs[vIdx] = new Vector2((float)x / resolution, (float)z / resolution);
                normals[vIdx] = GetNormal(worldX, worldZ, step);
                
                vIdx++;
            }
        }

        int[] indices = new int[resolution * resolution * 6];
        int iIdx = 0;
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int current = x + z * width;
                
                // Counter-Clockwise winding to prevent backface culling issues
                indices[iIdx++] = current;
                indices[iIdx++] = current + width;
                indices[iIdx++] = current + 1;

                indices[iIdx++] = current + 1;
                indices[iIdx++] = current + width;
                indices[iIdx++] = current + width + 1;
            }
        }

        var surfaceArrays = new Godot.Collections.Array();
        surfaceArrays.Resize((int)Mesh.ArrayType.Max);
        surfaceArrays[(int)Mesh.ArrayType.Vertex] = vertices;
        surfaceArrays[(int)Mesh.ArrayType.Normal] = normals;
        surfaceArrays[(int)Mesh.ArrayType.TexUV] = uvs;
        surfaceArrays[(int)Mesh.ArrayType.Index] = indices;
        
        if (Foliage != null)
        {
            Foliage.GenerateFoliage(vertices, normals);
        }

        ArrayMesh mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArrays);
        return mesh;
    }
}