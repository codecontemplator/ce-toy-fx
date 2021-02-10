using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace ce_toy_fx.sample.Dynamic
{
    public class JsonParser
    {
        public static MRule ParseMRule(string json)
        {
            return JsonConvert.DeserializeObject<MRule>(json, new MRuleConverter(), new SRuleConverter());            
        }
    }

    class MRuleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(MRule));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Ref: https://thomaslevesque.com/2019/10/15/handling-type-hierarchies-in-cosmos-db-part-2/
            var obj = JObject.Load(reader);
            switch (obj["type"]?.Value<string>())
            {
                case "MRuleJoin":
                    return obj.ToObject<MRuleJoin>(serializer);
                case "MRuleDef":
                    return obj.ToObject<MRuleDef>(serializer);
                case "SRuleLift":
                    return obj.ToObject<SRuleLift>(serializer);
                case "SRuleJoin":
                    return obj.ToObject<SRuleJoin>(serializer);
                case "SRuleDef":
                    return obj.ToObject<SRuleDef>(serializer);
                default:
                    throw new ArgumentException("Unhandled type");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;
    }

    class SRuleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(SRule));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Ref: https://thomaslevesque.com/2019/10/15/handling-type-hierarchies-in-cosmos-db-part-2/
            var obj = JObject.Load(reader);
            switch (obj["type"]?.Value<string>())
            {
                case "SRuleJoin":
                    return obj.ToObject<SRuleJoin>(serializer);
                case "SRuleDef":
                    return obj.ToObject<SRuleDef>(serializer);
                default:
                    throw new ArgumentException("Unhandled type");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;
    }

}
