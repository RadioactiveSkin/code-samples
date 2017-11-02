/**
 * Jonah Wallschlaeger
 * 
 * MovePattern.cs - Create and assign different move patterns for arbitrary objects
 * uses polymoprhism to assign the proper move pattern to the object in question
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Prog4
{
    /// <summary>
    /// Abstract class MovePattern. Pick a MovePattern using
    /// polymorphism. 
    /// </summary>
    abstract class MovePattern
    {
        /// <summary>
        /// Move the figure in 3D space.
        /// </summary>
        /// <param name="fig">the figure to move</param>
        abstract public void Move(Figure fig);
    }

    /// <summary>
    /// Tumble move pattern. Figure will "tumble" back and forth along the Z-Axis.
    /// </summary>
    class Tumble : MovePattern
    {
        private bool forward = false;
        private const int UPPER_BOUND = 5;
        private const int LOWER_BOUND = -10;
        private const float ROTATE_SPEED = 0.03f;
        private const float HORIZONTAL_SPEED = 0.1f;

        /// <summary>
        /// Move the figure in 3D space.
        /// </summary>
        /// <param name="fig"></param>
        public override void Move(Figure fig)
        {
            if (forward)
            {
                if (fig.CurrentCenter.Z < LOWER_BOUND)
                {
                    forward = false;
                }
                fig.Translate(0, 0, -HORIZONTAL_SPEED);
                fig.RotateX(-ROTATE_SPEED);
            }
            else
            {
                if (fig.CurrentCenter.Z > UPPER_BOUND)
                {
                    forward = true;
                }
                fig.Translate(0, 0, HORIZONTAL_SPEED);
                fig.RotateX(ROTATE_SPEED);
            }
        }
    }

    /// <summary>
    /// Bounce move pattern. Bounce the object repeatedly, applies gravitational height decay for each bounce.
    /// </summary>
    class Bounce : MovePattern
    {
        private float vertSpeed = 0.0f;
        private float horzSpeedX = 0.0f;
        private float horzSpeedZ = 0.0f;
        private const float GRAVITY = 0.1f; //Pseudo gravity. How fast should the vertical velocity change
        private const float UPPER_PICKUP_BOUND = 10.5f;
        private const float LOWER_PICKUP_BOUND = 9.5f;
        private const float PICKUP_SPEED = 0.2f;
        private const int RANDOM_CONVERSION_FACTOR = 10;
        private const float END_BOUNCE_TRESHOLD = 0.05f;
        private const int RANDOM_UPPER = 2;
        private enum Phase
        {
            Pickup, Bounce
        };
        private Phase currPhase = Phase.Pickup;
        public override void Move(Figure fig)
        {
            if (currPhase == Phase.Pickup)
            {
                if (fig.CurrentCenter.Y > UPPER_PICKUP_BOUND)
                {
                    fig.Translate(0, -PICKUP_SPEED, 0);
                }
                else if (fig.CurrentCenter.Y < LOWER_PICKUP_BOUND)
                {
                    fig.Translate(0, PICKUP_SPEED, 0);
                }
                else
                {
                    currPhase = Phase.Bounce;
                    vertSpeed = 0.0f;
                    horzSpeedX = (float)(RandomProvider.Instance.NextDouble() / RANDOM_CONVERSION_FACTOR * (RandomProvider.Instance.Next(0, RANDOM_UPPER) == 0 ? -1 : 1));
                    horzSpeedZ = (float)(RandomProvider.Instance.NextDouble() / RANDOM_CONVERSION_FACTOR * (RandomProvider.Instance.Next(0, RANDOM_UPPER) == 0 ? -1 : 1));
                }
            }
            else if (currPhase == Phase.Bounce)
            {
                if (fig.CurrentCenter.Y < 0 && vertSpeed < 0)
                {
                    vertSpeed = -(vertSpeed);
                    if (vertSpeed < END_BOUNCE_TRESHOLD)
                    {
                        currPhase = Phase.Pickup;
                    }
                }
                vertSpeed = vertSpeed - GRAVITY;
                fig.Translate(horzSpeedX, vertSpeed, horzSpeedZ);
            }
        }
    }

    /// <summary>
    /// Pulse move pattern. Pulses the object like a beating heart 
    /// </summary>
    class Pulse : MovePattern
    {
        private const float GROWTH_FACTOR = 1.06f;
        private const int PULSE_SPEED = 3;
        private const int RANDOM_COVERSION_FACTOR = 10;
        private const float RANDOM_OFFSET = 0.5f;
        private const int FIRST_SHRINK_END = 2;
        private const int SECOND_GROW_START = 3;
        private const int SECOND_SHRINK_START = 4;
        private const int SECOND_SHRINK_END = 5;
        private const int PULSE_DELAY = 20;
        private int frame = 0;
        private Phase currPhase = Phase.DoNothing;
        private bool isFirstMove = true;
        private Random rand = new Random();
		
        /// <summary>
        /// Phase states.
        /// </summary>
        private enum Phase
        {
            Grow, Shrink, DoNothing
        };

        /// <summary>
        /// Pulse the object in 3D space.
        /// </summary>
        /// <param name="fig"></param>
        public override void Move(Figure fig)
        {
            if (isFirstMove)
            {
                isFirstMove = false;
                //Random Translate movement
                var translateX = (RandomProvider.Instance.NextDouble() - RANDOM_OFFSET) * RANDOM_COVERSION_FACTOR;
                var translateY = (RandomProvider.Instance.NextDouble() - RANDOM_OFFSET) * RANDOM_COVERSION_FACTOR;
                var translateZ = (RandomProvider.Instance.NextDouble() - RANDOM_OFFSET) * RANDOM_COVERSION_FACTOR;
                fig.Translate((float)translateX, (float)translateY, (float)translateZ);
                return;
            }

            if (frame == 0)
            {
                currPhase = Phase.Grow;
            }
            else if(frame == PULSE_SPEED)
            {
                currPhase = Phase.Shrink;
            }
            else if (frame == PULSE_SPEED * FIRST_SHRINK_END)
            {
                currPhase = Phase.DoNothing;
            }
            else if (frame == PULSE_SPEED * SECOND_GROW_START) 
            {
                currPhase = Phase.Grow;
            }
            else if (frame == PULSE_SPEED * SECOND_SHRINK_START)
            {
                currPhase = Phase.Shrink;
            }
            else if (frame == PULSE_SPEED * SECOND_SHRINK_END)
            {
                currPhase = Phase.DoNothing;
            }
            else if (frame == PULSE_SPEED * PULSE_DELAY)
            {
                frame = -1;
                return;
            }

            if (currPhase == Phase.Grow)
            {
                fig.Scale(GROWTH_FACTOR);
            }
            else if (currPhase == Phase.Shrink)
            {
                fig.Scale(1 / GROWTH_FACTOR);
            }
            frame++;
        }
    }

	/// <summary>
    /// Randomly applies different rotations, scalings, and translations to
	/// an object.
    /// </summary>
    class Freakout : MovePattern
    {
        private const float RANDOM_OFFSET = 0.5f;
        private const int DOUBLE = 2;
        private const int RANDOM_FACTOR = 10;
        private const int RANDOM_UPPER = 2;
        public override void Move(Figure fig)
        {
            //Random Translate
            var translateX = RandomProvider.Instance.NextDouble() - RANDOM_OFFSET;
            var translateY = RandomProvider.Instance.NextDouble() - RANDOM_OFFSET;
            var translateZ = RandomProvider.Instance.NextDouble() - RANDOM_OFFSET;
            fig.Translate((float)translateX, (float)translateY, (float)translateZ);


            //Random Rotate
            var rotateX = RandomProvider.Instance.NextDouble() - RANDOM_OFFSET;
            var rotateY = RandomProvider.Instance.NextDouble() - RANDOM_OFFSET;
            var rotateZ = RandomProvider.Instance.NextDouble() - RANDOM_OFFSET;

            fig.RotateX((float)rotateX);
            fig.RotateY((float)rotateY);
            fig.RotateZ((float)rotateZ);

            //Random Scale
            var scale = (RandomProvider.Instance.NextDouble() * DOUBLE / RANDOM_FACTOR) + 1;
            var scaleUp = RandomProvider.Instance.Next(0, RANDOM_UPPER) == 0;
            if (scaleUp)
            {
                fig.Scale((float)scale);
            }
            else
            {
                fig.Scale((float)(1 / scale));
            }
        }
    }

	/// <summary>
    /// Move pattern for the projectile being shot from the ship.
    /// </summary>
    class Shoot : MovePattern
    {
       private Vector3 direction;
       private Vector3 position;
       private bool isFirstMove = true;

       public Shoot(Vector3 path, Vector3 location)
       {
          direction = path;
          position = location;
       }
	   //checks if it's currently being shot, and if not, to continue along the vector it is already traveling
       public override void Move(Figure fig)
       {
          if (isFirstMove)
          {
             isFirstMove = false;
             
             fig.Translate(position.X + (direction.X * 4), (position.Y + (direction.Y * 4) - 2), position.Z + (direction.Z * 4));
          }
          fig.Translate(direction.X, direction.Y, direction.Z);
       }
    }
}
