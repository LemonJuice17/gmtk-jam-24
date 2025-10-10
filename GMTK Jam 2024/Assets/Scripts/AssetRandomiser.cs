using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetRandomiser : MonoBehaviour
{
    //[SerializeField] Transform targetTransform;

    Vector3 initialPos;
    Vector3 initialRot;
    Vector3 initialScale;

    [SerializeField] bool setInitialValuesOnAwake = true;
    [SerializeField] bool randomiseOnAwake = true;
    [Header("Position")]
    [SerializeField][Range(0, 2)] float positionVarianceX;
    [SerializeField][Range(0, 2)] float positionVarianceY;
    [SerializeField][Range(0, 2)] float positionVarianceZ;
    [Header("Rotation")]
    [SerializeField][Range(0, 180)] float rotationVarianceX;
    [SerializeField][Range(0, 180)] float rotationVarianceY;
    [SerializeField][Range(0, 180)] float rotationVarianceZ;
    [Header("Scale")]
    [SerializeField][Range(0, 2)] float scaleVarianceX;
    [SerializeField][Range(0, 2)] float scaleVarianceY;
    [SerializeField][Range(0, 2)] float scaleVarianceZ;
    [SerializeField] bool XZ_ProportionalScale;

    private void Awake()
    {
        if (setInitialValuesOnAwake)
        {
            SetInitialValues();
        }

        if (randomiseOnAwake)
        {
            Randomise();
        }
        
    }

    public void SetInitialValues()
    {
        initialPos = transform.position;
        initialRot = transform.rotation.eulerAngles;
        initialScale = transform.localScale;
    }

    public void Randomise()
    {
        transform.position = initialPos;
        transform.rotation = Quaternion.Euler(initialRot);
        transform.localScale = initialScale;

        Vector3 targetPosition = new Vector3(Random.Range(-positionVarianceX, positionVarianceX),
                                             Random.Range(-positionVarianceY, positionVarianceY),
                                             Random.Range(-positionVarianceZ, positionVarianceZ));
        Vector3 targetRotation = new Vector3(Random.Range(-rotationVarianceX, rotationVarianceX),
                                             Random.Range(-rotationVarianceY, rotationVarianceY),
                                             Random.Range(-rotationVarianceZ, rotationVarianceZ));

        Vector3 targetScale;
        if (XZ_ProportionalScale)
        {
            float xzScale = Random.Range(-scaleVarianceX, scaleVarianceX);
            targetScale = new Vector3(xzScale, Random.Range(-scaleVarianceY, scaleVarianceY), xzScale);
        }
        else
        {
            targetScale = new Vector3(Random.Range(-scaleVarianceX, scaleVarianceX),
                                             Random.Range(-scaleVarianceY, scaleVarianceY),
                                             Random.Range(-scaleVarianceZ, scaleVarianceZ));
        }


        transform.position = transform.position + targetPosition;
        transform.rotation = Quaternion.Euler(targetRotation.x, targetRotation.y, targetRotation.z);
        transform.localScale = transform.localScale + targetScale;
    }
}
