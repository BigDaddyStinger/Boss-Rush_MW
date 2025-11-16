using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthDisplay : MonoBehaviour
{
    List<GameObject> hearts;
    [SerializeField] Sprite heart;
    [SerializeField] Sprite empty;

    Transform _transform;

    private void Awake()
    {
        hearts = new List<GameObject>();
        _transform = transform;
    }

    private void Start()
    {

    }

    public void SetMaxHearts(int maxHealth)
    {
        for (int i = 0; i < hearts.Count; i++)
            Destroy(hearts[i]);

        hearts.Clear();

        for(int i = 0; i < maxHealth; i++)
        {
            var current = new GameObject();
            var image = current.AddComponent<Image>();
            image.sprite = empty;
            image.color = Color.white;

            current.transform.SetParent(_transform);

            hearts.Add(current);
        }
    }

    public void UpdateHearts(int damageAmount, int newCurrent)
    {
        // reset to all being empty and white
        SetMaxHearts(hearts.Count);

        // ensure that only as many as we currently have are filled
        for(int i = 0; i < newCurrent; i++)
        {
            var currentHeart = hearts[i].GetComponent<Image>();
            currentHeart.sprite = heart;
            currentHeart.color = Color.white;
        }
    }
}
