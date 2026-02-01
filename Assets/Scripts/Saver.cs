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
    public const string fileExtension = ".vec2";
    public static GameObject objectPrefab;
    public static bool Loading { get; private set; } = false;

    public static void SaveState(string fileName)
    {
        fileName += fileExtension;
        SaveData data = new();
        foreach (var physObj in physObjects)
        {
            var props = physObj.Properties;
            ObjectData objData = new() { name = physObj.gameObject.name, scale = physObj.transform.localScale, pos = props.pos, rot = props.Rot, v = props.v, av = props.av,
                a = props.a, aa = props.aa, f = props.f, t = props.t, p = props.p, ke = props.ke, m = props.m,
                e = props.e, cof = props.cof, moi = props.moi };

            objData.spriteData = FromSprite(physObj.gameObject.GetComponent<SpriteRenderer>().sprite);

            objData.components.Add(props.GetType().Name);
            foreach (var comp in physObj.gameObject.GetComponents<V2Component>())
            {
                objData.components.Add(comp.GetType().Name);
            }

            data.data.Add(objData);
        }
        string json = ToJson(data, true);
        File.WriteAllText(fileName, json);
    }

    public static void LoadState(string fileName)
    {
        Loading = true;
        fileName += fileExtension;
        if (File.Exists(fileName))
        {
            // clear current state

            ClearState();

            // load state from file

            string json = File.ReadAllText(fileName);
            var data = FromJson<SaveData>(json);

            foreach (var objData in data.data)
            {
                var obj = GameObject.Instantiate(objectPrefab);

                obj.name = objData.name;
                obj.transform.localScale = objData.scale;
                obj.GetComponent<SpriteRenderer>().sprite = objData.spriteData.ToSprite();

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
        }
        Loading = false;
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
    public float xMin, xMax, yMin, yMax;
    public Vector2 pivot;
    public byte[] textureData;

    public static SpriteData FromSprite(Sprite sprite)
    {
        SpriteData data = new()
        {
            name = sprite.name,
            xMin = sprite.rect.xMin,
            xMax = sprite.rect.xMax,
            yMin = sprite.rect.yMin,
            yMax = sprite.rect.yMax,
            pivot = sprite.pivot,
        };

        var readableTex = MakeReadable(sprite.texture);
        data.textureData = readableTex.EncodeToPNG();

        return data;
    }

    public Sprite ToSprite()
    {
        Texture2D texture = new(2, 2);
        texture.LoadImage(textureData);

        Rect rect = new(xMin, yMin, xMax, yMax);

        var sprite = Sprite.Create(texture, rect, pivot);
        sprite.name = name;
        return sprite;
    }

    public static Sprite ToSprite(SpriteData data)
    {
        Texture2D texture = new(2, 2);
        texture.LoadImage(data.textureData);

        Rect rect = new(data.xMin, data.yMin, data.xMax, data.yMax);
        var pivot = data.pivot;

        var sprite = Sprite.Create(texture, rect, pivot);
        sprite.name = data.name;
        return sprite;
    }
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

        Texture2D readable = new Texture2D(source.width, source.height);
        readable.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readable.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        return readable;
    }
}