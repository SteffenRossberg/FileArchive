namespace FileArchive.Core
{
    public enum CompareResults
    {
        MissingTarget,
        Different,
        MissingSource,
        ReadSourceFailed,
        ReadTargetFailed,
        WriteTargetFailed,
        Equal,
    }
}