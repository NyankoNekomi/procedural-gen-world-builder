using Godot;

[GlobalClass]
public partial class FoliageSceneItem : Resource
{
    // Uses PackedScene to load full .tscn files instead of .obj or .glb
    [Export] public PackedScene Scene { get; set; }

    [Export(PropertyHint.Range, "0.0005,1.0,0.0001")] 
    public float SpawnChance { get; set; } = 0.02f;

    [Export(PropertyHint.Range, "0.1,20.0,0.1")] 
    public float MinDistance { get; set; } = 2.5f;

    [Export(PropertyHint.Range, "0.1,5.0,0.05")]
    public float MinScale { get; set; } = 0.8f;

    [Export(PropertyHint.Range, "0.1,5.0,0.05")]
    public float MaxScale { get; set; } = 1.2f;

    [Export(PropertyHint.Range, "0,1,0.01")] 
    public float MaxSlopeAngle { get; set; } = 0.85f;
}