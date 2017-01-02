using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour
{
    
    public GUIStyle textStyle;
    void Start()
    {
        Cursor.visible = false;
    }
    void OnGUI()
    {
        GUI.Label(new Rect(5, 5, 200, 30), string.Format("World #{0:#,###}", World.Instance.seed), textStyle);

        GUI.Label(new Rect(Screen.width - 150, 5, 100, 30), string.Format("{0:#,###} Chunks", World.Instance.displayChunkCount), textStyle);
    }

}
