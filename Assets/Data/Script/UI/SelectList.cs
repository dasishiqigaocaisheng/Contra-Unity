using System;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Modules.LogSystem;

namespace Contra.UI
{
    public partial class SelectList : MonoBehaviour
    {
        private Text _Title;

        private List<Text> _Txtlist;

        [SerializeField]
        private GameObject _TextPrefab;

        public RectTransform CursorRect { get; private set; }

        public UnityEvent<int> OnSelected;

        public string Title
        {
            get => _Title.text;
            set => _Title.text = value;
        }

        private int _Index;
        public int Index
        {
            get => _Index;
            set
            {
                if (_Txtlist.Count == 0)
                {
                    _Index = 0;
                    CursorRect.gameObject.SetActive(false);
                    return;
                }

                if (value >= 0 && value < _Txtlist.Count)
                {
                    RectTransform rt = _Txtlist[value].rectTransform;
                    CursorRect.anchoredPosition = new Vector2(15, rt.anchoredPosition.y - 4);
                    _Index = value;
                }
            }
        }

        private void Awake()
        {
            _Txtlist = (from idx in Enumerable.Range(0, transform.childCount)
                        where transform.GetChild(idx).gameObject.name == "Text"
                        select transform.GetChild(idx).GetComponent<Text>()).ToList();
            _Title = transform.Find("Description").GetComponent<Text>();
            CursorRect = transform.Find("Cursor").GetComponent<RectTransform>();
        }

        private void Start()
        {
            Index = 0;
        }

        private void OnEnable()
        {
            Index = 0;
            UIInput.Inst.OnSelectLast.AddListener(_OnSelectLast);
            UIInput.Inst.OnSelectNext.AddListener(_OnSelectNext);
            UIInput.Inst.OnConfirm.AddListener(_OnConfirm);
        }

        private void OnDisable()
        {
            UIInput.Inst.OnSelectLast.RemoveListener(_OnSelectLast);
            UIInput.Inst.OnSelectNext.RemoveListener(_OnSelectNext);
            UIInput.Inst.OnConfirm.RemoveListener(_OnConfirm);
        }

        private void _OnConfirm()
        {
            OnSelected.Invoke(Index);
        }

        private void _OnSelectLast()
        {
            Index--;
        }

        private void _OnSelectNext()
        {
            Index++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PointTo(int idx)
        {
            Index = idx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveNext()
        {
            Index++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveLast()
        {
            Index--;
        }

        public void Clear()
        {
            for (int i = 0; i < _Txtlist.Count; i++)
            {
                Destroy(_Txtlist[0].gameObject);
                _Txtlist.RemoveAt(0);
            }
            Index = 0;
        }

        public void Add(string txt)
        {
            Text t = Instantiate(_TextPrefab).GetComponent<Text>();
            t.text = $"   {txt}";
            t.transform.SetParent(transform, false);
            t.transform.SetAsLastSibling();
            _Txtlist.Add(t);
            CursorRect.gameObject.SetActive(true);
        }
    }
}
