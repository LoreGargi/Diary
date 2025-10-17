using Assets.Script.Common.Enum;
using Script.Assets.Script.Common.Utility;
using Script.Assets.Script.Diary;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Assets.Script.Manager
{
    /// <summary>
    /// manager per gestire il diario e le sue notifiche
    /// </summary>
    public class DiaryManager : MonoBehaviour
    {
        private GameObject Player;

        #region CANVAS
        private CanvasGroup _canvasGroup;
        private GameObject _diaryCanvas;
        private TextMeshProUGUI _pageTitle;
        private Image _pageImage;
        private TextMeshProUGUI _pageDescription;
        private List<Button> _pageButtons;
        private GameObject _diaryPageUnlockUI;
        [SerializeField]
        private AudioClip[] _diaryPageUnlockAudio;
        [SerializeField]
        private AudioClip[] _diaryOpenAudio;
        [SerializeField]
        private AudioClip[] _pageChangeAudio;
        private GameObject _diaryFirstUnlock;
        #endregion

        #region PagesLists
        [Header("Inserire le pagine in ordine di visualizzazione del diario")]
        [SerializeField]
        private List<DiaryPageSO> _pages;
        private List<DiaryPageSO> _usedList = new List<DiaryPageSO>();
        [SerializeField]
        public List<int> _unlockedPagesIDs = new List<int>();
        #endregion

        [SerializeField]
        private bool _visibleButtons;

        private int _pageIndex;

        private bool _firstUnlock;

        public static DiaryManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        /// <summary>
        /// Ottiene i riferimenti ai componenti UI all'interno del Canvas e all'avvio nasconde il canvas
        /// </summary>
        private void Start()
        {
            Player = GameObject.FindGameObjectWithTag("Player");
            _diaryCanvas = transform.GetChild(0).gameObject;
            _diaryPageUnlockUI = transform.GetChild(1).gameObject;
            _diaryFirstUnlock = transform.GetChild(2).gameObject;
            _canvasGroup = _diaryCanvas.GetComponent<CanvasGroup>();
            _diaryCanvas.transform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0;
            _pageIndex = 0;
            _pageTitle = _diaryCanvas.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            _pageImage = _diaryCanvas.transform.GetChild(2).GetComponent<Image>();
            _pageDescription = _diaryCanvas.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            _pageButtons = _diaryCanvas.transform.GetComponentsInDirectChildren<Button>();
            _diaryCanvas.SetActive(false);
            _diaryPageUnlockUI.SetActive(false);
            _diaryFirstUnlock.SetActive(false);
            HideButtons();
            // Verifica che il Canvas sia stato assegnato
            if (_diaryCanvas == null)
            {
                print("Diario non impostato");
            }
        }

        /// <summary>
        /// Metodo richiamato dallo stato play del gamemanager
        /// Serve ad aprire il diario quando siamo in play
        /// </summary>
        public void OnPlayDiary()
        {
            if (InputManager.Instance.DiaryButton())
            {
                OpenDiary();
            }
        }

        /// <summary>
        /// Col tasto J si apre e chiude il diario
        /// </summary>
        public void UpdateDiary()
        {
            if (InputManager.Instance.DiaryButton())
            {
                if (_diaryCanvas.activeInHierarchy)
                {
                    CloseDiary();
                }
            }
            if (InputManager.Instance.BackButton())
            {
                CloseDiary();
            }
        }

        /// <summary>
        /// Metodo che anima il diario all'apertura con un rimbalzo e cambio di alpha,
        /// cambia lo stato a Diary
        /// </summary>
        private void OpenDiary()
        {
            ///disattivazione prima notifica del diario se questa è attiva e se clicco J
            if (_diaryFirstUnlock.activeInHierarchy)
            {
                LeanTween.scale(_diaryFirstUnlock, Vector3.zero, 0.2f).setEaseInBack();
                LeanTween.alphaCanvas(_canvasGroup, 0f, 0.2f).setOnComplete(() =>
                {
                    _diaryFirstUnlock.SetActive(false);
                });
            }
            ///attivazione del diario e cambio di stato
            _diaryCanvas.SetActive(true);
            _diaryCanvas.transform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0;

            LeanTween.scale(_diaryCanvas, Vector3.one, 0.3f).setEaseOutBack();
            LeanTween.alphaCanvas(_canvasGroup, 1f, 0.3f);

            AudioManager.Instance.Play3DAudio(Camera.main.transform.position, _diaryOpenAudio);
            ShowTutorialPages();
            GameManager.Instance.GameState = GameState.Diary;
        }

        /// <summary>
        /// Chiude il diario con animazioni inverse all'apertura e cambia lo stato a Play
        /// </summary>
        private void CloseDiary()
        {
            LeanTween.scale(_diaryCanvas, Vector3.zero, 0.2f).setEaseInBack();
            LeanTween.alphaCanvas(_canvasGroup, 0f, 0.2f).setOnComplete(() =>
            {
                _diaryCanvas.SetActive(false);
            });

            GameManager.Instance.GameState = GameState.Play;
        }
        /// <summary>
        /// Nasconde i pulsanti Next e Previous
        /// </summary>
        private void HideButtons()
        {
            foreach (Button button in _pageButtons)
            {
                button.gameObject.SetActive(false);
            }
            _visibleButtons = false;
        }
        /// <summary>
        /// Mostra i pulsanti Next e Previous
        /// </summary>
        private void ShowButtons()
        {
            foreach (Button button in _pageButtons)
            {
                if (!button.gameObject.activeInHierarchy)
                {
                    button.gameObject.SetActive(true);
                }
            }
            _visibleButtons = true;
        }

        /// <summary>
        /// Coroutine che visualizza a schermo con animazione un popup ogni volta che
        /// una pagina viene aggiunta a una lista e la notifica all'inizio quando
        /// sblocchi la prima pagina e viene detto come aprire il diario
        /// </summary>
        /// <returns>timer attesta</returns>
        public IEnumerator NewPageUnlocked()
        {
            AudioManager.Instance.Play3DAudio(Player.transform.position, _diaryPageUnlockAudio);
            _diaryPageUnlockUI.SetActive(true);
            _diaryPageUnlockUI.transform.localScale = Vector3.zero;
            _canvasGroup.alpha = 0;

            LeanTween.scale(_diaryPageUnlockUI, Vector3.one, 0.3f).setEaseOutBack();
            LeanTween.alphaCanvas(_canvasGroup, 1f, 0.3f);

            yield return new WaitForSeconds(2);

            LeanTween.scale(_diaryPageUnlockUI, Vector3.zero, 0.2f).setEaseInBack();
            LeanTween.alphaCanvas(_canvasGroup, 0f, 0.2f).setOnComplete(() =>
            {
                _diaryPageUnlockUI.SetActive(false);
            });

            yield return new WaitForSeconds(1);
            if (!_firstUnlock)
            {
                _diaryFirstUnlock.SetActive(true);
                _diaryFirstUnlock.transform.localScale = Vector3.zero;
                _canvasGroup.alpha = 0;

                LeanTween.scale(_diaryFirstUnlock, Vector3.one, 0.3f).setEaseOutBack();
                LeanTween.alphaCanvas(_canvasGroup, 1f, 0.3f);
                _firstUnlock = true;
            }
        }
        /// <summary>
        /// Aggiorna il titolo, immagine e descrizione della pagina corrente
        /// </summary>
        /// <param name="pagesList">Lista delle pagine da visualizzare</param>
        private void ShowDiaryPages(List<DiaryPageSO> pagesList)
        {
            if (!_visibleButtons)
            {
                ShowButtons();
            }
            _pageTitle.text = pagesList[_pageIndex].PageTitle;
            SetPageImageToScreen(pagesList);
            _pageDescription.text = PageDescriptionToString(pagesList[_pageIndex].PageText);

            AudioManager.Instance.Play3DAudio(Camera.main.transform.position, _pageChangeAudio);
        }

        /// <summary>
        /// Crea una lista di pagine che appartengono alla categoria tutorial
        /// </summary>
        public void ShowTutorialPages()
        {
            List<DiaryPageSO> tutorialPages = new List<DiaryPageSO>();
            foreach (DiaryPageSO page in _pages)
            {
                if (_unlockedPagesIDs.Contains(page.PageID) && page.Category == DiaryPageCategory.Tutorial)
                {
                    tutorialPages.Add(page);
                }
            }
            _usedList = tutorialPages;
            _pageIndex = 0;
            if (tutorialPages.Count > 0)
            {
                ShowDiaryPages(tutorialPages);
            }
        }

        /// <summary>
        /// Crea una lista di pagine che appartengono alla categoria Room
        /// </summary>
        public void ShowRoomPages()
        {
            List<DiaryPageSO> roomPages = new List<DiaryPageSO>();
            foreach (DiaryPageSO page in _pages)
            {
                if (_unlockedPagesIDs.Contains(page.PageID) && page.Category == DiaryPageCategory.Room)
                {
                    roomPages.Add(page);
                }
            }
            _usedList = roomPages;
            _pageIndex = 0;
            if (roomPages.Count > 0)
            {
                ShowDiaryPages(roomPages);
            }
        }

        /// <summary>
        /// Crea una lista di pagine che appartengono alla categoria collectable
        /// </summary>
        public void ShowCollectablesPages()
        {
            List<DiaryPageSO> collectablesPages = new List<DiaryPageSO>();
            foreach (DiaryPageSO page in _pages)
            {
                if (_unlockedPagesIDs.Contains(page.PageID) && page.Category == DiaryPageCategory.Collectables)
                {
                    collectablesPages.Add(page);
                }
            }
            _usedList = collectablesPages;
            _pageIndex = 0;
            if (collectablesPages.Count > 0)
            {
                ShowDiaryPages(collectablesPages);
            }
        }

        /// <summary>
        /// Controlla se l'immagine da visualizzare è stata assegnata
        /// Se è stata assegnata la visualizza, se no rende l'immagine invisibile
        /// </summary>
        /// <param name="pagesList">Lista attualmente visualizzata a schermo</param>
        private void SetPageImageToScreen(List<DiaryPageSO> pagesList)
        {
            if (pagesList[_pageIndex].PageImage == null)
            {
                _pageImage.color = new Color(255, 255, 255, 0);
            }
            else
            {
                _pageImage.sprite = pagesList[_pageIndex].PageImage;
                _pageImage.color = Color.white;
            }
        }

        /// <summary>
        /// Concatena le stringhe di testo in ogni pagina andando a capo per ogni elemento dell'array
        /// Così da avere un'unica stringa
        /// </summary>
        /// <param name="_pagesText">array di stringhe da unire</param>
        /// <returns></returns>
        private string PageDescriptionToString(string[] _pagesText)
        {
            string completeDescription = "";
            foreach (string _pageString in _pagesText)
            {
                completeDescription += _pageString + "\n";
            }
            return completeDescription;
        }

        /// <summary>
        /// Sblocca le pagine non bloccate
        /// </summary>
        /// <param name="pagesList">Pagine da sbloccare</param>
        public void UnlockPages(List<DiaryPageSO> pagesList)
        {
            foreach (DiaryPageSO page in pagesList)
            {
                if (!_unlockedPagesIDs.Contains(page.PageID))
                {
                    _unlockedPagesIDs.Add(page.PageID);
                }
            }
        }

        /// <summary>
        /// Metodo per passare alla pagina successiva se è presente ed è sbloccata
        /// </summary>
        public void Next()
        {
            _pageIndex++;
            if (_pageIndex < _usedList.Count && _unlockedPagesIDs.Contains(_usedList[_pageIndex].PageID))
            {
                ShowDiaryPages(_usedList);
                AudioManager.Instance.Play3DAudio(Camera.main.transform.position, _pageChangeAudio);
            }
            else
            {
                _pageIndex = _usedList.Count - 1;
                print("Pagine sbloccate finite");
            }
        }

        /// <summary>
        /// Metodo per passare alla pagina precedente se siamo a index>0 e se è sbloccatra
        /// </summary>
        public void Previous()
        {
            _pageIndex--;
            if (_pageIndex >= 0 && _unlockedPagesIDs.Contains(_usedList[_pageIndex].PageID))
            {
                ShowDiaryPages(_usedList);
                AudioManager.Instance.Play3DAudio(Camera.main.transform.position, _pageChangeAudio);
            }
            else
            {
                _pageIndex = 0;
                print("Pagine sbloccate finite");
            }
        }
    }
}