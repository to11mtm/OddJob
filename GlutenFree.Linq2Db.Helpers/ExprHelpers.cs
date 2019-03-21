using System.Collections.Generic;
using System.Linq;

namespace GlutenFree.Linq2Db.Helpers
{
    public static class ExprHelpers
    {
        public static bool LikeAny(this string myString, IEnumerable<string> likeSet)
        {
            return likeSet.Any(s => myString.Contains(s));
        }

        public static bool LikeAnyLower(this string myString, IEnumerable<string> likeSet)
        {
            return likeSet.Any(s => myString.ToLower().Contains(s.ToLower()));
        }

        public static bool LikeAnyUpper(this string myString, IEnumerable<string> likeSet)
        {
            return likeSet.Any(s => myString.ToUpper().Contains(s.ToUpper()));
        }

        public static bool LikeAll(this string myString, IEnumerable<string> likeSet)
        {
            return likeSet.All(s => myString.Contains(s));
        }

        public static bool LikeAllLower(this string myString, IEnumerable<string> likeSet)
        {
            return likeSet.All(s => myString.ToLower().Contains(s.ToLower()));
        }

        public static bool LikeAllUpper(this string myString, IEnumerable<string> likeSet)
        {
            return likeSet.All(s => myString.ToUpper().Contains(s.ToUpper()));
        }
    }
}
