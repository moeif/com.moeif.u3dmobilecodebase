using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using DG.Tweening;

/// <summary>
/// 功能:
/// 1. 左右、上下滑动
/// 2. 支持多行，或多列
/// 3. 支持不定高度或宽度
/// 4. Cell Lua层渲染
/// </summary>
public class DynamicScrollView : MonoBehaviour
{
    #region Public
    public GameObject prefab;
    [HideInInspector]
    public float itemWidth = 0.0f;
    [HideInInspector]
    public float itemHeight = 0.0f;
    public float ySpacing = 0.0f;
    public float xSpacing = 0.0f;
    public float xStartOffset = 0.0f;
    public bool alignCenter = false;
    public int AddWidth = 0;
    public int additionalRender = 2;

    public Action<int, GameObject> OnFill = delegate { };
    #endregion

    #region 居中对齐用到的变量
    private float alignCenterOfXOffset = 0.0f;
    #endregion

    private ScrollRect scrollRect;
    private RectTransform contentRectTrans;
    private bool isDirty = false;
    private Vector3 lastRectTransAnchorPos = Vector3.zero;
    private int dataCount = 0;
    private Dictionary<int, DynamicScrollViewCell> renderedItemIndexDict = new Dictionary<int, DynamicScrollViewCell>();
    private List<int> tmpCacheIdList = new List<int>();
    private float scrollViewWidth = 0.0f;
    private float scrollViewHeight = 0.0f;
    private Queue<DynamicScrollViewCell> pool = new Queue<DynamicScrollViewCell>();
    private RectTransform scrollViewRectTrans;
    private int col = 1;
    private int row = 1;

    private int maxRowInVerticle = 0;
    private int maxColInHorizontal = 0;

    private bool isHorizontalExpand = false;

    // Content 上下滑动时Y最小和最大位置
    private float minContentY = 0.0f;
    private float maxContentY = 0.0f;

    private float currStartRenderY = 0.0f;
    private float currEndRenderY = 0.0f;

    private float minContentX = 0.0f;
    private float maxContentX = 0.0f;

    private float currStartRenderX = 0.0f;
    private float currEndRenderX = 0.0f;


   
    public void InitData(int count, bool toPosZero = true)
    {
        RectTransform cellRectTrans = prefab.GetComponent<RectTransform>();
        itemWidth = cellRectTrans.sizeDelta.x;
        itemHeight = cellRectTrans.sizeDelta.y;

        scrollViewWidth = scrollViewRectTrans.rect.width; //sizeDelta.x;
        scrollViewHeight = scrollViewRectTrans.rect.height; //sizeDelta.y;

        Debug.LogFormat("PaintingUILuaScrollView Width: {0} Height: {1}", scrollViewWidth, scrollViewHeight);

        this.dataCount = count;
        isDirty = true;


        // 设置滑动区域Content大小
        if (scrollRect.vertical)
        {
            col = (int)((scrollViewWidth - itemWidth) / (itemWidth + xSpacing));
            col = Mathf.Clamp(col, 0, col);
            col += 1;

            if (alignCenter)
            {
                float useSize = col * itemWidth + (col - 1) * xSpacing;
                float spaceSize = scrollViewWidth - useSize;
                if (spaceSize > 0)
                {
                    alignCenterOfXOffset = spaceSize / 2.0f;
                }
                else
                {
                    Debug.LogErrorFormat("逻辑错误，Item列占用已经超过ScrollView宽度");
                }
            }

            int totalRow = (int)(count / col);
            totalRow += count % col == 0 ? 0 : 1;

            float contentHeight = totalRow * (itemHeight + ySpacing) - ySpacing;
            contentRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            contentRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollViewWidth);
            maxRowInVerticle = (int)(scrollViewHeight / (itemHeight + ySpacing)) + 1;

            minContentY = 0.0f;
            maxContentY = contentHeight - scrollViewHeight;
        }
        else
        {
            row = (int)((scrollViewHeight - itemHeight) / (itemHeight + ySpacing));
            row = Mathf.Clamp(row, 0, row);
            row += 1;
            int totalCol = (int)(count / row);
            totalCol += count % row == 0 ? 0 : 1;

            float contentWidth = totalCol * (itemWidth + xSpacing) - xSpacing + xStartOffset + AddWidth;
            contentRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
            contentRectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollViewHeight);
            maxColInHorizontal = (int)(scrollViewWidth / (itemWidth + xSpacing)) + 1;

