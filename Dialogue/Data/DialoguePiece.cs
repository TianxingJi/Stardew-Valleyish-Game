using UnityEngine;

namespace TFarm.Dialogue
{
    [System.Serializable]
    public class DialoguePiece
    {
        [Header("Dialogue Information")]
        public Sprite faceImage;
        public bool onLeft;
        public string name;

        [TextArea]
        public string dialogueText;
        public bool hasToPause;
        [HideInInspector]public bool isDone;
    }
}