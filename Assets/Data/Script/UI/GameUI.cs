using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameUI : MonoBehaviour
{
    public static GameUI Inst { get; private set; }

    private GameObject[] _Left;

    private GameObject[] _Right;

    private GameObject _Info;

    [SerializeField]
    public GameObject _InfoTextPrefab;


    private void Awake()
    {
        Inst = this;
        _Left = (from item in transform.Find("LeftPanel").GetComponentsInChildren<Image>(true)
                 select item.gameObject).ToArray();
        _Right = (from item in transform.Find("RightPanel").GetComponentsInChildren<Image>(true)
                  select item.gameObject).ToArray();
        _Info = transform.Find("Info").gameObject;

        float scale = GetComponent<Canvas>().transform.localScale.x;
        for (int i = 0; i < _Left.Length; i++)
        {
            _Left[i].transform.localScale = scale * 0.5f * Vector3.one;
            _Right[i].transform.localScale = scale * 0.5f * Vector3.one;
        }
    }

    public void SetLifeCount(int pid, int num)
    {
        if (num > 3)
            num = 3;
        GameObject[] gos = pid == 1 ? _Left : _Right;
        for (int i = 0; i < num; i++)
            gos[i].SetActive(true);
        if (num >= 0)
        {
            for (int i = 0; i < 3 - num; i++)
                gos[i + num].SetActive(false);
        }

        gos[3].SetActive(num < 0);
    }

    public void Info(string info)
    {
        Text txt = Instantiate(_InfoTextPrefab).GetComponent<Text>();
        txt.text = info;
        txt.transform.SetParent(_Info.transform, false);
        DOVirtual.DelayedCall(3, () => Destroy(txt.gameObject));
    }

    public void GameEndUI(bool is_success)
    {
        if (is_success)
            transform.Find("Success").gameObject.SetActive(true);
        else
            transform.Find("Fail").gameObject.SetActive(true);
    }
}
