using System;

namespace AutoCadComShared
{
    public abstract class AcDocBase : IDisposable
    {
        public abstract string Name { get; }
        public abstract string FullName { get; }
        public abstract void Close(bool pSaveChanges);

        public virtual void Dispose() { }
    }
}
