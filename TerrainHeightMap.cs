using System;
using System.Globalization;
using UnityEngine;

public class TerrainHeightMap : MonoBehaviour
{


    // The given OBJ terrain
    public TextAsset terrainFile;

    private string[] terrainText;

    // the needed size of the heightMap depends on your modelsize
    private float[,] heightMap = new float[3500, 3500];



    //each point of a triangle from the model
    private Vector3 vertex1;
    private Vector3 vertex2;
    private Vector3 vertex3;


    //each point of a triangle from the model in 2D, needed to find coordinates that could be inside the triangle
    private Vector2 p1;
    private Vector2 p2;
    private Vector2 p3;

    //Needed to find position of the vertices in the file
    private int index1;
    private int index2;
    private int index3;


   private int startX;
   private int endX;

    private int startZ;
    private int endZ;


    private string currentLine;

    private int lineIndex;

    private int fileOffset;

    private CultureInfo ci;

  

    void Awake()
    {
        // CultureInfo is needed to parse the the data correctly
        ci = new CultureInfo("en-US");


        lineIndex = 0;

        vertex1 = new Vector3();
        vertex2 = new Vector3();
        vertex3 = new Vector3();

        p1 = new Vector2();
        p2 = new Vector2();
        p3 = new Vector2();

        ReadTerrainFile();
        ProcessHeightMap();

    }
  


    private void ProcessHeightMap()
    {


        while (lineIndex <terrainText.Length)
        {
            currentLine = terrainText[lineIndex];
            lineIndex++;
         

           if(currentLine == null)
            {
                break;
            }

            // if line starts with f, it contains the indexes of the vertices which are building one triangle
            if (currentLine.StartsWith("f"))
            {
                string[] indexes = currentLine.Split(' ');




                //Now we get the index of each vertex
                for (int i = 1; i < indexes.Length; i++)
                {
                    string index = indexes[i];
                    string[] nextIndexes = index.Split('/');




                    switch (i)
                    {
                        case 1:
                            int.TryParse(nextIndexes[0], out index1);
                            break;
                        case 2:
                            int.TryParse(nextIndexes[0], out index2);
                            break;
                        case 3:
                            int.TryParse(nextIndexes[0], out index3);
                            break;

                    }

                }
                    // Now we find the vertices of the triangle and save them in a 3d and 2d Vector
                    try
                    {
                        string[] vertexLine = terrainText[index1 + fileOffset].Split(' ');

                        vertex1.x = float.Parse(vertexLine[1], ci);
                        vertex1.y = float.Parse(vertexLine[2], ci);
                        vertex1.z = float.Parse(vertexLine[3], ci);


                        vertexLine = terrainText[index2 + fileOffset].Split(' ');
                        vertex2.x = float.Parse(vertexLine[1], ci);
                        vertex2.y = float.Parse(vertexLine[2], ci);
                        vertex2.z = float.Parse(vertexLine[3], ci);


                        vertexLine = terrainText[index3 + fileOffset].Split(' ');
                        vertex3.x = float.Parse(vertexLine[1], ci);
                        vertex3.y = float.Parse(vertexLine[2], ci);
                        vertex3.z = float.Parse(vertexLine[3], ci);


                        p1 = new Vector2(vertex1.x, vertex1.z);
                        p2 = new Vector2(vertex2.x, vertex2.z);
                        p3 = new Vector2(vertex3.x, vertex3.z);

                    }
                    catch(Exception e)
                    {

                        Debug.Log("Vertices couldnt be parsed, Check format or cultureinfo" +e.Message);
                        break;
                    }

                    Sort();

                    float X;
                    float Z;


                    // Now all possible points will be checked to be in the triangle, basically half of the points wont be in the triangle which consults in the O-Nation  O(2n)
                    for (int f = startX; f <= endX; f++)
                    {
                        for (int d = startZ; d <= endZ; d++)
                        {
                           
                            // depending on your project it could be a good idea to overwrite this behaviour
                          
                          
                                X = f + 0.5f;
                                Z = d + 0.5f;


                                if (IsPointInsideTriangle(p1, p2, p3, new Vector2(X, Z)))
                                {

                            // if a point is inside the triangle, the heightmap stores the height of the position
                            // in this case we only store the highest point
                            if (GetHeight(vertex1, vertex2, vertex3, new Vector2(X, Z)) > heightMap[f,d])
                                    heightMap[f, d] = GetHeight(vertex1, vertex2, vertex3, new Vector2(X, Z));


                                }                    
                        }
                    }
            }
        }
    }



