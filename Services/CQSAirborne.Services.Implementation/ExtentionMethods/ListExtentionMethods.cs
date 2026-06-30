using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Implementation.ExtensionMethods
{
    public static class ListExtentionMethods
    {
        public static List<string> ToEmailList(this IQueryable<string> source)
        {
            var result = new List<string>();
            foreach (var item in source)
            {
                try
                {
                    new MailAddress(item.ToString());
                    result.Add(item.ToString());
                }
                catch (Exception ex)
                {
                    //throw;
                    continue;
                }
            }
            return result;
        }
        public static List<string> StringToEmailList(this string source, char SplitChar)
        {
            var result = new List<string>();
            foreach (var email in source.Split(SplitChar))
            {
                try
                {
                    var x = new MailAddress(email.Trim());
                    result.Add(email.Trim());
                }
                catch(Exception ex)
                {
                    //throw;
                }
            }
            return result;
        }
    }
}
