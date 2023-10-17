using UnityEngine;

/*
 * TriggerSmell Class
 * Description : The area around the slime that detects food and updates food sensed,
 * designed for new types to be included
*/
public class TriggerSmell : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case "Food":
                GetComponentInParent<ActorSlime>().AddSensedFood(collider.gameObject);
                break;
            default:
                break;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case "Food":
                GetComponentInParent<ActorSlime>().RemoveSensedFood(collider.gameObject);
                break;
            default:
                break;
        }
    }
}