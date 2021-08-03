using System;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.ChakraCore;

namespace JSTool
{
    public class JsSwitcher
    {
        IJsEngine engine;
        public JsSwitcher()
        {
            IJsEngineSwitcher engineSwitcher = JsEngineSwitcher.Current;
            engineSwitcher.EngineFactories.Add(new ChakraCoreJsEngineFactory());
            engineSwitcher.DefaultEngineName = ChakraCoreJsEngine.EngineName;
            engine = JsEngineSwitcher.Current.CreateDefaultEngine();
        }
        public string Execute(string script)
        {
            string res = engine.Evaluate(script).ToString();
            return res;
        }
    }
    public static class ChakraCore
    {
        static JsSwitcher engine = new JsSwitcher();
        public static string ExecuteScript(string script)
        {
            return engine.Execute(script);
        }
 
    }
    public static class Timestamp
    {
        public static long GetTimeStamp(bool AccurateToMilliseconds = false)
        {
            if (AccurateToMilliseconds)
            {
                return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            }
            else
            {
                return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            }
        }
        public static DateTime GetTime(long TimeStamp, bool AccurateToMilliseconds = false)
        {
            System.DateTime startTime = TimeZoneInfo.ConvertTimeFromUtc(new System.DateTime(1970, 1, 1),TimeZoneInfo.Local);
            if (AccurateToMilliseconds)
            {
                return startTime.AddTicks(TimeStamp * 10000);
            }
            else
            {
                return startTime.AddTicks(TimeStamp * 10000000);
            }
        }
    }
}
