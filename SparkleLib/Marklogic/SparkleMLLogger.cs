using System;
using MarkLogicLib;

namespace SparkleLib.Marklogic
{
    public class SparkleMLLogger : ILogger
    {
        public SparkleMLLogger ()
        {
        }

        public void log(string msg) {
            SparkleLogger.LogInfo ("SparkleMLLogger", msg);
        }
    }
}

