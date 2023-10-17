import matplotlib.pyplot as plt
import csv

#Name of the data
name = "Base"

#Label the plot graph
plt.xlabel('Generation')
plt.ylabel('Fitness')
plt.title(name + ' Parameters: Fitness History')

#Run through each instance
for exp in range(0, 15):
    generation = []
    data = []

    #Open the files
    with open('Results/' + name + '/' + str(exp) + '_generationhistory.csv','r') as csvfile:
        plots = csv.reader(csvfile, delimiter = ';')
      
        for row in plots:
            generation.append(row[0])
            data.append(int(row[1]))
    
    #Plot the data
    plt.plot(generation, y, label = str(exp) + " Instance")

#Show the legend
plt.legend()
plt.show()

