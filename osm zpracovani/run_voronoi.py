from scipy.spatial import Voronoi
import sys
import os
originalFile = open(sys.argv[1], "r", encoding="utf8")

voronoiPath = "./"+os.path.splitext(originalFile.name)[0]+"_regions.txt"
voronoiFile = open(voronoiPath, "w", encoding="utf8")

inputPoints = []
for line in originalFile:
    inputPoints.append([float(line.split(",")[0]),float(line.split(",")[1])])
voronoi = Voronoi(inputPoints)


for l in range(len(inputPoints)):
    point = voronoi.points[l]
    voronoiFile.write(str(point[1])+";"+str(point[0])+";'")
    regionIndex = voronoi.point_region[l]
    last = voronoi.vertices[voronoi.regions[regionIndex]][0]
    for verticeIndex in voronoi.regions[regionIndex]:
        vertex = voronoi.vertices[verticeIndex]
        voronoiFile.write("["+str(vertex[1])+","+str(vertex[0])+"],")
    voronoiFile.write("["+str(last[1])+","+str(last[0])+"]'")
    voronoiFile.write("\n")