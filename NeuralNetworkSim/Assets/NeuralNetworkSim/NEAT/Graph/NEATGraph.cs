using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * NEATGraph Class
 * Description : Handles the rendering of the NEAT graph
*/
public class NEATGraph : MonoBehaviour
{
    //Singleton to the NEAT graph
    public static NEATGraph instance;

    //Current neural network
    NeuralNetwork nNetwork;
    //Current actor selected
    Actor currentActor;

    //Input node image
    [SerializeField]
    Image inputImage;
    //Output node image
    [SerializeField]
    Image outputImage;
    //Hidden node image
    [SerializeField]
    Image hiddenImage;
    //Line GameObject
    [SerializeField]
    GameObject lineConnection;
    //Text on graph
    [SerializeField]
    Text graphText;

    //Graph positions
    [SerializeField]
    float graphX = 500.0f;
    [SerializeField]
    float graphY = 180.0f;
    [SerializeField]
    float graphYOffset = 60.0f;

    //List of elements in the graph
    Dictionary<int, Image> graphElements = new Dictionary<int, Image>();

    void Awake()
    {
        //Set singleton
        instance = this;
    }

    void Start()
    {
        //Set the current neural network
        nNetwork = NeuralNetwork.instance;
    }

    //Is the graph current active
    public bool IsGraphDisplayed()
    {
        return graphElements.Count > 0;
    }

    //Get the graph X
    public float GetGraphX()
    {
        return graphX;
    }

    //Get the graph Y
    public float GetGraphY()
    {
        return graphY;
    }

    //Get graph offset
    public float GetGraphYOffset()
    {
        return graphYOffset;
    }

    //Clear the graph
    public void ClearDisplay()
    {
        foreach(KeyValuePair<int, Image> img in graphElements)
        {
            Destroy(img.Value.gameObject);
        }

        graphElements.Clear();
    }

    //Set graph text position
    void GraphTextPosition(Text text, float x, float y, Transform tParent)
    {
        // set parent to get correct position
        text.transform.SetParent(transform); 
        text.rectTransform.anchoredPosition = new Vector2(x, y);
        // set correct parent
        text.transform.SetParent(tParent); 
    }

    //Render the node
    void RenderNode(Image imageType, Node node)
    {
        //Create image game object
        Image nImage = Instantiate(imageType);
        nImage.transform.SetParent(transform);
        nImage.rectTransform.anchoredPosition = new Vector2(node.GetX(), node.GetY());

        //Create description object
        Text nDescription = Instantiate(graphText);
        GraphTextPosition(nDescription, node.GetX(), node.GetY(), nImage.transform);
        nDescription.text = nNetwork.GetNodeName(node.GetInnovationNumber()) + node.GetActivationFunction().ToString();

        //Add to element list
        graphElements.Add(node.GetInnovationNumber(), nImage);
    }

    //Render current actors genome
    public void RenderGenome(Actor actor)
    {
        //Set the current actor
        currentActor = actor;
        //Get the genome
        Genome genome = actor.GetGenome();

        //Loop through each of the genomes nodes
        foreach (Node node in genome.GetNodes())
        {
            int iNum = node.GetInnovationNumber();

            if (!graphElements.ContainsKey(iNum))
            {
                if (nNetwork.IsInputNode(iNum))
                {
                    //Render input node
                    RenderNode(inputImage, node);
                }
                else if (nNetwork.IsOutputNode(iNum))
                {
                    //Render output node
                    RenderNode(outputImage, node);
                }
                else
                {
                    //Render hidden node
                    RenderNode(hiddenImage, node);
                }
            }
        }

        //Loop through genome connections
        foreach(Connection conn in genome.GetConnections())
        {
            //Only render enabled connections
            if (!conn.IsEnabled())
            {
                continue;
            }

            //Get the start and end nodes
            Node start = conn.GetStart();
            Node end = conn.GetEnd();

            Image nodeImage;
            //Ensure connection is not already displayed
            if (graphElements.TryGetValue(start.GetInnovationNumber(), out nodeImage))
            {
                //Create line gameobject
                GameObject lineObj = Instantiate(lineConnection);
                lineObj.transform.SetParent(nodeImage.transform);
                LineRenderer connLine = lineObj.GetComponent<LineRenderer>();

                //Setup line position based on screen position
                Vector3[] linePosition =
                {
                    Camera.main.ScreenToWorldPoint(new Vector3(start.GetX(), start.GetY(), 0) + new Vector3(Screen.width/2,Screen.height/2, 0))
                    + new Vector3(0, 0, -Camera.main.transform.position.z - 1),
                    Camera.main.ScreenToWorldPoint(new Vector3(end.GetX(), end.GetY(), 0) + new Vector3(Screen.width/2,Screen.height/2, 0))
                    + new Vector3(0, 0, -Camera.main.transform.position.z - 1)
                };
                connLine.positionCount = 2;
                connLine.SetPositions(linePosition);
                connLine.startWidth = 0.10f;
                connLine.endWidth = 0.10f;

                //Display weight text
                Vector3 weightTextPos = Vector3.Lerp(new Vector3(start.GetX(), start.GetY(), 0), new Vector3(end.GetX(), end.GetY(), 0), 0.5f);
                Text weightText = Instantiate(graphText);
                GraphTextPosition(weightText, weightTextPos.x, weightTextPos.y, nodeImage.transform);
                weightText.text = conn.GetWeight().ToString();
            }
        }
    }

    //Mutate button function
    public void MutateButton()
    {
        //Mutate the genome
        currentActor.GetGenome().Mutate();
        //Clear display
        ClearDisplay();
        //Render updated genome
        RenderGenome(currentActor);
    }
}
