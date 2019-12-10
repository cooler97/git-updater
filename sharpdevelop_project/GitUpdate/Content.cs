using System;
using RestSharp.Deserializers;

namespace GitUpdate
{

    public class Content
    {
        [DeserializeAsAttribute(Name = "name")]
        public string Name { get; set; }

        [DeserializeAsAttribute(Name = "path")]
        public string Path {get; set; }
        
        [DeserializeAsAttribute(Name = "sha")]
        public string Sha {get; set; }
        
        [DeserializeAsAttribute(Name = "size")]
        public int Size {get; set; }
        
        [DeserializeAsAttribute(Name = "Url")]
        public string Url {get; set; }
        
        [DeserializeAsAttribute(Name = "html_url")]
        public string HtmlUrl {get; set; }
        
        [DeserializeAsAttribute(Name = "git_url")]
        public string GitUrl {get; set; }
        
        [DeserializeAs(Name = "download_url")]
        public string DownloadUrl { get; set; }
        
        [DeserializeAs(Name = "type")]
        public string Type { get; set; }
        
        [DeserializeAsAttribute(Name = "_links")]
        public Links Links {get; set; }
        
        [DeserializeAsAttribute(Name = "content")]
        public string ContentString { get; set; }

        public Content()
        {
        }
    }
}
