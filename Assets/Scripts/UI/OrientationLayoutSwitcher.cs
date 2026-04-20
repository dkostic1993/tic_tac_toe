using UnityEngine;

namespace TicTacToe.UI
{
    public sealed class OrientationLayoutSwitcher : MonoBehaviour
    {
        [SerializeField] private GameObject portraitRoot;
        [SerializeField] private GameObject landscapeRoot;

        private bool? _lastPortrait;

        private void Start()
        {
            Apply();
        }

        private void Update()
        {
            Apply();
        }

        private void Apply()
        {
            var isPortrait = Screen.height >= Screen.width;
            if (_lastPortrait.HasValue && _lastPortrait.Value == isPortrait)
                return;

            _lastPortrait = isPortrait;
            if (portraitRoot != null) portraitRoot.SetActive(isPortrait);
            if (landscapeRoot != null) landscapeRoot.SetActive(!isPortrait);
        }
    }
}

