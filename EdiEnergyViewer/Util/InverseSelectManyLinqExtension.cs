using System;
using System.Collections.Generic;

namespace Fabsenet.EdiEnergy.Util
{
    internal static class InverseSelectManyLinqExtension
    {
        public static IEnumerable<IEnumerable<T>> InverseSelectMany<T>(this IEnumerable<T> source, Func<T, T, bool> belongsToSameGroupPredicate)
        {
            var internalList = new List<T>();
            T lastItem = default(T);
            bool isFirstRun = true;
            foreach (var item in source)
            {
                if (!isFirstRun && !belongsToSameGroupPredicate(lastItem, item))
                {
                    //start new group and return current list
                    yield return internalList;
                    internalList = new List<T>();
                }

                //add item to current list
                internalList.Add(item);
                lastItem = item;
                isFirstRun = false;
            }

            //return last list
            yield return internalList;
        }
    }
}