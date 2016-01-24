import json
import requests
import xml.etree.ElementTree as ET
import sys
import platform
import imp
import jsonpickle
import time
from datetime import datetime

print "NEW CODE EXECUTION" 

index = 5

class PollutantQuery(object):
  def __init__(self, date, pollutant_name, value):
    self.deviceID = 'UT-' + str(index)
    self.date = date
    self.pollutant_name = pollutant_name
    self.value = value

def unix_time_millis(dt):
  return (dt - datetime.utcfromtimestamp(0)).total_seconds() * 1000.0
  
apiUrls = [		"http://air.utah.gov/xmlFeed.php?id=boxelder",   
			    "http://air.utah.gov/xmlFeed.php?id=cache",     
			    "http://air.utah.gov/xmlFeed.php?id=p2",        
			    "http://air.utah.gov/xmlFeed.php?id=bv",        
			    "http://air.utah.gov/xmlFeed.php?id=rs",        
			    "http://air.utah.gov/xmlFeed.php?id=slc",       
			    "http://air.utah.gov/xmlFeed.php?id=tooele",    
			    "http://air.utah.gov/xmlFeed.php?id=v4",        
			    "http://air.utah.gov/xmlFeed.php?id=utah",      
			    "http://air.utah.gov/xmlFeed.php?id=washington",
			    "http://air.utah.gov/xmlFeed.php?id=weber"      
]

r = requests.get(apiUrls[index])
print "Status: " + str(r.status_code)

f = open(r'utah.xml','w')
f.write(r.text)
f.close()

pollutants = []

def add_to_pollutants_switch(pollutant_name):
  return {
    'bp' : 'BAROMETRIC_PRESSURE',
    'co' : 'CARBON_MONOXIDE',
    'nox' : 'NITROGEN_OXIDE',
    'no2' : 'NITROGEN_DIOXIDE',
    'ozone' : 'OZONE',
    'pm25' : 'PM2_5',
    'pm10' : 'PM10',
    'relative_humidity' : 'RELATIVE_HUMIDITY',
    'solar_radiation' : 'SOLAR_RADIATION',
    'so2' : 'SULFUR_DIOXIDE',
    'temperature' : 'TEMPERATURE',
    'wind_speed' : 'WIND_SPEED',
    'wind_direction' : 'WIND_DIRECTION'
  }.get(pollutant_name, -1)

def add_to_pollutants(dt, pollutant_name, pollutant_value):
  pollutant_name_correct = add_to_pollutants_switch(pollutant_name)
  if pollutant_name_correct != -1:
    print str(dt) + ": " + pollutant_name_correct + ": " + pollutant_value
    pollutants.append(PollutantQuery(unix_time_millis(dt), pollutant_name_correct, pollutant_value))
  return

tree = ET.parse('utah.xml')
root = tree.getroot()

for data in root.find('site').findall('data'):
  dt = datetime.strptime(data.find('date').text, "%m/%d/%Y %H:%M:%S")
  for item in data:
    if item.text is not None and item.tag != 'date':
      add_to_pollutants(dt, item.tag, item.text)
  print ""

f = open(r'out.json','w')
f.write(jsonpickle.encode(pollutants, unpicklable=False))
f.close()





