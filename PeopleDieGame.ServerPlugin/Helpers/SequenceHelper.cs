using System;
using System.Collections.Generic;
using System.Linq;

namespace PeopleDieGame.ServerPlugin.Helpers
{
    public static class SequenceHelper
    {
        public static bool ContainsSubequence<T>(this IEnumerable<T> parent, IEnumerable<T> target, Func<T, T, bool> predicate = null)
        {
            if (predicate == null)
                predicate = new Func<T, T, bool>((e1, e2) => e1.Equals(e2));

            bool foundOneMatch = false;
            var enumeratedTarget = target.ToList();
            int enumPos = 0;

            using (IEnumerator<T> parentEnum = parent.GetEnumerator())
            {
                while (parentEnum.MoveNext())
                {
                    if (predicate(enumeratedTarget[enumPos], parentEnum.Current))
                    {
                        // Match, so move the target enum forward
                        foundOneMatch = true;
                        if (enumPos == enumeratedTarget.Count - 1)
                        {
                            // We went through the entire target, so we have a match
                            return true;
                        }

                        enumPos++;
                    }
                    else if (foundOneMatch)
                    {
                        foundOneMatch = false;
                        enumPos = 0;

                        if (predicate(enumeratedTarget[enumPos], parentEnum.Current))
                        {
                            foundOneMatch = true;
                            enumPos++;
                        }
                    }
                }

                return false;
            }
        }

        public static int GetSubsequenceIndex<T>(this IEnumerable<T> parent, IEnumerable<T> target, Func<T, T, bool> predicate = null)
        {
            if (predicate == null)
                predicate = new Func<T, T, bool>((e1, e2) => e1.Equals(e2));

            bool foundOneMatch = false;
            var enumeratedTarget = target.ToList();
            int enumPos = 0;
            int index = 0;

            using (IEnumerator<T> parentEnum = parent.GetEnumerator())
            {
                while (parentEnum.MoveNext())
                {
                    if (predicate(enumeratedTarget[enumPos], parentEnum.Current))
                    {
                        // Match, so move the target enum forward
                        foundOneMatch = true;
                        if (enumPos == enumeratedTarget.Count - 1)
                        {
                            // We went through the entire target, so we have a match
                            return index - enumPos;
                        }

                        enumPos++;
                    }
                    else if (foundOneMatch)
                    {
                        foundOneMatch = false;
                        enumPos = 0;

                        if (predicate(enumeratedTarget[enumPos], parentEnum.Current))
                        {
                            foundOneMatch = true;
                            enumPos++;
                        }
                    }

                    index++;
                }

                return -1;
            }
        }
    }
}
