using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一定間隔でランダムに環境音(きしみ音、遠くの物音など)を3D再生する。
/// 廊下や部屋にこのコンポーネントを置くだけで環境ノイズが自動的に鳴り続ける。
/// </summary>
public class AmbientOneShotPlayer : MonoBehaviour
{
    [Serializable]
    public class AmbientSound
    {
        public string seId;
        [Range(0f, 1f)] public float weight = 1f;
    }

    [SerializeField] private List<AmbientSound> sounds = new();
    [SerializeField] private float minInterval = 8f;
    [SerializeField] private float maxInterval = 25f;
    [SerializeField] private float radius = 5f;

    private void OnEnable()
    {
        StartCoroutine(PlayLoop());
    }

    private IEnumerator PlayLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(minInterval, maxInterval));
            PlayRandom();
        }
    }

    private void PlayRandom()
    {
        if (sounds.Count == 0 || AudioManager.Instance == null) return;

        float totalWeight = 0f;
        foreach (var s in sounds) totalWeight += s.weight;
        if (totalWeight <= 0f) return;

        float r = UnityEngine.Random.Range(0f, totalWeight);
        foreach (var s in sounds)
        {
            if (r <= s.weight)
            {
                PlayAt(s.seId);
                return;
            }
            r -= s.weight;
        }
    }

    private void PlayAt(string id)
    {
        Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;
        Vector3 pos = transform.position + new Vector3(offset.x, 0f, offset.y);
        AudioManager.Instance.PlaySEAtPoint(id, pos);
    }
}
