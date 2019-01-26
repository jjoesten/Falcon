using System;
using System.Runtime.InteropServices;

namespace F4SharedMem.Headers
{
    [ComVisible(true)]
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct IntellivibeData
    {
        /// <summary>
        /// How many AA missiles fired.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte AAMissileFired;

        /// <summary>
        /// How many Maverick/Rockets fired.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte AGMissileFired;

        /// <summary>
        /// How many bombs dropped.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte BombDropped;

        /// <summary>
        /// How many flares dropped.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte FlaresDropped;

        /// <summary>
        /// How many chaff dropped.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte ChaffDropped;

        /// <summary>
        /// How many bullets fired.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public byte BulletsFired;

        /// <summary>
        /// Collisions
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int CollisionCounter;

        /// <summary>
        /// Gun is firing.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool IsFiringGun;

        /// <summary>
        /// Ending the flight from 3D
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool IsEndFlight;

        /// <summary>
        /// Ejected from aircraft.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool IsEjecting;

        /// <summary>
        /// In 3D view
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool In3D;

        /// <summary>
        /// Sim paused.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool IsPaused;

        /// <summary>
        /// Sim frozen?
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool IsFrozen;

        /// <summary>
        /// Are G limits being exceeded?
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool IsOverG;

        /// <summary>
        /// Aircraft on the ground.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool IsOnGround;

        /// <summary>
        /// Are we exiting Falcon BMS?
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool IsExitGame;

        /// <summary>
        /// G forces on the aircraft.
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float Gforce;

        /// <summary>
        /// Location of eyes in relationship to the aircraft (X)
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float eyex;

        /// <summary>
        /// Location of eyes in relationship to the aircraft (Y)
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float eyey;

        /// <summary>
        /// Location of eyes in relationship to the aircraft (Z)
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float eyez;

        /// <summary>
        /// Location of last incoming damage, 1-8 depending on location.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int lastdamage;

        /// <summary>
        /// Magnitude of incoming damage.
        /// </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float damageforce;

        /// <summary>
        /// Time of last incoming damage
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int whendamage;
    }
}
