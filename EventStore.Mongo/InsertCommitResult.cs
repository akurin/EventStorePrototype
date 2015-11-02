namespace EventStore.Mongo
{
    internal enum InsertCommitResult
    {
        Success,
        DuplicateCommitWithSameIndexInStream,
        DuplicateCommitWithSameIndexInAllStreams
    }
}