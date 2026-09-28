using Godot;
using System;

public partial class TerrainGeneration : Node3D
{
    [Export] public FastNoiseLite Noise { get; set; }
    [Export] public int Resolution { get; set; } = 32;
    [Export] public float ChunkSize { get; set; } = 32.0f;
    [Export] public float MaxHeight { get; set; } = 8.0f;
    
    // Expose a ShaderMaterial directly for the AI/Designer to tweak
    [Export] public ShaderMaterial TerrainShader { get; set; } 
// Unbreakable link to the child manager
    [Export] public FoliageManager Foliage { get; set; }
    public override void _Ready()
    {
        if (Noise == null)
        {
            Noise = new FastNoiseLite { NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex, Seed = 1337, Frequency = 0.03f };
        }

        MeshInstance3D meshInstance = new MeshInstance3D { Name = "TerrainMesh" };
        AddChild(meshInstance);

        var chunkMesh = GenerateChunkMesh(GlobalPosition, Resolution, ChunkSize, MaxHeight);
        meshInstance.Mesh = chunkMesh;

// Automatically apply the custom shader if one is assigned
        if (TerrainShader != null)
        {
            meshInstance.MaterialOverride = TerrainShader;
        }
        else
        {
            // Fallback material if no shader is loaded
            meshInstance.MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.35f, 0.65f, 0.25f), // Default grassy green
                CullMode = BaseMaterial3D.CullModeEnum.Disabled,
                Roughness = 0.9f
            };
        }

        // --- ADD COLLISION SETUP HERE ---
        StaticBody3D staticBody = new StaticBody3D { Name = "TerrainCollider" };
        CollisionShape3D colShape = new CollisionShape3D { Name = "CollisionShape3D" };

        var trimesh = chunkMesh.CreateTrimeshShape();
        if (trimesh is ConcavePolygonShape3D concave)
        {
            concave.BackfaceCollision = true; // Prevents the player from falling through the mesh
        }
    
        colShape.Shape = trimesh;
        staticBody.AddChild(colShape);
        AddChild(staticBody);
    }

    public float GetHeight(float worldX, float worldZ)
    {
        return Noise.GetNoise2D(worldX, worldZ);
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
                float height = GetHeight(worldX, worldZ) * maxHeight;

                vertices[vIdx] = new Vector3(x * step, height, z * step);
                uvs[vIdx] = new Vector2((float)x / resolution, (float)z / resolution);
                normals[vIdx] = Vector3.Up; // Simplified normals for shader math
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
        
            //ADD IT HERE: Trigger the foliage manager while we have vertices in memory
        if (Foliage != null)
        {
            Foliage.GenerateFoliage(vertices, normals);
        }
        ArrayMesh mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArrays);
        return mesh;
    }
}