using UnityEngine;

/*
 * InnovationNumber Class
 * Description : This abstract class is used for innovation number properties
*/
[System.Serializable]
public abstract class InnovationNumber
{
    //Innovation Number
    [SerializeField]
    int iNumber; 

    //Get the innovation number
    public int GetInnovationNumber()
    {
        return iNumber;
    }

    //Set innovation number
    public void SetInnovationNumber(int value)
    {
        iNumber = value;
    }
}
