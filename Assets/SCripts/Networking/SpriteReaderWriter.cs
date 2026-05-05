using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpriteReaderWriter
{
    public static void WriteSprite(this NetworkWriter writer, Sprite sprite)
    {
        byte[] data = sprite.texture.GetRawTextureData();
        writer.WriteBytes(data, 0, data.Length);
    }

    public static Sprite ReadSprite(this NetworkReader reader)
    {
        byte[] data = reader.ReadBytes(reader.Length);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(data);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
    }
}
