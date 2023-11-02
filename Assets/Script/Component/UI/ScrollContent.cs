using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Common;

public abstract class ScrollContent : CommonBehaviour
{
    public const int ITEM_EXTRA_COUNT = 1;

    [System.Serializable]
    public enum ScrollType
    {
        Vertical = 0,
        Horizontal
    }

    [System.Serializable]
    public struct ItemMargin
    {
        public float left;
        public float top;
        public float right;
        public float bottom;
    }


    public ScrollRect scrollRect;
    public bool _hideItemPrefab = true;
    public GameObject itemPrefab;

    public ScrollType scrollType = ScrollType.Vertical;

    [Range(1, 100)]
    public int lineCount = 1;

    public float itemScale = 1f;

    public ItemMargin itemMargin = new ItemMargin();

    public RectTransform contentTransform { get; protected set; }

    public abstract int itemCount { get; set; }

    public abstract void Reset();
    public abstract void TryItemUpdate(int _item_index);
    public abstract void TryListUpdate(int _item_count);
    public abstract void TryListUpdate();

    public abstract int GetInstanceObjectIndex(GameObject _item);
    public abstract GameObject GetInstanceObject(int _item_index);
}


public class ScrollContent<T> : ScrollContent where T : Component
{
    public System.Action<int, T> eventItemUpdate; //p1:index, p2:Component

    private readonly List<T> _instanceList = new List<T>();
    private readonly List<int> _itemIndexTable = new List<int>();

    private RectTransform _viewportTransform;

    private int _curLeftTopIndex = 0;
    private int _curRightBottomIndex = 0;
    private float _scrollArea_Width = 1.0f;
    private float _scrollArea_Height = 1.0f;

    private int _viewRowItemCount = 0;
    private int _viewColumnItemCount = 0;
    private int _viewTotalItemCount = 0;

    private Rect _rcItem = new Rect();
    private Rect _rcView = new Rect();

    private bool _isValidItemPrefab = false;
    private bool _isReserveUpdate = false;

    public override int itemCount
    {
        get
        {
            return _itemCount;
        }
        set
        {
            if (_itemCount != value)
            {
                _itemCount = value;

                if (this.isActiveAndEnabled)
                    UpdateContentSize();
                else
                    _isReserveUpdate = true;
            }
        }
    }
    private int _itemCount = 0;

    private static readonly List<int> _tempTableList = new List<int>();
    private static readonly List<T> _tempInstanceList = new List<T>();

    public void PreInit()
    {
        if (null != scrollRect)
        {
            contentTransform = scrollRect.content;
            _viewportTransform = scrollRect.GetComponent<RectTransform>();
        }
        InitItem();
        UpdateContentSize();
    }
    void Awake()
    {
        if (null != scrollRect)
        {
            scrollRect.onValueChanged.AddListener((_value) => OnValueChanged(_value));

            contentTransform = scrollRect.content;
            _viewportTransform = scrollRect.GetComponent<RectTransform>();
        }
        else
        {
            CommonDebug.LogError("invalid ScrollRect");
        }

        InitItem();
    }

    void Start()
    {
        if (itemCount > 0)
            UpdateContentSize();
    }

    void OnEnable()
    {
        if (_isReserveUpdate)
            UpdateContentSize();
    }

    void OnDisable()
    {
        _isReserveUpdate = false;
    }

    public override void Reset()
    {
        itemCount = 0;

        if (null != scrollRect)
        {
            scrollRect.velocity = Vector2.zero;
            scrollRect.normalizedPosition = Vector2.zero; // 비율값
        }
    }

    public override void TryItemUpdate(int _item_index)
    {
        if (null != eventItemUpdate)
        {
            T _instance = GetInstance(_item_index);
            if (null != _instance)
            {
                eventItemUpdate(_item_index, _instance);
            }
        }
    }

    public override void TryListUpdate(int _item_count)
    {
        if (itemCount != _item_count)
        {
            itemCount = _item_count;
        }
        else
        {
            if (this.isActiveAndEnabled)
                RefreshItemList();
            else
                _isReserveUpdate = true;
        }
    }

    public override void TryListUpdate()
    {
        RefreshItemList();
    }

