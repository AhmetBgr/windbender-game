using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public enum Direction {
    right,
    left,
    up,
    down,
    none
}

public static class Utility 
{
    static readonly Vector3[] vectorDirections = new Vector3[] {
        Vector3.right,
        Vector3.left,
        Vector3.up,
        Vector3.down,
        Vector3.zero
    };
    public static T[] ShuffleArray<T>(T[] array) {
		System.Random prng = new System.Random ();

		for (int i =0; i < array.Length -1; i ++) {
			int randomIndex = prng.Next(i,array.Length);
			T tempItem = array[randomIndex];
			array[randomIndex] = array[i];
			array[i] = tempItem;
		}

		return array; 
    }

    public static float RoundToNearestHalf(float val) {
        return Mathf.Round(val * 2) / 2;
    }
    public static Vector3 RoundToNearestHalf(Vector3 val) {
        return new Vector3(Mathf.Round(val.x * 2) / 2, Mathf.Round(val.y * 2) / 2, 0f);
    }

    public static IEnumerator SetActiveObjWithDelay(GameObject obj, bool active, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(active);
    }

	public static void SetActiveObj(GameObject obj, bool active){
		obj.SetActive(active);
	}

    public static IEnumerator ChangeTilemapColor(Tilemap tilemap, Color endColor, float duration, float delay = 0f, Action onCompleteCallBack = null)
    {
        yield return new WaitForSeconds(delay);

        Color startColor = tilemap.color;
        float startTime = Time.time;
        float time = 0;
        GameManager gameManager = GameManager.instance;
        while (time < duration)
        {
            float t = (Time.time - startTime) / duration;
            time += Time.deltaTime; // * gameManager.gameSpeed
            tilemap.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        if (onCompleteCallBack != null)
            onCompleteCallBack();
    }

    /*public static GameObject CheckForObjectAt(Vector3 pos, LayerMask lm)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, distance: 5f, lm);

        if (hit)
        {
            return hit.transform.gameObject;
        }
        return null;

    }

    public static GameObject CheckForObjectFrom(Vector3 pos, Vector3 dir, float distance, LayerMask lm)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, dir, distance: distance, lm);

        if (hit)
        {
            return hit.transform.gameObject;
        }
        return null;

    }*/

    public static void BinarySerialization(string folderName, string fileName, object saveData)
    {
        BinaryFormatter bf = new BinaryFormatter();
        string path = Application.persistentDataPath + folderName;

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        FileStream file = File.Create(path + "/" + fileName + ".save");
        bf.Serialize(file, saveData);
        file.Close();
    }

    public static object BinaryDeserialization(string folderName, string fileName)
    {
        string filePath = Application.persistentDataPath + folderName + fileName + ".save";

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.Open);

        var saveData = bf.Deserialize(file);
        file.Close();

        return saveData;
    }

    public static Vector3 DirToVectorDir(Direction dir)
    {
        return vectorDirections[(int)dir];
    }

    public static Direction VectorDirToDir(Vector3 vectorDir)
    {
        if (vectorDir == Vector3.right)
            return Direction.right;
        else if (vectorDir == Vector3.left)
            return Direction.left;
        else if (vectorDir == Vector3.up)
            return Direction.up;
        else if (vectorDir == Vector3.down)
            return Direction.down;
        else
            return Direction.none;
    }
}
