using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueTower
{
    public class WeightedItem<TItem>
    {
        public readonly TItem Item;
        public int Weight;

        internal WeightedItem(TItem item, int weight)
        {
            Item = item;
            Weight = weight;
        }
    }

    public class WeightedList<T> : IEnumerable<WeightedItem<T>>
    {
        List<WeightedItem<T>> Items = new List<WeightedItem<T>>();

        public int TotalWeight
        {
            get
            {
                return GetTotalWeight();
            }
        }

        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        public WeightedItem<T> this[T item]
        {
            get
            {
                return Items.Find(x => x.Item.Equals(item));
            }
        }

        public void Add(T item, int weight)
        {
            var weighteditem = this[item];

            if (weighteditem != null)
            {
                weighteditem.Weight = weight;
            }
            else
            {
                Items.Add(new WeightedItem<T>(item, weight));
            }
        }

        public bool Contains(T item)
        {
            return this[item] != null;
        }

        public void Remove(T item)
        {
            var weighteditem = this[item];

            if (weighteditem != null)
            {
                Items.Remove(weighteditem);
            }
        }

        public void Clear()
        {
            Items.Clear();
        }

        public T GetHighestWeight()
        {
            int highestweight = Items.Max(x => x.Weight);

            return Items.Find(x => x.Weight == highestweight).Item;
        }

        public T GetLowestWeight()
        {
            int lowestweight = Items.Min(x => x.Weight);

            return Items.Find(x => x.Weight == lowestweight).Item;
        }

        public int GetTotalWeight()
        {
            return Items.Sum(x => x.Weight);
        }

        public int GetTotalWeight(IEnumerable<T> blacklist)
        {
            int sum = 0;

            sum = Items.Sum(x => blacklist.Contains(x.Item) ? 0 : x.Weight);

            return sum;
        }

        public T GetWeighted(int value)
        {
            if (Items.Count <= 0)
            {
                return default(T);
            }

            int pick = value;
            int i = 0;

            while (pick > 0 && i < Items.Count)
            {
                pick -= Items[i].Weight;

                if (pick > 0)
                    i++;
            }

            return Items[i].Item;
        }

        public T GetWeighted(Random selector)
        {
            int totalweight = TotalWeight;

            return GetWeighted(selector.Next(totalweight));
        }

        public T GetWeighted(Random selector, IEnumerable<T> blacklist)
        {
            int totalweight = GetTotalWeight(blacklist);

            if (Items.Count <= 0)
            {
                return default(T);
            }

            int pick = selector.Next(totalweight);
            int i = 0;

            while (pick > 0 && i < Items.Count)
            {
                if (blacklist.Contains(Items[i].Item))
                {
                    i++;
                    continue;
                }

                pick -= Items[i].Weight;

                if (pick > 0)
                    i++;
            }

            return Items[i].Item;
        }

        public IEnumerator<WeightedItem<T>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
