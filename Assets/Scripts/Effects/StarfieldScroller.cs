using UnityEngine;

namespace RetroSpaceInvader.Effects
{
    /// <summary>
    /// 고전 아케이드 우주 배경 별빛 스크롤러
    /// (Pygame _create_stars 와 1:1 대응)
    /// </summary>
    public class StarfieldScroller : MonoBehaviour
    {
        private struct Star
        {
            public GameObject obj;
            public float speed;
        }

        private const int StarCount = 70;
        private Star[] _stars;

        private void Start()
        {
            _stars = new Star[StarCount];
            Texture2D dotTexture = new Texture2D(2, 2);
            dotTexture.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
            dotTexture.Apply();
            Sprite starSprite = Sprite.Create(dotTexture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);

            for (int i = 0; i < StarCount; i++)
            {
                GameObject starObj = new GameObject($"Star_{i}");
                starObj.transform.SetParent(transform);

                float x = Random.Range(-400f, 400f);
                float y = Random.Range(-300f, 300f);
                starObj.transform.localPosition = new Vector3(x, y, 10f);

                SpriteRenderer sr = starObj.AddComponent<SpriteRenderer>();
                sr.sprite = starSprite;
                float brightness = Random.Range(0.35f, 0.95f);
                sr.color = new Color(brightness, brightness, brightness, 0.9f);

                float scale = Random.Range(1.5f, 3.5f);
                starObj.transform.localScale = new Vector3(scale, scale, 1f);

                _stars[i] = new Star
                {
                    obj = starObj,
                    speed = Random.Range(20f, 60f)
                };
            }
        }

        private void Update()
        {
            for (int i = 0; i < StarCount; i++)
            {
                Vector3 pos = _stars[i].obj.transform.localPosition;
                pos.y -= _stars[i].speed * Time.deltaTime;
                if (pos.y < -300f)
                {
                    pos.y = 300f;
                    pos.x = Random.Range(-400f, 400f);
                }
                _stars[i].obj.transform.localPosition = pos;
            }
        }
    }
}
