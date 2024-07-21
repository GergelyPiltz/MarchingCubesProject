//using System.Collections;
//using System.Collections.Generic;
//using System.Threading;
//using UnityEngine;

//public class ChunkBuildThread
//{
//    private bool isRunning = true;
//    private int count = 0;
//    private List<CubicChunk> chunks = new();
//    private int delay = 10;

//    public void AddToQueue(CubicChunk chunk)
//    {
//        chunks.Add(chunk);
//        count++;
//    }

//    public void Run()
//    {
//        while (isRunning)
//        {
//            if (count > 0)
//            {
//                chunks[0].Build();
//                chunks.RemoveAt(0);
//                count--;
//            }
//            Thread.Sleep(delay);
//        }
//    }

//    public void Stop()
//    {
//        isRunning = false;
//        chunks.Clear();
//    }

//    public void SetDelay(int delayInMilliseconds)
//    {
//        delay = delayInMilliseconds;
//    }
//}
