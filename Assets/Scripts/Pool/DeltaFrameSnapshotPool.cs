namespace Jy.Packets
{
    public class DeltaFrameSnapshotPool : CustomPoolBase<DeltaFrameSnapshot>
    {
        #region SINGLETON
        static DeltaFrameSnapshotPool instance;
        public static DeltaFrameSnapshotPool Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new DeltaFrameSnapshotPool();
                }

                return instance;
            }
        }
        #endregion
        protected override int InitCapacity => 16;
        protected override int MaxCapacity => 32;
    }
}