            minContentX = 0.0f;
            maxContentX = contentWidth - scrollViewWidth;
        }

        CacheRenderedItems();

        if (toPosZero)
        {
            scrollRect.enabled = false;
            scrollRect.enabled = true;
            contentRectTrans.anchoredPosition3D = Vector3.zero;
        }

        // 这里不要立刻调用，或许会设置位置
        //Refresh();
    }

    public void ScrollTo(int index)
    {
        if (scrollRect.vertical)
        {
            ScrollOfVerticle(index, 0);
        }
        else
        {
            ScrollOfHorizontal(index, 0);
        }
    }

    public void ScrollTo(Vector2 anchorPos, float duration = 0.0f)
    {
        anchorPos.y = Mathf.Clamp(anchorPos.y, minContentY, maxContentY);
        if(duration > 0)
        {
            contentRectTrans.DOAnchorPos(anchorPos, duration);
        }
        else
        {
            contentRectTrans.anchoredPosition = anchorPos;
        }
    }

    public void ScrollToWithDuration(int index, float duration)
    {
        if (scrollRect.vertical)
        {
            ScrollOfVerticle(index, duration);
        }
        else
        {
            ScrollOfHorizontal(index, duration);
        }
    }

    private void ScrollOfHorizontal(int index, float duration)
    {
        float startXPos = GetItemStartXPos(index);
        if (startXPos >= currStartRenderX && startXPos <= (currEndRenderX - (itemWidth + xSpacing)))
        {
            return;
        }

        float targetMiddlePos = startXPos - (scrollViewWidth / 2.0f);
        targetMiddlePos = Mathf.Clamp(targetMiddlePos, minContentX, maxContentX);
        Vector3 targetAnchorPos3D = new Vector3(-targetMiddlePos, 0, 0);
        contentRectTrans.DOAnchorPos3D(targetAnchorPos3D, duration);
    }

    private void ScrollOfVerticle(int index, float duration)
    {
        float startYPos = GetItemStartYPos(index);
        if (startYPos >= currStartRenderY && startYPos <= (currEndRenderY - (itemHeight + ySpacing)))
        {
            // 在显示范围内，不需要调整滑动区域
            return;
        }


        float targetMiddlePos = startYPos - (scrollViewHeight / 2.0f);
        targetMiddlePos = Mathf.Clamp(targetMiddlePos, minContentY, maxContentY);

        Vector3 targetAnchorPos3D = new Vector3(0, targetMiddlePos, 0);

        contentRectTrans.DOAnchorPos3D(targetAnchorPos3D, duration);
    }


    private void Awake()
    {
        scrollRect = gameObject.GetComponent<ScrollRect>();
        contentRectTrans = scrollRect.content;
        scrollViewRectTrans = scrollRect.GetComponent<RectTransform>();
        //prefab.gameObject.SetActive(false);
    }


    private void Update()
    {
        if (dataCount <= 0)
        {
            return;
        }

        if (scrollRect.vertical)
        {
            float yDiff = Mathf.Abs(contentRectTrans.anchoredPosition3D.y - lastRectTransAnchorPos.y);
            if (yDiff > 10)
            {
                lastRectTransAnchorPos = contentRectTrans.anchoredPosition3D;
                isDirty = true;
            }
        }
        else
        {
            float xDiff = Mathf.Abs(contentRectTrans.anchoredPosition3D.x - lastRectTransAnchorPos.x);
            if (xDiff > 10)
            {
                lastRectTransAnchorPos = contentRectTrans.anchoredPosition3D;
                isDirty = true;
            }
        }
        _Refresh();
    }

    public void Refresh(bool dataChanged = false)
    {
        isDirty = true;
        _Refresh(dataChanged);
    }

    private void _Refresh(bool dataChanged = false)
    {
        if (!isDirty)
        {
            return;
        }

        isDirty = false;


        tmpCacheIdList.Clear();

        if (scrollRect.vertical)
        {
            ScrollVerticle(dataChanged);
        }
        else
        {
            ScrollHorizontal(dataChanged);
        }

        for (int i = 0; i < tmpCacheIdList.Count; ++i)
        {
            int index = tmpCacheIdList[i];
            CacheItem(index);
        }
        tmpCacheIdList.Clear();
    }

    private void ScrollHorizontal(bool dataChanged= false)
    {
        float leftPosX = contentRectTrans.anchoredPosition.x;
        currStartRenderX = leftPosX;
        currEndRenderX = currStartRenderX + scrollViewWidth;

        leftPosX = Mathf.Clamp(leftPosX, leftPosX, 0);
        leftPosX = Mathf.Abs(leftPosX);

        int startCol = (int)(leftPosX / (itemWidth + xSpacing));
        startCol -= 1;
        startCol = Mathf.Clamp(startCol, 0, startCol);

        int endCol = startCol + maxColInHorizontal + 1;

        int startDataIndex = startCol * row;
        int endDataIndex = endCol * row + (row - 1);

        tmpCacheIdList.Clear();
        var enumer = renderedItemIndexDict.GetEnumerator();
        while (enumer.MoveNext())
        {
            int index = enumer.Current.Key;
            if (index < startDataIndex || index > endDataIndex)
            {
                tmpCacheIdList.Add(index);
            }
        }

        CacheItems();

        for (int i = startDataIndex; i <= endDataIndex; ++i)
        {
            if (i < this.dataCount)
            {
                int itemCol = (int)(i / row);
                int itemRow = i % row;
                Vector3 itemPos = new Vector3(itemCol * (itemWidth + xSpacing) + xStartOffset, -itemRow * (itemHeight + ySpacing), 0);
                ShowItem(i, itemPos, dataChanged);
            }
            else
            {
                break;
            }
        }

    }

    /// <summary>
    /// 只适合于上下滑动的模式
    /// </summary>
    /// <param name="index"></param>
    private float GetItemStartYPos(int index)
    {
        float rowHeight = itemHeight + ySpacing;
        int rowIndex = (int)(index / col);
        return rowIndex * rowHeight;
    }

    private float GetItemStartXPos(int index)
    {
        float colWidth = itemWidth + xSpacing;
        int colIndex = (int)(index / row);
        return colIndex * colWidth;
    }

    private void ScrollVerticle(bool dataChanged = false)
    {
        float upPosY = contentRectTrans.anchoredPosition.y;
        currStartRenderY = upPosY;
        currEndRenderY = currStartRenderY + scrollViewHeight;
        upPosY = Mathf.Clamp(upPosY, 0, upPosY);

        int startRow = (int)(upPosY / (itemHeight + ySpacing));
        startRow -= 1;
        startRow = Mathf.Clamp(startRow, 0, startRow);

        int endRow = startRow + maxRowInVerticle + additionalRender;

        int startDataIndex = startRow * col;
        int endDataIndex = endRow * col + (col - 1);

        tmpCacheIdList.Clear();
        var enumer = renderedItemIndexDict.GetEnumerator();
        while (enumer.MoveNext())
        {
            int index = enumer.Current.Key;
            if (index < startDataIndex || index > endDataIndex)
            {
                tmpCacheIdList.Add(index);
            }
        }

        CacheItems();

        for (int i = startDataIndex; i <= endDataIndex; ++i)
        {
            if (i < this.dataCount)
            {
                int itemRow = (int)(i / col);
                int itemCol = i % col;
                Vector3 itemPos = new Vector3(itemCol * (itemWidth + xSpacing), -itemRow * (itemHeight + ySpacing), 0);

                if (alignCenter)
                {
                    itemPos.x += alignCenterOfXOffset;
                }

                ShowItem(i, itemPos, dataChanged);
            }
            else
            {
                break;
            }
        }
    }

    private void CacheRenderedItems()
    {
        tmpCacheIdList.Clear();
        var enumer = renderedItemIndexDict.GetEnumerator();
        while (enumer.MoveNext())
        {
            int index = enumer.Current.Key;
            tmpCacheIdList.Add(index);
        }
        CacheItems();
    }


    private void CacheItems()
    {
        for (int i = 0; i < tmpCacheIdList.Count; ++i)
        {
            int index = tmpCacheIdList[i];
            CacheItem(index);
        }
    }

    private void CacheItem(int index)
    {
        if (renderedItemIndexDict.ContainsKey(index))
        {
            DynamicScrollViewCell cell = renderedItemIndexDict[index];
            renderedItemIndexDict.Remove(index);
            cell.UpdatePosition(new Vector2(20000, 0));
            pool.Enqueue(cell);
            //cell.gameObject.SetActive(false);
            //GameObject.Destroy(cell.gameObject);
        }
    }

    private void ShowItem(int index, Vector3 anchorPos3D, bool dataChanged)
    {
        if (renderedItemIndexDict.ContainsKey(index))
        {
            if (dataChanged)
            {
                OnFill?.Invoke(index, renderedItemIndexDict[index].gameObject);
            }
            //renderedItemIndexDict[index].UpdatePosition(anchorPos3D);
        }
        else
        {
            DynamicScrollViewCell nCell;
            if (pool.Count == 0)
            {
                GameObject newItem = Instantiate(prefab);
                nCell = newItem.GetComponent<DynamicScrollViewCell>();
                newItem.transform.SetParent(contentRectTrans);
                newItem.transform.localScale = Vector3.one;
                RectTransform rectTrans = newItem.GetComponent<RectTransform>();
                nCell.SetRectTrans(rectTrans);
                //newItem.SetActive(true);
            }
            else
            {
                nCell = pool.Dequeue();
                //nCell.gameObject.SetActive(true);
            }

            renderedItemIndexDict.Add(index, nCell);

            //nCell.UpdateContent(index);
            nCell.UpdatePosition(anchorPos3D);
            OnFill?.Invoke(index, nCell.gameObject);
        }
    }




    private void GetItemRect(int index, out float width, out float height)
    {
        width = itemWidth;
        height = itemHeight;
    }


}
