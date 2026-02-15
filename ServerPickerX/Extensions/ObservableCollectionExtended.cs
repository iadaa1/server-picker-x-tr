using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ServerPickerX.Extensions
{
    public class ObservableCollectionExtended<T>: ObservableCollection<T>
    {
        public ObservableCollectionExtended()
        {
        }

        public ObservableCollectionExtended(IEnumerable<T> collection) : base([.. collection ?? throw new ArgumentNullException(nameof(collection))])
        {
        }

        public ICollection<T> AddRange(ICollection<T> values)
        {
            foreach (var item in values)
            {
                Add(item);
            }

            return this;
        }
    }
}
