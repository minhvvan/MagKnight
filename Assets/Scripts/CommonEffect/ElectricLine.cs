using UnityEngine;

public class ElectricLine : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public int segmentCount = 10;
    public float noiseStrength = 0.3f;
    public float animationSpeed = 2f;

    private LineRenderer[] lines;
    private Vector3[][] positions;

    void Awake()
    {
        lines = GetComponentsInChildren<LineRenderer>();
        foreach (var line in lines)
        {
            line.positionCount = segmentCount;
            line.useWorldSpace = true;
        }
        positions = new Vector3[lines.Length][];
        for (int i = 0; i < lines.Length; i++)
        {
            positions[i] = new Vector3[segmentCount];
        }
    }

    void Update()
    {
        int lineIndex = 0;
        foreach(var line in lines)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                float t = i / (float)(segmentCount - 1);
                Vector3 point = Vector3.Lerp(startPosition, endPosition, t);

                float randomOffset = Random.Range(1f, 1.1f);

                // 노이즈를 통한 지글거림
                Vector3 noise = new Vector3(
                    Mathf.PerlinNoise(Time.time * randomOffset * animationSpeed + i, 0) - 0.5f,
                    Mathf.PerlinNoise(0, Time.time * randomOffset * animationSpeed + i) - 0.5f,
                    Mathf.PerlinNoise(Time.time * randomOffset * animationSpeed + i, 1) - 0.5f
                ) * noiseStrength * randomOffset;

                positions[lineIndex][i] = point + noise;
            }
            line.SetPositions(positions[lineIndex]);

            lineIndex++;
        }
    }

    public void ShowEffect(Vector3 start, Vector3 end)
    {
        startPosition = start;
        endPosition = end;

        gameObject.SetActive(true);
    }

    public void HideEffect()
    {
        gameObject.SetActive(false);
    }
}