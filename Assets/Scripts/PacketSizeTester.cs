using UnityEngine;

namespace Jy
{
    public class PacketSizeTester : MonoBehaviour
    {
        public void Start()
        {
            //FrameSnapshot fs = new FrameSnapshot();
            //fs.frameNumber = 1;
            //fs.creationTime = Time.time;

            //for (int i = 0; i < 1024; i++)
            //{
            //    fs.netObjectSnapshotById.Add(i, new NetworkObjectSnapshot()
            //    {
            //        networkId = i,
            //        position = Random.onUnitSphere * Random.Range(1f, 10f),
            //        rotation = Random.rotation
            //    });
            //}

            //NetworkByteStream stream = new NetworkByteStream(1024 * 1024);

            //fs.Serialize(stream);
            //Debug.Log($"Before compression, {stream.GetSize()}");

            //var output = new MemoryStream();
            //using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Fastest))
            //{
            //    gzip.Write(stream.data, 0, stream.data.Length);
            //}

            //Debug.Log($"After compression, {output.ToArray().Length}");
        }
    }
}