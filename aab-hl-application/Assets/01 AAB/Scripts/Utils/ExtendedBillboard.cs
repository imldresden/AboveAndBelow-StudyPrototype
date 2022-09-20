using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    /// <summary>
    /// The Billboard class implements the behaviors needed to keep a GameObject oriented towards the user.
    /// </summary>
    public class ExtendedBillboard : Billboard
    {

        /// <summary>
        /// Should this rotation be applied to the local rotation? If not, it will be applied to the global rotation.
        /// </summary>
        public bool UseLocalRotation
        {
            get { return useLocalRotation; }
            set { useLocalRotation = value; }
        }

        [Tooltip("Specifies where the rotation will be applied to.")]
        [SerializeField]
        private bool useLocalRotation = true;

        [SerializeField]
        private bool uglyFix = true;

        /// <summary>
        /// Keeps the object facing the camera.
        /// </summary>
        private void Update()
        {
            if (TargetTransform == null)
            {
                return;
            }
            if (uglyFix)
            {
                float tempX = transform.localRotation.eulerAngles.x;
                float tempY = transform.localRotation.eulerAngles.y;
                float tempZ = transform.localRotation.eulerAngles.z;
                Transform tempTransform = transform;
                Vector3 signVector = transform.InverseTransformDirection(Vector3.up);
                int sign = Math.Sign(signVector.y);
                int correction = 0;
                if (sign == -1)
                    correction = 180;
                

                //versuchen: zwei achsen sperren und nur lookat();
                tempTransform.LookAt(TargetTransform, transform.InverseTransformDirection(Vector3.up));
                switch (PivotAxis)
                {
                    case PivotAxis.X:
                        transform.localRotation = Quaternion.Euler(-tempTransform.localRotation.eulerAngles.x, tempY, tempZ);
                        break;
                    case PivotAxis.Y:
                        transform.localRotation = Quaternion.Euler(tempX, -tempTransform.localRotation.eulerAngles.y, tempZ);
                        break;
                    case PivotAxis.Z:
                        transform.localRotation = Quaternion.Euler(tempX, tempY, -tempTransform.localRotation.eulerAngles.z*sign);
                        break;
                    case PivotAxis.XY:
                        transform.localRotation = Quaternion.Euler(-tempTransform.localRotation.eulerAngles.x, -tempTransform.localRotation.eulerAngles.y, tempZ);
                        break;
                    case PivotAxis.XZ:
                        transform.localRotation = Quaternion.Euler(-tempTransform.localRotation.eulerAngles.x, tempY, -tempTransform.localRotation.eulerAngles.z);
                        break;
                    case PivotAxis.YZ:
                        transform.localRotation = Quaternion.Euler(tempX, -tempTransform.localRotation.eulerAngles.y, -tempTransform.localRotation.eulerAngles.z);
                        break;
                    default:
                        break;
                }
                return;
            }

            // Get a Vector that points from the target to the main camera.
            Vector3 directionToTarget = TargetTransform.position - transform.position;
        //    Debug.Log("Global: " + directionToTarget);
            if (useLocalRotation)
            {
                directionToTarget = transform.InverseTransformDirection(directionToTarget);
               // Debug.Log("Local: " + directionToTarget);
            }
            Vector3 parentRotation = Vector3.zero;

            bool useCameraAsUpVector = true;

            // Adjust for the pivot axis.
            switch (PivotAxis)
            {
                case PivotAxis.X:
                    directionToTarget.x = 0.0f;
                    parentRotation.x = transform.parent.rotation.eulerAngles.x;
                    useCameraAsUpVector = false;
                    break;

                case PivotAxis.Y:
                    directionToTarget.y = 0.0f;
                    parentRotation.y = transform.parent.rotation.eulerAngles.y;
                    useCameraAsUpVector = false;
                    break;

                case PivotAxis.Z:
                    directionToTarget.x = 0.0f;
                    directionToTarget.y = 0.0f;
                    parentRotation.z = transform.parent.rotation.eulerAngles.z;

                    break;

                case PivotAxis.XY:
                    parentRotation.x = transform.parent.rotation.eulerAngles.x;
                    parentRotation.y = transform.parent.rotation.eulerAngles.y;
                    useCameraAsUpVector = false;
                    break;

                case PivotAxis.XZ:
                    parentRotation.x = transform.parent.rotation.eulerAngles.x;
                    parentRotation.z = transform.parent.rotation.eulerAngles.z;
                    directionToTarget.x = 0.0f;
                    break;

                case PivotAxis.YZ:
                    parentRotation.y = transform.parent.rotation.eulerAngles.y;
                    parentRotation.z = transform.parent.rotation.eulerAngles.z;
                    directionToTarget.y = 0.0f;
                    break;

                case PivotAxis.Free:
                default:
                    // No changes needed.
                    break;
            }

            // If we are right next to the camera the rotation is undefined. 
            if (directionToTarget.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion rotation;
            // Calculate and apply the rotation required to reorient the object
            if (useCameraAsUpVector)
            {
                rotation = Quaternion.LookRotation(-directionToTarget, CameraCache.Main.transform.up);
                if (useLocalRotation)
                {
                    rotation = Quaternion.LookRotation(-directionToTarget, transform.InverseTransformDirection(CameraCache.Main.transform.up));
                }
            }

            else
                rotation = Quaternion.LookRotation(-directionToTarget);

            if (useLocalRotation)
            {
                rotation *= Quaternion.Euler(-parentRotation);
                transform.localRotation = rotation;
            }
            else
            {
                transform.rotation = rotation;
            }


        }
    }
}