import matplotlib.pyplot as plt
import csv

#Name of the data
name = "Nodes"
#Experiment types
experiments = ["0.10", "0.25", "0.50", "0.75", "0.90"]
#Filename types
fileNames = ["Mutations", "Species", "WeightAdjust"]

#Label the plot graph
plt.xlabel('Simulation Instance')
plt.ylabel(name + ' Count')
plt.title(name + ' Count: Simulation Instance History')

#Run through each filename
for fileName in fileNames:
    #Run through each instance
    for exp in experiments:
        instance = []
        data = []

        #Setup load path
        loadPath = exp + ' ' + fileName + "/" + exp + ' ' + fileName + '_' + name + 'count'

        #Include base results
        if (exp == "0.50"):
            loadPath = "Base/" + "/" + "base_" + name + 'count'

        #Open the files
        with open('Results/' + loadPath + '.csv','r') as csvfile:
            plots = csv.reader(csvfile, delimiter = ';')
      
            for row in plots:
                instance.append(row[0])
                data.append(int(row[1]))
    
        #Plot the data
        plt.plot(instance, data, label = fileName + exp)


#Show the legend
plt.legend()
plt.show()

