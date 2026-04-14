using Jy.Packets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FrameSnapshotContainer
{
    const int TARGET_FRAME_RATE = 60;
    const float FRAME_TIME_EPSILON = 0.0001f;

    [SerializeField]
    float maintainElapsedTime = 1f;

    [SerializeField]
    List<FrameSnapshot> snapshots = null;

    public FrameSnapshotContainer()
    {
        //기본값은 60개, 반드시 프레임순으로 정렬되어 있어야 함
        snapshots = new List<FrameSnapshot>(TARGET_FRAME_RATE * Mathf.RoundToInt(maintainElapsedTime + 0.5f));
    }

    public void RemoveOldFrameSnapshot()
    {
        //지금 시간으로부터 1초전 스냅샷은 모두 삭제
        float now = Time.time;
        for (int i = 0; i < snapshots.Count; i++)
        {
            if (now - snapshots[i].creationTime > maintainElapsedTime)
                snapshots.RemoveAt(i);
        }
    }

    public FrameSnapshot this[uint frameNumber]
    {
        get
        {
            for (int i = 0; i < snapshots.Count; i++)
            {
                if (snapshots[i].frameNumber == frameNumber)
                    return snapshots[i];
            }

            return null;
        }
    }

    /// <summary>
    /// 클라이언트를 위한 메소드, 스냅샷을 잃어버렸고 중간 프레임을 복원하기 위한 것
    /// </summary>
    /// <param name="frameNumber"></param>
    /// <returns></returns>
    public FrameSnapshot Lerp(int frameNumber)
    {
        throw new NotImplementedException();
    }


    public FrameSnapshot Lerp(float time)
    {
        FrameSnapshot left = null;
        FrameSnapshot right = null;

        //아래의 3개 forloop 이게 논리가 단순해서 좋음...
        for (int i = 0; i <= snapshots.Count; ++i)
        {
            //approximatley same, return that
            if (Mathf.Abs(snapshots[i].creationTime - time) <= FRAME_TIME_EPSILON)
            {
                return snapshots[i];
            }
        }

        for (int i = 0; i < snapshots.Count - 1; ++i)
        {
            //find left
            if (snapshots[i].creationTime < time && time < snapshots[i + 1].creationTime)
            {
                left = snapshots[i];
                break;
            }
        }

        for (int i = 1; i < snapshots.Count; ++i)
        {
            //find left
            if (snapshots[i - 1].creationTime < time && time < snapshots[i].creationTime)
            {
                right = snapshots[i];
                break;
            }
        }

        Assert.IsNotNull(left);
        Assert.IsNotNull(right);

        return FrameSnapshot.LerpByTime(left, right, time);
    }

    public void Add(FrameSnapshot newSnapshot)
    {
        snapshots.Add(newSnapshot);
    }
}