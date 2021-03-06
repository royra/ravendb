﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Raven.Studio.Infrastructure
{
    public interface IVirtualCollectionSource<T>
    {
        event EventHandler<VirtualCollectionSourceChangedEventArgs> CollectionChanged;
        event EventHandler<DataFetchErrorEventArgs> DataFetchError;

        int Count { get; }

        Task<IList<T>> GetPageAsync(int start, int pageSize, IList<SortDescription> sortDescriptions);

        void Refresh(RefreshMode mode);
    }

    public class VirtualCollectionSourceChangedEventArgs : EventArgs
    {
        public ChangeType ChangeType { get; private set; }

        public VirtualCollectionSourceChangedEventArgs(ChangeType changeType)
        {
            ChangeType = changeType;
        }
    }

    public enum ChangeType
    {
        Reset,
        Refresh,
    }
}
