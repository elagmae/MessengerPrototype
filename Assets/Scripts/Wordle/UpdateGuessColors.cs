using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateGuessColors : MonoBehaviour
{
    [SerializeField]
    private GameObject _guessContainer;
    public List<Word> Lines { get; private set; } = new();

    private void Awake()
    {
        HorizontalLayoutGroup[] lines = _guessContainer.GetComponentsInChildren<HorizontalLayoutGroup>();
        foreach(HorizontalLayoutGroup line in lines)
        {
            List<Image> imgs = line.GetComponentsInChildren<Image>().ToList();
            imgs.Remove(line.GetComponent<Image>());
            List<TextMeshProUGUI> tmps = new();

            foreach (Image image in imgs) 
            {
                tmps.Add(image.GetComponentInChildren<TextMeshProUGUI>());
            }

            Lines.Add(new Word
            {
                Images = imgs.ToList(),
                Tmps = tmps
            });
        }
    }

    public void UpdateColors(string colors, int line)
    {
        print(colors + " on line " + line);
        for (int i = 0; i < colors.Length; i++)
        {
            Color color = Color.white;
            switch (colors[i])
            {
                case 'G':
                    color = Color.green;
                    break;

                case 'Y':
                    color = Color.yellow;
                    break;

                case 'R':
                    color = Color.red;
                    break;
            }

            Lines[line].Images[i].color = color;
        }

    }
}