    public override int GetInstanceObjectIndex(GameObject _item)
    {
        if (itemCount > 0)
        {
            int count = _instanceList.Count;
            for (int n = 0; n < count; ++n)
            {
                if (_instanceList[n].gameObject == _item)
                {
                    return _itemIndexTable[n];
                }
            }
        }
        return -1;
    }

    public override GameObject GetInstanceObject(int _item_index)
    {
        if (_item_index >= 0 && _item_index < itemCount)
        {
            int count = _itemIndexTable.Count;
            for (int n = 0; n < count; ++n)
            {
                if (_itemIndexTable[n] == _item_index)
                {
                    if (_instanceList[n].gameObject.activeSelf)
                        return _instanceList[n].gameObject;
                    else
                        return null;
                }
            }
        }
        return null;
    }

    public int GetInstanceIndex(T _item)
    {
        if (itemCount > 0)
        {
            int count = _instanceList.Count;
            for (int n = 0; n < count; ++n)
            {
                if (_instanceList[n] == _item)
                {
                    return _itemIndexTable[n];
                }
            }
        }
        return -1;
    }

    public T GetInstance(int _item_index)
    {
        if (_item_index >= 0 && _item_index < itemCount)
        {
            int count = _itemIndexTable.Count;
            for (int n = 0; n < count; ++n)
            {
                if (_itemIndexTable[n] == _item_index)
                {
                    if (_instanceList[n].gameObject.activeSelf)
                        return _instanceList[n];
                    else
                        return null;
                }
            }
        }
        return null;
    }

    private void InitItem()
    {
        if (null != itemPrefab)
        {
            T _component = itemPrefab.GetComponent<T>();
            if (null != _component)
            {
                RectTransform _item_transform = itemPrefab.GetComponent<RectTransform>();
                if (null != _item_transform)
                {
                    _isValidItemPrefab = true;

                    _rcItem = _item_transform.rect;

                    _rcItem.xMin -= itemMargin.left;
                    _rcItem.xMax += itemMargin.right;
                    _rcItem.yMin -= itemMargin.top;
                    _rcItem.yMax += itemMargin.bottom;

        
                    return;
                }
                else
                {
                    CommonDebug.LogError("not found RectTransform");
                }
            }
            else
            {
                CommonDebug.LogError("not fount component : " + typeof(T).ToString());
            }
        }
        else
        {
            CommonDebug.LogError("invalid itemPrefab");
        }

        _isValidItemPrefab = false;
        _rcItem.width = _rcItem.height = 1.0f;
    }

    public void UpdateContentSize()
    {
        if (null != contentTransform && null != _viewportTransform)
        {
            float _content_width = GetContentWidth();
            float _content_height = GetContentHeight();

            contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _content_width);
            contentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _content_height);

            _rcView.size = _viewportTransform.rect.size;

            if (_content_width < _rcView.width)
                _scrollArea_Width = 0.0f;
            else
                _scrollArea_Width = _content_width - _rcView.width;

            if (_content_height < _rcView.height)
                _scrollArea_Height = 0.0f;
            else
                _scrollArea_Height = _content_height - _rcView.height;

