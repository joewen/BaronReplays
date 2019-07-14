using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace BaronReplays
{
    public class ExtendedObservableCollection<T> : ObservableCollection<T>
    {
        public ExtendedObservableCollection()
            : base()
        {
        }

        public ExtendedObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var i in collection) Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void InsertRange(int pos, IEnumerable<T> collection)
        {
            foreach (var i in collection) Items.Insert(pos, i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }



    }
}
