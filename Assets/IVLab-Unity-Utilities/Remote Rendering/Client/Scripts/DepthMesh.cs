using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class DepthMesh
{

    public static byte GetDepthAt(byte[] rgbd, int texWidth, int texHeight, Vector2 uv)
    {
        int row = (int)(uv.x * (texWidth - 1));
        int col = (int)(uv.y * (texHeight - 1));

        return rgbd[4 * texWidth * col + 4 * row + 3];

    }
    public static Vector3 GetMeshCoordAt(Vector2 uv)
    {
        return new Vector3(Mathf.Lerp(-0.5f, 0.5f, uv.x), Mathf.Lerp(-0.5f, 0.5f, uv.y), 0);
    }

    public static Vector3 GetDisplacedMeshVertForDepth(byte depth, Vector3 eyePosition, Vector2 uv)
    {
        Vector3 vert = Vector3.Lerp(eyePosition, GetMeshCoordAt(uv), depth / 255.0f);
        //Vector3 vert = Vector3.Lerp(GetMeshCoordAt(uv) + new Vector3(0, 0, -10), GetMeshCoordAt(uv), depth) ;
        return vert;
    }
    public static Vector3 GetDisplacedMeshCoordAt(byte[] rgbd, int texWidth, int texHeight, Vector3 eyePosition, Vector2 uv)
    {
        byte depth = GetDepthAt(rgbd, texWidth, texHeight, uv);
        return GetDisplacedMeshVertForDepth((byte)(depth), eyePosition, uv);
    }

    static Vector2 uvFromIndex(ValueTuple<int, int> index, int width, int height)
    {
        return new Vector2(index.Item1 * 1f / width, index.Item2 * 1f / height);
    }
    public static void MakeDepthMesh(
        byte[] rgbd,
        int texWidth,
        int texHeight,
        float eyetoFarPlaneDist,
        int depthChangeThreshold,
        int maxDepth,
        int meshCols,
        int meshRows,
        ref List<Vector3> vertices,
        ref List<Vector2> uvs,
        ref List<Vector2> depthUV,
        ref List<int> indices)
    {
        vertices.Clear();
        uvs.Clear();
        depthUV.Clear();
        indices.Clear();

        Dictionary<System.ValueTuple<int, int>, int> vertHash = new Dictionary<System.ValueTuple<int, int>, int>();
        Dictionary<System.ValueTuple<int, int>, float> depthHash = new Dictionary<System.ValueTuple<int, int>, float>();

        Vector3 eyePosition = new Vector3(0, 0, -eyetoFarPlaneDist);

        float deltaU = 1f / meshCols;
        float deltaV = 1f / meshRows;

        float deltaDepth = 1 / 255.0f;
        int maxDepthSteps = depthChangeThreshold;
        /*
        // Populate first full column of verts
        for (int row = 0; row <= meshRows; row++)
        { 
            var index = (0, row);

            float u = 0;
            float v = row * 1f / meshRows;
            var vertex = GetDisplacedMeshCoordAt(rgbd, texWidth, texHeight, eyePosition, u, v);
            vertices.Add(vertex);
            uvs.Add(new Vector2(u, v));
            float depth = GetDepthAt(rgbd, texWidth, texHeight, u, v);
            depthHash.Add(index, depth);

            depthUV.Add(new Vector2(depth, 0));
            vertHash.Add(index, vertices.Count-1);
        }

        // Populate first full row of verts minus corner
        for (int col = 1; col <= meshCols; col++)
        {
            var index = (col, 0);

            float u = col * 1f / meshCols;
            float v = 0;
            var vertex = GetDisplacedMeshCoordAt(rgbd, texWidth, texHeight, eyePosition, u, v);
            vertices.Add(vertex);
            uvs.Add(new Vector2(u, v));
            float depth = GetDepthAt(rgbd, texWidth, texHeight, u, v);
            depthHash.Add(index, depth) ;

            depthUV.Add(new Vector2(depth, 0));

            vertHash.Add(index, vertices.Count - 1);
        }
        */
        try
        {
            byte[,] depthLookup = new byte[meshCols + 1, meshRows + 1];
            Dictionary<ValueTuple<int, int, byte>, int> indexLookup = new Dictionary<(int, int, byte), int>();

            //Parallel.For(0, (meshRows + 1) * (meshCols + 1), (i, state) =>
            //{
            //    int row = i / (meshRows + 1);
            //    int col = i % (meshRows + 1);
            //    float v = row * 1f / meshRows;
            //    float u = col * 1f / meshCols;

            //    depthLookup[col, row] = GetDepthAt(rgbd, texWidth, texHeight, new Vector2(u, v));
            //});

            var eye = new Vector3(0.5f, 0.5f, 0);
            for (int row = 1; row <= meshRows; row++)
            {
                float v = row * 1f / meshRows;

                for (int col = 1; col <= meshCols; col++)
                {

                    float u = col * 1f / meshCols;

                    var indexThis = (col, row);
                    var indexAbove = (col, row - 1);
                    var indexLeftAbove = (col - 1, row - 1);
                    var indexLeft = (col - 1, row);



                    var indexList = new ValueTuple<int, int>[] { indexThis, indexAbove, indexLeftAbove, indexLeft }.ToArray();

                    var uvList = indexList.Select(i => uvFromIndex(i, meshCols, meshRows)).ToArray();
                    var depthList = uvList.Select(uv => GetDepthAt(rgbd, texWidth, texHeight, uv)).ToArray();

                    var depthIndices = depthList.Select((val, i) => new { Value = val, Index = i })
                                    .OrderByDescending(x => x.Value)
                                    .Select(x => x.Index).ToArray();

                    int previousToKeep = 0;
                    float previousDepth = depthList[depthIndices[0]];
                    int countAtMaxDepth = 0;
                    for (int i = 1; i <= 4; i++)
                    {
                        previousToKeep++;

                        float thisDepth = 0;

                        if (i < 4) thisDepth = depthList[depthIndices[i]];
                        if (Mathf.Abs(previousDepth - thisDepth) >  maxDepthSteps || i == 4)
                        {
                            int count = 0;
                            float depthSum = 0;
                            for (int j = i - 1; j >= i - previousToKeep; j--)
                            {
                                count++;
                                depthSum += depthList[depthIndices[j]];
                            }
                            float depthAvg = depthSum / count;

                            float[] newDepthList = new float[4] { depthAvg, depthAvg, depthAvg, depthAvg };


                            if (false && previousToKeep == 3)
                            {
                                Vector3[] pins = new Vector3[3];
                                int I = 0;
                                for (int j = i - 1; j >= i - previousToKeep; j--)
                                {

                                    newDepthList[depthIndices[j]] = depthList[depthIndices[j]];
                                    pins[I] = new Vector3(uvList[depthIndices[j]].x, uvList[depthIndices[j]].y, depthList[depthIndices[j]]);
                                    I++;
                                }

                                Plane plane = new Plane(pins[0], pins[1], pins[2]);
                                
                                for(int J = 0; J < 4; J++)
                                {
                                    var dir = new Vector3(uvList[J].x, uvList[J].y, depthList[J]);
                                    Ray ray = new Ray(eye, dir);
                                    float intersectP = 0;
                                    plane.Raycast(ray, out intersectP);
                                    newDepthList[J] = Mathf.RoundToInt(Mathf.Abs(intersectP));

                                }
                            }
                            for (int j = i - 1; j >= i - previousToKeep; j--)
                            {
                                newDepthList[depthIndices[j]] = depthList[depthIndices[j]];
                            }

                            countAtMaxDepth = 0;
                            for (int k = 0; k < 4; k++)
                            {
                                if (newDepthList[k] > maxDepth) countAtMaxDepth++;
                            }
                            if (countAtMaxDepth < 4)
                                for (int k = 0; k < 4; k++)
                                {

                                    vertices.Add(GetDisplacedMeshVertForDepth((byte)(newDepthList[k]), eyePosition, uvList[k]));
                                    uvs.Add(uvList[k]);
                                    depthUV.Add(new Vector2(newDepthList[k] / 255.0f, 0));
                                    indices.Add(vertices.Count - 1);
                                }
                            previousToKeep = 0;
                        }
                        previousDepth = thisDepth;
                    }

                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        /*
        var vertex = GetDisplacedMeshCoordAt(rgbd, texWidth, texHeight, eyePosition, u, v);
        vertices.Add(vertex);
        uvs.Add(new Vector2(u, v));
        float depth = GetDepthAt(rgbd, texWidth, texHeight, u, v);
        depthHash.Add(index, depth);
        depthUV.Add(new Vector2(depth, 0));
        vertHash.Add(index, vertices.Count - 1);
        */
        //if(false)
        //try
        //{
        //    /*
        //    var vert = vertHash[(col, row)];
        //    var vertLeftAbove = vertHash[(col - 1, row - 1)];
        //    var vertLeft = vertHash[(col - 1, row)];
        //    var vertAbove = vertHash[(col, row - 1)];
        //    */
        //    var uv = uvThis;
        //   // vertices.Add(GetDisplacedMeshVertForDepth(depthThis, eyePosition, uv));
        //    uvs.Add(uvThis);
        //    depthUV.Add(new Vector2(depthThis, 0));
        //    indices.Add(vertices.Count - 1);

        //    uv = uvAbove;
        //    vertices.Add(GetDisplacedMeshCoordAt(rgbd, texWidth, texHeight, eyePosition, uv));
        //    uvs.Add(uv);
        //    depthUV.Add(new Vector2(GetDepthAt(rgbd, texWidth, texHeight, uv), 0));
        //    indices.Add(vertices.Count - 1);

        //    uv = uvLeftAbove;
        //    vertices.Add(GetDisplacedMeshCoordAt(rgbd, texWidth, texHeight, eyePosition, uv));
        //    uvs.Add(uv);
        //    depthUV.Add(new Vector2(GetDepthAt(rgbd, texWidth, texHeight, uv), 0));
        //    indices.Add(vertices.Count - 1);

        //    uv = uvLeft;
        //    vertices.Add(GetDisplacedMeshCoordAt(rgbd, texWidth, texHeight, eyePosition, uv));
        //    uvs.Add(uv);
        //    depthUV.Add(new Vector2(GetDepthAt(rgbd, texWidth, texHeight, uv), 0));
        //    indices.Add(vertices.Count - 1);

        //    //if ((depthUV] > 254.0 / 255.0) && (depthHash[(col - 1, row)] > 254.0 / 255.0) && (depthHash[(col - 1, row - 1)] > 254.0 / 255.0) && (depthHash[(col, row - 1)] > 254.0 / 255.0)) continue ;
        //    /*
        //    indices.Add(vert);
        //    indices.Add(vertAbove);
        //    indices.Add(vertLeftAbove);
        //    indices.Add(vertLeft);
        //    */


        //} catch(Exception e)
        //{
        //    Debug.LogError((col, row));
        //    Debug.LogError(e);
        //}





    }
}




