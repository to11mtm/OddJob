using System.Linq;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// A helper class to allow for appending to arrays.
    /// </summary>
    public static class ArrayHelper
    {
        public static T[] WithItem<T>(this T[] array, T newItem)
        {
            //Probably not the most efficient way to do any of this, but it's just for the example. =)
            return array.Append(newItem).ToArray();
        }
    }
}