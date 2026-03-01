using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

[ExecuteInEditMode]//编辑的模式下进行运行
public class GridMap : MonoBehaviour
{
    public MapData_SO mapData;
    public GridType gridType;
    private Tilemap currentTilemap;
    //组件启动
    private void OnEnable()
    {
        //是否在运行
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();

            if (mapData != null)
            {
                mapData.tileProperties.Clear();
            }
        }
    }
    //组件关闭
    private void OnDisable()
    {
        if (!Application.IsPlaying(this))
        {
            currentTilemap = GetComponent<Tilemap>();

            UpdateTileProperties();
#if UNITY_EDITOR
            if (mapData != null)
            {
                EditorUtility.SetDirty(mapData);
            }
#endif
        }
    }

    private void UpdateTileProperties()
    {
        currentTilemap.CompressBounds();

        if (!Application.IsPlaying(this))
        {
            if (mapData != null)
            {
                //已绘制范围的左下角坐标
                Vector3Int startPos = currentTilemap.cellBounds.min;
                //已绘制范围的右上角坐标
                Vector3 endPos = currentTilemap.cellBounds.max;

                for (int x = startPos.x; x < endPos.x; x++)
                {
                    for (int y = startPos.y; y < endPos.y; y++)
                    {
                        TileBase tile = currentTilemap.GetTile(new Vector3Int(x, y, 0));

                        if (tile != null)
                        {
                            TileProperty newTile = new TileProperty
                            {
                                tileCoordinate = new Vector2Int(x, y),
                                gridType = this.gridType,
                                boolTypeValue = true
                            };

                            mapData.tileProperties.Add(newTile);
                        }
                    }
                }
            }
        }
    }
}
