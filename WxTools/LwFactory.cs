using System.Collections.Generic;
using LwSoft;

namespace WxTools.Client
{
    public static class LwFactory
    {
        public static List<Lwsoft3> LwList = new List<Lwsoft3>();

        public static Lwsoft3 GetLw(int i)
        {
            if (LwList.Count <= i)
            {
                LwList.Add(new Lwsoft3());
            }
            return LwList[i];
        }

        public static Lwsoft3 GetDefault()
        {
            return GetLw(0);
        }

        public static Lwsoft3 GetNew()
        {
            return GetLw(LwList.Count);
        }

        public static void Clear()
        {
            for (int i = 0; i < LwList.Count; i++)
            {
                LwList[i] = null;
            }
            LwList.Clear();
        }
    }
}
