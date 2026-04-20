using System.Collections;
using System.Collections.Generic;
using TicTacToe.Game;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public sealed class ParticleBackground : MonoBehaviour
    {
        [Header("Theme")]
        [SerializeField] private ThemeRegistry themeRegistry;

        [Header("Turn sprites")]
        [SerializeField] private Sprite particle1;
        [SerializeField] private Sprite particle2;
        [SerializeField] private Sprite particle3;
        [SerializeField] private Sprite particle4;

        [Header("Spawn")]
        [SerializeField, Range(10, 20)] private int count = 14;
        [SerializeField] private Vector2 sizeRange = new Vector2(22f, 72f);
        [SerializeField] private Vector2 speedRange = new Vector2(12f, 48f);
        [SerializeField] private Vector2 rotationSpeedRange = new Vector2(-30f, 30f);
        [SerializeField] private Vector2 alphaRange = new Vector2(0.12f, 0.32f);

        [Header("Turn sizing (X = Player 1 / particle1+4)")]
        [SerializeField, Min(0.5f)] private float playerXParticleScale = 1.55f;
        [SerializeField, Min(0.5f)] private float playerOParticleScale = 1f;

        [Header("Motion polish (kept subtle so PNG art stays recognizable)")]
        [SerializeField] private Vector2 floatAmplitude = new Vector2(8f, 14f);
        [SerializeField] private Vector2 floatFrequency = new Vector2(0.35f, 0.85f);
        [SerializeField] private Vector2 pulseScaleRange = new Vector2(0.97f, 1.03f);
        [SerializeField] private Vector2 pulseFrequency = new Vector2(0.6f, 1.4f);

        private readonly ThemeService _themeService = new ThemeService();
        private readonly List<Item> _items = new List<Item>(32);

        private RectTransform _root;
        private Sprite _a;
        private Sprite _b;
        private PlayerMark _activeTurn = PlayerMark.None;

        private Color _tintA = new Color(0.45f, 0.78f, 1f, 1f);
        private Color _tintB = new Color(1f, 0.55f, 0.92f, 1f);
        private bool _rebuildScheduled;
        private static Sprite _whiteSprite;

        private void Awake()
        {
            _root = GetComponent<RectTransform>();
        }

        private void Start()
        {
            ResolveThemeTints();
            SetTurn(PlayerMark.X, force: true);
            TryRebuildDeferred();
        }

        public void SetTurn(PlayerMark turn, bool force = false)
        {
            if (!force && _activeTurn == turn)
                return;

            _activeTurn = turn;

            // Player 1 (X): particle1 + particle4
            // Player 2 (O): particle2 + particle3
            if (turn == PlayerMark.O)
            {
                _a = particle2 != null ? particle2 : _a;
                _b = particle3 != null ? particle3 : _b;
            }
            else
            {
                _a = particle1 != null ? particle1 : _a;
                _b = particle4 != null ? particle4 : _b;
            }

            if ((_a == null && _b == null) && themeRegistry != null)
            {
                var themeName = _themeService.LoadThemeName();
                var theme = themeRegistry.GetByNameOrDefault(themeName);
                if (theme != null)
                {
                    _a = theme.particleA;
                    _b = theme.particleB;
                }
            }

            for (var i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                if (it.img == null)
                    continue;
                it.img.sprite = PickSprite(i);
                _items[i] = it;
            }

            ApplyTurnParticleScale();
        }

        private float CurrentTurnParticleScale()
        {
            return _activeTurn == PlayerMark.O ? playerOParticleScale : playerXParticleScale;
        }

        private void ApplyTurnParticleScale()
        {
            var mul = CurrentTurnParticleScale();
            for (var i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                if (it.rt == null)
                    continue;
                var scaled = it.coreSize * mul;
                it.baseSize = scaled;
                it.size = scaled;
                _items[i] = it;
            }
        }

        private void TryRebuildDeferred()
        {
            if (_rebuildScheduled)
                return;
            _rebuildScheduled = true;
            StartCoroutine(RebuildWhenReady());
        }

        private IEnumerator RebuildWhenReady()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            Rebuild();
            _rebuildScheduled = false;
        }

        private void Update()
        {
            if (_root == null)
                return;

            var dt = Time.deltaTime;
            var rect = _root.rect;
            var w = rect.width;
            var h = rect.height;
            if (w <= 1f || h <= 1f)
            {
                if (_items.Count == 0 && !_rebuildScheduled)
                    TryRebuildDeferred();
                return;
            }

            var t = Time.unscaledTime;

            for (var i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                if (it.rt == null)
                    continue;

                it.pos += it.vel * dt;
                it.rot += it.rotSpeed * dt;

                var floatOffset = new Vector2(
                    Mathf.Sin((t * it.floatFreq.x) + it.phase) * floatAmplitude.x,
                    Mathf.Cos((t * it.floatFreq.y) + it.phase) * floatAmplitude.y);

                var pulse = Mathf.Lerp(pulseScaleRange.x, pulseScaleRange.y,
                    (Mathf.Sin((t * it.pulseFreq) + it.phase) + 1f) * 0.5f);
                var s = it.baseSize * pulse;
                it.rt.sizeDelta = new Vector2(s, s);

                var m = it.size * 0.6f;
                if (it.pos.x < -m) it.pos.x = w + m;
                else if (it.pos.x > w + m) it.pos.x = -m;
                if (it.pos.y < -m) it.pos.y = h + m;
                else if (it.pos.y > h + m) it.pos.y = -m;

                it.rt.anchoredPosition = it.pos + floatOffset;
                it.rt.localRotation = Quaternion.Euler(0f, 0f, it.rot);

                if (it.img != null)
                {
                    var a = Mathf.Clamp01(it.baseAlpha * (0.82f + 0.18f * Mathf.Sin((t * it.pulseFreq * 1.3f) + it.phase)));
                    var c = it.baseColor;
                    c.a = a;
                    it.img.color = c;
                }

                _items[i] = it;
            }
        }

        private void ResolveThemeTints()
        {
            if (themeRegistry == null)
                return;

            var themeName = _themeService.LoadThemeName();
            var theme = themeRegistry.GetByNameOrDefault(themeName);
            if (theme == null)
                return;

            _tintA = Color.Lerp(theme.xColor, Color.white, 0.35f);
            _tintB = Color.Lerp(theme.oColor, Color.white, 0.35f);
        }

        private void Rebuild()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            _items.Clear();

            if (_root == null)
                return;

            var useSprites = _a != null || _b != null;
            var fallback = useSprites ? null : GetWhiteSprite();

            var rect = _root.rect;
            var w = Mathf.Max(2f, rect.width);
            var h = Mathf.Max(2f, rect.height);

            var n = Mathf.Clamp(count, 10, 20);
            for (var i = 0; i < n; i++)
            {
                var go = new GameObject($"P_{i:00}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);

                var img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.sprite = useSprites ? PickSprite(i) : fallback;
                img.type = Image.Type.Simple;
                img.preserveAspect = true;

                var core = Random.Range(sizeRange.x, sizeRange.y);
                var size = core * CurrentTurnParticleScale();
                rt.sizeDelta = new Vector2(size, size);

                var alpha = Random.Range(alphaRange.x, alphaRange.y);
                Color baseColor;
                if (useSprites)
                    baseColor = new Color(1f, 1f, 1f, alpha);
                else
                {
                    var tc = Color.Lerp(_tintA, _tintB, Random.value);
                    baseColor = new Color(tc.r, tc.g, tc.b, alpha);
                }

                img.color = baseColor;

                var pos = new Vector2(Random.Range(0f, w), Random.Range(0f, h));
                rt.anchoredPosition = pos;

                var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                var speed = Random.Range(speedRange.x, speedRange.y);
                var vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

                var item = new Item
                {
                    rt = rt,
                    img = img,
                    pos = pos,
                    vel = vel,
                    rot = Random.Range(0f, 360f),
                    rotSpeed = Random.Range(rotationSpeedRange.x, rotationSpeedRange.y),
                    coreSize = core,
                    size = size,
                    baseSize = size,
                    baseAlpha = alpha,
                    baseColor = baseColor,
                    phase = Random.Range(0f, Mathf.PI * 2f),
                    floatFreq = new Vector2(Random.Range(floatFrequency.x, floatFrequency.y),
                        Random.Range(floatFrequency.x, floatFrequency.y)),
                    pulseFreq = Random.Range(pulseFrequency.x, pulseFrequency.y)
                };
                _items.Add(item);
            }
        }

        private Sprite PickSprite(int i)
        {
            if (_a != null && _b != null)
                return (i % 2 == 0) ? _a : _b;
            return _a != null ? _a : _b;
        }

        private static Sprite GetWhiteSprite()
        {
            if (_whiteSprite != null)
                return _whiteSprite;
            var tex = Texture2D.whiteTexture;
            _whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            _whiteSprite.name = "ParticleBackground_White";
            return _whiteSprite;
        }

        private struct Item
        {
            public RectTransform rt;
            public Image img;
            public Vector2 pos;
            public Vector2 vel;
            public float rot;
            public float rotSpeed;
            public float coreSize;
            public float size;
            public float baseSize;
            public float baseAlpha;
            public Color baseColor;
            public float phase;
            public Vector2 floatFreq;
            public float pulseFreq;
        }
    }
}
