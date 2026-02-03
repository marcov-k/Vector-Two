using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using static V2Objects;
using static UnityEngine.JsonUtility;
using static SpriteData;
using static TextureUtils;

public static class Saver
{
    public const string folderName = "SavedStates";
    public const string fileExtension = ".vec2";
    public static GameObject objectPrefab;
    public static bool Loading { get; private set; } = false;
    static string lastSave;

    public static bool SaveState(string fileName)
    {
        if (fileName == string.Empty) return false;

        string filePath = Path.Combine(Application.dataPath, folderName, fileName + fileExtension);

        SaveData data = new();
        foreach (var physObj in physObjects)
        {
            var props = physObj.Properties;
            ObjectData objData = new() { name = physObj.gameObject.name, scale = physObj.transform.localScale, pos = props.pos, rot = props.Rot, v = props.v, av = props.av,
                a = props.a, aa = props.aa, f = props.f, t = props.t, p = props.p, ke = props.ke, m = props.m,
                e = props.e, cof = props.cof, moi = props.moi };

            var renderer = physObj.GetComponent<SpriteRenderer>();

            objData.spriteData = FromSprite(renderer.sprite);

            objData.rendererData = new() { color = renderer.color, flipX = renderer.flipX, flipY = renderer.flipY, sortingID = renderer.sortingLayerID, sortingOrder = renderer.sortingOrder };

            objData.components.Add(props.GetType().Name);
            foreach (var comp in physObj.gameObject.GetComponents<V2Component>())
            {
                objData.components.Add(comp.GetType().Name);
            }

            data.data.Add(objData);
        }

        string json = ToJson(data, true);
        File.WriteAllText(filePath, json);

        lastSave = fileName;
        return true;
    }

    public static bool LoadState(string fileName)
    {
        bool success = false;
        Loading = true;
        string filePath = Path.Combine(Application.dataPath, folderName, fileName + fileExtension);

        if (File.Exists(filePath))
        {
            // clear current state

            ClearState();

            // load state from file

            string json = File.ReadAllText(filePath);
            var data = FromJson<SaveData>(json);

            foreach (var objData in data.data)
            {
                var obj = GameObject.Instantiate(objectPrefab);

                obj.name = objData.name;
                obj.transform.localScale = objData.scale;

                var renderer = obj.GetComponent<SpriteRenderer>();

                renderer.sprite = objData.spriteData.ToSprite();
                renderer.color = objData.rendererData.color;
                (renderer.flipX, renderer.flipY) = (objData.rendererData.flipX, objData.rendererData.flipY);
                (renderer.sortingLayerID, renderer.sortingOrder) = (objData.rendererData.sortingID, objData.rendererData.sortingOrder);

                if (objData.components.Contains("Properties"))
                {
                    var comp = Type.GetType("Properties");
                    obj.AddComponent(comp);
                    objData.components.Remove("Properties");
                }
                if (objData.components.Contains("PhysObject"))
                {
                    var comp = Type.GetType("PhysObject");
                    obj.AddComponent(comp);
                    objData.components.Remove("PhysObject");
                }
                foreach (string compName in objData.components)
                {
                    var comp = Type.GetType(compName);
                    obj.AddComponent(comp);
                }

                var phys = obj.GetComponent<PhysObject>();
                phys.SetData(objData);
                var props = obj.GetComponent<Properties>();
                props.SetData(objData);
            }
            success = true;
        }

        lastSave = fileName;
        Loading = false;
        return success;
    }

    public static bool DeleteState(string fileName)
    {
        string filePath = Path.Combine(Application.dataPath, folderName, fileName + fileExtension);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        else return false;
    }

    public static void InitSaveDirectory()
    {
        string dirPath = Path.Combine(Application.dataPath, folderName);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
    }

    public static void ResetState()
    {
        LoadState(lastSave);
    }
}

[Serializable]
public class SaveData
{
    public List<ObjectData> data = new();
}

[Serializable]
public class ObjectData
{
    public string name;
    public SpriteData spriteData;
    public RendererData rendererData;
    public Vector3 scale;
    public List<string> components = new();
    public Vector2 pos, v, a, f, p;
    public float rot, av, aa, t;
    public float ke;
    public float m;
    public float e;
    public float cof;
    public float moi;
}

[Serializable]
public class SpriteData
{
    public string name;
    public Rect rect;
    public Vector2 pivot;
    public float ppu;
    public byte[] textureData;

    public static SpriteData FromSprite(Sprite sprite)
    {
        SpriteData data = new()
        {
            name = sprite.name,
            rect = sprite.rect,
            pivot = new(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height),
            ppu = sprite.pixelsPerUnit
        };

        var readableTex = MakeReadable(sprite.texture);
        data.textureData = readableTex.EncodeToPNG();

        return data;
    }

    public Sprite ToSprite()
    {
        Texture2D texture = new(1, 1);
        texture.LoadImage(textureData);

        var sprite = Sprite.Create(texture, rect, pivot, ppu);
        sprite.name = name;
        return sprite;
    }

    public static Sprite ToSprite(SpriteData data)
    {
        Texture2D texture = new(1, 1);
        texture.LoadImage(data.textureData);

        var sprite = Sprite.Create(texture, data.rect, data.pivot, data.ppu);
        sprite.name = data.name;
        return sprite;
    }
}

[Serializable]
public class RendererData
{
    public Color color;
    public bool flipX, flipY;
    public int sortingID, sortingOrder;
}

public static class TextureUtils
{
    public static Texture2D MakeReadable(Texture source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        Texture2D readable = new(source.width, source.height);
        readable.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readable.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        return readable;
    }
}