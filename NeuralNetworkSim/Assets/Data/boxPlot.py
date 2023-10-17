import matplotlib.pyplot as plt
import csv

#Setup file names
name = "WeightAdjust"
fileName = "Fitness"
experiments = ["0.10", "0.25", "0.50", "0.75", "0.90"]

#Label the tables
plt.xlabel(name + ' Value')
plt.ylabel(fileName)
plt.title(fileName + ' Box Plot: ' + name)

#Setup 2D array for boxplot data
data = [[] for i in range(len(experiments))]

#Loop through each experiment
for exp in range(0, len(experiments)):

    #Setup load path
    loadPath = experiments[exp] + ' ' + name + "/" + experiments[exp] + ' ' + name + '_' + fileName

    #Include base results
    if (experiments[exp] == "0.50"):
        loadPath = "Base/" + "/" + "base_" + fileName

    #Open the results file
    with open('Results/' + loadPath + '.csv','r') as csvfile:
        plots = csv.reader(csvfile, delimiter = ';')
        for row in plots:
            data[exp].append(int(row[1]))
  
    #Add the boxplot data
    plt.boxplot(data)

#Label the elements
plt.xticks([1, 2, 3, 4, 5], experiments)
plt.show()

