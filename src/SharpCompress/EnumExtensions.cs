using System;

namespace SharpCompress
{
    internal static class FlagsExtensions
    {
        #if NET35
        public static bool HasFlag(this Enum enumRef, Enum flag)
        {
            long value = Convert.ToInt64(enumRef);
            long flagVal = Convert.ToInt64(flag);

            return (value & flagVal) == flagVal;
        }
        #endif
        
        public static bool HasFlag(this long flaggedInt, long flag)
        {
            return (flaggedInt & flag) == flag;
        }
    }
}
