using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;


public class InventoryGridCell
{
    /// <summary>
    /// The transform of the cell parent
    /// </summary>
    public RectTransform RectTransform;
    
    /// <summary>
    /// The renderer of the cells child 
    /// </summary>
    public Image ChildImage;
}

/// <summary>
/// Manages the visuals of the inventory grid layouts
/// </summary>
public class InventoryGridPresenter : MonoBehaviour
{
    /// <summary>
    /// How many columns and rows are there
    /// </summary>
    public Vector2Int Dimensions;

    /// <summary>
    /// The spacing between each grid cell, this is added to the CellSize
    /// </summary>
    public float Spacing;

    /// <summary>
    /// The size of each grid cell
    /// </summary>
    public float CellSize = 315;

    [SerializeField] private RectTransform _backPanelsParent;
    [SerializeField] private Sprite _backPanelSprite;
    
    /// <summary>
    /// Total width and height of the grid
    /// </summary>
    private float _width;
    private float _height;
    
    private List<InventoryGridCell> _gridCells;
    
    public void Awake()
    {
        SpawnBackingPanels();
    }
    
    /// <summary>
    /// Dynamically spawns the background grid visuals
    /// </summary>
    private void SpawnBackingPanels()
    {
        _width = (Dimensions.x * CellSize) + (Mathf.Max(0, Dimensions.x - 1) * Spacing);
        _height = (Dimensions.y * CellSize) + (Mathf.Max(0, Dimensions.y - 1) * Spacing);

        float halfWidth = _width / 2.0f;
        float halfHeight = _height / 2.0f;
        
        float halfCellSize = CellSize / 2.0f;

        _gridCells = new();
        
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                Vector2Int gridCell = new(x, y);

                GameObject backingPanel = new($"BackingPanel({gridCell.x}, {gridCell.y})", typeof(RectTransform));
                
                RectTransform rectTransform = backingPanel.GetComponent<RectTransform>();
                rectTransform.SetParent(_backPanelsParent);
                
                // Set the position and size
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(CellSize, CellSize);
                rectTransform.anchoredPosition3D = new Vector3(
                    (x * (CellSize + Spacing)) - halfWidth + halfCellSize, 
                    -((y * (CellSize + Spacing)) - halfHeight + halfCellSize),
                    _backPanelsParent.anchoredPosition3D.z
                );
               
                // Add background sprite
                
                var spriteRenderer = backingPanel.AddComponent<Image>();
                spriteRenderer.sprite = _backPanelSprite;
                spriteRenderer.color = Helpers.Colours.UiInventoryGridBackingPanel;

                // Add cell to list
                
                InventoryGridCell cell = new();
                cell.RectTransform = rectTransform;
                cell.ChildImage = CreateChildImage(rectTransform);
                cell.ChildImage.enabled = false;
                
                _gridCells.Add(cell);
            }
        }
    }
    
    /// <summary>
    /// Creates an image renderer for a specific grid cell
    /// </summary>
    private Image CreateChildImage(RectTransform parentCell)
    {
        GameObject childRenderer = new($"ChildRenderer", typeof(RectTransform));
        childRenderer.transform.SetParent(parentCell);
        childRenderer.transform.localPosition = Vector3.zero;
        
        return childRenderer.AddComponent<Image>();
    }
    
    /// <summary>
    /// Converts a (x,y) coordinate to an index
    /// Used to access elements in the gridTransforms list
    /// </summary>
    private int CoordinateToIndex(Vector2Int coordinate)
    {
        return coordinate.x + coordinate.y * Dimensions.x;
    }
    
    /// <summary>
    /// Set the sprite of a specific cell
    /// Note: Sprite can be set to null to clear the cell
    /// </summary>
    public void AssignSpriteToCell(Vector2Int coordinate, [CanBeNull] Sprite sprite)
    {
        var listIndex = CoordinateToIndex(coordinate);
        AssignSpriteToCell(listIndex, sprite);
    }
    public void AssignSpriteToCell(int index, [CanBeNull] Sprite sprite)
    {
        _gridCells[index].ChildImage.GetComponent<RectTransform>().sizeDelta = new Vector2(CellSize, CellSize);
        _gridCells[index].ChildImage.sprite = sprite;
        
        // Disable if the sprite is null
        _gridCells[index].ChildImage.enabled = sprite != null;
    }

}
