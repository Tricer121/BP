
import sys
import os

filePath = sys.argv[1].removesuffix(".osm")+"copy.txt"

originalFile = open(sys.argv[1], "r", encoding="utf8")
file = open(os.getcwd() + "\\osmnodes.txt", "w+",encoding="utf8")

array = []
for line in originalFile:
    if "node" not in line:
        continue
    if "lat" in line:
        part = line.split('lat="')[1]
        lat=part.split('" ')[0]
        lon=part.split(' lon="')[1]
        lon=lon.split('"')[0]
        file.write("{}".format(lat))
        file.write(",")
        file.write("{}".format(lon))
        file.write(",\n")
    

