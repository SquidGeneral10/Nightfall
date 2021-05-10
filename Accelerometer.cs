#region 'Using' Information
using Microsoft.Xna.Framework;
using System;
#endregion

namespace Nightfall
{
    public static class Accelerometer
    {
        private static bool isInitialized = false; // Stops the accelerometer from being initialised twice, which would result in everything being way too fast.

        private static bool isActive = false; // Used by enemies and player - sets accelerometer's active status.

        public static void Initialize() // Initialises the accelerator.
        {

            if (isInitialized) // Makes sure this only happens ONCE.
            {
                throw new InvalidOperationException("Initialize can only be called once");
            }

            isInitialized = true; // Makes extra, extra sure that it's initialised and will not run this code again
        }

        public static AccelerometerState GetState()
        {
            if (!isInitialized) // Makes sure the accelerometer is initialised before trying to 'get' it. Error message if it's not ready.
            {
                throw new InvalidOperationException("You must Initialize before you can call GetState");
            }

            Vector3 stateValue = new Vector3(); // Creates a new vector3 value for the accelerometer's state.

            return new AccelerometerState(stateValue, isActive);
        }
    }

    public struct AccelerometerState
    {
        public Vector3 Acceleration { get; private set; } // Gets the accelerometer's current acceleration value to determine how fast something will go.

        public bool IsActive { get; private set; } // Used to 'get' the accelerometer's current  active value.

        public AccelerometerState(Vector3 acceleration, bool isActive) // Checks to see if the accelerometer is currently active (e.g player or enemy is moving) before starting acceleration.
            : this()
        {
            Acceleration = acceleration;
            IsActive = isActive;
        }
    }
}
