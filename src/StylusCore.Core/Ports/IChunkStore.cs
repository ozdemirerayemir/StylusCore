namespace StylusCore.Core.Ports
{
    /// <summary>
    /// Port for spatial chunk storage operations.
    /// Implementations: SqliteChunkStore (Infrastructure)
    /// </summary>
    public interface IChunkStore
    {
        // TODO: Define chunk operations when spatial chunking is implemented
        // - SaveChunkAsync(ChunkId, byte[] compressedPayload)
        // - LoadChunkAsync(ChunkId) -> byte[]
        // - QueryChunksInViewport(RectD viewport) -> ChunkId[]
        // - DeleteChunkAsync(ChunkId)
    }
}
