using fastJSON;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;

namespace CryptoTools.Generic.Helpers
{
    internal class JsonObject : JObject { }
    //internal class JsonObject : Dictionary<string, object> { }

    internal class JsonArray : JArray { }
    //internal class JsonArray : List<object> { }

    public static class JsonHelper
    {
        internal static string ToJson(dynamic dynobj)
        {
            return JsonConvert.SerializeObject(dynobj);
            //return JSON.ToJSON(dynobj);
        }

        internal static dynamic ToDynamic(this string jsonstr)
        {
            return JsonConvert.DeserializeObject(jsonstr);
            //return JSON.ToDynamic(jsonstr);
        }

        /*internal static dynamic ParseJson(this string jsonstr)
        {
            return JSON.Parse(jsonstr);
        }*/

        /*internal static dynamic ToDynamicArray(this object dynobj)
        {
            var obj = new ExpandoObject();
            foreach (var item in (JsonArray)dynobj)
                (obj as IList<object>).Add(item);
            return obj;
        }

        internal static dynamic ToDynamicObject(this object dynobj)
        {
            var obj = new ExpandoObject();
            foreach (var kvp in (JsonObject)dynobj)
                (obj as IDictionary<string, object>)[kvp.Key] = kvp.Value;
            return obj;
        }*/

        internal static bool IsJsonArray(dynamic dynobj)
        {
            return dynobj.GetType().Name.ToString() == "JArray";
            //return dynobj is List<object>;
        }

        internal static bool IsJsonObject(dynamic dynobj)
        {
            return dynobj.GetType().Name.ToString() == "JObject";
            //return dynobj is Dictionary<string, object>;
        }
    }
}
