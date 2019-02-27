namespace Replicator
{
    public enum ReplicationStatus
    {
        Unknown,
        Initializing,
        WriteDateMismatch,
        IncompleteReplication,
        Synced,
        Removed,
        Skipped,
        NoFiles,
        Aborted
    }
}