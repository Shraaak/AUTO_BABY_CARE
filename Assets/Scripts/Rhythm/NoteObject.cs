using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public Transform uiCanvas;
    public bool canBePressed;
    public KeyCode keyToPress;

    public GameObject hitEffect, goodEffect, perfectEffect, missEffect;

    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            if (canBePressed)
            {
                gameObject.SetActive(false);
                RectTransform myRect = GetComponent<RectTransform>();
                float y = myRect.anchoredPosition.y;

                //RhythmManager.Instance.NoteHit();

                if( Mathf.Abs(y) > 0.25f )
                {
                    RhythmManager.Instance.NormalHit();

                    GameObject effect = Instantiate(hitEffect, uiCanvas);
                    effect.GetComponent<RectTransform>().position = myRect.position;
                }
                else if ( Mathf.Abs(y) > 0.05f )
                {
                    RhythmManager.Instance.GoodHit();

                    GameObject effect = Instantiate(goodEffect, uiCanvas);
                    effect.GetComponent<RectTransform>().position = myRect.position;
                }
                else
                {
                    RhythmManager.Instance.PerfectHit();
                    GameObject effect = Instantiate(perfectEffect, uiCanvas);
                    effect.GetComponent<RectTransform>().position = myRect.position;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Activator")
        {
            canBePressed = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag == "Activator")
        {
            canBePressed = false;

            RhythmManager.Instance.NoteMissed();

            RectTransform myRect = GetComponent<RectTransform>();
            GameObject effect = Instantiate(missEffect, uiCanvas);
            effect.GetComponent<RectTransform>().position = myRect.position;

            gameObject.SetActive(false);
        }
    }
}
