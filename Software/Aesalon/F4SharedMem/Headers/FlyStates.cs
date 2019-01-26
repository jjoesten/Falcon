using System;
using System.Runtime.InteropServices;

namespace F4SharedMem
{
    [ComVisible(true)]
    [Serializable]
    public enum FlyStates : byte
    {
        /// <summary>
        /// UI - In the UI
        /// </summary>
        IN_UI = 0,
        /// <summary>
        /// UI->3D - Loading the sim data
        /// </summary>
        LOADING = 1,
        /// <summary>
        /// UI->3D - Waiting for other players
        /// </summary>
        WAITING = 2,
        /// <summary>
        /// 3D - Flying
        /// </summary>
        FLYING = 3,
        /// <summary>
        /// 3D->Dead - Dead, waiting for respawn
        /// </summary>
        DEAD = 4,
        /// <summary>
        /// Unknown flying state
        /// </summary>
        UNKNOWN = 5,
    };
}
