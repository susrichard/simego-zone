import csv
import podio_auth
import httplib2

from pypodio2 import api
import json


def get_app_calc_fields(app_id):
    c = api.OAuthClient(
    podio_auth.client_id,
    podio_auth.client_secret,
    podio_auth.username,
    podio_auth.password
    )
    app = c.Application.find(app_id)
    fields = []
    for field in app['fields']:
        if field['type'] == 'calculation':
            return_type = field['config']['settings']['return_type']
            unit = field['config']['settings']['unit']
            external_id = field['external_id']
            label = field['label']
            if return_type != 'text':
                field_info = {'label':label,
                            'external_id': external_id,
                            'return_type': return_type,
                            'unit' : unit}
                fields.append(field_info)
            print(f"{field['external_id']} -  {field ['label']} - {return_type} - {unit})")
    return fields