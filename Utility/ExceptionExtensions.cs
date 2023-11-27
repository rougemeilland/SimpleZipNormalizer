using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class ExceptionExtensions
    {
        public static String GetFullExceptionMessage(this Exception exception)
        {
            return String.Join("\n--------------------\n", GetMessages(exception));

            static IEnumerable<String> GetMessages(Exception exception)
            {
                yield return exception.Message;
                if (!String.IsNullOrEmpty(exception.StackTrace))
                    yield return exception.StackTrace;

                if (exception.InnerException is not null)
                {
                    foreach (var message in GetMessages(exception.InnerException))
                        yield return message;
                }

                if (exception is AggregateException aggregateException)
                {
                    foreach (var ex in aggregateException.InnerExceptions)
                    {
                        foreach (var message in GetMessages(ex))
                            yield return message;
                    }
                }
            }
        }
    }
}
