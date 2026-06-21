using UnityEngine;
using System.IO;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Collections;




#if UNITY_EDITOR
using UnityEditor;
#endif


public class LevelPreviewImageGenerator : MonoBehaviour
{
    [SerializeField] private LevelFullMap _fullMap;
    [SerializeField] private RenderTexture _renderTexture;
    [SerializeField] private int _screenshotWidth = 1920;
    [SerializeField] private int _screenshotHeight = 1080;
    [SerializeField] private string _outputFolderName = "LevelPreviews";
    private string _outputFolderPath { get => "Assets/Resources/" + _outputFolderName; }
    private FilmGrain _filmGrain;
    private bool _filmGrainWasActive;

    public static LevelPreviewImageGenerator Singleton;

    private void Awake()
    {
        Helpers.CreateSingleton<LevelPreviewImageGenerator>(ref Singleton, this);
    }


    public IEnumerator GenerateLevelScreenshotsCoroutine()
    {
        EnsureOutputFolderExists();
        
        for(int i = 0; i < _fullMap.Levels.Length; i++)
        {


            _fullMap.LoadLevel(i);
            
            DisableFilmGrain();

            // Wait for a frame to ensure everything is rendered
            
            Level level = _fullMap.GetActiveLevel();

            Vector3 initalCamPos = Camera.main.transform.position;
            Quaternion initalCamRot = Camera.main.transform.rotation;
            
            Camera.main.fieldOfView = FindFirstObjectByType<SimpleWalker>().CameraMinFov;

            Camera.main.transform.position = level.CameraPreviewPosition.position;
            Camera.main.transform.rotation = level.CameraPreviewPosition.rotation;

            // allow multiple frames to render to ensure TAA and other other time effects have settled
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();

            Camera.main.Render();
            
            SaveRenderTextureTofile(i);

            RestoreFilmGrain();
        }

        // refresh assets if in editor
        #if UNITY_EDITOR
        SetTextureImportSettings();
        AssetDatabase.Refresh();
        #endif
    }
    
    private void DisableFilmGrain()
    {
        Volume volume = FindFirstObjectByType<Volume>();

        if (volume != null &&
            volume.profile.TryGet(out _filmGrain))
        {
            _filmGrainWasActive = _filmGrain.active;
            _filmGrain.active = false;
        }
    }
    private void RestoreFilmGrain()
    {
        if (_filmGrain != null)
        {
            _filmGrain.active = _filmGrainWasActive;
        }
    }


    private void SaveRenderTextureTofile(int levelIndex)
    {
        RenderTexture.active = _renderTexture;
        
        Texture2D screenshot = new Texture2D(_screenshotWidth, _screenshotHeight, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, _screenshotWidth, _screenshotHeight), 0, 0);
        screenshot.Apply();
        
        byte[] bytes = screenshot.EncodeToPNG();
        
        // screenshots must be saved in the Resources folder, to allow loading through code in GetPreviewFromIndex()
        string fileName = "level_" + levelIndex + ".png";
        string filePath = Path.Combine(_outputFolderPath, fileName);
        
        File.WriteAllBytes(filePath, bytes);
        
        DestroyImmediate(screenshot);
        Debug.Log($"Saved screenshot: {filePath}");
    }

    public Texture GetPreviewFromIndex(int levelIndex)
    {
        string texturePath = $"{_outputFolderName}/level_{levelIndex}";
        Texture2D previewTexture = Resources.Load<Texture2D>(texturePath);
        return previewTexture;
    }


    #if UNITY_EDITOR
    private void SetTextureImportSettings()
    {
        string[] pngFiles = Directory.GetFiles(_outputFolderPath, "*.png");
        
        foreach (string filePath in pngFiles)
        {
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            
            if (importer != null)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.isReadable = false;

                AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            }
        }
        
        Debug.Log("Set all screenshots to no compression");
    }
    #endif

    private void EnsureOutputFolderExists()
    {
        if (!Directory.Exists(_outputFolderPath))
        {
            Directory.CreateDirectory(_outputFolderPath);
            Debug.Log($"Created output folder: {_outputFolderPath}");
        }
    }
}

