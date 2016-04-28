using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HuntersWP.Services
{
    public static class Helpers
    {
        public static string GetAppVersion()
        {
            return "Imported";
        }

        public static void LogEvent(string name, Dictionary<string, string> values)
        {
        }

        public static void ShowProgressIndicatorService(string message)
        {
    }

        public static void ShowMessageBox(string message)
        {


        }

        public static void DebugMessage(string message)
        {
            Console.WriteLine(message);
        }

        public static void HideProgressIndicatorService()
        {
        }

    public static string Sha1(string value)
        {
            var sha = new SHA1Managed();
            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            byte[] resultHash = sha.ComputeHash(bytes);

            return resultHash.Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
        }

        public static MemberInfo GetMemberInfo(this System.Linq.Expressions.Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }

            return memberExpression.Member;
        }
    }
}
