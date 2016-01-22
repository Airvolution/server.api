import json
import requests
import xml.etree.ElementTree as ET
import sys
import platform
import imp
print "NEW CODE EXECUTION" 
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
 
index = 5
 
r = requests.get(apiUrls[index])
print "Status: " + str(r.status_code)

f = open(r'utah.xml','w')
f.write(r.text)
f.close()

tree = ET.parse('utah.xml')
root = tree.getroot()

print root

state = root.find('state').text
print state

site = root.find('site')
print site

name = site.find('name').text
print name

#data = site.find('data')
#print data
#
#date = data.find('date')
#print date.text


for data in root.iter('data'):
	for item in data:
		if item.text is not None:
			print item.tag + ": " + item.text
	print ""

#for data in site.findall('data'):
#	date = data.get('date')
#	print date






