import os
import re
import sys
import xml.etree.ElementTree as ET
import get_calc_field_types as podio

# files should be schema.table.dsprj (workspace.app.dsprj)
simego_file = sys.argv[1]
#simego_file = 'Simego Data Sync Projects\public.richard_sql_zone.dsprj'
try:
    file_path, file_name = os.path.split(simego_file)
    print(file_name)
    parts = file_name.split('.')
    sql_schema = parts[0]
    sql_table = parts[1]
except:
    sql_schema = 'schema'
    sql_table = 'table'

calc_fields = []

DATA_TYPES=[{"simego_in":"System.Int32[]","simego_out":"System.String","postgres":"TEXT"},
            {"simego_in":"System.String[]","simego_out":"System.String","postgres":"TEXT"},
            {"simego_in":"System.String","simego_out":"System.String","postgres":"TEXT"},
            {"simego_in":"System.Int32","simego_out":"System.Int32","postgres":"INTEGER"},
            {"simego_in":"System.DateTime","simego_out":"System.DateTime","postgres":"TIMESTAMP"},
            {"simego_in":"System.Double","simego_out":"System.Double","postgres":"DOUBLE PRECISION"},
            {"simego_in":"System.Decimal","simego_out":"System.Double","postgres":"DOUBLE PRECISION"},
            {"simego_in":"System.Single","simego_out":"System.Double","postgres":"DOUBLE PRECISION"},
            {"simego_in":"System.Boolean","simego_out":"System.Boolean","postgres":"BOOLEAN"},
            {"simego_in":"System.String","simego_out":"System.String","postgres":"TEXT"}]
            
def search_type_mapping(list, key, value):
    for entry in list:
        if entry[key] == value:
            return entry
          
class Field:
    def __init__(self, simego_name, simego_display_name, simego_data_type, simego_allow_null, simego_unique):
        self.simego_name = simego_name
        self.simego_display_name = simego_display_name
        self.simego_data_type = simego_data_type
        self.simego_allow_null = simego_allow_null
        self.simego_unique = simego_unique

        mapping = search_type_mapping(DATA_TYPES, 'simego_in', self.simego_data_type)
        self.postgres_type = mapping['postgres']
        self.simego_data_type_revised = mapping['simego_out']

        self.calc_mapping = search_type_mapping(calc_fields, 'external_id', self.simego_name)
        if self.calc_mapping:
            # print(self.calc_mapping['return_type'])
            if self.calc_mapping['return_type'] == 'number':
                self.postgres_type = 'DOUBLE PRECISION'
                self.simego_data_type_revised ='System.Double'
            if self.calc_mapping['return_type'] == 'date':
                self.postgres_type = 'TIMESTAMP'
                self.simego_data_type_revised ='System.DateTime'
        if self.simego_display_name:
            self.postgres_col_name = strip_unsafe_characters(self.simego_display_name)
        else:
            self.postgres_col_name = strip_unsafe_characters(self.simego_name)

def strip_unsafe_characters(str):
 regex_bad_chars = r"[^\w\d\_\n]"
 return re.sub(regex_bad_chars, '_', str)

our_fields = []
app_id = ''
# read dsprj xml
tree = ET.parse(simego_file)
root = tree.getroot()
params = root.find('DataSource').find('Initialization')
for param in params.iter('Parameter'):
    # print(param.attrib['Name'])
    # print(param.attrib['Value'])
    if param.attrib['Name'] == 'AppID':
        app_id = param.attrib['Value']

if app_id:
    calc_fields = podio.get_app_calc_fields(app_id)

xml_columns = root.find('DataSource').find('Columns')
for xml_col in xml_columns.iter("Column"):
    # only item_id is primary key (called unique here)
    if xml_col.get('Unique') == 'True' and xml_col.get('Name') != 'item_id':
        xml_col.set('Unique', 'False')

    our_field = Field(simego_name=xml_col.get('Name'), 
                      simego_display_name=xml_col.get('DisplayName'),
                      simego_data_type=xml_col.get('DataType'),
                      simego_allow_null= xml_col.get('AllowNull'),
                      simego_unique=xml_col.get('Unique'))
    our_fields.append(our_field)
                      
    #mutate xml tree to our new simego data type (generated on class init)         
    xml_col.set('DataType', our_field.simego_data_type_revised)

# write full backup dsprj
tree.write(f'{sql_schema}.{sql_table}.postgres.dsprj')

# write incremental dsprj
root.set('SyncOption','SyncAtoBIncremental')
tree.write(f'{sql_schema}.{sql_table}.postgres.incremental.dsprj')

#generate postgres create statement
sql_txt = f'CREATE TABLE {sql_schema}.{sql_table}(\n'
for field in our_fields:
    if field.postgres_col_name == 'item_id':
        sql_txt += f'{field.postgres_col_name} {field.postgres_type} PRIMARY KEY,\n'
    elif field.simego_data_type == 'System.DateTime':
        sql_txt += f'{field.postgres_col_name}_cst {field.postgres_type} WITH TIME ZONE,\n'
        # sql_txt += f"{field.postgres_col_name}_cst {field.postgres_type} WITH TIME ZONE GENERATED ALWAYS AS ({field.postgres_col_name}_utc at time zone 'cst') STORED,\n"
    else:
        sql_txt += f'{field.postgres_col_name} {field.postgres_type} NULL,\n'
sql_txt = sql_txt[:-2] + '\n)' #strip trailing comma

#write sql
f = open(f"{sql_schema}.{sql_table}.postgres.sql", "w")
f.write(sql_txt)
f.close()

