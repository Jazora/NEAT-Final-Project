import matplotlib.pyplot as plt
import statistics
import csv

#Setup file names
name = "Mutations"
fileName = "Generations"
experiments = ["0.10", "0.25", "0.50", "0.75", "0.90"]

#Label the graph
plt.xlabel(name + ' Value')
plt.ylabel('Average ' + fileName)
plt.title(fileName + ' Standard Deviation: ' + name)

#Run through all experiment types
for exp in experiments:
    x = []
    y = []
    e = []

    #Path to load data from
    loadPath = exp + ' ' + name + "/" + exp + ' ' + name + '_' + fileName

    #Include base results
    if (exp == "0.50"):
        loadPath = "Base/" + "/" + "base_" + fileName

    #Open the file
    with open('Results/' + loadPath + '.csv','r') as csvfile:
        plots = csv.reader(csvfile, delimiter = ';')

        x.append(exp)
        
        values = []
        for row in plots:
            values.append(int(row[1]))

        #Plot the mean
        y.append(statistics.mean(values))
        #Plot the standard deviation
        e.append(statistics.stdev(values))
  
    #Plot the data
    plt.errorbar(x, y, e, capsize=4, linestyle='None', marker='.', markersize=9)

#Show legend
plt.legend()
plt.show()