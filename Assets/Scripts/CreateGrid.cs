using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGrid : MonoBehaviour
{
    [SerializeField]
    private GameObject _gridPrefab;

    private bool _createdGrid;

    public void Create(int width, int height) {
        if (!_createdGrid) {
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    GameObject grid = Instantiate(_gridPrefab, transform);
                    grid.transform.localPosition = new Vector3(j, i, 0f);
                }
            }
        }
    }
}
