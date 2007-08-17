using System;

namespace FlexWiki
{
    public interface IFileInformation
    {
        string Extension { get; }
        string FullName { get; }
        DateTime LastWriteTime { get; }
        string Name { get; }
        string NameWithoutExtension { get; }
    }
}
