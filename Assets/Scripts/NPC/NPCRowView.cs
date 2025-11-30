using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuiltTown.NPC
{
    /// <summary>
    /// Small view component to expose prefab children explicitly.
    /// Pøipoj na prefab øádku (miner/collector/builder) a nastav pøes Inspector.
    /// </summary>
    public class NPCRowView : MonoBehaviour
    {
        [SerializeField] private Image icon = null;
        [SerializeField] private TextMeshProUGUI nameText = null;
        [SerializeField] private TextMeshProUGUI statusText = null;
        [SerializeField] private Transform actionsContainer = null;
        [SerializeField] private Button removeButton = null;

        public Image Icon => icon;
        public TextMeshProUGUI NameText => nameText;
        public TextMeshProUGUI StatusText => statusText;
        public Transform ActionsContainer => actionsContainer;
        public Button RemoveButton => removeButton;
    }
}
