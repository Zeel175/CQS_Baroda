using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Services.API.Extensions
{
    public static class SMTPEmailExtensions
    {
        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> items)
        {
            foreach(var item in items)
            {
                target.Add(item);
            }
        }
    }
}
