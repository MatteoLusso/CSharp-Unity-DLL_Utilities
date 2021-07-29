/*
   ___ ___  ___   ___ ___ ___  _   _ ___    _   _        
  | _ \ _ \/ _ \ / __| __|   \| | | | _ \  /_\ | |       
  |  _/   / (_) | (__| _|| |) | |_| |   / / _ \| |__     
  |_| |_|_\\___/ \___|___|___/ \___/|_|_\/_/ \_\____|    
                                                        
 ___ __  __   _   ___ ___ _  _   _ _____ ___ ___  _  _ 
|_ _|  \/  | /_\ / __|_ _| \| | /_\_   _|_ _/ _ \| \| |
 | || |\/| |/ _ \ (_ || || .` |/ _ \| |  | | (_) | .` |
|___|_|  |_/_/ \_\___|___|_|\_/_/ \_\_| |___\___/|_|\_|
                                                           
                                              UTILITIES
                                   Author: Matteo Lusso
                                                 © 2020
*/

/*
 
This is a DLL for Unity that adds some utility functions.
Compile to generate the DLL file (use NET. Framework 4.7.1).

*/

using System.Collections.Generic;
using UnityEngine;


namespace DLL_Utilities
{
    public static class Monitor
    {
        // This function returns the angle in degrees between the screen x-axis and its diagonal.
        public static float AspectRatioAngle()
        {
            return Mathf.Atan2(Screen.height, Screen.width) * Mathf.Rad2Deg;
        }

        public static bool IsObjectWithTagInsideVolume(Renderer ObjRenderer, Plane[] VolumeSides, string Tag)
        {
            if(!ObjRenderer.gameObject.CompareTag(Tag))
            {
                return false;
            }
            else
            {
                return GeometryUtility.TestPlanesAABB(VolumeSides, ObjRenderer.bounds);
            }
        }

        // This method is similar to the Unity IsVisible() one and it returns true when the target is in front of the camera.
        // The difference is it considers the distance of the far clipping plane infinite. It works with both orthographic and perspective projections.
        //
        //           camera limits
        //                         ↘    •
        //                           •  •
        //                        •     •                       target
        //                     •        •                     ↗
        //                  •           •                   o
        //               •       o      •                 ↙
        // camera C → •        ↙        •               ✔ visible
        //               •   ✔ visible •
        //                  •           •
        //                     •        •
        //               o        •     •
        //             ↙             •  •
        //           ✘ not visible     •
        //
        public static bool IsTargetOnScreen(Camera Cam, GameObject Target)
        {
            Plane[] Planes = new Plane[6];

            if (Cam.orthographic)
            {
                Planes[0] = new Plane(Cam.transform.right, Cam.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, Cam.nearClipPlane)));             // Left plane
                Planes[1] = new Plane(-Cam.transform.right, Cam.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, Cam.nearClipPlane)));    // Right plane

                Planes[2] = new Plane(Cam.transform.up, Cam.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, Cam.nearClipPlane)));                // Down plane
                Planes[3] = new Plane(-Cam.transform.up, Cam.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, Cam.nearClipPlane)));      // Down plane

                Planes[4] = new Plane(Cam.transform.forward, Cam.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, Cam.nearClipPlane)));           // Near plane
            }
            else
            {
                Planes = GeometryUtility.CalculateFrustumPlanes(Cam);
            }

            Planes[5] = new Plane(-Cam.transform.forward, Target.transform.position);                                                       // Far plane

            return GeometryUtility.TestPlanesAABB(Planes, Target.GetComponent<Renderer>().bounds);
        }

        // This function is used to determine on which side of the screen the object is situated.
        // AngleUI is the angle formed by the screen diagonal and the x-axis. AngleWorld is the
        // angle between the direction from the camera to the target and Vector3.right around the
        // Vector3.forward.
        //
        //                                  y-axis
        //    screen                          │
        //           ↘                        │                             • → screen diagonal 
        //            ┌───────────────────────┼───────────────────────•   
        //            │                       │                 •     │
        //            │                    ┌──┼──┐        •           │
        //            │                    │  │  │  •     │ → AngleUI │
        //            │                    │  •───────────────────────┼──── x-axis
        //            │       AngleWorld ← •                          │
        //            │                 •                             │
        //            │              •                                │
        //            └───────────•───────────────────────────────────┘
        //                     •                       
        //                   o → target is below the screen
        //
        public static Monitor.Side TargetPosition(float AngleWorld, float AngleUI)
        {
            if (AngleWorld >= 360.0f - AngleUI || AngleWorld < AngleUI)                 // Right.
            {
                return Monitor.Side.Right;
            }
            else if (AngleWorld >= AngleUI && AngleWorld < 180.0f - AngleUI)            // Above.
            {
                return Monitor.Side.Up;
            }
            else if (AngleWorld >= 180.0f - AngleUI && AngleWorld < 180.0f + AngleUI)   // Left.
            {
                return Monitor.Side.Left;
            }
            else if (AngleWorld >= 180.0f + AngleUI && AngleWorld < 360.0f - AngleUI)   // Below
            {
                return Monitor.Side.Down;
            }
            else
            {
                return Monitor.Side.None;
            }
        }

        public enum Side {None, Right, Down, Left, Up};
    }
}
