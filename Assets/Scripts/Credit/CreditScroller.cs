using System.Collections;
using hvvan;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScroller : MonoBehaviour
{
    public GameObject scrollGroupObject;   // ScrollGroup GameObject (비활성 → 활성 대상)
    public GameObject duration;
    public GameObject directedby;
    public GameObject something;
    public GameObject theend;
    
    public RectTransform scrollGroup;      // ScrollGroup의 RectTransform (스크롤 대상)
    public float scrollSpeed = 50f;
    public float delayToActivate = 3f;
    public float delayToScroll = 6f;

    private bool startScroll = false;

    void Start()
    {
        scrollGroupObject.SetActive(false); // 초기엔 비활성화
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        AudioManager.Instance.SetBGMVolume(1f);
        
        yield return new WaitForSeconds(1.8f);
        AudioManager.Instance.PlayBGM(AudioBase.BGM.Ending.Main);
        
        yield return new WaitForSeconds(1.5f);
        duration.SetActive(true);
        
        yield return new WaitForSeconds(6.2f);
        duration.SetActive(false);
        directedby.SetActive(true);
        
        yield return new WaitForSeconds(6.2f);
        something.SetActive(true);
        
        yield return new WaitForSeconds(4.9f);
        directedby.SetActive(false);
        something.SetActive(false);
        
        yield return new WaitForSeconds(1f);
        scrollGroupObject.SetActive(true);
        
        

        yield return new WaitForSeconds(12.3f);
        startScroll = true;                // 2초 추가 후 스크롤 시작
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(15f);
        scrollGroupObject.SetActive(false);
        yield return new WaitForSeconds(5f);
        theend.SetActive(true);
        yield return new WaitForSeconds(10f);
        theend.SetActive(false);
        
        yield return new WaitForSeconds(8f);
        AudioManager.Instance.PlayBGM(AudioBase.BGM.Title.Main);
        AudioManager.Instance.SetBGMVolume(0.5f);
        SceneManager.LoadScene("StartUpScene");
        
    }
    void Update()
    {
        if (startScroll)
        {
            scrollGroup.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            if (scrollGroup.anchoredPosition.y > 8880f)
            {
                StopCoroutine(Sequence());
                StartCoroutine(Wait());
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("StartUpScene");
            AudioManager.Instance.SetBGMVolume(0.5f);
        }
        
        /*if (scrollGroup.anchoredPosition.y >= 9500f)
        {
            SceneManager.LoadScene("StartUpScene");
            AudioManager.Instance.SetBGMVolume(0.5f);
        }*/
    }
}