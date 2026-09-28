using Godot;

[GlobalClass]
public partial class FoliageItem : Resource
{
    // Core visual mesh (e.g., .obj or .glb file)
    [Export] public Mesh Mesh { get; set; }

    // Generation rules
    // Range: 0.0005 (0.05%) to 1.0 (100%), stepping by 0.0001 (0.01%)
    [Export(PropertyHint.Range, "0.0005,1.0,0.0001")] 
    public float SpawnChance { get; set; } = 0.02f;
    [Export(PropertyHint.Range, "0.1,20.0,0.1")]
    public float MinDistance { get; set; } = 2.5f;

    [Export(PropertyHint.Range, "0.1,5.0,0.05")]
    public float MinScale { get; set; } = 0.4f;

    [Export(PropertyHint.Range, "0.1,5.0,0.05")]
    public float MaxScale { get; set; } = 1.0f;

    [Export(PropertyHint.Range, "0,1,0.01")] 
    public float MaxSlopeAngle { get; set; } = 0.85f;

    // Physics parameters for MultiMesh collisions
    [ExportGroup("Physics")]
    [Export] public bool HasCollision { get; set; } = true;
    [Export] public float CollisionRadius { get; set; } = 0.5f;
    [Export] public float CollisionHeight { get; set; } = 4.0f;
}