using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ce_toy_fx.sample.web.Models
{
    public class EvaluateRequest
    {
        public object Process { get; set; }
        public EvaluateRequestApplication Application { get; set; }
    }

    public class EvaluateRequestApplication
    {
        public int RequestedAmount { get; set; }
        public EvaluateRequestApplicant[] Applicants { get; set; }
    }

    public class EvaluateRequestApplicant
    {
        public string Id { get; set; }
        public EvaluateRequestKeyValue[] KeyValues { get; set; }
        public Applicant ToInternalModel()
        {
            return new Applicant
            {
                Id = Id,
                KeyValueMap = KeyValues.ToDictionary(kv => kv.Key, kv => (object)kv.Value).ToImmutableDictionary()
            };
        }
    }

    public class EvaluateRequestKeyValue
    {
        public string Key { get; set; }
        public int Value { get; set; }  // TODO: Handle other types
    }
}
