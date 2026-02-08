using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateGuessColors : MonoBehaviour
{
    [SerializeField]
    private GameObject _guessContainer;
    [SerializeField]
    private Canvas _wonUi;
    public List<Word> Lines { get; private set; } = new();
    [SerializeField]
    private ClientUpdater _client;

    private void Awake()
    {
        // Gets all the components needed to fill colors and texts (could have been done in editor, but saved me some time).
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
        if (line > 4) return;

        if(colors == "GGGGG") // if the word is the right one, we show the victory panel.
        {
            _wonUi.gameObject.SetActive(true);
            _client.EndGame = true;
        }

        for (int i = 0; i < colors.Length; i++) // We read the answer provided by the host : R = red, Y = yellow, G = green.
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
