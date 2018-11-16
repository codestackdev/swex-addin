using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.AddIn.Helpers
{
    internal static class Trace
    {
        const string CATEGORY = "SwEx.AddIn";

        internal static void Log(string msg)
        {
            System.Diagnostics.Trace.WriteLine(msg, CATEGORY);
        }

        internal static void Log(Exception ex)
        {
            var exMsg = new StringBuilder();
            ParseExceptionLog(ex, exMsg);

            Log(exMsg.ToString());
        }

        private static void ParseExceptionLog(Exception ex, StringBuilder exMsg)
        {
            exMsg.AppendLine(ex?.Message + ex?.StackTrace);

            if (ex?.InnerException != null)
            {
                ParseExceptionLog(ex.InnerException, exMsg);
            }
        }
    }
}
