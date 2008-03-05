using System;
using System.Xml.Serialization; 

namespace FlexWiki.Tools.RenderDriver
{
    [XmlRoot("configuration")]
    public class RenderDriverConfiguration
    {
        private FederationConfiguration _federationConfiguration;

        public FederationConfiguration FederationConfiguration
        {
            get { return _federationConfiguration; }
            set { _federationConfiguration = value; }
        }
    }
}
