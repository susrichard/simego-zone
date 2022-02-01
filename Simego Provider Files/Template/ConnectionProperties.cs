using System.ComponentModel;

namespace _TEMPLATE_NAMESPACE_PROVIDER_
{
    class ConnectionProperties
    {
        private readonly _TEMPLATE_PROVIDER_DatasourceReader _reader;
        
        [Category("Settings")]
        public string ExampleSetting { get { return _reader.ExampleSetting; } set { _reader.ExampleSetting = value; } }

        public ConnectionProperties(_TEMPLATE_PROVIDER_DatasourceReader reader)
        {
            _reader = reader;
        }        
    }
}