    private void ReadTerrainFile()
    {

        //creates an String Array that contains each line of the terrainFile

        terrainText = terrainFile.text.Split('\n');
   

        //finds the line where the vertices start
        for(int i =0; i < terrainText.Length; i++)
        {
            if (terrainText[i].StartsWith("v "))
            {
                fileOffset = i-1;
             
                break;
            }

        }

    }






    //This method checks if  point p is inside a given triangle
    private bool IsPointInsideTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
    {

        float d = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));
        float nomA = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y));
        float nomB = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y));
        float a = nomA / d;
        float b = nomB / d;
        float c = 1 - a - b;
        //(0 <=  a <= 1 && 0 <= b <= 1 a&& 0 <= c <= 1);

        return ((0 <= a && a <= 1) && (0 <= b && b <= 1) && (0 <= c && c <= 1));

    }

    //this method calculates the height of a given 2D point in a 3D triangle
    private float GetHeight(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 pos)
    {
        float det = (p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z);
        float l1 = ((p2.z - p3.z) * (pos.x - p3.x) + (p3.x - p2.x) * (pos.y - p3.z)) / det;
        float l2 = ((p3.z - p1.z) * (pos.x - p3.x) + (p1.x - p3.x) * (pos.y - p3.z)) / det;
        float l3 = 1.0f - l1 - l2;
        return l1 * p1.y + l2 * p2.y + l3 * p3.y;
    }



    // Sorts the coordinates and finds the intervall within to search for possible points inside a triangle 
    private void Sort()
    {


      
        float smallestX;
        float greatestX;

        if (p1.x <= p2.x && p1.x <= p3.x)
        {
            smallestX = p1.x;
        }
        else if (p2.x <= p3.x && p2.x <= p1.x)
        {
            smallestX = p2.x;
        }
        else
        {
            smallestX = p3.x;
        }

        if (p1.x >= p2.x && p1.x >= p3.x)
        {
            greatestX = p1.x;
        }
        else if (p2.x >= p3.x && p2.x >= p1.x)
        {
            greatestX = p2.x;
        }
        else
        {
            greatestX = p3.x;
        }


        int smaller = (int)(smallestX);
        int greater = (int)(greatestX);

        float checkSmaller = 0.5f + smaller;
        float checkGreater = 0.5f + greater;


       

        if (checkGreater > greatestX)
        {
            endX = greater - 1;
            if (endX < 0)
            {
                endX = 0;
            }
        }
        else
        {
           
            endX = greater;
        }

        if (checkSmaller < smallestX)
        {
            startX = smaller + 1;
        }
        else
        {
           
            startX = smaller;
        }


      
        float greatestZ;
        float smallestZ;

        if (p1.y <= p2.y && p1.y <= p3.y)
        {
            smallestZ = p1.y;
        }
        else if (p2.y <= p3.y && p2.y <= p1.y)
        {
            smallestZ = p2.y;
        }
        else
        {
            smallestZ = p3.y;
        }

        if (p1.y >= p2.y && p1.y >= p3.y)
        {
            greatestZ = p1.y;
        }
        else if (p2.y >= p3.y && p2.y >= p1.y)
        {
            greatestZ = p2.y;
        }
        else
        {
            greatestZ = p3.y;
        }

        smaller = (int)smallestZ;
        greater = (int)greatestZ;
        checkSmaller = 0.5f + smaller;
        checkGreater = 0.5f + greater;


        

        if (checkGreater > greatestZ)
        {
            endZ = greater - 1;
            if (endZ < 0)
            {
                endZ = 0;
            }
        }
        else
        {
           
            endZ = greater;
        }

        if (checkSmaller < smallestZ)
        {
            startZ = smaller + 1;
        }
        else
        {
            
            startZ = smaller;
        }

    }




    // this method gives the height of a position
    // expects the x-coordinate to be positiv
    public float GetHeightOfPosition(Vector3 position)
    {

        try
        {
            return heightMap[(int)position.x, (int)position.z];
        }
        catch(IndexOutOfRangeException)
        {
            Debug.Log("Index out of Range, heightmap size is to small or the coordinates have the wrong format! Vector: " + position);
            return 0;
        }

    }

    //this method gives you the vector with terrainHeight
    //expects the actual coordinate
    public Vector3 GetPosition(Vector3 position)
    {
        try
        {
            return new Vector3(position.x, heightMap[(int) (position.x *(-1)), (int)position.z], position.z);
        }
        catch(IndexOutOfRangeException)
        {
            Debug.Log("Index out of Range, heightmap size is to small or the coordinates have the wrong format! Vector: " + position);
           

            return position;
        }

    }
}
