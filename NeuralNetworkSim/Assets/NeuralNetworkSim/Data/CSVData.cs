using System.Collections.Generic;

/*
 * CSVData
 * Description : Used for converting string data to a CSV format
*/
public class CSVData
{
    //Data to change into CSV
    List<string> data = new List<string>();

    //New data for the current line
    public void NewData(string newData)
    {
        data.Add(newData + ";");
    }

    //Set ending data of the line
    public void NewLine(string newData)
    {
        data.Add(newData + "\n");
    }

    //Ouput to CSV format
    public override string ToString()
    {
        string result = "";

        foreach (string d in data)
        {
            result += d;
        }

        return result;
    }
}