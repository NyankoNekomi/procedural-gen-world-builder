using Godot;
using System.Collections.Generic;

public partial class FoliageManager : Node3D
{
    [Export] public Godot.Collections.Array<FoliageItem> FoliageTypes { get; set; } = new Godot.Collections.Array<FoliageItem>();
    [Export] public Godot.Collections.Array<FoliageSceneItem> SceneFoliage { get; set; } = new Godot.Collections.Array<FoliageSceneItem>();

    public void GenerateFoliage(Vector3[] vertices, Vector3[] normals)
    {
        if (vertices.Length == 0) return;

        // 1. Process standard MultiMesh resources (High-density grass/trees)
        if (FoliageTypes != null)
        {
            for (int i = 0; i < FoliageTypes.Count; i++)
            {
                if (FoliageTypes[i] != null && IsInstanceValid(FoliageTypes[i]) && FoliageTypes[i].Mesh != null)
                {
                    SpawnFoliageLayer(FoliageTypes[i], vertices, normals, i);
                }
            }
        }

        // 2. Process PackedScene resources (Low-density prefabs like campfires)
        if (SceneFoliage != null)
        {
            for (int i = 0; i < SceneFoliage.Count; i++)
            {
                if (SceneFoliage[i] != null && IsInstanceValid(SceneFoliage[i]) && SceneFoliage[i].Scene != null)
                {
                    SpawnSceneLayer(SceneFoliage[i], vertices, normals, i);
                }
            }
        }
    }

    private void SpawnFoliageLayer(FoliageItem item, Vector3[] vertices, Vector3[] normals, int index)
    {
        var multiMesh = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            Mesh = item.Mesh
        };

        List<Transform3D> validTransforms = new();
        RandomNumberGenerator rng = new();
        rng.Randomize();

        float minDistanceSquared = item.MinDistance * item.MinDistance; 

        for (int i = 0; i < vertices.Length; i++)
        {
            if (rng.Randf() <= item.SpawnChance && normals[i].Y >= item.MaxSlopeAngle)
            {
                Vector3 candidatePos = vertices[i] + new Vector3(
                    rng.RandfRange(-0.5f, 0.5f), 
                    0, 
                    rng.RandfRange(-0.5f, 0.5f)
                );
                
                bool tooClose = false;
                foreach (var existingTransform in validTransforms)
                {
                    if (candidatePos.DistanceSquaredTo(existingTransform.Origin) < minDistanceSquared)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    float randomScale = rng.RandfRange(item.MinScale, item.MaxScale);
                    float randomRotationY = rng.RandfRange(0, Mathf.Tau); 

                    Transform3D t = new Transform3D(Basis.Identity, candidatePos);
                    t.Basis = t.Basis.Rotated(Vector3.Up, randomRotationY);
                    t.Basis = t.Basis.Scaled(new Vector3(randomScale, randomScale, randomScale));
                    
                    validTransforms.Add(t);
                }
            }
        }

        if (validTransforms.Count == 0) return;

        multiMesh.InstanceCount = validTransforms.Count;
        for (int i = 0; i < validTransforms.Count; i++)
        {
            multiMesh.SetInstanceTransform(i, validTransforms[i]);
        }

        MultiMeshInstance3D mmi = new MultiMeshInstance3D
        {
            Multimesh = multiMesh,
            Name = $"FoliageLayer_{index}"
        };

        AddChild(mmi);
    }

    private void SpawnSceneLayer(FoliageSceneItem item, Vector3[] vertices, Vector3[] normals, int index)
    {
        List<Vector3> validPositions = new();
        RandomNumberGenerator rng = new();
        rng.Randomize();

        float minDistanceSquared = item.MinDistance * item.MinDistance; 

        // Create a parent node to keep the scene tree organized
        Node3D layerParent = new Node3D { Name = $"SceneLayer_{index}" };
        AddChild(layerParent);

        for (int i = 0; i < vertices.Length; i++)
        {
            if (rng.Randf() <= item.SpawnChance && normals[i].Y >= item.MaxSlopeAngle)
            {
                Vector3 candidatePos = vertices[i] + new Vector3(
                    rng.RandfRange(-0.5f, 0.5f), 
                    0, 
                    rng.RandfRange(-0.5f, 0.5f)
                );
                
                bool tooClose = false;
                foreach (var existingPos in validPositions)
                {
                    if (candidatePos.DistanceSquaredTo(existingPos) < minDistanceSquared)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    validPositions.Add(candidatePos);
                    
                    // Instantiate the actual scene
                    Node3D instance = item.Scene.Instantiate<Node3D>();
                    
                    // Apply transforms directly to the node
                    instance.Position = candidatePos;
                    float randomScale = rng.RandfRange(item.MinScale, item.MaxScale);
                    instance.Scale = new Vector3(randomScale, randomScale, randomScale);
                    instance.RotateY(rng.RandfRange(0, Mathf.Tau));
                    
                    layerParent.AddChild(instance);
                }
            }
        }
    }
}