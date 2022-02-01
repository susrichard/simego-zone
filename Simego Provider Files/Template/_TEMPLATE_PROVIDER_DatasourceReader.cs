using Simego.DataSync;
using Simego.DataSync.Interfaces;
using Simego.DataSync.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace _TEMPLATE_NAMESPACE_PROVIDER_
{
    [ProviderInfo(Name = "_TEMPLATE_PROVIDER_", Description = "_TEMPLATE_PROVIDER_ Description")]
    public class _TEMPLATE_PROVIDER_DatasourceReader : DataReaderProviderBase, IDataSourceSetup
    {
        private ConnectionInterface _connectionIf;
        
        [Category("Settings")]
        public string ExampleSetting { get; set; }               

        public _TEMPLATE_PROVIDER_DatasourceReader()
        {           
            //Setup any Provider Defaults here
            ExampleSetting = "Some Initial Value";

            SupportsIncrementalReconciliation = false; //Enable Incremental Mode?            
        }
        
        public override DataTableStore GetDataTable(DataTableStore dt)
        {
            //Creates a basic Dictionary with some data you would replace this with calls to your target system to get the data.
            var result = new List<Dictionary<string, object>>(
                new[] {
                    new Dictionary<string, object>()
                        {
                            { "item_id", 1 }, { "name", "name1" }, { "value", "value1" }
                        },
                    new Dictionary<string, object>()
                        {
                            { "item_id", 2 }, { "name", "name2" }, { "value", "value2" }
                        },
                    new Dictionary<string, object>()
                        {
                            { "item_id", 3 }, { "name", "name3" }, { "value", "value3" }
                        }
            });
            
            
            //This data source has a single integer ID value we use this to store the ID to assist with UPDATE and DELETE actions.
            dt.AddIdentifierColumn(typeof(int));
            
            DataSchemaMapping mapping = new DataSchemaMapping(SchemaMap, Side);
            IList<DataSchemaItem> columns = SchemaMap.GetIncludedColumns();
              
            //Loop around your data adding it to the DataTableStore dt object.
            foreach (var item_row in result)
            {            
                if (dt.Rows.AddWithIdentifier(mapping, columns,
                    (item, columnName) => 
                    {
                        return item_row[columnName];
                    }
                    , DataSchemaTypeConverter.ConvertTo<int>(item_row["item_id"])) == DataTableStore.ABORT)
                {
                    break;
                }
            }
            
            return dt;
        }

        public override DataTableStore GetDataTable(DataTableStore dt, DataTableKeySet keyset)
        {
            //This is called if the Project mode is SyncAtoBIncremental, used to only load rows that match the Key value from the source. 
            //An optimization when the source is small and the Target is large.

            //This data source has a single integer ID value we use this to store the ID to assist with UPDATE actions.
            dt.AddIdentifierColumn(typeof(int));
            
            DataSchemaMapping mapping = new DataSchemaMapping(SchemaMap, Side);
            IList<DataSchemaItem> columns = SchemaMap.GetIncludedColumns();

            var keycolumn = mapping.MapColumnToDestination(keyset.KeyColumn);

            foreach (var keyItem in keyset.KeyValues)
            {
                try
                {
                    //TODO: Load each item Incrementally, fill out the row.
                    var item_row = new Dictionary<string, object>() 
                        { 
                            { "item_id", keyItem }, { "name", "name" + keyItem }, { "value", "value" + keyItem } 
                        };

                    if (dt.Rows.AddWithIdentifier(mapping, columns,
                        (item, columnName) =>
                        {
                            return item_row[columnName];
                        }
                        , DataSchemaTypeConverter.ConvertTo<int>(item_row["item_id"])) == DataTableStore.ABORT)
                    {
                        break;
                    }

                }
                catch (SystemException)
                {

                }
            }

            return dt;
        }

        public override DataSchema GetDefaultDataSchema()
        {
            //Return the Data source default Schema.

            DataSchema schema = new DataSchema();

            schema.Map.Add(new DataSchemaItem("item_id", typeof(int), true, false, false, -1));
            schema.Map.Add(new DataSchemaItem("name", typeof(string), false, false, true, 500));
            schema.Map.Add(new DataSchemaItem("value", typeof(string), false, false, true, 500));
  
            return schema;

        }

        public override List<ProviderParameter> GetInitializationParameters()
        {
            //Return the Provider Settings so we can save the Project File.
            return new List<ProviderParameter>
                       {
                            new ProviderParameter("ExampleSetting", ExampleSetting, GetConfigKey("ExampleSetting"))
                       };
        }

        public override void Initialize(List<ProviderParameter> parameters)
        {
            //Load the Provider Settings from the File.
            foreach (ProviderParameter p in parameters)
            {
                AddConfigKey(p.Name, p.ConfigKey);

                switch (p.Name)
                {
                    case "ExampleSetting":
                        {
                            ExampleSetting = p.Value;
                            break;
                        }                    
                    default:
                        {
                            break;
                        }
                }
            }
        }

        public override IDataSourceWriter GetWriter()
        {
            //if your provider is read-only return null here.
            return new _TEMPLATE_PROVIDER_DataSourceWriter { SchemaMap = SchemaMap };
        }

        #region IDataSourceSetup - Render Custom Configuration UI
        
        public void DisplayConfigurationUI(Control parent)
        {
            if (_connectionIf == null)
            {
                _connectionIf = new ConnectionInterface();
                _connectionIf.PropertyGrid.SelectedObject = new ConnectionProperties(this);
            }

            _connectionIf.Font = parent.Font;
            _connectionIf.Size = new Size(parent.Width, parent.Height);
            _connectionIf.Location = new Point(0, 0);
            _connectionIf.Dock = System.Windows.Forms.DockStyle.Fill;

            parent.Controls.Add(_connectionIf);
        }

        public bool Validate()
        {
            try
            {
                if (string.IsNullOrEmpty(ExampleSetting))
                {
                    throw new ArgumentException("You must specify a valid ExampleSetting.");
                }

                //GetDefaultDataSchema(); // Option - Verify the Schema Loads.
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "_TEMPLATE_PROVIDER_DatasourceReader", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return false;

        }

        public IDataSourceReader GetReader()
        {
            return this;
        }

        #endregion
    }
}