            UpdateDefaultItemCount();
        }
    }

    private void UpdateDefaultItemCount()
    {
        if (!_isValidItemPrefab)
            return;

        //영역에 조금이라도 보이는 아이템도 포함하여야하기 때문에 itemWidth, itemHeight 추가로 더 해준다
        _viewRowItemCount = (int)((_rcView.height + _rcItem.height) / _rcItem.height);
        _viewColumnItemCount = (int)((_rcView.width + _rcItem.width) / _rcItem.width);

        _viewRowItemCount += (ITEM_EXTRA_COUNT * 2);
        _viewColumnItemCount += (ITEM_EXTRA_COUNT * 2);

        if (scrollType == ScrollType.Horizontal)
        {
            if (_viewRowItemCount > lineCount)
                _viewRowItemCount = lineCount;
        }
        else
        {
            if (_viewColumnItemCount > lineCount)
                _viewColumnItemCount = lineCount;
        }

        int _view_item_count = _viewRowItemCount * _viewColumnItemCount;
        if (_viewTotalItemCount != _view_item_count)
        {
            _viewTotalItemCount = _view_item_count;

            _itemIndexTable.Clear();
            for (int n = 0; n < _viewTotalItemCount; ++n)
            {
                _itemIndexTable.Add(-1);
            }

            int _cur_instance_count = _instanceList.Count;
            if (_viewTotalItemCount > _cur_instance_count)
            {
                CreateItemInstance(_viewTotalItemCount - _cur_instance_count);
            }
            else if (_viewTotalItemCount < _cur_instance_count)
            {
                RemoveItemInstance(_cur_instance_count - _viewTotalItemCount);
            }
        }

        RefreshItemList();
    }

    private void RefreshItemList()
    {
        _curLeftTopIndex = _curRightBottomIndex = 0;

        if (null != scrollRect)
        {
            scrollRect.velocity = Vector2.zero;

            FindItemIndex(scrollRect.normalizedPosition, out _curLeftTopIndex, out _curRightBottomIndex);

            OnDynamicAlign(true);
        }
    }

    public void SelectedItemIndex(int _index)
    {
        if (null == contentTransform)
            return;

        if (0 <= _index && _index < _itemCount)
        {
            if (scrollType == ScrollType.Horizontal)
            {
                float posX = GetPosition(_index, _rcView.width, _rcItem.width);
                contentTransform.anchoredPosition = new Vector2(-posX, 0);
            }
            else
            {
                int lineIndex = _index / lineCount;
                int index = (0 < lineCount) ? lineIndex : _index;

                float posY = GetPosition(index, _rcView.height, _rcItem.height);
                posY = Mathf.Min(_scrollArea_Height, posY);
                contentTransform.anchoredPosition = new Vector2(0f, posY);
            }
        }

        UpdateDefaultItemCount();
    }

    private float GetPosition(int _index, float _view_value, float _item_value)
    {
        int minViewCount = (int)(_view_value / _item_value) - 1;
        int maxIndex = _index + minViewCount;

        int lastIndex = _itemCount - minViewCount;
        int value = (_index < (lastIndex + 1)) ? 0 : 1;

        int startIndex = (maxIndex < _itemCount) ? _index : lastIndex + value;
        if (0 <= startIndex)
        {
            return (startIndex * _item_value);
        }

        return 0f;
    }

    private void OnValueChanged(Vector2 _ratio)
    {
        int _lt_item_index, _rb_item_index;
        FindItemIndex(_ratio, out _lt_item_index, out _rb_item_index);

        if (_curLeftTopIndex != _lt_item_index || _curRightBottomIndex != _rb_item_index)
        {
            _curLeftTopIndex = _lt_item_index;
            _curRightBottomIndex = _rb_item_index;

            OnDynamicAlign(false);
        }
    }

    private void FindItemIndex(Vector2 _ratio, out int _left_top_index, out int _right_bottom_index)
    {
        _rcView.x = _scrollArea_Width * _ratio.x;
        _rcView.y = _scrollArea_Height * (1.0f - _ratio.y);

        int _left_index = (int)(_rcView.x / _rcItem.width);
        int _top_index = (int)(_rcView.y / _rcItem.height);
        int _right_index = (int)(_rcView.xMax / _rcItem.width);
        int _bottom_index = (int)(_rcView.yMax / _rcItem.height);

        if (scrollType == ScrollType.Horizontal)
        {
            _left_index *= lineCount;
            _right_index *= lineCount;

            int _column_max_index = lineCount - 1;
            _top_index = Mathf.Clamp(_top_index, 0, _column_max_index);
            _bottom_index = Mathf.Clamp(_bottom_index, 0, _column_max_index);
        }
        else
        {
            _top_index *= lineCount;
            _bottom_index *= lineCount;

            int _row_max_index = lineCount - 1;
            _left_index = Mathf.Clamp(_left_index, 0, _row_max_index);
            _right_index = Mathf.Clamp(_right_index, 0, _row_max_index);
        }

        _left_top_index = _left_index + _top_index;
        _right_bottom_index = _right_index + _bottom_index;
    }

    private void OnDynamicAlign(bool _is_all)
    {
        int _fixed_start_index, _fixed_end_index;

        if (_is_all)
            _fixed_start_index = _fixed_end_index = -1;
        else
            SortInstanceList(out _fixed_start_index, out _fixed_end_index);


        int _start_item_index = _curLeftTopIndex - (ITEM_EXTRA_COUNT * lineCount);
        int _max_line = lineCount - (_curLeftTopIndex % lineCount);

        Vector3 _temp_pos = new Vector3();

        int count = _instanceList.Count;
        for (int n = 0; n < count; ++n)
        {
            if (_fixed_start_index > -1)
            {
                //기존에 갱신되어 있는 경우는 건너 뛴다
                if (_fixed_start_index <= n && _fixed_end_index >= n)
                    continue;
            }

            T _instance = _instanceList[n];

            int _item_index = _start_item_index;

            if (scrollType == ScrollType.Horizontal)
            {
                int _cur_line = n % _viewRowItemCount;
                if (_max_line < _cur_line)
                {
                    _item_index = -1;
                }
                else
                {
                    int _interval = (n / _viewRowItemCount) * lineCount;
                    _item_index += _cur_line + _interval;
                }
            }
            else
            {
                int _cur_line = n % _viewColumnItemCount;
                if (_max_line < _cur_line)
                {
                    _item_index = -1;
                }
                else
                {
                    int _interval = (n / _viewColumnItemCount) * lineCount;
                    _item_index += _cur_line + _interval;
                }
            }

            if (_item_index > -1 && _item_index < itemCount)
            {
                _itemIndexTable[n] = _item_index;

                _instance.gameObject.SetActive(true);

                _temp_pos.x = GetItemPosX(_item_index);
                _temp_pos.y = GetItemPosY(_item_index);
                _instance.transform.localPosition = _temp_pos;

                if (null != eventItemUpdate)
                    eventItemUpdate(_item_index, _instance);
            }
            else
            {
                _itemIndexTable[n] = -1;

                _instance.gameObject.SetActive(false);
            }
        }
    }

    private void SortInstanceList(out int _fixed_start_index, out int _fixed_end_index)
    {
        int _table_count = _itemIndexTable.Count;
        if (_table_count > 0)
        {
            int _extra_count = ITEM_EXTRA_COUNT * lineCount;
            int _lt_item_index = _curLeftTopIndex - _extra_count;
            int _rb_item_index = _curRightBottomIndex + _extra_count;


            int _fixed_slot_index = -1;
            int _fixed_slot_count = 0;
            int _fixed_slot_insert_index = -1;

            int _check_count = 0;
            int _interval = 0;
            if (scrollType == ScrollType.Vertical)
            {
                _check_count = _viewTotalItemCount / _viewColumnItemCount;
                _interval = _viewColumnItemCount;
            }
            else
            {
                _check_count = _viewTotalItemCount / _viewRowItemCount;
                _interval = _viewRowItemCount;
            }

            for (int n = 0; n < _check_count; ++n)
            {
                int _check_index = (n * _interval);
                if (_check_index < _table_count)
                {
                    int _item_index = _itemIndexTable[_check_index];
                    if (_item_index > -1)
                    {
                        if (_item_index >= _lt_item_index && _item_index <= _rb_item_index)
                        {
                            _fixed_slot_index = _check_index;
                            _fixed_slot_insert_index = _item_index - _lt_item_index;

                            for (int k = _fixed_slot_index; k < _table_count; ++k)
                            {
                                _item_index = _itemIndexTable[k];
                                if (_item_index > -1)
                                    ++_fixed_slot_count;
                                else
                                    break;
                            }

                            break;
                        }
                    }
                }
            }

            if (_fixed_slot_count > 0)
            {
                if (_fixed_slot_index != _fixed_slot_insert_index)
                {
                    if (_fixed_slot_index < _fixed_slot_insert_index)
                    {
                        int _last_index = (_fixed_slot_index + _fixed_slot_count - 1);
                        int _over_count = (_last_index + _fixed_slot_insert_index) - _table_count;
                        if (_over_count > 0)
                        {
                            _fixed_slot_count -= _over_count;
                        }

                        int _move_limit_count = (_fixed_slot_count - _fixed_slot_index) + _fixed_slot_insert_index;
                        if (_move_limit_count > _table_count)
                        {
                            _fixed_slot_count -= _move_limit_count - _table_count;
                        }
                    }

                    //값이 변경될 수 있으므로 재 확인
                    if (_fixed_slot_count > 0)
                    {
                        MoveInstance(_fixed_slot_index, _fixed_slot_count, _fixed_slot_insert_index);

                        _fixed_start_index = _fixed_slot_insert_index;
                        _fixed_end_index = _fixed_start_index + _fixed_slot_count - 1;
                        return;
                    }
                }
                else
                {
                    _fixed_start_index = _fixed_slot_insert_index;
                    _fixed_end_index = _fixed_start_index + _fixed_slot_count - 1;
                    return;
                }
            }
        }

        //전체 재 갱신
        _fixed_start_index = -1;
        _fixed_end_index = -1;
    }

    private void MoveInstance(int _start_index, int _move_count, int _insert_index)
    {
        _tempTableList.Clear();
        _tempInstanceList.Clear();


        int _last_index = _start_index + _move_count - 1;
        if (_last_index >= _itemIndexTable.Count)
            _last_index = _itemIndexTable.Count - 1;

        for (int n = _start_index; n <= _last_index; ++n)
        {
            _tempTableList.Add(_itemIndexTable[n]);
        }
        _itemIndexTable.RemoveRange(_start_index, _move_count);
        _itemIndexTable.InsertRange(_insert_index, _tempTableList);

        for (int n = _start_index; n <= _last_index; ++n)
        {
            _tempInstanceList.Add(_instanceList[n]);
        }
        _instanceList.RemoveRange(_start_index, _move_count);
        _instanceList.InsertRange(_insert_index, _tempInstanceList);


        _tempTableList.Clear();
        _tempInstanceList.Clear();
    }


    private void CreateItemInstance(int _create_count)
    {
        if (_hideItemPrefab && itemPrefab != null)
        {
            itemPrefab.SetActive(true);
        }

        for (int n = 0; n < _create_count; ++n)
        {

            T _instance = CreateItemInstance();
            if (null != _instance)
            {
                _instanceList.Add(_instance);
            }
        }

        if (_hideItemPrefab && itemPrefab != null)
        {
            itemPrefab.SetActive(false);
        }
    }

    private T CreateItemInstance()
    {
        GameObject _game_obj = (GameObject)Instantiate(itemPrefab);
        if (null != _game_obj)
        {
            T _result = _game_obj.GetComponent<T>();
            if (null != _result)
            {
                RectTransform _inst_transform = _game_obj.GetComponent<RectTransform>();
                if (null != _inst_transform)
                {
                    _inst_transform.SetParent(contentTransform);
                    _inst_transform.localScale = new Vector3(itemScale, itemScale, 1.0f);
                }

                return _result;
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.LogError("Not found component : " + typeof(T).ToString());

                Destroy(_game_obj);
            }
        }

        return null;
    }

    private void RemoveItemInstance(int _remove_count)
    {
        int _cur_count = _instanceList.Count;
        if (_remove_count > 0 && _remove_count <= _cur_count)
        {
            int startIdx = _cur_count - _remove_count;
            for (int n = startIdx; n < _cur_count; ++n)
            {
                Destroy(_instanceList[n].gameObject);
                _instanceList[n] = null;
            }
            _instanceList.RemoveRange(startIdx, _remove_count);
        }
    }

    private float GetItemPosX(int _item_index)
    {
        float _result = 0.0f;

        if (scrollType == ScrollType.Horizontal)
        {
            _result = ((_item_index / lineCount) * _rcItem.width);
        }
        else
        {
            _result = (_item_index % lineCount) * _rcItem.width;
        }

        _result -= _rcItem.xMin;

        return _result;
    }

    private float GetItemPosY(int _item_index)
    {
        float _result = 0.0f;

        if (scrollType == ScrollType.Vertical)
        {
            _result = -((_item_index / lineCount) * _rcItem.height);
        }
        else
        {
            _result = -(_item_index % lineCount) * _rcItem.height;
        }

        _result -= _rcItem.yMax;

        return _result;
    }

    private float GetContentWidth()
    {
        float _result = 0.0f;

        if (scrollType == ScrollType.Horizontal)
        {
            int _row_count = itemCount / lineCount;
            if ((itemCount % lineCount) != 0)
                ++_row_count;

            _result = _row_count * _rcItem.width;
        }
        else
        {
            _result = lineCount * _rcItem.width;
        }

        return _result;
    }

    private float GetContentHeight()
    {
        float _result = 0.0f;

        if (scrollType == ScrollType.Vertical)
        {
            int _column_count = itemCount / lineCount;
            if ((itemCount % lineCount) != 0)
                ++_column_count;

            _result = _column_count * _rcItem.height;
        }
        else
        {
            _result = lineCount * _rcItem.height;
        }

        return _result;
    }
}
