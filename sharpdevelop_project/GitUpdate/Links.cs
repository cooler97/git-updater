using System;
using RestSharp.Deserializers;

namespace GitUpdate
{
    public class Links
    {
        [DeserializeAsAttribute(Name = "self")]
        public string Self {get; set;}
        
        [DeserializeAsAttribute(Name = "git")]
        public string Git {get; set;}
        
        [DeserializeAsAttribute(Name = "Html")]
        public string Html {get; set;}
        
        public Links()
        {
        }
    }
}
