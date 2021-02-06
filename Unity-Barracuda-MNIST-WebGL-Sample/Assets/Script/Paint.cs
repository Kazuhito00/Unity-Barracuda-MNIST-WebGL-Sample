using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

using Unity.Barracuda;

public class Paint : MonoBehaviour
{
    // 描画エリアサイズ
    const int width = 28;
    const int height = 28;

    // ペイント関連
    Color[] drawBuffer;
    Texture2D drawTexture;
    Vector2 prevPoint;
    bool drawStartPointFlag = true;
    
    // MNISTモデル関連
    public NNModel modelAsset;    
    private MNIST mnist;

    // 推論結果描画用テキスト
    public Text text;

    void Start()
    {
        //  描画用バッファ
        drawBuffer = new Color[width * height];

        // 描画用テクスチャ準備
        drawTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        drawTexture.filterMode = FilterMode.Point;
        ClearCanvas();

        // MNIST推論用クラス
        mnist = new MNIST(modelAsset);
    }

    void Update()
    {
        // 線の描画色
        int color_r = 0;
        int color_g = 0;
        int color_b = 0;
        Color color = new Color((float)(color_r/255f), (float)(color_g/255f), (float)(color_b/255f));

        // マウス左クリック時にオブジェクトの座標情報を取得
        bool raycastResult = false;
        var drawPoint = new Vector2(0, 0);
        if (Input.GetMouseButton(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            raycastResult = Physics.Raycast(ray, out hit, 100.0f);
            if (raycastResult) {
                drawPoint = new Vector2(hit.textureCoord.x * drawTexture.width, hit.textureCoord.y * drawTexture.height);
            }
        }
        // マウス軌跡を描画
        if (raycastResult) {
            // 開始点
            if (drawStartPointFlag) {
                DrawPoint(drawPoint, color);
            }
            // ドラッグ状態
            else {
                DrawLine(prevPoint, drawPoint, color);
            }

            drawStartPointFlag = false;
            prevPoint = drawPoint;
        } else {
            drawStartPointFlag = true;
        }
        // 描画バッファをテクスチャへ反映
        drawTexture.SetPixels(drawBuffer);
        drawTexture.Apply();
        GetComponent<Renderer>().material.mainTexture = drawTexture;

        // MNIST推論、および画面反映
        MnistInferenceAndDisplay();
    }

    private void DrawPoint(Vector2 point, Color color, double brushSize = 1.0f)
    {
        point.x = (int)point.x;
        point.y = (int)point.y;

        int start_x = Mathf.Max(0, (int)(point.x - (brushSize - 1)));
        int end_x = Mathf.Min(drawTexture.width, (int)(point.x + (brushSize + 1)));
        int start_y =  Mathf.Max(0, (int)(point.y - (brushSize - 1)));
        int end_y = Mathf.Min(drawTexture.height, (int)(point.y + (brushSize + 1)));

        for (int x = start_x; x < end_x; x++) {
            for (int y = start_y; y < end_y; y++) {
                double length = Mathf.Sqrt(Mathf.Pow(point.x - x, 2) + Mathf.Pow(point.y - y, 2));
                if (length < brushSize) {
                    drawBuffer.SetValue(color, x + drawTexture.width * y);
                }
            }
        }
    }

    private void DrawLine(Vector2 point1, Vector2 point2, Color color, int lerpNum = 10)
    {
        for(int i=0; i < lerpNum + 1; i++) {
            var point = Vector2.Lerp(point1, point2, i * (1.0f / lerpNum));
            DrawPoint(point, color);
        }
    }

    public void ClearCanvas()
    {
        int color_r = 255;
        int color_g = 255;
        int color_b = 255;
        Color color = new Color((float)(color_r/255f), (float)(color_g/255f), (float)(color_b/255f));
        
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var drawPoint = new Vector2(x, y);
                DrawPoint(drawPoint, color);
            }
        }
    }

    private void MnistInferenceAndDisplay()
    {
        // Input用バッファ準備
        string resultText = "";
        float[] input = new float[width * height];
        int copyCount = 0;
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                input[copyCount] = (drawBuffer[(width - x - 1) + (y * width)].r - 1) * -1; // 左右反転
                copyCount++;
            }
        }

        // 推論
        var scores = mnist.Inference(input);

        // 描画用テキスト構築
        for (int i = 0; i < scores.Length; i++)
        {
            float score = scores[i];
            resultText = resultText + i.ToString() + " : " + score.ToString("F3") + "\n";
        }
        resultText = resultText + "\n";

        var maxScore = float.MinValue;
        int maxIndex = -1;
        for (int i = 0; i < scores.Length; i++)
        {
            float score = scores[i];
            if ((maxScore < score) && (0.5f < score))
            {
                maxScore = score;
                maxIndex = i;
            }
        }
        if (maxIndex >= 0) {
            resultText = resultText + "Result:" + maxIndex.ToString();
        } else {
            resultText = resultText + "Result:?";
        }

        // テキスト画面反映
        text.text = resultText;
    }
}
