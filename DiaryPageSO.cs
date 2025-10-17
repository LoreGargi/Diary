using UnityEngine;

namespace Script.Assets.Script.Diary
{
    /// <summary>
    /// ScriptableObject composto da ID, categoria, titolo, immagine e descrizione di ogni pagina
    /// </summary>
    [CreateAssetMenu(fileName = "DiaryPageSO", menuName = "Diario/DiaryPageSO")]
    public class DiaryPageSO : ScriptableObject
    {
        public int PageID;
        public DiaryPageCategory Category;
        public string PageTitle;
        public Sprite PageImage;
        [TextArea(3, 10)]
        public string[] PageText;
    }
}
