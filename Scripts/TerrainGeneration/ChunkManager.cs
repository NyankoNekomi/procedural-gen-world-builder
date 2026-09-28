using Godot;
using System.Collections.Generic;

public partial class ChunkManager : Node3D
{
    [Export] public Node3D Player { get; set; }
    [Export] public PackedScene ChunkScene { get; set; }
    [Export] public float ChunkSize { get; set; } = 32.0f;
    [Export] public int ViewDistance { get; set; } = 3; // Kept small for fast shader testing

    private readonly Dictionary<Vector2I, Node3D> _activeChunks = new();
    private Vector2I _lastPlayerChunk = new Vector2I(int.MaxValue, int.MaxValue);

    public override void _Process(double delta)
    {
        if (Player == null || ChunkScene == null) return;

        Vector2I currentChunk = new Vector2I(
            Mathf.FloorToInt(Player.GlobalPosition.X / ChunkSize),
            Mathf.FloorToInt(Player.GlobalPosition.Z / ChunkSize)
        );

        if (currentChunk != _lastPlayerChunk)
        {
            _lastPlayerChunk = currentChunk;
            UpdateChunks(currentChunk);
        }
    }

    private void UpdateChunks(Vector2I currentChunk)
    {
        List<Vector2I> requiredChunks = new List<Vector2I>();

        for (int x = -ViewDistance; x <= ViewDistance; x++)
        {
            for (int z = -ViewDistance; z <= ViewDistance; z++)
            {
                Vector2I chunkPos = new Vector2I(currentChunk.X + x, currentChunk.Y + z);
                requiredChunks.Add(chunkPos);

                if (!_activeChunks.ContainsKey(chunkPos))
                {
                    Node3D newChunk = ChunkScene.Instantiate<Node3D>();
                    newChunk.Position = new Vector3(chunkPos.X * ChunkSize, 0, chunkPos.Y * ChunkSize);
                    newChunk.Name = $"Chunk_{chunkPos.X}_{chunkPos.Y}";
                    
                    AddChild(newChunk);
                    _activeChunks.Add(chunkPos, newChunk);
                }
            }
        }

        List<Vector2I> chunksToRemove = new List<Vector2I>();
        foreach (var chunk in _activeChunks.Keys)
        {
            if (!requiredChunks.Contains(chunk))
            {
                chunksToRemove.Add(chunk);
            }
        }

        foreach (var chunk in chunksToRemove)
        {
            _activeChunks[chunk].QueueFree();
            _activeChunks.Remove(chunk);
        }
    }
}