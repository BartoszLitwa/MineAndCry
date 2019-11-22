using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartItem : MonoBehaviour
{
    [SerializeField] private GameObject HeartPanel;
    [SerializeField] private GameObject FullHeartImg;
    [SerializeField] private GameObject HalfheartImg;

    public bool Fullheart { get; private set; }

    public HeartItem(bool _FullHeart)
    {
        Fullheart = _FullHeart;
    }

    public void HeartStateChanged(bool _FullHeart, bool ShoudlDisplayThisheart = true)
    {
        Fullheart = _FullHeart;
        if (!ShoudlDisplayThisheart)
        {
            HeartPanel.SetActive(false);
            FullHeartImg.SetActive(false);
            HalfheartImg.SetActive(false);
            return;
        }

        if (Fullheart)
        {
            FullHeartImg.SetActive(true);
            HalfheartImg.SetActive(false);
        }
        else
        {
            FullHeartImg.SetActive(false);
            HalfheartImg.SetActive(true);
        }
    }
}
