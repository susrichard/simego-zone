This template project is the basis for developing your own provider to connect to some new system.

The basic requirements are :-

1. Return Schema Information - Method GetDefaultDataSchema() must return the Schema of the System i.e. the Columns and Data Types.
2. Return Data - Method  GetDataTable(DataTableStore dt) - must return your data, simply fill the supplied DataTableStore object with the Row data, you must handle mapping where the source and column names might be different and convert the data to the requested data type of the supplied schema.

To support the Writer

Ensure GetWriter() returns an Instance of the Writer and has the SchemaMap initialized. 

Implement the Add/Update/Delete methods that should apply these actions against your Target System. The DataCompareColumnItem object contains the changes SourceRow contains the complete row information from the Source and Row contains the changed columns. 
We have used helper functions AddItemToDictionary and UpdateItemToDictionary which convert the change set and apply the mapping into a Dictionary<string, object> that includes all the changes that you will need to send to the target.

Method - GetInitializationParameters() is called when DS Attempts to save the project so this should return your configuration data.

Method - Initialize(List<ProviderParameter> parameters) is called when the project is opened, you should use the supplied values to set-up your configuration.

Once the provider is built and tested simply copy the output assembly *.dll into the Data Sync Installation Directory (C:\Program Files\Simego\Data Synchronisation Studio 3.0). Data Sync uses Reflection to load your Provider Assembly.


NOTE: To enable debugging the Start-up Program has been configured to point to the Simego.DataSync.Studio.exe and a command line argument -debug will force Data Sync to load your Assembly.